using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;


namespace ChamadosAPI
{
    /// <summary>
    /// Interação lógica para AdminPageTicketCheck.xam
    /// </summary>
    public partial class AdminPageTicketCheck : Page
    {
        private readonly Admin _admin_window;
        public Ticket Chamado { get; }
        public List<TicketResponse> Respostas;
        private int currentPage = 1;
        private const int itemsPerPage = 5;

        public AdminPageTicketCheck(Admin admin_window, Ticket ticket)
        {
            InitializeComponent();
            _admin_window = admin_window;
            Chamado = ticket;
            DataContext = this; // Define o contexto de dados da página
            Respostas = [];
            Loaded += StatusLoaded;
        }

        private void StatusLoaded(object sender, RoutedEventArgs e)
        {
            LoadStatus();
        }

        private async void FetchResponsesButtonClick(object sender, RoutedEventArgs e)
        {
            Retorno retorno = await _admin_window._user.GetTicketResponsesAsync(Chamado.Id);

            if (retorno.Sucesso && retorno.ObjetoRetornado is List<TicketResponse> respostas)
            {
                Respostas = respostas;
                UpdatePagination();
            }
            else
            {
                MessageBox.Show(retorno.Mensagem, retorno.Titulo);
            }
        }

        private void UpdatePagination()
        {
            ResponsesStackPanel.Children.Clear();

            int totalPages = (int)Math.Ceiling((double)Respostas.Count / itemsPerPage);
            if (totalPages == 0) totalPages = 1;
            currentPage = Math.Max(1, Math.Min(currentPage, totalPages));

            var respostasPagina = Respostas.Skip((currentPage - 1) * itemsPerPage).Take(itemsPerPage);

            foreach (var resposta in respostasPagina)
            {
                var setorPanel = CreateResponsePanel(resposta);
                ResponsesStackPanel.Children.Add(setorPanel);
            }
            AddPaginationButtons(totalPages);
        }

        private StackPanel CreateResponsePanel(TicketResponse resposta)
        {
            var setorPanel = new StackPanel
            {
                Orientation = Orientation.Vertical,
                Margin = new Thickness(5),
                Background = new SolidColorBrush(Colors.LightGray)
            };

            var descricaoBuilder = new StringBuilder()
                .Append($"Resposta: {resposta.Id}");

            if (resposta.Responder is not null)
            {
                descricaoBuilder.Append($", Remetente: {resposta.Responder.Username}");
            }

            if (resposta.CreatedAt is not null)
            {
                descricaoBuilder.Append($", Data da resposta: {resposta.CreatedAt}");
            }

            var descricaoResposta = new TextBlock
            {
                Text = descricaoBuilder.ToString(),
                Margin = new Thickness(5)
            };

            setorPanel.Children.Add(descricaoResposta);
            setorPanel.Children.Add(CreateTicketButtons(resposta));

            return setorPanel;
        }

        private StackPanel CreateTicketButtons(TicketResponse response)
        {
            var botoesPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(5)
            };

            var editarButton = new Button
            {
                Content = "Ver resposta",
                Margin = new Thickness(5),
                Width = 80,
                Height = 30
            };
            editarButton.Click += (s, e) =>
            {
                _admin_window.AdminFrame.Navigate(new AdminPageTicketResponse(_admin_window, response));
            };

            botoesPanel.Children.Add(editarButton);

            return botoesPanel;
        }

        public void BackButton(object sender, RoutedEventArgs e)
        {
            Respostas.Clear();
            _admin_window.Back();
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

            ResponsesStackPanel.Children.Add(paginacaoPanel);
        }

        private void NextPage_Click(object sender, RoutedEventArgs e)
        {
            if ((currentPage * itemsPerPage) < Respostas.Count)
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

        private void LoadStatus()
        {
            StatusComboBox.Items.Clear();

            var statusAtual = Chamado.Status.ToString() == "pending" ? "Pendente" : "Finalizado";
            StatusComboBox.Items.Add(new ComboBoxItem
            {
                Content = statusAtual,
                Tag = null,
                IsSelected = true
            });

            string opcaoAlternativa = statusAtual == "Pendente" ? "Finalizado" : "Pendente";
            string tagAlternativa = statusAtual == "Pendente" ? "finalizado" : "pendente";

            StatusComboBox.Items.Add(new ComboBoxItem
            {
                Content = opcaoAlternativa,
                Tag = tagAlternativa
            });
        }

        private async void ChangeStatusButtonClick(object sender, RoutedEventArgs e)
        {
            if (StatusComboBox.SelectedItem is ComboBoxItem selectedItem && selectedItem.Tag is string newStatus)
            {
                Chamado.Status = newStatus;
                Retorno retorno = await _admin_window._user.ChangeTicketStatusAsync(Chamado.Id, newStatus);

                if (retorno.Sucesso)
                {
                    MessageBox.Show(retorno.GetMessage(), retorno.Titulo, MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else if (!retorno.Sucesso) 
                {
                    MessageBox.Show(retorno.Mensagem, retorno.Titulo, MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("Selecione um status válido.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
    }
}
