using System.Windows;
using System.Windows.Controls;


namespace ChamadosAPI
{
    /// <summary>
    /// Interação lógica para AdminPageHome.xam
    /// </summary>
    public partial class AdminPageHome : Page
    {
        private readonly Admin _admin_window;
        public AdminPageHome(Admin admin)
        {
            InitializeComponent();
            _admin_window = admin;
        }

        private void SectorsButtonClick(object sender, RoutedEventArgs e)
        {
            _admin_window.AdminFrame.Navigate(new AdminPageSectors(_admin_window));
        }

        private void TicketsButtonClick(object sender, RoutedEventArgs e)
        {
            _admin_window.AdminFrame.Navigate(new AdminPageTickets(_admin_window));
        }

        private void UsersButtonClick(object sender, RoutedEventArgs e)
        {
            _admin_window.AdminFrame.Navigate(new AdminPageUsers(_admin_window));
        }

        private void ReturnToLogadoButtonClick(object sender, RoutedEventArgs e)
        {
            _admin_window.ReturnToLogado();
        }
    }
}
