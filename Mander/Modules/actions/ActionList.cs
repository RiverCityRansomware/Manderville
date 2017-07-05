using Newtonsoft.Json;

namespace Mander.Modules {
    internal class ActionList
    {
        [JsonProperty("ActionBasic")]
        public ActionBasic Actionbasic { get; set; }
    }

}