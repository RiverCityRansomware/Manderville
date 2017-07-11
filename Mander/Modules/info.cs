using System;
using System.Linq;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Diagnostics;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Discord.Addons.Paginator;
using System.Collections.Generic;

namespace Manderville.Modules {

    public class Mander : ModuleBase {
        [Command("Manderville Dance")]
        [Alias("Mdance")]
        [Remarks("")]
        [Summary("Do the manderville")]
        public async Task mdance() {
            await ReplyAsync(":musical_note: I'm a Mander-Mander-Manderville man :musical_note:\n" +
                ":musical_note: Doing what only a Manderville can :musical_note:\n" +
                ":musical_note: From the peaks of Coerthas to Thanalan :musical_note:\n" +
                ":musical_note: Mander-Mander-Manderville man... :musical_note:");
        }
    }

    [Group("Bot Info")]
    [Alias("b")]
    public class Info : ModuleBase {

        private readonly PaginationService paginator;

        public Info(PaginationService paginationService) {
            paginator = paginationService;
        }

        [Command("Author")]
        [Alias("a")]
        [Summary("Echos the author")]
        public async Task Author() {
            var application = await Context.Client.GetApplicationInfoAsync();
            await ReplyAsync($"- Author: {application.Owner.Username} (ID {application.Owner.Id})\n");
        }

        [Command("Library")]
        [Alias("l")]
        [Summary("Prints the version of Discord.net")]
        public async Task Library() {
            await ReplyAsync($"- Library: Discord.net {DiscordConfig.Version}\n");
        }

        [Command("Runtime")]
        [Alias("r")]
        [Summary("Prints the runtime enviroment")]
        public async Task Runtime() {
            await ReplyAsync($"- Runtime: {RuntimeInformation.FrameworkDescription} {RuntimeInformation.OSArchitecture}\n");
        }

        [Command("Uptime")]
        [Alias("u")]
        [Summary("How long the bot has been running")]
        public async Task Uptime() {
            await ReplyAsync($"- Uptime: {GetUptime()}\n");
        }

        [Command("Stats")]
        [Alias("s")]
        [Summary("Lists server stats")]
        public async Task Stats() {
            await ReplyAsync(
                $"- Heap Size: {GetHeapSize()} MB\n" +
                $"- Guilds: {(Context.Client as DiscordSocketClient).Guilds.Count}\n" +
                $"- Channels: {(Context.Client as DiscordSocketClient).Guilds.Sum(g => g.Channels.Count)}\n" +
                $"- Users: {(Context.Client as DiscordSocketClient).Guilds.Sum(g => g.Users.Count)}\n" +
                $"- Average Users Per Guild: {(Context.Client as DiscordSocketClient).Guilds.Average(g => g.Users.Count)}"
                );
        }

        [Command("list")]
        [Alias("sl")]
        [Summary("lists all servers")]
        public async Task ListGuilds() {
            var guildlist = (Context.Client as DiscordSocketClient).Guilds;
            int i = 0;
            int j = 0;

            List<string> reply = new List<string>();
            reply.Add("");
            foreach ( var guild in guildlist) {
                if (reply[j].Length >= 1800) {
                    reply.Add("");
                    j++;
                }

                reply[j] += $"{guild.Name} - {guild.Users.Count} ";
                if (i != 0 && i != 1 && i % 5 == 0) {
                    reply[j] += "\n";
                }
                i++;
            }
            foreach(var msg in reply) {
                await ReplyAsync(msg);
            }
        }

        [Command("admin")]
        public async Task adminGuilds() {
            var guildlist = (Context.Client as DiscordSocketClient).Guilds;
            var i = 0;
            var j = 0;
            foreach(var guild in guildlist) {
                foreach (var role in guild.CurrentUser.Roles) {
                    foreach (var perm in role.Permissions.ToList()) {
                        if (perm.ToString() == "Administrator" && j >= 0) {
                            i++;
                            j++;
                        }
                    }
                }
                j = 0;
            }
            await ReplyAsync($"number of servers with admin: {i}");
        }

        [Command("info")]
        [Alias("i")]
        [Summary("Lists info about specified server")]
        public async Task GuildInfo([Remainder]string input) {
            var guildlist = (Context.Client as DiscordSocketClient).Guilds;
            var guildResult = from g in guildlist
                        where g.Name.ToLower().Contains(input.ToLower())
                        select g;

            SocketGuild guild;
            try {
                guild = guildResult.First();
            } catch {
                await ReplyAsync("No guild with that name found");
                return;
            }

            var channels = "";
            foreach(var channel in guild.Channels) {
                channels += $" - {channel}: {channel.Position}\n";
            }
            var roles = "";

            foreach (var role in guild.Roles) {
                roles += $" - {role.Name}\n";
            }

            var botRoles = "";
            foreach(var botRole in guild.CurrentUser.Roles) {
                if (botRole.ToString() != "@everyone") {
                    botRoles += $"{botRole.Name}\n";
                    foreach (var rolePerm in botRole.Permissions.ToList()) {
                        botRoles += $" - {rolePerm}\n";
                    }
                }
                
            }


            var reply = $"__**Stats**__\n" +
                $"__Channels__\n" +
                $"Channel Count: {guild.Channels.Count}\n" +
                $"{channels}\n" +
                $"__Roles__\n" +
                $"{roles}\n" +
                $"__Bot Roles__\n" +
                $"{botRoles}\n";

            var embed = new EmbedBuilder()
                .WithTitle($"{guild.Name} - {guild.MemberCount}")
                .WithColor(new Color(250, 140, 73))
                .WithDescription(reply)
                .Build();

            await ReplyAsync("", embed: embed);
        }

		private static string GetUptime() => (DateTime.Now - Process.GetCurrentProcess().StartTime).ToString(@"dd\.hh\:mm\:ss");
		private static string GetHeapSize() => Math.Round(GC.GetTotalMemory(true) / (1024.0 * 1024.0), 2).ToString();
	}

}
