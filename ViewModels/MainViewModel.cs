using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using QrCodeRecovery.Helpers;
using QrCodeRecovery.Models;

namespace QrCodeRecovery.ViewModels;

public class MainViewModel : INotifyPropertyChanged, IDisposable
{
    private string _wystawca = "";
    private string _konto = "";
    private string _recoveryPhrase = "";
    private bool _isRecoveryMode = true;
    private BitmapImage? _qrCodeImage;
    private string _verificationCode = "";
    private string _generatedSecretKey = "";
    private string _errorMessage = "";
    private int _secondsRemaining;
    private readonly DispatcherTimer _timer;
    private readonly DispatcherTimer _clipboardTimer;
    private string? _copiedSecret;
    private bool _disposed;

    public string Wystawca { get => _wystawca; set { _wystawca = value; OnPropertyChanged(); RefreshQrAndCode(); } }
    public string Konto { get => _konto; set { _konto = value; OnPropertyChanged(); RefreshQrAndCode(); } }
    public string RecoveryPhrase { get => _recoveryPhrase; set { _recoveryPhrase = value; OnPropertyChanged(); RefreshQrAndCode(); } }
    public bool IsRecoveryMode
    {
        get => _isRecoveryMode;
        set { _isRecoveryMode = value; OnPropertyChanged(); OnPropertyChanged(nameof(IsSecretMode)); OnPropertyChanged(nameof(InputModeDescription)); RefreshQrAndCode(); }
    }
    public bool IsSecretMode { get => !_isRecoveryMode; set => IsRecoveryMode = !value; }
    public string InputModeDescription => IsRecoveryMode
        ? "Text Phrase: tekst UTF-8 zostanie zakodowany jako NOWY sekret TOTP (nie odzyskuje konta)."
        : "Recovery Key: wpisz istniejący sekret TOTP w formacie Base32 (nie kod zapasowy).";
    public BitmapImage? QrCodeImage { get => _qrCodeImage; private set { _qrCodeImage = value; OnPropertyChanged(); } }
    public string VerificationCode { get => _verificationCode; private set { _verificationCode = value; OnPropertyChanged(); OnPropertyChanged(nameof(VerificationCodeFormatted)); } }
    public string VerificationCodeFormatted => _verificationCode.Length == 6 ? _verificationCode.Insert(3, " ") : "";
    public string GeneratedSecretKey { get => _generatedSecretKey; private set { _generatedSecretKey = value; OnPropertyChanged(); } }
    public int SecondsRemaining { get => _secondsRemaining; private set { _secondsRemaining = value; OnPropertyChanged(); } }
    public string ErrorMessage { get => _errorMessage; private set { _errorMessage = value; OnPropertyChanged(); } }
    public bool ShowSecret { get; set; } // Reserved for a future explicit reveal control.

    public ICommand CopySecretCommand { get; }
    public ICommand ClearCommand { get; }

    public MainViewModel()
    {
        CopySecretCommand = new RelayCommand(_ => CopySecret());
        ClearCommand = new RelayCommand(_ => Clear());
        _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
        _timer.Tick += OnTimerTick;
        _clipboardTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(30) };
        _clipboardTimer.Tick += OnClipboardTimeout;
    }

    private void CopySecret()
    {
        if (string.IsNullOrWhiteSpace(GeneratedSecretKey)) return;
        try
        {
            Clipboard.SetText(GeneratedSecretKey);
            _copiedSecret = GeneratedSecretKey;
            _clipboardTimer.Stop();
            _clipboardTimer.Start();
            ErrorMessage = "Sekret skopiowany. Schowek zostanie wyczyszczony za 30 s, jeśli nie zmienisz jego zawartości.";
        }
        catch (System.Runtime.InteropServices.ExternalException)
        {
            ErrorMessage = "Schowek jest niedostępny. Spróbuj ponownie.";
        }
    }

    private void OnClipboardTimeout(object? sender, EventArgs e)
    {
        _clipboardTimer.Stop();
        try
        {
            if (_copiedSecret is not null && Clipboard.ContainsText() && Clipboard.GetText() == _copiedSecret)
                Clipboard.Clear();
        }
        catch (System.Runtime.InteropServices.ExternalException) { /* Clipboard may be locked by another process. */ }
        finally { _copiedSecret = null; }
    }

    public void ClearAll() => Clear();

    private void Clear()
    {
        RecoveryPhrase = "";
        Wystawca = "";
        Konto = "";
        ResetResults();
        ErrorMessage = "";
    }

    private void ResetResults()
    {
        _timer.Stop();
        QrCodeImage = null;
        VerificationCode = "";
        GeneratedSecretKey = "";
        SecondsRemaining = 0;
    }

    private void OnTimerTick(object? sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(GeneratedSecretKey)) { ResetResults(); return; }
        long seconds = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        SecondsRemaining = 30 - (int)(seconds % 30);
        if (SecondsRemaining == 30) UpdateVerificationCode();
    }

    private void UpdateVerificationCode() => VerificationCode = TotpGenerator.GenerateTotp(GeneratedSecretKey);

    private void RefreshQrAndCode()
    {
        ResetResults();
        ErrorMessage = "";
        if (string.IsNullOrWhiteSpace(Wystawca) || string.IsNullOrWhiteSpace(Konto) || string.IsNullOrWhiteSpace(RecoveryPhrase)) return;
        try
        {
            string secret = IsRecoveryMode
                ? Base32Utils.StringToBase32(RecoveryPhrase)
                : RecoveryPhrase.Trim().Replace(" ", "").ToUpperInvariant();
            // Decode once to validate Base32 before creating the QR.
            byte[] bytes = Base32Utils.FromBase32String(secret);
            if (bytes.Length < 10)
                throw new ArgumentException("Sekret jest za krótki (minimum 10 bajtów).");
            System.Security.Cryptography.CryptographicOperations.ZeroMemory(bytes);
            GeneratedSecretKey = secret;
            QrCodeImage = QrCodeHelper.GenerateQrCode(Wystawca, Konto, secret);
            UpdateVerificationCode();
            SecondsRemaining = 30 - (int)(DateTimeOffset.UtcNow.ToUnixTimeSeconds() % 30);
            _timer.Start();
        }
        catch (ArgumentException ex) { ResetResults(); ErrorMessage = "Nieprawidłowy sekret: " + ex.Message; }
        catch (Exception ex) { ResetResults(); ErrorMessage = "Nie udało się wygenerować kodu QR: " + ex.Message; }
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        _timer.Stop();
        _timer.Tick -= OnTimerTick;
        _clipboardTimer.Stop();
        _clipboardTimer.Tick -= OnClipboardTimeout;
        ResetResults();
        _recoveryPhrase = "";
        _copiedSecret = null;
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? name = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}

public class RelayCommand(Action<object?> execute, Func<object?, bool>? canExecute = null) : ICommand
{
    public bool CanExecute(object? parameter) => canExecute?.Invoke(parameter) ?? true;
    public void Execute(object? parameter) => execute(parameter);
    public event EventHandler? CanExecuteChanged
    {
        add => CommandManager.RequerySuggested += value;
        remove => CommandManager.RequerySuggested -= value;
    }
}
