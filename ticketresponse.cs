using System.Text.Json;
using System.Text.Json.Nodes;


namespace ChamadosAPI
{
    public class TicketResponse
    {
        public int Id { get; set; }
        public int TicketId { get; set; }
        public User? Responder { get; set; }
        public string Content { get; set; }
        public DateTime? CreatedAt { get; set; }

        public TicketResponse(Dictionary<string, object>? ticketResponse) 
        {
            if (ticketResponse is not null)
            {    // Inicializar propriedades obrigatórias
                Id = Convert.ToInt32(ticketResponse["id"]?.ToString());
                TicketId = Convert.ToInt32(ticketResponse["ticket"]?.ToString());
                Content = ticketResponse["content"].ToString() ?? string.Empty;
                CreatedAt = ConvertToDateTime(ticketResponse["created_at"]?.ToString());

                // Inicializar propriedades opcionais
                if (ticketResponse.TryGetValue("responder", out var responderObj) && responderObj is JsonElement responderJsonElement)
                {
                    JsonNode? node_responder = JsonNode.Parse(responderJsonElement.GetRawText());
                    Dictionary<string, object>? responderDict = node_responder?.Deserialize<Dictionary<string, object>>();
                    Responder = new(responderDict);
                }
            }
            else
            {
                throw new InvalidOperationException($"Resposta do chamado incorreto ou faltando, recebido: {ticketResponse}");
            }
        }

        private static DateTime? ConvertToDateTime(object? obj)
        {
            return obj != null && DateTime.TryParse(obj.ToString(), out var result) ? result : null;
        }
    }
}
