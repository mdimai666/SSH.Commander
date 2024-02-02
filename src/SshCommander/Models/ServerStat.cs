using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace SSH.Commander.Models
{
    public class ServerStat
    {
        public class CpuStat
        {
            [JsonPropertyName("load")]
            public float LoadPercent { get; set; }
        }

        [JsonPropertyOrder(0)]
        [JsonPropertyName("cpu")]
        public CpuStat Cpu { get; set; } = default!;

        public class DiskStat
        {
            [JsonPropertyName("used")]
            public float UsedPercent { get; set; }

            [JsonPropertyName("current")]
            public int CurrentGB { get; set; }

            [JsonPropertyName("total")]
            public int TotalGB { get; set; }
        }

        [JsonPropertyOrder(2)]
        [JsonPropertyName("disk")]
        public DiskStat Disk { get; set; } = default!;

        public class MemStat
        {
            [JsonPropertyName("load")]
            public float LoadPercent { get; set; }

            [JsonPropertyName("current")]
            public int CurrentMB { get; set; }

            [JsonPropertyName("total")]
            public int TotalMB { get; set; }
        }

        [JsonPropertyOrder(1)]
        [JsonPropertyName("mem")]
        public MemStat Mem { get; set; } = default!;
    }
}
