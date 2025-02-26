using System;
using System.Collections.Generic;

namespace ChamadosAPI
{
    public class Sector
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public int? Leader_ID { get; set; }

        public Sector(Dictionary<string, object>? sector)
        {
            ArgumentNullException.ThrowIfNull(sector);
            Id = Convert.ToInt32(sector["id"]?.ToString());
            Name = sector["sector_name"]?.ToString() ?? string.Empty;
            Description = sector["sector_description"]?.ToString() ?? string.Empty;

            if (sector.TryGetValue("sector_leader", out var leaderValue) && int.TryParse(leaderValue?.ToString(), out var leaderId))
            {
                Leader_ID = leaderId;
            }
            else
            {
                Leader_ID = null;
            }
        }
    }
}