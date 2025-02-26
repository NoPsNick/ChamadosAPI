using System.Windows;
using System.Windows.Controls;


namespace ChamadosAPI
{
    /// <summary>
    /// Interação lógica para AdminPageUserChange.xam
    /// </summary>
    public partial class AdminPageUserChange : Page
    {
        private readonly Admin _admin_window;
        public User Usuario { get; }

        public AdminPageUserChange(Admin admin_window, User user)
        {
            InitializeComponent();
            _admin_window = admin_window;
            Usuario = user;
            DataContext = this; // Define o contexto de dados da página
            Loaded += LoadComboBoxed;
        }

        private async void LoadComboBoxed(object sender, RoutedEventArgs e)
        {
            await _admin_window.LoadSectors();
            LoadComboBoxes();
        }

        public void BackButton(object sender, RoutedEventArgs e)
        {
            _admin_window.Back();
        }

        private void LoadComboBoxes()
        {
            StaffComboBox.Items.Clear();
            SectorComboBox.Items.Clear();
            ActiveComboBox.Items.Clear();

            bool isStaff = Usuario.IsStaff ?? false;
            if (_admin_window._user.IsSuperAdmin ?? false)
            {
                StaffComboBox.Items.Add(new ComboBoxItem
                {
                    Content = "Sim",
                    Tag = isStaff ? null : true,
                    IsSelected = isStaff
                });

                StaffComboBox.Items.Add(new ComboBoxItem
                {
                    Content = "Não",
                    Tag = !isStaff ? null : false,
                    IsSelected = !isStaff
                });
            }
            else
            {
                StaffComboBox.Items.Add(new ComboBoxItem
                {
                    Content = isStaff ? "Sim" : "Não",
                    Tag = null,
                    IsSelected = true
                });
            }

            bool isActive = Usuario.IsActive ?? false;
            if (_admin_window._user.IsSuperAdmin ?? false)
            {
                ActiveComboBox.Items.Add(new ComboBoxItem
                {
                    Content = "Sim",
                    Tag = isActive ? null : true,
                    IsSelected = isActive
                });

                ActiveComboBox.Items.Add(new ComboBoxItem
                {
                    Content = "Não",
                    Tag = !isActive ? null : false,
                    IsSelected = !isActive
                });
            }
            else
            {
                ActiveComboBox.Items.Add(new ComboBoxItem
                {
                    Content = isActive ? "Sim" : "Não",
                    Tag = null,
                    IsSelected = true
                });
            }

            // Adiciona a opção "Sem setor" se o usuário não tiver setor
            SectorComboBox.Items.Add(new ComboBoxItem
            {
                Content = "Sem setor",
                Tag = Usuario.Sector is null ? null : -1,
                IsSelected = Usuario.Sector is null
            });

            // Adiciona os setores disponíveis
            foreach (Sector setor in _admin_window.Sectors)
            {
                bool isTheUserSector = Usuario.Sector?.Id == setor.Id;
                SectorComboBox.Items.Add(new ComboBoxItem
                {
                    Content = setor.Name,
                    Tag = isTheUserSector ? null : setor.Id,
                    IsSelected = isTheUserSector
                });
            }
        }

        private async void ChangeUserButtonClick(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show(
                        $"Você tem certeza que deseja alterar o usuário {Usuario.Username} #{Usuario.Id}?",
                        "Confirmar Alteração",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Information);
            if (result == MessageBoxResult.Yes)
            {
                ComboBoxItem? selectedSector = SectorComboBox.SelectedItem as ComboBoxItem;
                ComboBoxItem? selectedStaff = StaffComboBox.SelectedItem as ComboBoxItem;
                ComboBoxItem? selectedActive = ActiveComboBox.SelectedItem as ComboBoxItem;

                int? newSectorId = selectedSector?.Tag as int?;
                bool? isStaff = selectedStaff?.Tag as bool?;
                bool? isActive = selectedActive?.Tag as bool?;

                int userId = Usuario.Id ?? 0;

                if (userId == 0)
                {
                    throw new Exception("Usuário sem ID.");
                }

                Retorno retorno = await _admin_window._user.ChangeUserAsync(userId, newSectorId, isStaff, isActive);

                if (retorno.Sucesso)
                {
                    MessageBox.Show(retorno.GetMessage(), retorno.Titulo, MessageBoxButton.OK, MessageBoxImage.Information);
                    _admin_window.Back();
                }
                else
                {
                    MessageBox.Show(retorno.Mensagem, retorno.Titulo, MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void CheckUserSectorButtonClick(object sender, RoutedEventArgs e)
        {
            if (Usuario.Sector is not null)
            {
                var result = MessageBox.Show(
                        $"Você deseja visualisar o setor {Usuario.Sector.Name} do usuário {Usuario.Username} #{Usuario.Id}?",
                        "Confirmar",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Information);
                if (result == MessageBoxResult.Yes)
                {
                    _admin_window.AdminFrame.Navigate(new AdminPageSectorChange(_admin_window, Usuario.Sector));
                }
            }
            else
            {
                MessageBox.Show("Este usuário não tem setor ou não foi carregado corretamente.", "Sem setor", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
    }
}
