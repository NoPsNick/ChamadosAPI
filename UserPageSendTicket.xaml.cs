using System.Windows;
using System.Windows.Controls;


namespace ChamadosAPI
{
    /// <summary>
    /// Interação lógica para UserPageSendTicket.xam
    /// </summary>
    public partial class UserPageSendTicket : Page
    {
        private readonly Logado _user_window;
        public UserPageSendTicket(Logado user_window)
        {
            InitializeComponent();
            _user_window = user_window;
            Loaded += UserPage_Loaded;
        }

        private void BackButton(object sender, RoutedEventArgs e)
        {
            _user_window.Back();
        }

        private async void UserPage_Loaded(object sender, RoutedEventArgs e)
        {
            await _user_window.LoadUsers();
            await _user_window.LoadSectors();
            LoadTreeView();
        }

        private void LoadTreeView()
        {
            SectorUserTreeView.Items.Clear();

            // Adiciona setores apenas se houver usuários associados
            foreach (var setor in _user_window.Sectors)
            {
                // Seleciona os usuários que pertencem ao setor e remove o próprio usuário logado.
                var usuariosNoSetor = _user_window.Users.Where(u => u.Sector?.Id == setor.Id && u.Id != _user_window._user.Id).ToList();

                if (usuariosNoSetor.Count == 0)
                    continue; // Pula setores sem usuários

                TreeViewItem sectorNode = new();
                CheckBox sectorCheckBox = new()
                {
                    Content = setor.Name,
                    Tag = setor
                };

                sectorNode.Header = sectorCheckBox;
                sectorNode.Tag = setor;

                sectorCheckBox.Checked += (s, e) => SelectAllUsers(sectorNode, true);
                sectorCheckBox.Unchecked += (s, e) => SelectAllUsers(sectorNode, false);

                foreach (var usuario in usuariosNoSetor)
                {
                    CheckBox userCheckbox = new()
                    {
                        Content = usuario.Username,
                        Tag = usuario
                    };

                    sectorNode.Items.Add(userCheckbox);
                }

                SectorUserTreeView.Items.Add(sectorNode);
            }

            // Adiciona a categoria "Usuários sem setor"
            var usuariosSemSetor = _user_window.Users.Where(u => u.Sector == null).ToList();
            if (usuariosSemSetor.Count != 0)
            {
                TreeViewItem noSectorNode = new();
                CheckBox noSectorCheckBox = new()
                {
                    Content = "Usuários sem setor",
                    Tag = "NoSector"
                };

                noSectorNode.Header = noSectorCheckBox;
                noSectorNode.Tag = "NoSector";

                noSectorCheckBox.Checked += (s, e) => SelectAllUsers(noSectorNode, true);
                noSectorCheckBox.Unchecked += (s, e) => SelectAllUsers(noSectorNode, false);

                foreach (var usuario in usuariosSemSetor)
                {
                    CheckBox userCheckbox = new()
                    {
                        Content = usuario.Username,
                        Tag = usuario
                    };

                    noSectorNode.Items.Add(userCheckbox);
                }

                SectorUserTreeView.Items.Add(noSectorNode);
            }
        }


        private static void SelectAllUsers(TreeViewItem sectorNode, bool isChecked)
        {
            foreach (var item in sectorNode.Items)
            {
                if (item is CheckBox checkBox)
                {
                    checkBox.IsChecked = isChecked;
                }
            }
        }

        private async void SendTicketButton_Click(object sender, RoutedEventArgs e)
        {
            string title = TitleTextBox.Text;
            string description = DescriptionTextBox.Text;

            if (!ValidateFields(
                (TitleTextBox.Text, "Título"),
                (DescriptionTextBox.Text, "Descrição"))
                )
            {
                return;
            }

            HashSet<int> selectedUserIds = [];

            foreach (TreeViewItem sectorNode in SectorUserTreeView.Items)
            {
                if (sectorNode.Header is CheckBox sectorCheckBox && sectorCheckBox.IsChecked == true)
                {
                    // Se um setor foi selecionado, adiciona todos os usuários do setor
                    if (sectorCheckBox.Tag is Sector setor)
                    {
                        var usersInSector = _user_window.Users
                                            .Where(u => u.Sector?.Id == setor.Id && u.Id.HasValue && u.Id != _user_window._user.Id) // Ignora usuários sem ID e o próprio usuário.
                                            .Select(u => u.Id!.Value); // Usa apenas IDs válidos

                        selectedUserIds.UnionWith(usersInSector);
                    }
                }

                // Percorre os usuários dentro do setor
                foreach (var item in sectorNode.Items)
                {
                    if (item is CheckBox userCheckbox && userCheckbox.IsChecked == true)
                    {
                        if (userCheckbox.Tag is User usuario && usuario.Id.HasValue) // Verifica se o ID não é nulo
                        {
                            selectedUserIds.Add(usuario.Id.Value);
                        }
                    }
                }
            }
            if ( selectedUserIds.Count > 0 )
            {
                Retorno retorno = await _user_window._user.CreateTicketAsync(title, description, selectedUserIds);

                if (retorno.Sucesso && retorno.ObjetoRetornado is Ticket ticket)
                {
                    MessageBox.Show($"Chamado #{ticket.Id} enviado com sucesso.", "Chamado enviado", MessageBoxButton.OK, MessageBoxImage.Information); 
                    _user_window.Back();
                }
            }

        }

        private static bool ValidateFields(params (string FieldValue, string FieldName)[] fields)
        {
            foreach (var (FieldValue, FieldName) in fields)
            {
                if (string.IsNullOrWhiteSpace(FieldValue))
                {
                    MessageBox.Show($"Por favor, preencha o campo '{FieldName}'.", "Campo obrigatório", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return false;
                }
            }
            return true;
        }
    }
}
