using System;
using System.Linq;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Diagnostics;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Discord.Addons.Paginator;

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


        [Command("hi")]
        public async Task hi() {
            await ReplyAsync("hi");
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

        [Command("servers")]
        [Summary("lists all servers")]
        public async Task ListGuilds() {
            var guildlist = (Context.Client as DiscordSocketClient).Guilds;
            await ReplyAsync("Not implemented");


        }

        [Command("list")]
        [Alias("l")]
        [Summary("Lists info about specified server")]
        public async Task GuildInfo() {
            await ReplyAsync("Not implemented");
        }

		private static string GetUptime() => (DateTime.Now - Process.GetCurrentProcess().StartTime).ToString(@"dd\.hh\:mm\:ss");
		private static string GetHeapSize() => Math.Round(GC.GetTotalMemory(true) / (1024.0 * 1024.0), 2).ToString();
	}

}
