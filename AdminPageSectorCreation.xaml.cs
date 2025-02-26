using System.Windows;
using System.Windows.Controls;


namespace ChamadosAPI
{
    /// <summary>
    /// Interação lógica para AdminPageSectorCreation.xam
    /// </summary>
    public partial class AdminPageSectorCreation : Page
    {
        private readonly Admin _admin_window;

        public AdminPageSectorCreation(Admin admin_window)
        {
            InitializeComponent();
            _admin_window = admin_window;
            Loaded += AdminPageUsers_Loaded;
        }

        private async void AdminPageUsers_Loaded(object sender, RoutedEventArgs e)
        {
            await _admin_window.LoadUsers();
            LoadUsers();
        }

        private void LoadUsers()
        {
            GerenteComboBox.Items.Clear();

            ComboBoxItem nenhumItem = new()
            {
                Content = "Sem gerente",
                Tag = null,
                IsSelected = true,
            };
            GerenteComboBox.Items.Add(nenhumItem);

            if (_admin_window.Users is List<User> users)
            {
                foreach (var user in users)
                {
                    var item = new ComboBoxItem
                    {
                        Content = user.Username,
                        Tag = user.Id
                    };
                    GerenteComboBox.Items.Add(item);
                }
            }
        }

        private void BackButton(object sender, RoutedEventArgs e)
        {
            _admin_window.Back();
        }

        private async void CreateSectorButtonClick(object sender, RoutedEventArgs e)
        {
            var nome = NomeTextBox.Text;
            var descricao = DescricaoTextBox.Text;


            var gerenteId = GerenteComboBox.SelectedItem is ComboBoxItem selectedItem && selectedItem.Tag != null
                ? (int?)selectedItem.Tag
                : null;

            Retorno retorno = await _admin_window._user.CreateSectorAsync(nome, descricao, gerenteId);

            if (retorno.Sucesso)
            {
                MessageBox.Show(retorno.GetMessage(), retorno.Titulo, MessageBoxButton.OK, MessageBoxImage.Information);
                _admin_window.Back();
            }
            else
            {
                MessageBox.Show(retorno.Mensagem, retorno.Titulo, MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
    }
}
