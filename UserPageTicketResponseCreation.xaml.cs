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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ChamadosAPI
{
    /// <summary>
    /// Interação lógica para UserPageTicketResponseCreation.xam
    /// </summary>
    public partial class UserPageTicketResponseCreation : Page
    {
        private readonly Logado _user_window;
        private readonly Ticket Chamado;

        public UserPageTicketResponseCreation(Logado user_window, Ticket ticket)
        {
            InitializeComponent();
            _user_window = user_window;
            Chamado = ticket;
        }

        private async void CreateTicketResponseButton_Click(object sender, RoutedEventArgs e)
        {
            string response = ReponseTextBox.Text;

            if (!string.IsNullOrWhiteSpace(response))
            {
                Retorno retorno = await _user_window._user.CreateTicketResponseAsync(Chamado.Id, response);

                if(retorno.Sucesso && retorno.ObjetoRetornado is TicketResponse ticket_response)
                {
                    var result = MessageBox.Show(
                        $"Você deseja ver a resposta criada?",
                        "Ver Resposta",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Information);
                    if (result == MessageBoxResult.Yes)
                    {
                        _user_window.UserFrame.Navigate(new UserPageTicketResponse(_user_window, ticket_response));
                    }
                    else 
                    { 
                        _user_window.Back();
                    }
                }
                else
                {
                    MessageBox.Show(retorno.Mensagem, retorno.Titulo, MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        public void BackButton(object sender, RoutedEventArgs e)
        {
            _user_window.Back();
        }
    }
}
