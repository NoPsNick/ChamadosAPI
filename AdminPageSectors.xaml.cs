using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;


namespace ChamadosAPI
{
    public partial class AdminPageSectors : Page
    {
        private readonly Admin _admin_window;
        private int currentPage = 1;
        private const int itemsPerPage = 5;

        public AdminPageSectors(Admin admin_window)
        {
            InitializeComponent();
            _admin_window = admin_window;
            Loaded += AdminPageSectors_Loaded;
        }

        private async void AdminPageSectors_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadSectors();
        }

        private async Task LoadSectors()
        {
            await _admin_window.LoadSectors();
            UpdatePagination();
        }

        private void UpdatePagination()
        {
            SetoresStackPanel.Children.Clear();

            int totalPages = (int)Math.Ceiling((double)_admin_window.Sectors.Count / itemsPerPage);
            if (totalPages == 0) totalPages = 1;
            currentPage = Math.Max(1, Math.Min(currentPage, totalPages));

            var setoresPagina = _admin_window.Sectors.Skip((currentPage - 1) * itemsPerPage).Take(itemsPerPage);

            foreach (var setor in setoresPagina)
            {
                var setorPanel = CreateSectorPanel(setor);
                SetoresStackPanel.Children.Add(setorPanel);
            }

            AddPaginationButtons(totalPages);
        }

        private StackPanel CreateSectorPanel(Sector setor)
        {
            var setorPanel = new StackPanel
            {
                Orientation = Orientation.Vertical,
                Margin = new Thickness(5),
                Background = new SolidColorBrush(Colors.LightGray)
            };

            var descricaoBuilder = new StringBuilder()
                .Append($"Setor: {setor.Name}, Descrição: {setor.Description}");

            if (setor.Leader_ID.HasValue)
            {
                descricaoBuilder.Append($", ID do gerente: {setor.Leader_ID}");
            }

            var descricaoSetor = new TextBlock
            {
                Text = descricaoBuilder.ToString(),
                Margin = new Thickness(5)
            };

            setorPanel.Children.Add(descricaoSetor);
            setorPanel.Children.Add(CreateSectorButtons(setor));

            return setorPanel;
        }

        private StackPanel CreateSectorButtons(Sector setor)
        {
            var botoesPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(5)
            };

            var editarButton = new Button
            {
                Content = "Editar",
                Margin = new Thickness(5),
                Width = 80,
                Height = 30
            };
            editarButton.Click += (s, e) =>
            {
                _admin_window.AdminFrame.Navigate(new AdminPageSectorChange(_admin_window, setor));
            };

            botoesPanel.Children.Add(editarButton);

            if (_admin_window._user.IsSuperAdmin ?? false)
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
                        $"Você tem certeza que deseja deletar o setor '{setor.Name}'?",
                        "Confirmar Deleção",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Warning);

                    if (result == MessageBoxResult.Yes)
                    {
                        await DeleteSectorAsync(setor);
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

            SetoresStackPanel.Children.Add(paginacaoPanel);
        }

        private void NextPage_Click(object sender, RoutedEventArgs e)
        {
            if ((currentPage * itemsPerPage) < _admin_window.Sectors.Count)
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

        private async Task DeleteSectorAsync(Sector setor)
        {
            Retorno retorno = await _admin_window._user.DeleteSectorAsync(setor.Id);

            if (retorno.Sucesso)
            {
                MessageBox.Show(retorno.GetMessage(), retorno.Titulo, MessageBoxButton.OK, MessageBoxImage.Information);
                _admin_window.ClearLists();
                await LoadSectors();
            }
            else
            {
                MessageBox.Show($"{retorno.Mensagem}", retorno.Titulo, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void BackButton(object sender, RoutedEventArgs e)
        {
            _admin_window.Back();
        }

        private void Criar_Setor_Click(object sender, RoutedEventArgs e)
        {
            _admin_window.AdminFrame.Navigate(new AdminPageSectorCreation(_admin_window));
        }
    }
}
