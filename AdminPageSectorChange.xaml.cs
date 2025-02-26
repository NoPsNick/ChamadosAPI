using System.Windows;
using System.Windows.Controls;


namespace ChamadosAPI
{
    public partial class AdminPageSectorChange : Page
    {
        private readonly Admin _admin_window;
        private readonly Sector Setor;

        public AdminPageSectorChange(Admin admin, Sector setor)
        {
            InitializeComponent();
            _admin_window = admin;
            Setor = setor;
            Loaded += AdminPageUsers_Loaded;
        }

        private async void AdminPageUsers_Loaded(object sender, RoutedEventArgs e)
        {
            LoadSectorDetails();
            await _admin_window.LoadUsers();
            LoadUsers();
        }

        private void LoadUsers()
        {
            GerenteComboBox.Items.Clear();

            ComboBoxItem placeholderItem = new()
            {
                Content = "Não alterar o gerente atual",
                Tag = null,
                IsSelected = true
            };
            GerenteComboBox.Items.Add(placeholderItem);

            ComboBoxItem removerItem = new()
            {
                Content = "Remover o gerente atual",
                Tag = -1
            };
            GerenteComboBox.Items.Add(removerItem);

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

        private void LoadSectorDetails()
        {
            NomeTextBox.Text = Setor.Name;
            DescricaoTextBox.Text = Setor.Description;
        }

        private async void ChangeSectorButtonClick(object sender, RoutedEventArgs e)
        {
            var nome = NomeTextBox.Text;
            var descricao = DescricaoTextBox.Text;

            var gerenteId = GerenteComboBox.SelectedItem is ComboBoxItem selectedItem && selectedItem.Tag != null
                ? (int?)selectedItem.Tag
                : null;

            Retorno retorno = await _admin_window._user.ChangeSectorAsync(Setor.Id, nome, descricao, gerenteId);

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

        private void BackButton(object sender, RoutedEventArgs e)
        {
            _admin_window.Back();
        }
    }
}
