using System.Windows;
using System.Windows.Controls;


namespace ChamadosAPI
{
    /// <summary>
    /// Interação lógica para UserPageTicketResponse.xam
    /// </summary>
    public partial class UserPageTicketResponse : Page
    {
        private readonly Logado _user_window;
        public TicketResponse Response { get; }
        public UserPageTicketResponse(Logado user_window, TicketResponse ticket_response)
        {
            InitializeComponent();
            _user_window = user_window;
            Response = ticket_response;
            DataContext = this; // Define o contexto de dados da página
        }

        public void BackButton(object sender, RoutedEventArgs e)
        {
            _user_window.Back();
        }
    }
}
