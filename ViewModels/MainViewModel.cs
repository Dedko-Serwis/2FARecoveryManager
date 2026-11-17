using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using QrCodeRecovery.Models;
using QrCodeRecovery.Helpers;

namespace QrCodeRecovery.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private string _wystawca = string.Empty;
        private string _konto = string.Empty;
        private string _recoveryPhrase = string.Empty;
        private bool _isRecoveryMode = true;
        private BitmapImage? _qrCodeImage;
        private string _verificationCode = string.Empty;
        private readonly DispatcherTimer _timer;
        private bool _isGenerated;
        private int _secondsRemaining;
        private string myPrivateSecretKey = string.Empty;
        private string _generatedSecretKey = string.Empty;

        public string GeneratedSecretKey
        {
            get => _generatedSecretKey;
            set
            {
                _generatedSecretKey = value;
                OnPropertyChanged();
            }
        }

        public int SecondsRemaining
        {
            get => _secondsRemaining;
            set { _secondsRemaining = value; OnPropertyChanged();                
            }
        }

        public string Wystawca
        {
            get => _wystawca;
            set { _wystawca = value; OnPropertyChanged();
                RefreshQrAndCode();
            }
        }

        public string Konto
        {
            get => _konto;
            set { _konto = value; OnPropertyChanged();
                RefreshQrAndCode();
            }
        }

        public string RecoveryPhrase
        {
            get => _recoveryPhrase;
            set { _recoveryPhrase = value; OnPropertyChanged();
                RefreshQrAndCode();
            }
        }

        public bool IsRecoveryMode
        {
            get => _isRecoveryMode;
            set
            {
                _isRecoveryMode = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsSecretMode));
                OnPropertyChanged(nameof(InputModeDescription));
                RefreshQrAndCode();
            }
        }

        public bool IsSecretMode
        {
            get => !_isRecoveryMode;
            set
            {
                IsRecoveryMode = !value;
            }
        }

        public string InputModeDescription
        {
            get
            {
                return IsRecoveryMode
                    ? "Tryb Phrase: klucz zostanie zakodowany do Base32"
                    : "Tryb Secret: fraza zostanie uzyta bezposrednio";                
            }
        }

        public BitmapImage? QrCodeImage
        {
            get => _qrCodeImage;
            set { _qrCodeImage = value; OnPropertyChanged(); }
        }

        public string VerificationCode
        {
            get => _verificationCode;
            set {
                _verificationCode = value; 
                OnPropertyChanged();
                OnPropertyChanged(nameof(VerificationCodeFormatted));
            }
        }
        public string VerificationCodeFormatted
        {
            get
            {
                if (string.IsNullOrWhiteSpace(_verificationCode) ||
                    _verificationCode.Length < 6)
                    return string.Empty;

                return _verificationCode.Insert(3, " ");
            }
        }


        public MainViewModel()
        {
            CopySecretCommand = new RelayCommand(_ =>
            {
                if (!string.IsNullOrWhiteSpace(GeneratedSecretKey))
                {
                    Clipboard.SetText(GeneratedSecretKey);
                }
            });

            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            _timer.Tick += OnTimerTick;
        }

        private bool CanGenerate()
        {
            return !string.IsNullOrWhiteSpace(Wystawca) &&
                   !string.IsNullOrWhiteSpace(Konto) &&
                   !string.IsNullOrWhiteSpace(RecoveryPhrase);
        }

        private string GetSecretForOtp()
        {
            if (IsRecoveryMode)
            {
                // Tryb Recovery: kodujemy do Base32                
                return Base32Utils.StringToBase32(RecoveryPhrase);
            }
            else
            {
                // Tryb Secret: używamy frazy bezpośrednio
                return RecoveryPhrase.Trim().Replace(" ", "").ToUpperInvariant();
            }
        }

        private void OnTimerTick(object? sender, EventArgs e)
        {
            if (!_isGenerated || string.IsNullOrWhiteSpace(RecoveryPhrase))
                return;

            DateTime now = DateTime.UtcNow;
            long unixTime = (long)(now - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds;
            SecondsRemaining = 30 - (int)(unixTime % 30);

            if (SecondsRemaining == 30 || SecondsRemaining == 0)
            {
                UpdateVerificationCode();
            }
        }

        private void UpdateVerificationCode()
        {
            VerificationCode = TotpGenerator.GenerateTotp(myPrivateSecretKey);             
        }

        private void RefreshQrAndCode()
        {
            if (!CanGenerate())
                return;

            try
            {
                myPrivateSecretKey = GetSecretForOtp();
                GeneratedSecretKey = myPrivateSecretKey;

                QrCodeImage = QrCodeHelper.GenerateQrCode(
                    Wystawca,
                    Konto,
                    myPrivateSecretKey);

                UpdateVerificationCode();

                if (!_timer.IsEnabled)
                {
                    _isGenerated = true;
                    _timer.Start();
                }
            }
            catch
            {
                // opcjonalnie ignoruj błędy podczas wpisywania
            }
        }
        public ICommand CopySecretCommand { get; }


        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    public class RelayCommand(
        Action<object?> execute,
        Func<object?, bool>? canExecute = null) : ICommand
    {
        private readonly Action<object?> _execute = execute;
        private readonly Func<object?, bool>? _canExecute = canExecute;

        public bool CanExecute(object? parameter)
            => _canExecute == null || _canExecute(parameter);

        public void Execute(object? parameter)
            => _execute(parameter);

        public event EventHandler? CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }
    }

}