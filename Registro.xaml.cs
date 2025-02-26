using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ChamadosAPI
{
    /// <summary>
    /// Lógica interna para Registro.xaml
    /// </summary>
    public partial class Registro : Window
    {
        public Registro()
        {
            InitializeComponent();
        }

        private async void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            // Verificar se todos os campos estão preenchidos
            if (!ValidateFields(
                (UsernameTextBox.Text, "Usuário"),
                (PasswordBox.Password, "Senha"),
                (EmailTextBox.Text, "Email"))
                )
            {
                return;
            }

            // Todos os campos estão preenchidos, prossiga com a criação do usuário
            string username = UsernameTextBox.Text;
            string password = PasswordBox.Password;
            string email = EmailTextBox.Text;

            User user = new (null); // Como precisa do manager, criado sem passar nenhuma informação.

            Retorno retorno = await user.RegisterAsync(username, password, email);

            if (retorno.Sucesso)
            {
                this.Close();
                MessageBox.Show(retorno.Mensagem, retorno.Titulo, MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show(retorno.Mensagem, retorno.Titulo, MessageBoxButton.OK, MessageBoxImage.Warning);
            }
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
}
