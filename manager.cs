using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;

namespace ChamadosAPI
{
    public class Manager(string? token = null) : BaseAPI(token)
    {
        private async Task<T> RequestAsync<T>(HttpMethod method, string endpoint, object? data = null)
        {
            return method switch
            {
                HttpMethod m when m == HttpMethod.Post => await PostAsync<T>(endpoint, data),
                HttpMethod m when m == HttpMethod.Get => await GetAsync<T>(endpoint),
                HttpMethod m when m == HttpMethod.Delete => await DeleteAsync<T>(endpoint),
                _ => throw new ArgumentException($"Método {method} não suportado.")
            };
        }

        private static void SetAuthHeader(string accessToken)
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        }

        private static List<int> FormatReceivers(object receivers)
        {
            return receivers switch
            {
                string s => s.Split(',').Select(int.Parse).ToList(),
                int i => [i],
                IEnumerable<int> list => list.ToList(),
                _ => throw new ArgumentException("Formato inválido de destinatários.")
            };
        }

        public async Task<Dictionary<string, string>> LoginAsync(string username, string password)
        {
            return await RequestAsync<Dictionary<string, string>>(HttpMethod.Post, "login/", new { username, password });
        }

        public async Task<Dictionary<string, string>> RegisterAsync(string username, string password, string email)
        {
            return await RequestAsync<Dictionary<string, string>>(HttpMethod.Post, "registrar/", new { username, password, email });
        }

        public async Task<Dictionary<string, string>> LogoutAsync(string accessToken, string refreshToken)
        {
            SetAuthHeader(accessToken);
            return await RequestAsync<Dictionary<string, string>>(HttpMethod.Post, "logout/", new { refresh = refreshToken });
        }

        public async Task<Dictionary<string, string>> RefreshTokenAsync(string refreshToken)
        {
            return await RequestAsync<Dictionary<string, string>>(HttpMethod.Post, "token/refresh/", new { refresh = refreshToken });
        }

        public async Task<List<Dictionary<string, object>>> GetUsersAsync(string accessToken)
        {
            SetAuthHeader(accessToken);
            return await RequestAsync<List<Dictionary<string, object>>>(HttpMethod.Get, "usuarios/");
        }

        public async Task<Dictionary<string, object>> GetUserByUsernameAsync(string accessToken, string username)
        {
            SetAuthHeader(accessToken);
            return await RequestAsync<Dictionary<string, object>>(HttpMethod.Get, $"usuarios/buscar/?username={username}");
        }

        public async Task<Dictionary<string, object>> GetUserByIDAsync(string accessToken, int user_id)
        {
            SetAuthHeader(accessToken);
            return await RequestAsync<Dictionary<string, object>>(HttpMethod.Get, $"usuarios/buscar/?user_id={user_id}");
        }

        public async Task<Dictionary<string, string>> ChangeUserAsync(string accessToken, int userId, int? newSectorId = null, bool? isStaff = null, bool? isActive = null)
        {
            SetAuthHeader(accessToken);
            var data = new Dictionary<string, object>();
            if (newSectorId.HasValue) data["new_sector_id"] = newSectorId;
            if (isStaff.HasValue) data["is_staff"] = isStaff;
            if (isActive.HasValue) data["new_active"] = isActive;

            return await RequestAsync<Dictionary<string, string>>(HttpMethod.Post, $"usuarios/alterar/{userId}/", data);
        }

        public async Task<Dictionary<string, object>> GetTicketAsync(string accessToken, int ticketId)
        {
            SetAuthHeader(accessToken);
            return await RequestAsync<Dictionary<string, object>>(HttpMethod.Get, $"chamados/buscar/?ticket_id={ticketId}");
        }

        public async Task<List<Dictionary<string, object>>> GetTicketsFromUserAsync(string accessToken, int userId)
        {
            SetAuthHeader(accessToken);
            return await RequestAsync<List<Dictionary<string, object>>>(HttpMethod.Get, $"chamados/buscar/?user_id={userId}");
        }

        public async Task<Dictionary<string, object>> CreateTicketAsync(string accessToken, string title, string description, object receivers)
        {
            SetAuthHeader(accessToken);
            var formattedReceivers = FormatReceivers(receivers);
            return await RequestAsync<Dictionary<string, object>>(HttpMethod.Post, "chamados/criar/", new { title, description, receivers = formattedReceivers });
        }

        public async Task<Dictionary<string, object>> CreateTicketResponseAsync(string accessToken, int ticketId, string content)
        {
            SetAuthHeader(accessToken);
            return await RequestAsync<Dictionary<string, object>>(HttpMethod.Post, $"chamados/responder/{ticketId}/", new { content });
        }

        public async Task<Dictionary<string, string>> ChangeTicketStatusAsync(string accessToken, int ticketId, string newStatus)
        {
            SetAuthHeader(accessToken);
            return await RequestAsync<Dictionary<string, string>>(HttpMethod.Post, $"chamados/status/{ticketId}/", new { new_status = newStatus });
        }

        public async Task<Dictionary<string, string>> DeleteTicketAsync(string accessToken, int ticketId)
        {
            SetAuthHeader(accessToken);
            return await RequestAsync<Dictionary<string, string>>(HttpMethod.Delete, $"chamados/remover/{ticketId}/");
        }

        public async Task<List<Dictionary<string, object>>> GetSectorsAsync(string accessToken)
        {
            SetAuthHeader(accessToken);
            return await RequestAsync<List<Dictionary<string, object>>>(HttpMethod.Get, "setores/");
        }

        public async Task<Dictionary<string, object>> GetSectorAsync(string accessToken, int sectorId)
        {
            SetAuthHeader(accessToken);
            return await RequestAsync<Dictionary<string, object>>(HttpMethod.Get, $"setores/{sectorId}/");
        }

        public async Task<Dictionary<string, object>> CreateSectorAsync(string accessToken, string name, string description, int? leaderId = null)
        {
            SetAuthHeader(accessToken);
            return await RequestAsync<Dictionary<string, object>>(HttpMethod.Post, "setores/criar/", new { sector_name = name, sector_description = description, leader_id = leaderId });
        }

        public async Task<Dictionary<string, string>> DeleteSectorAsync(string accessToken, int sectorId)
        {
            SetAuthHeader(accessToken);
            return await RequestAsync<Dictionary<string, string>>(HttpMethod.Delete, $"setores/remover/{sectorId}/");
        }

        public async Task<Dictionary<string, string>> ChangeSectorAsync(string accessToken, int sectorId, string? name = null, string? description = null, int? leaderId = null)
        {
            SetAuthHeader(accessToken);
            var data = new Dictionary<string, object>();
            if (!string.IsNullOrEmpty(name)) data["name"] = name;
            if (!string.IsNullOrEmpty(description)) data["description"] = description;
            if (leaderId.HasValue) data["leader_id"] = leaderId;

            return await RequestAsync<Dictionary<string, string>>(HttpMethod.Post, $"setores/alterar/{sectorId}/", data);
        }

        public async Task<List<Dictionary<string, object>>> GetSentTicketsAsync(string accessToken)
        {
            SetAuthHeader(accessToken);
            return await RequestAsync<List<Dictionary<string, object>>>(HttpMethod.Get, "chamados/enviados/");
        }

        public async Task<List<Dictionary<string, object>>> GetReceivedTicketsAsync(string accessToken)
        {
            SetAuthHeader(accessToken);
            return await RequestAsync<List<Dictionary<string, object>>>(HttpMethod.Get, "chamados/recebidos/");
        }

        public async Task<List<Dictionary<string, object>>> GetTicketResponsesAsync(string accessToken, int ticketId)
        {
            SetAuthHeader(accessToken);
            return await RequestAsync<List<Dictionary<string, object>>>(HttpMethod.Get, $"chamados/respostas/{ticketId}/");
        }
    }
}
