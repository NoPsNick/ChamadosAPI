using System.Windows;


namespace ChamadosAPI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private void LoginControl_LoginSuccessful(object sender, LoginEventArgs e)
        {
            Logado logado = new(e.User);
            logado.Show();
            this.Hide();

            MessageBox.Show($"Seja bem-vindo, {e.User.Username}!", "Bem-Vindo", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}