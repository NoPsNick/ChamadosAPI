using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;


namespace ChamadosAPI
{
    /// <summary>
    /// Interação lógica para AdminPageUsers.xam
    /// </summary>
    public partial class AdminPageUsers : Page
    {
        private readonly Admin _admin_window;
        private int currentPage = 1;
        private const int itemsPerPage = 5;
        public AdminPageUsers(Admin admin_window)
        {
            InitializeComponent();
            _admin_window = admin_window;
            Loaded += AdminPageUsers_Loaded;
        }

        private async void AdminPageUsers_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadUsers();
        }

        private async Task LoadUsers()
        {
            await _admin_window.LoadUsers();
            UpdatePagination();
        }

        private void UpdatePagination()
        {
            UsuariosStackPanel.Children.Clear();

            int totalPages = (int)Math.Ceiling((double)_admin_window.Users.Count / itemsPerPage);
            if (totalPages == 0) totalPages = 1;
            currentPage = Math.Max(1, Math.Min(currentPage, totalPages));

            var usuariosPagina = _admin_window.Users.Skip((currentPage - 1) * itemsPerPage).Take(itemsPerPage);

            foreach (var usuario in usuariosPagina)
            {
                var usuarioPanel = CreateUserPanel(usuario);
                UsuariosStackPanel.Children.Add(usuarioPanel);
            }

            AddPaginationButtons(totalPages);
        }

        private StackPanel CreateUserPanel(User usuario)
        {
            var usuarioPanel = new StackPanel
            {
                Orientation = Orientation.Vertical,
                Margin = new Thickness(5),
                Background = new SolidColorBrush(Colors.LightGray)
            };

            var descricaoBuilder = new StringBuilder()
                .Append($"ID: {usuario.Id}, username: {usuario.Username}");

            if (usuario.Sector is not null)
            {
                descricaoBuilder.Append($", Setor: {usuario.Sector.Name}");
            }

            var descricaoUsuario = new TextBlock
            {
                Text = descricaoBuilder.ToString(),
                Margin = new Thickness(5)
            };

            usuarioPanel.Children.Add(descricaoUsuario);
            usuarioPanel.Children.Add(CreateUserButtons(usuario));

            return usuarioPanel;
        }

        private StackPanel CreateUserButtons(User usuario)
        {
            var botoesPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(5)
            };

            var editarButton = new Button
            {
                Content = "Ver usuário",
                Margin = new Thickness(5),
                Width = 80,
                Height = 30
            };
            editarButton.Click += (s, e) =>
            {
                _admin_window.AdminFrame.Navigate(new AdminPageUserChange(_admin_window, usuario));
            };

            botoesPanel.Children.Add(editarButton);
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

            UsuariosStackPanel.Children.Add(paginacaoPanel);
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

        private void BackButton(object sender, RoutedEventArgs e)
        {
            _admin_window.Back();
        }
    }
}
