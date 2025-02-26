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
    /// Lógica interna para Logado.xaml
    /// </summary>
    public partial class Logado : Window
    {
        public User _user = new(null);
        public List<Sector> Sectors = [];
        public List<User> Users = [];
        public List<Ticket> SentTickets = [];
        public List<Ticket> ReceivedTickets = [];
        public Logado(User user)
        {
            _user = user;
            InitializeComponent();
            UserFrame.Navigate(new UserPageHome(this));
        }

        public void ClearLists()
        {
            Sectors.Clear();
            Users.Clear();
            SentTickets.Clear();
            ReceivedTickets.Clear();
        }

        public void AdminButtonClick(User user)
        {
            if ((user.IsStaff ?? false) || (user.IsSuperAdmin ?? false))
            {
                Admin admin = new(user);
                admin.Show();
                this.Close();
            }
        }

        public void Back()
        {
            if (UserFrame.NavigationService.CanGoBack)
            {
                ClearLists();
                UserFrame.NavigationService.GoBack();
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

        public async Task LoadSentTickets()
        {
            if (SentTickets.Count == 0)
            {
                Retorno retorno = await _user.GetSentTicketsAsync();

                if (retorno.Sucesso && retorno.ObjetoRetornado is List<Ticket> tickets)
                {
                    SentTickets.Clear();
                    SentTickets = tickets;
                }
                else
                {
                    MessageBox.Show(retorno.Mensagem, retorno.Titulo, MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        public async Task LoadReceivedTickets()
        {
            if (ReceivedTickets.Count == 0)
            {
                Retorno retorno = await _user.GetReceivedTicketsAsync();

                if (retorno.Sucesso && retorno.ObjetoRetornado is List<Ticket> tickets)
                {
                    ReceivedTickets.Clear();
                    ReceivedTickets = tickets;
                }
                else
                {
                    MessageBox.Show(retorno.Mensagem, retorno.Titulo, MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

    }
}
