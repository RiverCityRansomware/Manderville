using System;
using System.Linq;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Diagnostics;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Discord.Addons.Paginator;
using Newtonsoft.Json;
using System.IO;

namespace Manderville.Modules {

    [Group("Config")]
    [Alias("c")]
    [RequireUserPermission(GuildPermission.Administrator)]
    public class Config : ModuleBase {

        public string path = Path.Combine(AppContext.BaseDirectory, "guildConfig/");

        [Command("Create")]
        [Alias("cr")]
        [RequireOwner]
        public async Task CreateServerSettings() {
            EnsureFolderExists();
            var guildList = (Context.Client as DiscordSocketClient).Guilds;

            foreach (var guild in guildList) {
                var settings = new GuildSettings();
                settings.serverName = guild.Name;
                if (!File.Exists(path +$"{guild.Name}.json")) {
                    Console.WriteLine($"Creating {guild.Name}.json");
                    File.WriteAllText(path+$"{guild.Name}.json", JsonConvert.SerializeObject(settings));
                }
            }

            await ReplyAsync("Done\n");
        }

        [Command("Config")]
        [Alias("c")]
        public async Task ChangeConfig(string setting, [Remainder] string input) {
            var guildSettings = JsonConvert.DeserializeObject<GuildSettings>(File.ReadAllText(path+$"{Context.Guild.Name}.json"));
            if (setting.ToLower() == "p" || setting.ToLower() == "prefix") {
                guildSettings.prefix = input;
                File.WriteAllText(path + $"{Context.Guild.Name}.json", JsonConvert.SerializeObject(guildSettings));
                await ReplyAsync($"{guildSettings.serverName} prefix changed to {guildSettings.prefix}");
                return;
            } else if (setting.ToLower() == "ar" || setting.ToLower() == "addrole" || setting.ToLower() == "addroles") {
                if (!(guildSettings.allowedRoles.Contains(input))) {
                    guildSettings.allowedRoles.Add(input);
                    File.WriteAllText(path + $"{Context.Guild.Name}.json", JsonConvert.SerializeObject(guildSettings));
                    await ReplyAsync($"{guildSettings.serverName} has added {input} to allowed roles");
                    return;
                }
            } else if (setting.ToLower() == "rr" || setting.ToLower() == "removerole" || setting.ToLower() == "removeroles") {
                if (!(guildSettings.allowedRoles.Contains(input))) {
                    guildSettings.allowedRoles.Remove(input);
                    File.WriteAllText(path + $"{Context.Guild.Name}.json", JsonConvert.SerializeObject(guildSettings));
                    await ReplyAsync($"{guildSettings.serverName} has removed {input} from allowed roles");
                    return;
                }
            }
            await ReplyAsync($"No Settings saved");
        }

        [Command("info")]
        [Alias("i")]
        public async Task ConfigInfo() {
            var guildSettings = JsonConvert.DeserializeObject<GuildSettings>(File.ReadAllText(path + $"{Context.Guild.Name}.json"));
            var desc = $"prefix: {guildSettings.prefix}\n" +
                $"__**Allowed Roles**__\n";

            foreach (var role in guildSettings.allowedRoles) {
                desc += $" - {role}\n";
            }


            var embed = new EmbedBuilder()
                .WithTitle(guildSettings.serverName)
                .WithDescription(desc)
                .WithColor(new Color(250, 140, 73))
                .Build();

            await ReplyAsync("", embed: embed);

        }

        public void EnsureFolderExists() {
            
            if (!Directory.Exists(path)) {
                Console.Write($"Creating folder {path}");
                Directory.CreateDirectory(path);
                Console.Write(" ...Done\n");
            }
        }
    }

    

}
