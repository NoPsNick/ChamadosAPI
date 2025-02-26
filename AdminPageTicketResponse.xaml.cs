using System.Windows;
using System.Windows.Controls;


namespace ChamadosAPI
{
    /// <summary>
    /// Interação lógica para AdminPageTicketResponse.xam
    /// </summary>
    public partial class AdminPageTicketResponse : Page
    {
        private readonly Admin _admin_window;
        public TicketResponse Response { get; }
        public AdminPageTicketResponse(Admin admin_window, TicketResponse ticket_response)
        {
            InitializeComponent();
            _admin_window = admin_window;
            Response = ticket_response;
            DataContext = this; // Define o contexto de dados da página
        }

        public void BackButton(object sender, RoutedEventArgs e)
        {
            _admin_window.Back();
        }
    }
}
