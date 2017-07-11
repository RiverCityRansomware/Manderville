using Discord;
using Discord.WebSocket;
using Discord.Commands;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

namespace Manderville.Modules {
    public class GuildSettings : ModuleBase{
        public string serverName = "";
        public string prefix = "";
        public List<string> allowedRoles = new List<string>();

        public ICommandContext _context;

        public GuildSettings() {
            prefix = "!m";
            allowedRoles.Add("@everyone");
            allowedRoles.Add("Admin");
            _context = Context;
        }

        public GuildSettings Load(string name) {
            string path = Path.Combine(AppContext.BaseDirectory, $"guildConfig/{name}.json");
            return JsonConvert.DeserializeObject<GuildSettings>(File.ReadAllText(path));
        }
    }

}
