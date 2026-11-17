using System.Windows;
using QrCodeRecovery.ViewModels;

namespace QrCodeRecovery
{
    public partial class MainWindow : Window
    {
        private readonly MainViewModel _viewModel;

        public MainWindow()
        {
            InitializeComponent();
            _viewModel = new MainViewModel();
            DataContext = _viewModel;

            Closed += (_, _) => _viewModel.Dispose();

            // Synchronizacja hasła z ViewModel
            PasswordBoxSecret.PasswordChanged += (s, e) =>
            {
                _viewModel.RecoveryPhrase = PasswordBoxSecret.Password;
            };
        }
        private void PasswordBoxSecret_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext is MainViewModel vm)
            {
                vm.RecoveryPhrase = PasswordBoxSecret.Password;
            }
        }
    }
}