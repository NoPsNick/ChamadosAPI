using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;


namespace ChamadosAPI
{
    /// <summary>
    /// Interação lógica para AdminPageTickets.xam
    /// </summary>
    public partial class AdminPageTickets : Page
    {
        private readonly Admin _admin_window;
        private int currentPage = 1;
        private const int itemsPerPage = 5;

        public AdminPageTickets(Admin admin_window)
        {
            InitializeComponent();
            _admin_window = admin_window;
        }

        private async Task TicketSearch(int ticketId)
        {
            await _admin_window.TicketSearch(ticketId);
            UpdatePagination();
        }

        private async Task UserTicketsSearch(int userId)
        {
            await _admin_window.UserTicketsSearch(userId);
            UpdatePagination();
        }

        private void UpdatePagination()
        {
            TicketsStackPanel.Children.Clear();

            int totalPages = (int)Math.Ceiling((double)_admin_window.Tickets.Count / itemsPerPage);
            if (totalPages == 0) totalPages = 1;
            currentPage = Math.Max(1, Math.Min(currentPage, totalPages));

            var chamadosPagina = _admin_window.Tickets.Skip((currentPage - 1) * itemsPerPage).Take(itemsPerPage);

            foreach (var chamado in chamadosPagina)
            {
                var chamadoPanel = CreateTicketPanel(chamado);
                TicketsStackPanel.Children.Add(chamadoPanel);
            }

            AddPaginationButtons(totalPages);
        }

        private StackPanel CreateTicketPanel(Ticket chamado)
        {
            var chamadoPanel = new StackPanel
            {
                Orientation = Orientation.Vertical,
                Margin = new Thickness(5),
                Background = new SolidColorBrush(Colors.LightGray)
            };

            var descricaoBuilder = new StringBuilder()
                .Append($"Chamado: {chamado.Id}");

            if (chamado.Sender is not null)
            {
                descricaoBuilder.Append($", Remetente: {chamado.Sender.Username}");
            }

            if (chamado.Receivers is not null && chamado.Receivers.Count != 0)
            {
                string destinatarios = string.Join(", ", chamado.Receivers.Select(user => user.Username));
                descricaoBuilder.Append($", Destinatários: {destinatarios}");
            }

            var descricaoChamado = new TextBlock
            {
                Text = descricaoBuilder.ToString(),
                Margin = new Thickness(5)
            };

            chamadoPanel.Children.Add(descricaoChamado);
            chamadoPanel.Children.Add(CreateTicketButtons(chamado));

            return chamadoPanel;
        }

        private StackPanel CreateTicketButtons(Ticket chamado)
        {
            var botoesPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(5)
            };

            var editarButton = new Button
            {
                Content = "Ver chamado",
                Margin = new Thickness(5),
                Width = 80,
                Height = 30
            };
            editarButton.Click += (s, e) =>
            {
                _admin_window.AdminFrame.Navigate(new AdminPageTicketCheck(_admin_window, chamado));
            };

            var removerButton = new Button
            {
                Content = "Remover",
                Margin = new Thickness(5),
                Width = 80, Height = 30,
                Background = new SolidColorBrush(Colors.Yellow),
                Foreground = new SolidColorBrush(Colors.Black)
            };

            removerButton.Click += (s, e) =>
            {
                var result = MessageBox.Show(
                    $"Você tem certeza que deseja remover o chamado '{chamado.Title}' #{chamado.Id} da lista? Ele " +
                    "NÃO será removido do banco de dados.",
                    "Confirmar Remoção",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Information);

                if (result == MessageBoxResult.Yes)
                {
                    _admin_window.RemoveTicket(chamado.Id);
                    UpdatePagination();
                }
            };
            botoesPanel.Children.Add(editarButton);
            botoesPanel.Children.Add(removerButton);

            bool canDelete = (_admin_window._user.IsSuperAdmin ?? false) || (_admin_window._user.Id == chamado.Sender?.Id);
            if (canDelete)
            {
                var deletarButton = new Button
                {
                    Content = "Deletar",
                    Margin = new Thickness(5),
                    Width = 80,
                    Height = 30,
                    Background = new SolidColorBrush(Colors.Red),
                    Foreground = new SolidColorBrush(Colors.White)
                };
                deletarButton.Click += async (s, e) =>
                {
                    var result = MessageBox.Show(
                        $"Você tem certeza que deseja deletar o chamado '{chamado.Title}' #{chamado.Id}? Ele será DELETADO" +
                        " do banco de dados.",
                        "Confirmar Deleção",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Warning);

                    if (result == MessageBoxResult.Yes)
                    {
                        await DeleteTicketAsync(chamado);
                    }
                };
                botoesPanel.Children.Add(deletarButton);
            }

            return botoesPanel;
        }

        private void AddPaginationButtons(int totalPages)
        {
            var paginacaoPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(10)
            };

            var prevButton = new Button
            {
                Content = "Anterior",
                Width = 100,
                Height = 30,
                Margin = new Thickness(5),
                IsEnabled = currentPage > 1
            };
            prevButton.Click += PreviousPage_Click;
            paginacaoPanel.Children.Add(prevButton);

            var pageText = new TextBlock
            {
                Text = $"Página {currentPage} de {totalPages}",
                FontSize = 16,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(10)
            };
            paginacaoPanel.Children.Add(pageText);

            var nextButton = new Button
            {
                Content = "Próximo",
                Width = 100,
                Height = 30,
                Margin = new Thickness(5),
                IsEnabled = currentPage < totalPages
            };
            nextButton.Click += NextPage_Click;
            paginacaoPanel.Children.Add(nextButton);

            TicketsStackPanel.Children.Add(paginacaoPanel);
        }

        private void NextPage_Click(object sender, RoutedEventArgs e)
        {
            if ((currentPage * itemsPerPage) < _admin_window.Tickets.Count)
            {
                currentPage++;
                UpdatePagination();
            }
        }

        private void PreviousPage_Click(object sender, RoutedEventArgs e)
        {
            if (currentPage > 1)
            {
                currentPage--;
                UpdatePagination();
            }
        }

        public void BackButton(object sender, RoutedEventArgs e)
        {
            _admin_window.Back();
        }

        private async void GetTicketButtonClick(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(TicketIdTextBox.Text, out int ticketId))
            {
                await TicketSearch(ticketId);
            }
        }

        private async void GetUserTicketsButtonClick(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(UserIdTextBox.Text, out int userId))
            {
                await UserTicketsSearch(userId);
            }
        }

        private async Task DeleteTicketAsync(Ticket chamado)
        {
            Retorno retorno = await _admin_window._user.DeleteTicketAsync(chamado.Id);

            if (retorno.Sucesso)
            {
                MessageBox.Show(retorno.GetMessage(), retorno.Titulo, MessageBoxButton.OK, MessageBoxImage.Information);
                _admin_window.RemoveTicket(chamado.Id);
                UpdatePagination();
            }
            else
            {
                MessageBox.Show($"{retorno.Mensagem}", retorno.Titulo, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ClearList(object sender, RoutedEventArgs e)
        {
            if (_admin_window.Tickets.Count > 0)
            {
                var result = MessageBox.Show(
                        $"Você tem certeza que deseja limpar a lista?",
                        "Confirmar Limpeza",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Information);
                if (result == MessageBoxResult.Yes)
                {
                    _admin_window.Tickets.Clear();
                    UpdatePagination();
                    MessageBox.Show("Chamados foram removidos da lista com sucesso.", "Lista Limpa", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }

    }
}
