using System.Windows;
using System.Windows.Controls;


namespace ChamadosAPI
{
    public partial class LoginControl : UserControl
    {
        private bool logando = false;
        public LoginControl() => InitializeComponent();

        // Evento para notificar sucesso no login
        public event EventHandler<LoginEventArgs> LoginSuccessful;

        private async void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            if (logando) // Bloqueia múltiplos cliques
            {
                MessageBox.Show("Você já está tentando realizar um login, aguarde um momento.", "Aguarde", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                logando = true;
                LoginButton.IsEnabled = false; // Desativa botão visualmente

                if (!ValidateFields(
                    (UsernameTextBox.Text, "Usuário"),
                    (PasswordBox.Password, "Senha"))
                   )
                {
                    return;
                }

                string username = UsernameTextBox.Text;
                string password = PasswordBox.Password;

                User user = new(null);
                Retorno retorno = await user.LoginAsync(username, password);

                if (retorno.Sucesso && retorno.ObjetoRetornado is User authenticatedUser)
                {
                    OnLoginSuccessful(authenticatedUser);
                }
                else
                {
                    MessageBox.Show(retorno.Mensagem, retorno.Titulo, MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro inesperado: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                logando = false;
                LoginButton.IsEnabled = true; // Reativa o botão após a tentativa de login
            }
        }

        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            var registro = new Registro();
            registro.Show();
        }

        protected virtual void OnLoginSuccessful(User authenticatedUser)
        {
            LoginSuccessful?.Invoke(this, new LoginEventArgs { User = authenticatedUser });
        }

        private static bool ValidateFields(params (string FieldValue, string FieldName)[] fields)
        {
            foreach (var (FieldValue, FieldName) in fields)
            {
                if (string.IsNullOrWhiteSpace(FieldValue))
                {
                    MessageBox.Show($"Por favor, preencha o campo '{FieldName}'.", "Campo obrigatório", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return false;
                }
            }
            return true;
        }
    }

    public class LoginEventArgs : EventArgs
    {
        public required User User { get; set; }
    }
}
