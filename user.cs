using System.Text.Json;
using System.Text.Json.Nodes;


namespace ChamadosAPI
{
    public class User
    {
        public string? Username { get; set; }
        public int? Id { get; set; }
        public DateTime? DateJoined { get; set; }
        public bool? IsStaff { get; set; }
        public bool? IsActive { get; set; }
        public bool? IsSuperAdmin { get; set; }
        public Sector? Sector { get; set; }
        public string? AccessToken { get; private set; }
        public string? RefreshToken { get; private set; }
        private readonly Manager? _manager;

        public User(Dictionary<string, object>? user)
        {
            if (user != null)
            {
                Username = user.TryGetValue("username", out var usernameObj) ? usernameObj?.ToString() : null;
                Id = user.TryGetValue("id", out var idObj) ? ConvertToInt(idObj?.ToString()) : null;
                DateJoined = user.TryGetValue("date_joined", out var dateJoinedObj) ? ConvertToDateTime(dateJoinedObj?.ToString()) : null;
                IsStaff = user.TryGetValue("is_staff", out var isStaffObj) && ConvertToBool(isStaffObj?.ToString());
                IsActive = user.TryGetValue("is_active", out var isActiveObj) && ConvertToBool(isActiveObj?.ToString());
                IsSuperAdmin = user.TryGetValue("is_superuser", out var isSuperAdminObj) && ConvertToBool(isSuperAdminObj?.ToString());
                if (user.TryGetValue("sector", out var sectorObj) && sectorObj is JsonElement sectorJsonElement)
                {
                    JsonNode? node_sector = JsonNode.Parse(sectorJsonElement.GetRawText());
                    Dictionary<string, object>? sector_dict = node_sector?.Deserialize<Dictionary<string, object>>();
                    Sector = new(sector_dict);
                }
                else
                {
                    Sector = null;
                }
            }
            else
            {
                // Somente o usuário logado possui _manager
                _manager = new Manager();
            }
        }

        // Métodos auxiliares para conversão e validação
        private static int? ConvertToInt(object? obj)
        {
            return obj != null && int.TryParse(obj.ToString(), out var result) ? result : null;
        }

        private static DateTime? ConvertToDateTime(object? obj)
        {
            return obj != null && DateTime.TryParse(obj.ToString(), out var result) ? result : null;
        }

        private static bool ConvertToBool(object? obj)
        {
            return obj != null && bool.TryParse(obj.ToString(), out var result) && result;
        }

        private void StoreTokens(Dictionary<string, string> response)
        {
            // Tenta obter os valores das chaves
            if (response.TryGetValue("access", out var accessToken) &&
                response.TryGetValue("refresh", out var refreshToken))
            {
                AccessToken = accessToken;
                RefreshToken = refreshToken;
            }
            else
            {
                // Lança exceção se não encontrar as chaves necessárias
                throw new InvalidOperationException("Login ou Senha inválidos.");
            }

            // Verifica se os tokens obtidos são válidos
            if (string.IsNullOrEmpty(AccessToken) || string.IsNullOrEmpty(RefreshToken))
            {
                throw new InvalidOperationException("Tokens inválidos ou ausentes.");
            }
        }

        private async Task<object> GetUserAsync(string username)
        {
            VerifyLogin();
            var user = await _manager.GetUserByUsernameAsync(AccessToken, username);

            return user;
        }

        private async Task StoreUserInfosAsync(string username)
        {
            VerifyLogin();
            Dictionary<string, object> user = (Dictionary<string, object>)await GetUserAsync(username);

            Username = user["username"]?.ToString();
            Id = Convert.ToInt32(user["id"]?.ToString());
            DateJoined = Convert.ToDateTime(user["date_joined"]?.ToString());
            IsStaff = Convert.ToBoolean(user["is_staff"]?.ToString());
            IsActive = Convert.ToBoolean(user["is_active"]?.ToString());
            IsSuperAdmin = Convert.ToBoolean(user["is_superuser"]?.ToString());
        }

        private void ClearTokens()
        {
            AccessToken = null;
            RefreshToken = null;
            Username = null;
            Id = null;
            DateJoined = null;
            IsStaff = null;
            IsSuperAdmin = null;
        }

        private void VerifyLogin(bool logged = true, bool login = false)
        {
            ArgumentNullException.ThrowIfNull(_manager);
            bool isLogged = !string.IsNullOrEmpty(AccessToken) && !string.IsNullOrEmpty(RefreshToken);

            if (logged && !isLogged && !login)
            {
                throw new Exception("Usuário deslogado.");
            }
            else if (!logged && !isLogged && !login)
            {
                throw new Exception("Usuário já está deslogado.");
            }
            else if (isLogged && login)
            {
                throw new Exception("Usuário já está logado.");
            }
            if (_manager is null)
            {
                throw new Exception("Usuário sem manager, erro interno.");
            }
        }

        public async Task<Retorno> GetUsersAsync()
        {
            try
            {
                VerifyLogin();
                List<Dictionary<string, object>> usersDict = await _manager.GetUsersAsync(AccessToken);

                if (usersDict == null || usersDict.Count == 0)
                {
                    return new Retorno
                    {
                        Sucesso = false,
                        Mensagem = "Não foi possível encontrar os usuários, ou eles não existem",
                        Titulo = "Sem usuários"
                    };
                }

                List<User> users = usersDict
                        .OfType<Dictionary<string, object>>()
                        .Select(userDict => new User(userDict))
                        .ToList();

                return new Retorno
                {
                    Sucesso = true,
                    ObjetoRetornado = users,
                    Titulo = "Lista dos usuários"
                };
            }
            catch (Exception ex)
            {
                return new Retorno
                {
                    Sucesso = false,
                    Titulo = "Erro ao buscar usuários",
                    Mensagem = ex.Message
                };
            }
        }

        public async Task<Retorno> LoginAsync(string username, string password)
        {
            try
            {
                VerifyLogin(login: true);
                Dictionary<string, string> response = await _manager.LoginAsync(username, password);
                StoreTokens(response);
                await StoreUserInfosAsync(username);

                return new Retorno
                {
                    Sucesso = true,
                    Titulo = "Login Bem-sucedido",
                    Mensagem = $"Usuário {Username} #{Id} logado com sucesso!",
                    ObjetoRetornado = this
                };
            }
            catch (Exception ex)
            {
                return new Retorno
                {
                    Sucesso = false,
                    Titulo = "Erro no Login",
                    Mensagem = ex.Message
                };
            }
        }

        public async Task<Retorno> LogoutAsync()
        {
            try
            {
                VerifyLogin(logged: false);
                await _manager.LogoutAsync(AccessToken, RefreshToken);
                ClearTokens();

                return new Retorno
                {
                    Sucesso = true,
                    Titulo = "Logout Bem-sucedido",
                    Mensagem = "Deslogado com sucesso!"
                };
            }
            catch (Exception ex)
            {
                return new Retorno
                {
                    Sucesso = false,
                    Titulo = "Erro no Logout",
                    Mensagem = ex.Message
                };
            }
        }

        public async Task<Retorno> RegisterAsync(string username, string password, string email)
        {
            try
            {
                Dictionary<string, string> response = await _manager.RegisterAsync(username, password, email);

                return new Retorno
                {
                    Sucesso = true,
                    Titulo = "Registro Bem-sucedido",
                    Mensagem = "Usuário registrado com sucesso!",
                    ObjetoRetornado = response
                };
            }
            catch (Exception ex)
            {
                return new Retorno
                {
                    Sucesso = false,
                    Titulo = "Erro no Registro",
                    Mensagem = ex.Message
                };
            }
        }

        public async Task<Retorno> RefreshAccessTokenAsync()
        {
            try
            {
                Dictionary<string, string> response = await _manager.RefreshTokenAsync(RefreshToken);
                AccessToken = response["access"].ToString();

                return new Retorno
                {
                    Sucesso = true,
                    Titulo = "Token Atualizado",
                    Mensagem = "Access token renovado com sucesso!"
                };
            }
            catch (Exception ex)
            {
                return new Retorno
                {
                    Sucesso = false,
                    Titulo = "Erro na Renovação do Token",
                    Mensagem = ex.Message
                };
            }
        }

        public async Task<Retorno> ChangeUserAsync(int userId, int? newSectorId = null, bool? isStaff = null, bool? isActive = null)
        {
            try
            {
                VerifyLogin();

                bool hasAccess = this.IsSuperAdmin ?? false;
                if (!hasAccess && (isStaff is not null | isActive is not null))
                {
                    return new Retorno
                    {
                        Sucesso = false,
                        Titulo = "Sem permissão",
                        Mensagem = "Você não possuí permissão para esta ação.",
                    };
                }

                Dictionary<string, string> message = await _manager.ChangeUserAsync(AccessToken, userId, newSectorId, isStaff, isActive);

                return new Retorno
                {
                    Sucesso = true,
                    ObjetoRetornado = message,
                    Titulo = "Usuário alterado"
                };
            }
            catch (Exception ex)
            {
                return new Retorno
                {
                    Sucesso = false,
                    Titulo = "Erro ao alterar usuário",
                    Mensagem = ex.Message
                };
            }
        }

        public async Task<Retorno> GetTicketAsync(int ticketId) 
        {
            try
            {
                VerifyLogin();
                // Apenas staffs e super admins podem buscar um ticket em específico.
                if (this.IsStaff ?? false | this.IsSuperAdmin ?? false)
                {
                    Dictionary<string, object> ticket_dict = await _manager.GetTicketAsync(AccessToken, ticketId);

                    Ticket ticket = new(ticket_dict);

                    return new Retorno
                    {
                        Sucesso = true,
                        ObjetoRetornado = ticket,
                        Titulo = "Chamado"
                    };
                }
                else
                {
                    return new Retorno
                    {
                        Sucesso = false,
                        Titulo = "Sem permissão",
                        Mensagem = "Você não possuí permissão para esta ação."
                    };
                }
            }
            catch (Exception ex)
            {
                return new Retorno
                {
                    Sucesso = false,
                    Titulo = "Erro ao buscar chamado",
                    Mensagem = ex.Message
                };
            }
        }

        public async Task<Retorno> GetTicketsFromUserAsync(int userId)
        {
            try
            {
                VerifyLogin();
                // Apenas staffs e super admins podem buscar os tickets de outro usuário.
                if (this.IsStaff ?? false | this.IsSuperAdmin ?? false)
                {
                    List<Dictionary<string, object>> tickets_dict = await _manager.GetTicketsFromUserAsync(AccessToken, userId);
                    if (tickets_dict == null || tickets_dict.Count == 0)
                    {
                        return new Retorno
                        {
                            Sucesso = false,
                            Mensagem = "Não foi possível encontrar os chamados, ou eles não existem",
                            Titulo = "Sem chamados"
                        };
                    }

                    List<Ticket> tickets = tickets_dict
                        .OfType<Dictionary<string, object>>()
                        .Select(ticketDict => new Ticket(ticketDict))
                        .ToList();

                    return new Retorno
                    {
                        Sucesso = true,
                        ObjetoRetornado = tickets,
                        Titulo = "Chamados"
                    };
                }
                else
                {
                    return new Retorno
                    {
                        Sucesso = false,
                        Titulo = "Sem permissão",
                        Mensagem = "Você não possuí permissão para esta ação."
                    };
                }
            }
            catch (Exception ex)
            {
                return new Retorno
                {
                    Sucesso = false,
                    Titulo = "Erro ao buscar chamado",
                    Mensagem = ex.Message
                };
            }
        }

        public async Task<Retorno> GetSentTicketsAsync()
        {
            try
            {
                VerifyLogin();
                List<Dictionary<string, object>> tickets_dict = await _manager.GetSentTicketsAsync(AccessToken);

                if (tickets_dict == null || tickets_dict.Count == 0)
                {
                    return new Retorno
                    {
                        Sucesso = false,
                        Mensagem = "Não foi possível encontrar os chamados, ou eles não existem",
                        Titulo = "Sem chamados"
                    };
                }

                List<Ticket> tickets = tickets_dict
                        .OfType<Dictionary<string, object>>()
                        .Select(ticketDict => new Ticket(ticketDict))
                        .ToList();

                return new Retorno { 
                    Sucesso = true,
                    ObjetoRetornado = tickets,
                    Titulo = "Chamados enviados"
                };
            }
            catch (Exception ex)
            {
                return new Retorno
                {
                    Sucesso = false,
                    Titulo = "Erro ao buscar chamados enviados",
                    Mensagem = ex.Message
                };
            }
        }
        public async Task<Retorno> GetReceivedTicketsAsync()
        {
            try
            {
                VerifyLogin();
                List<Dictionary<string, object>> tickets_dict = await _manager.GetReceivedTicketsAsync(AccessToken);

                if (tickets_dict == null || tickets_dict.Count == 0)
                {
                    return new Retorno
                    {
                        Sucesso = false,
                        Mensagem = "Não foi possível encontrar os chamados, ou eles não existem",
                        Titulo = "Sem chamados"
                    };
                }

                List<Ticket> tickets = tickets_dict
                        .OfType<Dictionary<string, object>>()
                        .Select(ticketDict => new Ticket(ticketDict))
                        .ToList();

                return new Retorno
                {
                    Sucesso = true,
                    ObjetoRetornado = tickets,
                    Titulo = "Chamados recebidos"
                };
            }
            catch (Exception ex)
            {
                return new Retorno
                {
                    Sucesso = false,
                    Titulo = "Erro ao buscar chamados recebidos",
                    Mensagem = ex.Message
                };
            }
        }

        public async Task<Retorno> GetTicketResponsesAsync(int ticketId)
        {
            try
            {
                VerifyLogin();
                List<Dictionary<string, object>> responses_dict = await _manager.GetTicketResponsesAsync(AccessToken, ticketId);

                if (responses_dict == null || responses_dict.Count == 0)
                {
                    return new Retorno
                    {
                        Sucesso = false,
                        Mensagem = "Não foi possível encontrar as respostas para o chamado, ou ele não tem",
                        Titulo = "Sem respostas"
                    };
                }

                List<TicketResponse> responses = responses_dict
                        .OfType<Dictionary<string, object>>()
                        .Select(responseDict => new TicketResponse(responseDict))
                        .ToList();

                return new Retorno {
                    Sucesso = true,
                    ObjetoRetornado = responses,
                    Titulo = "Respostas do chamado"
                };
            }
            catch (Exception ex)
            {
                return new Retorno
                {
                    Sucesso = false,
                    Titulo = "Erro ao buscar as respostas do chamado",
                    Mensagem = ex.Message
                };
            }
        }

        public async Task<Retorno> CreateTicketAsync(string title, string description, object receivers)
        {
            try
            {
                VerifyLogin();
                Dictionary<string, object> ticket_dict = (Dictionary<string, object>)await _manager.CreateTicketAsync(AccessToken, title, description, receivers);

                Ticket ticket = new(ticket_dict);

                return new Retorno {
                    Sucesso = true,
                    ObjetoRetornado = ticket,
                    Titulo = "Chamado enviado"
                };
            }
            catch (Exception ex)
            {
                return new Retorno
                {
                    Sucesso = false,
                    Titulo = "Erro ao enviar chamado",
                    Mensagem = ex.Message
                };
            }
        }

        public async Task<Retorno> CreateTicketResponseAsync(int ticketId, string content)
        {
            try
            {
                VerifyLogin();
                Dictionary<string, object> response_dict = (Dictionary<string, object>)await _manager.CreateTicketResponseAsync(AccessToken, ticketId, content);

                TicketResponse response = new(response_dict);

                return new Retorno {
                    Sucesso = true,
                    ObjetoRetornado = response,
                    Titulo = "Resposta do chamado criada"
                };
            }
            catch (Exception ex)
            {
                return new Retorno
                {
                    Sucesso = false,
                    Titulo = "Erro ao responder o chamado",
                    Mensagem = ex.Message
                };
            }
        }

        public async Task<dynamic> ChangeTicketStatusAsync(int ticketId, string newStatus)
        {
            try
            {
                VerifyLogin();
                Dictionary<string, string> message = await _manager.ChangeTicketStatusAsync(AccessToken, ticketId, newStatus);

                return new Retorno {
                    Sucesso = true,
                    ObjetoRetornado = message,
                    Titulo = "Status alterado"
                };
            }
            catch (Exception ex)
            {
                return new Retorno
                {
                    Sucesso = false,
                    Titulo = "Erro ao alterar o status",
                    Mensagem = ex.Message
                };
            }
        }

        public async Task<dynamic> DeleteTicketAsync(int ticketId)
        {
            try
            {
                VerifyLogin();
                Dictionary<string, string> message = await _manager.DeleteTicketAsync(AccessToken, ticketId);

                return new Retorno {
                    Sucesso = true,
                    ObjetoRetornado = message,
                    Titulo = "Chamado removido"
                };
            }
            catch (Exception ex)
            {
                return new Retorno
                {
                    Sucesso = false,
                    Titulo = "Erro ao remover chamado",
                    Mensagem = ex.Message
                };
            }
        }

        public async Task<Retorno> GetSectorsAsync()
        {
            try
            {
                VerifyLogin();
                List<Dictionary<string, object>> sectors = (List<Dictionary<string, object>>)await _manager.GetSectorsAsync(AccessToken);

                // Mapeando os setores para instâncias da classe Sector
                List<Sector> sectorList = sectors
                    .OfType<Dictionary<string, object>>() // Garante que a lista contém dicionários
                    .Select(sectorDict => new Sector(sectorDict)) // Converte cada dicionário em um objeto Sector
                    .ToList();

                return new Retorno
                {
                    Sucesso = true,
                    ObjetoRetornado = sectorList,
                    Titulo = "Setores"
                };
            }
            catch (Exception ex)
            {
                return new Retorno
                {
                    Sucesso = false,
                    Titulo = "Erro ao buscar os setores",
                    Mensagem = ex.Message
                };
            }
        }

        public async Task<Retorno> GetSectorAsync(int sector_id)
        {
            try
            {
                VerifyLogin();
                Dictionary<string, object> sector_dict = (Dictionary<string, object>)await _manager.GetSectorAsync(AccessToken, sector_id);

                // Mapeando os setores para instâncias da classe Sector
                Sector sector = new(sector_dict);

                return new Retorno
                {
                    Sucesso = true,
                    ObjetoRetornado = sector,
                    Titulo = "Setor"
                };
            }
            catch (Exception ex)
            {
                return new Retorno
                {
                    Sucesso = false,
                    Titulo = "Erro ao buscar o setor",
                    Mensagem = ex.Message
                };
            }
        }

        public async Task<Retorno> CreateSectorAsync(string name, string description, int? leaderId = null)
        {
            try 
            {
                VerifyLogin();
                Dictionary<string, object> sector_dict = (Dictionary<string, object>)await _manager.CreateSectorAsync(AccessToken, name, description, leaderId);

                Sector sector = new (sector_dict);

                return new Retorno { 
                    Sucesso = true,
                    ObjetoRetornado = sector,
                    Titulo = "Setor criado com sucesso"
                };
            }
            catch (Exception ex)
            {
                return new Retorno
                {
                    Sucesso = false,
                    Titulo = "Erro ao criar setor",
                    Mensagem = ex.Message
                };
            }
        }

        public async Task<Retorno> ChangeSectorAsync(int sectorId, string name = null, string description = null, int? leaderId = null)
        {
            try
            {
                VerifyLogin();
                Dictionary<string, string> message = await _manager.ChangeSectorAsync(AccessToken, sectorId, name, description, leaderId);

                return new Retorno
                {
                    Sucesso = true,
                    ObjetoRetornado = message,
                    Titulo = "Setor alterado"
                };
            }
            catch (Exception ex)
            {
                return new Retorno
                {
                    Sucesso = false,
                    Titulo = "Erro ao alterar setor",
                    Mensagem = ex.Message
                };
            }
        }

        public async Task<Retorno> DeleteSectorAsync(int sectorId)
        {
            try
            {
                VerifyLogin();
                // Apenas super admins podem deletar setores.
                if (this.IsSuperAdmin ?? false)
                {
                    Dictionary<string, string> message = await _manager.DeleteSectorAsync(AccessToken, sectorId);

                    return new Retorno
                    {
                        Sucesso = true,
                        ObjetoRetornado = message,
                        Titulo = "Setor deletado"
                    };
                }
                else
                {
                    return new Retorno
                    {
                        Sucesso = false,
                        Titulo = "Sem permissão",
                        Mensagem = "Você não possuí permissão para esta ação."
                    };
                }
            }
            catch (Exception ex) 
            {
                return new Retorno
                {
                    Sucesso = false,
                    Titulo = "Erro ao deletar setor",
                    Mensagem = ex.Message
                };
            }
        }

    }
}
