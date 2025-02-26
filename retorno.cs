using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChamadosAPI
{
    public class Retorno
    {
        public string? Titulo { get; set; }
        public string? Mensagem { get; set; }
        public object? ObjetoRetornado { get; set; }
        public required bool Sucesso { get; set; }

        public string GetMessage()
        {
            if (!Sucesso)
            {
                return Mensagem ?? "Erro desconhecido.";
            }

            return ObjetoRetornado switch
            {
                Dictionary<string, string> messageDict when messageDict.Count > 0
                    => string.Join("; ", messageDict.Select(kv => $"{kv.Key}: {kv.Value}")),

                Dictionary<string, object> objectDict when objectDict.Count > 0
                    => string.Join("; ", objectDict.Select(kv => $"{kv.Key}: {kv.Value?.ToString()}")),

                IEnumerable<object> list when list.Any()
                    => string.Join("; ", list.Select(item => item?.ToString())),

                _ => "Operação realizada com sucesso."
            };
        }

    }
}