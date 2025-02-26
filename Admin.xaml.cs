using System.Windows;


namespace ChamadosAPI
{
    /// <summary>
    /// Lógica interna para Admin.xaml
    /// </summary>
    public partial class Admin : Window
    {
        public User _user = new(null);
        public List<Sector> Sectors = [];
        public List<User> Users = [];
        public List<Ticket> Tickets = [];

        public Admin(User user)
        {
            _user = user;
            bool isAllowed = (user.IsStaff ?? false) || (user.IsSuperAdmin ?? false);
            if (isAllowed)
            {
                InitializeComponent();
                AdminFrame.Navigate(new AdminPageHome(this));
            }
            else
            {
                ReturnToLogado();
            }
        }

        public void ReturnToLogado()
        {
            ClearLists();
            Logado voltar = new(_user);
            voltar.Show();

            Close();
        }

        public void ClearLists()
        {
            Sectors = [];
            Users = [];
            Tickets = [];
        }

        public void Back()
        {
            if (AdminFrame.NavigationService.CanGoBack)
            {
                ClearLists();
                AdminFrame.NavigationService.GoBack();
            }
        }

        public async Task LoadSectors()
        {
            if (Sectors.Count == 0)
            {
                Retorno retorno = await _user.GetSectorsAsync();

                if (retorno.Sucesso && retorno.ObjetoRetornado is List<Sector> sectors)
                {
                    Sectors.Clear();
                    Sectors = sectors;
                }
                else
                {
                    MessageBox.Show(retorno.Mensagem, retorno.Titulo, MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        public async Task LoadUsers()
        {
            if (Users.Count == 0)
            {
                Retorno retorno = await _user.GetUsersAsync();

                if (retorno.Sucesso && retorno.ObjetoRetornado is List<User> users)
                {
                    Users.Clear();
                    Users = users;
                }
                else
                {
                    MessageBox.Show(retorno.Mensagem, retorno.Titulo, MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        public async Task TicketSearch(int ticketId)
        {
            var retorno = await _user.GetTicketAsync(ticketId);

            if (retorno.Sucesso && retorno.ObjetoRetornado is Ticket ticket)
            {
                if (!Tickets.Any(t => t.Id == ticket.Id)) // Evita duplicatas
                {
                    Tickets.Add(ticket);
                }
            }
            else
            {
                MessageBox.Show(retorno.Mensagem, retorno.Titulo, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        public async Task UserTicketsSearch(int userId)
        {
            var retorno = await _user.GetTicketsFromUserAsync(userId);

            if (retorno.Sucesso && retorno.ObjetoRetornado is List<Ticket> userTickets)
            {
                var existingIds = new HashSet<int>(Tickets.Select(t => t.Id));

                foreach (var ticket in userTickets)
                {
                    if (!existingIds.Contains(ticket.Id))
                    {
                        Tickets.Add(ticket);
                    }
                }
            }
            else
            {
                MessageBox.Show(retorno.Mensagem, retorno.Titulo, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void RemoveTicket(int ticketId)
        {
            var ticket = Tickets.FirstOrDefault(c => c.Id == ticketId);
            if (ticket != null)
            {
                Tickets.Remove(ticket);
            }
        }
    }
}
