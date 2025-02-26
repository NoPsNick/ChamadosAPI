using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;


namespace ChamadosAPI
{
    /// <summary>
    /// Interação lógica para UserPageTickets.xam
    /// </summary>
    public partial class UserPageTickets : Page
    {
        private readonly Logado _user_window;
        private int currentPage = 1;
        private const int itemsPerPage = 5;
        private List<Ticket> currentList = [];
        private bool _sent;

        public UserPageTickets(Logado user_window, bool sent = true)
        {
            InitializeComponent();
            _user_window = user_window;
            _sent = sent;
            Loaded += Page_Loaded;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (_sent)
            {
                LoadUserSentTickets();
            }
            else
            {
                LoadUserReceivedTickets();
            }
        }

        private void BackButton(object sender, RoutedEventArgs e)
        {
            _user_window.Back();
        }

        private async void LoadUserSentTickets()
        {
            _user_window.SentTickets.Clear();
            await _user_window.LoadSentTickets();
            if (_user_window.SentTickets.Count > 0)
            {
                currentList = _user_window.SentTickets;
                UpdatePagination();
            }
            else
            {
                MessageBox.Show("Você não possuí chamados enviados ou algum erro ocorreu.", "Sem chamados", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private async void LoadUserReceivedTickets()
        {
            _user_window.ReceivedTickets.Clear();
            await _user_window.LoadReceivedTickets();
            if (_user_window.ReceivedTickets.Count > 0)
            {
                currentList = _user_window.ReceivedTickets;
                UpdatePagination();
            }
            else
            {
                MessageBox.Show("Você não possuí chamados recebidos ou algum erro ocorreu.", "Sem chamados", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void UpdatePagination()
        {
            TicketsStackPanel.Children.Clear();

            int totalPages = (int)Math.Ceiling((double)currentList.Count / itemsPerPage);
            if (totalPages == 0) totalPages = 1;
            currentPage = Math.Max(1, Math.Min(currentPage, totalPages));

            var chamadosPagina = currentList.Skip((currentPage - 1) * itemsPerPage).Take(itemsPerPage);

            foreach (var chamado in chamadosPagina)
            {
                var setorPanel = CreateTicketPanel(chamado);
                TicketsStackPanel.Children.Add(setorPanel);
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

            string status = chamado.Status.ToString() == "pending" ? "Pendente" : "Finalizado";
            descricaoBuilder.Append($", Status: {status}");

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
                _user_window.UserFrame.Navigate(new UserPageTicketCheck(_user_window, chamado));
            };

            var removerButton = new Button
            {
                Content = "Remover",
                Margin = new Thickness(5),
                Width = 80,
                Height = 30,
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
                    RemoveTicket(chamado.Id);
                    UpdatePagination();
                }
            };
            botoesPanel.Children.Add(editarButton);
            botoesPanel.Children.Add(removerButton);

            var responseButton = new Button
            {
                Content = "Responder",
                Margin = new Thickness(5),
                Width = 80,
                Height = 30,
                Background = new SolidColorBrush(Colors.Blue),
                Foreground = new SolidColorBrush(Colors.White)
            };

            responseButton.Click += (s, e) =>
            {
                _user_window.UserFrame.Navigate(new UserPageTicketResponseCreation(_user_window, chamado));
            };
            botoesPanel.Children.Add(responseButton);

            bool canDelete = (_user_window._user.IsSuperAdmin ?? false) || (_user_window._user.Id == chamado.Sender?.Id);
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
            if ((currentPage * itemsPerPage) < currentList.Count)
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

        private void ClearList(object sender, RoutedEventArgs e)
        {
            if (currentList.Count > 0)
            {
                var result = MessageBox.Show(
                        $"Você tem certeza que deseja limpar a lista?",
                        "Confirmar Limpeza",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Information);
                if (result == MessageBoxResult.Yes)
                {
                    currentList.Clear();
                    _user_window.ClearLists();
                    UpdatePagination();
                    MessageBox.Show("Chamados foram removidos da lista com sucesso.", "Lista Limpa", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }

        public void RemoveTicket(int ticketId)
        {
            var ticket = currentList.FirstOrDefault(c => c.Id == ticketId);
            if (ticket != null)
            {
                currentList.Remove(ticket);
            }
        }

        private async Task DeleteTicketAsync(Ticket chamado)
        {
            Retorno retorno = await _user_window._user.DeleteTicketAsync(chamado.Id);

            if (retorno.Sucesso)
            {
                MessageBox.Show(retorno.GetMessage(), retorno.Titulo, MessageBoxButton.OK, MessageBoxImage.Information);
                RemoveTicket(chamado.Id);
                UpdatePagination();
            }
            else
            {
                MessageBox.Show(retorno.Mensagem, retorno.Titulo, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SentButton_Click(object sender, RoutedEventArgs e)
        {
            LoadUserSentTickets();
            _sent = true;
        }

        private void ReceivedButton_Click(object sender, RoutedEventArgs e)
        {
            LoadUserReceivedTickets();
            _sent = false;
        }
    }
}
