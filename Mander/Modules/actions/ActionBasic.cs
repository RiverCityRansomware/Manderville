using System;
using Newtonsoft.Json;

namespace Mander.Modules {
    internal class ActionBasic
    {
        [JsonProperty("id")]
        public UInt32 Id { get; set; }
        [JsonProperty("name")]
        public string name { get; set; }
        [JsonProperty("name_ja")]
        public string name_ja { get; set; }
        [JsonProperty("name_en")]
        public string name_en { get; set; }
        [JsonProperty("name_fr")]
        public string name_fr { get; set; }
        [JsonProperty("name_de")]
        public string name_de { get; set; }
        [JsonProperty("name_cns")]
        public string name_cns { get; set; }
        [JsonProperty("lodestone_id")]
        public string lodestone_id { get; set; }
        [JsonProperty("lodestone_type")]
        public string lodestone_type { get; set; }
    }

}