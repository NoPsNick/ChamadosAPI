using System.Text.Json;
using System.Text.Json.Nodes;


namespace ChamadosAPI
{
    public class Ticket
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public User? Sender { get; set; }
        public List<User> Receivers { get; set; } = [];
        public string Status { get; set; }
        public DateTime? CreatedAt { get; set; }
        public User? LastTimeChangedBy { get; set; }

        public Ticket(Dictionary<string, object>? ticket)
        {
            if (ticket is not null)
            {    // Inicializar propriedades obrigatórias
                Id = Convert.ToInt32(ticket["id"]?.ToString());
                Title = ticket["title"].ToString() ?? string.Empty;
                Description = ticket["description"].ToString() ?? string.Empty;
                Status = ticket["status"].ToString() ?? string.Empty;
                CreatedAt = ConvertToDateTime(ticket["created_at"]);

                // Validar e inicializar propriedades opcionais

                if (ticket.TryGetValue("sender", out var senderObj) && senderObj is JsonElement senderJsonElement)
                {
                    JsonNode? node_sender = JsonNode.Parse(senderJsonElement.GetRawText());
                    Dictionary<string, object>? sender_dict = node_sender?.Deserialize<Dictionary<string, object>>();
                    Sender = new(sender_dict);
                }

                if (ticket.TryGetValue("receivers_display", out var receiversObj) && receiversObj is JsonElement receiversJsonElement)
                {
                    JsonNode? node_receivers = JsonNode.Parse(receiversJsonElement.GetRawText());
                    List<Dictionary<string, object>>? receiversList = node_receivers?.Deserialize<List<Dictionary<string, object>>>();

                    // Se "Receivers" for uma propriedade da classe, atribuímos diretamente
                    Receivers = receiversList?
                        .Select(userDict => new User(userDict)) // Converte cada dicionário em um objeto User
                        .ToList() ?? []; // Garante que Receivers nunca seja null
                }
                else
                {
                    Receivers = []; // Se receivers_display não existir, inicializa uma lista vazia
                }


                if (ticket.TryGetValue("last_status_changed_by", out var lastChangedByObj) && lastChangedByObj is JsonElement lastChangedByJsonElement)
                {
                    JsonNode? node_lastchangedby = JsonNode.Parse(lastChangedByJsonElement.GetRawText());
                    Dictionary<string, object>? lastChangedByDict = node_lastchangedby?.Deserialize<Dictionary<string, object>>();

                    LastTimeChangedBy = new User(lastChangedByDict);
                }
                else
                {
                    LastTimeChangedBy = null;
                }
            }
            else
            {
                throw new InvalidOperationException($"Chamado incorreto ou faltando, recebido: {ticket}");
            }
        }

        private static DateTime? ConvertToDateTime(object? obj)
        {
            return obj != null && DateTime.TryParse(obj.ToString(), out var result) ? result : null;
        }
    }
}