using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using System;
using System.IO;
using Newtonsoft.Json;
using System.Reflection;

namespace Manderville.Modules {
	public class HelpModule : ModuleBase<SocketCommandContext> {
		private CommandService _service;

        // Create a constructor for the CommandService dependency
        public HelpModule(CommandService service) {
			_service = service;
		}

        [Command("help")]
        [Alias("h")]
        [Summary("Lists all commands")]
        public async Task HelpAsync() {
            var application = await Context.Client.GetApplicationInfoAsync();
            string reply = $"Available Commands\n" +
                $"**Basic Commands**\n" +
                $"- **H**elp - Displays available commands and syntax.\n" +
                $"- **H**elp `Command` - Displays information about the specified command.\n" +
                $"- **M**dance\n\n" +
                $"__**A**__ctions:\n" +
                $"- **I**nfo `action`: Lists info about the specified action, exact spelling, case insensitive.\n" +
                $"- **S**earch `action`: Searches for all action names that match the input.\n\n" +
                $"__**I**tems__\n" +
                $"- **I**nfo `item name`: Lists info about the specified item.\n" +
                $"- **S**earch `item name` or `ilvl item name`: Searches either by item name or by ilv & item name\n\n" +
                $"__**R**__ecipes:\n" +
                $"- **I**nfo `recipe`: Lists truncated info about the recipe.\n" +
                $"- **I**nfo**A**ll `recipe`: Lists all info about the recipe\n" +
                $"- **S**earch `Job Level Recipe`: Searches for the specified recipe. Ex:\n" +
                $"      search potion\n" +
                $"      search alc potion\n" +
                $"      search alc 50 potion\n" +
                $"- **S**hopping **C**art `recipe, # recipe, recipe`: Input a comma seperated list of recipes to receive a list of all required base materials. Numbers(#) Optional. Ex:\n" +
                $"      `shoppingcart Bronze Ingot, 50 Iron Ingot, 999 Mhachi Coffin`\nLists all required base materials to craft 1 bronze ingot, 50 iron ingots and 999 mhachi coffins\n\n" +
                $"__**S**__tormblood:\n" +
                $"- **P**review `command`: Embeds a video pertaining to the input. Ex:\n" +
                $"      preview `class`: Links to a preview video of the specified class\n" +
                $"      preview class guage: Links to video showing every class gauge\n" +
                $"      preview mount speed: Links to a video showing the enhanced mount speed\n\n" +
                $"**Required Role Permissions**\n" +
                $"  Read Messages\n" +
                $"  Send Messages\n" +
                $"  Embed Links\n" +
                $"  Attach Files\n" +
                $"  Read Message History\n" +
                $"  Add reactions\n\n" +
                $"**Contact**\n" +
                $"Please send all feature suggestions and bot problems to: " +
                $"{application.Owner.Mention} " +
                $"or join the discord at: https://discord.gg/apjcVAw";

            DateTime buildDate = new DateTime(2000, 1, 1).AddDays(Assembly.GetExecutingAssembly().GetName().Version.Build).AddSeconds(Assembly.GetExecutingAssembly().GetName().Version.MinorRevision * 2);

            var embed = new EmbedBuilder()
                .WithColor(new Color(250, 140, 73))
                .WithTitle("Help")
                .WithFooter(new EmbedFooterBuilder().WithText($"Manderville v0.1.1 - {buildDate}"))
                .WithDescription(reply)
                .WithUrl($"https://gist.github.com/Infinifrui/b0ef72b070cda47c22352b6e0f0a8b81")
                .Build();

            await ReplyAsync("", embed: embed);
        }

        [Command("help")]
		[Alias("h")]
		[Summary("Lists help for specified command")]
		public async Task HelpAsync([Remainder]string input) {
            var result = from Module in _service.Modules
                         from command in Module.Commands
                         where command.Name.ToLower().Contains(input.ToLower()) && Module.Name != "RecipeMisc"
                         select command;

            var builder = new EmbedBuilder() {
                Color = new Color(250, 140, 73),
                Description = $"Here are some commands like **{input}**"
            };

            foreach (var cmd in result) {
                Console.WriteLine($"Input: {input}\nCommand: {cmd.Name}\nmatch: {input.ToLower() == cmd.Name.ToLower()}");
                if (cmd.Remarks != null) {
                    builder.AddField(x => {
                        
                        x.Name = cmd.Module.Name + ": " + string.Join(", ", cmd.Name);
                        x.Value = $"Aliases: `{string.Join(", ", cmd.Aliases)}`\nSummary: {cmd.Summary}\n";
                    });
                }
                
                Console.WriteLine("Builder: Field Added");
            }

            builder.Build();

            if (result == null) {
				await ReplyAsync($"Sorry, I couldn't find a command like **{input}**");
				return;
			}

			await ReplyAsync("", embed: builder);
		}














    }
}
