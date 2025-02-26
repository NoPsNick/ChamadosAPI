using System.Windows;
using System.Windows.Controls;


namespace ChamadosAPI
{
    /// <summary>
    /// Interação lógica para UserPageHome.xam
    /// </summary>
    public partial class UserPageHome : Page
    {
        private readonly Logado _user_window;
        public UserPageHome(Logado user_window)
        {
            InitializeComponent();
            _user_window = user_window;
            WelcomeTextBlock.Text = $"Seja bem-vindo {_user_window._user.Username}.";
            if ((_user_window._user.IsStaff ?? false) || (_user_window._user.IsSuperAdmin ?? false))
            {
                Button admin_button = new()
                {
                    Content = "Página de administração",
                    Width = 200,
                    Height = 80,
                };

                admin_button.Click += (sender, e) => _user_window.AdminButtonClick(_user_window._user);

                AdminButton.Children.Add(admin_button);
            }
        }

        private void SendTicketButton_Click(object sender, RoutedEventArgs e)
        {
            _user_window.UserFrame.Navigate(new UserPageSendTicket(_user_window));
        }

        private void UserSentButton_Click(object sender, RoutedEventArgs e)
        {
            _user_window.UserFrame.Navigate(new UserPageTickets(_user_window, true));
        }

        private void UserReceivedButton_Click(object sender, RoutedEventArgs e)
        {
            _user_window.UserFrame.Navigate(new UserPageTickets(_user_window, false));
        }

        private async void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Tem certeza que deseja sair?", "Confirmação", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                Retorno retorno = await _user_window._user.LogoutAsync();
                if (retorno.Sucesso)
                {
                    MessageBox.Show(retorno.GetMessage(), retorno.Titulo, MessageBoxButton.OK, MessageBoxImage.Information);
                    Application.Current.MainWindow.Show();
                    _user_window.Close();
                }
                else
                {
                    MessageBox.Show(retorno.Mensagem, retorno.Titulo, MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}
