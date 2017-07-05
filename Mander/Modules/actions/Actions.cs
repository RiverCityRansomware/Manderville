using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.Addons.Paginator;
using System.Net;
using Newtonsoft.Json;
using Manderville.Common;
using SaintCoinach;
using Manderville.Modules;

namespace Mander.Modules {
    [Group("Actions")]
    [Alias("a")]
    public class Actions : ModuleBase
    {
        private readonly PaginationService paginator;
        private readonly ARealmReversed _realm;
        const string GameDirectory = @"C:\FFXIV\SquareEnix\FINAL FANTASY XIV - A Realm Reborn";

        public Actions(PaginationService paginationservice) {
            paginator = paginationservice;

            _realm = new ARealmReversed(GameDirectory, SaintCoinach.Ex.Language.English);

            if (!_realm.IsCurrentVersion) {
                const bool IncludeDataChanges = true;
                var updateReport = _realm.Update(IncludeDataChanges);
            }
        }

      

        //Lists specified action info
        // TODO: use saint coinach instead;
        /*
        [Command("Info")]
        [Alias("i")]
        [Remarks("action name")]
        [Summary("Lists info about specified action")]
        public async Task AbilityList([Remainder] string input ){
            var actionList = JsonConvert.DeserializeObject<List<ActionBasic>>(File.ReadAllText(actionListSavePath));
            var actionResult = actionList.Find(x => x.name.ToLower().Equals(input.ToLower()));

            Console.WriteLine($"actionResult: {actionResult.name}");

            var settings = new JsonSerializerSettings {
                //NullValueHandling = NullValueHandling.Ignore,
                MissingMemberHandling = MissingMemberHandling.Ignore
            };

            var action = JsonConvert.DeserializeObject<RootObject>(File.ReadAllText($"{actionSavePath+actionResult.Id}.json"),settings);

            Console.WriteLine($"Action: {action.name}\nURL: {action.url_xivdb}");

            var Embed = new EmbedBuilder()
                .WithTitle(action.name)
                .WithUrl(action.url_xivdb)
                .WithThumbnailUrl(action.icon_hq);
                
            if (action.level == 0) {
                Embed.WithFooter(new EmbedFooterBuilder().WithText($"{action.class_name} 1"));
            } else {
                Embed.WithFooter(new EmbedFooterBuilder().WithText($"{action.class_name} {action.level.ToString()} - {action.cast_range} yalms"));
            }


            if (action.help != "") {
                Embed.WithDescription(action.help);
            }

                Embed.Build();

            await ReplyAsync("", embed: Embed);

        }*/

        [Command("Info")]
        [Alias("i")]
        [Remarks("action name")]
        [Summary("Lists info about specified action")]
        public async Task AbilityInfo([Remainder] string input) {
            var actions = _realm.GameData.GetSheet<SaintCoinach.Xiv.Action>();

            var actionResult = from act in actions
                               where act.Name.ToString().ToLower().Contains(input.ToLower()) && act.ClassJob != null
                               select act;

            Console.WriteLine("Creating Action: ");
            SaintCoinach.Xiv.Action action;

            try {
                action = actionResult.First();
            }
            catch {
                await ReplyAsync("That isn't an action");
                return;
            }

            Console.WriteLine($"action: {action.Name}");

            var Embed = new EmbedBuilder()
                .WithTitle(action.Name)
                .WithDescription(action.Description)
                .WithColor(new Color(250, 140, 73))
                .Build();


            await ReplyAsync("", embed: Embed);

        }

        [Command("Search")]
        [Alias("s")]
        [Summary("Searches for an action by name")]
        public async Task SearchCommand([Remainder] string input) {
            var actions = _realm.GameData.GetSheet<SaintCoinach.Xiv.Action>();

            var actionResult = from act in actions
                               where act.Name.ToString().ToLower().Contains(input.ToLower()) && act.ClassJob != null
                               select act;

            

            var pages = new List<string>();
            pages.Add("");
            var searchSize = actionResult.Count();
            string[] searchResults = new String[searchSize];
            for (var i = 0; i < searchSize; i++) {
                var actionTemp = actionResult.ElementAt(i);
                if (actionTemp.ClassJob != null) {
                    searchResults[i] = $"{actionTemp.ClassJob.Abbreviation} {actionTemp.Level}: {actionTemp.Name}";
                }

            }

            Array.Sort(searchResults, new AlphanumComparatorFast());

            int pageIndex = 0;
            for (var i = 0; i < searchSize; i++) {

                if (i % 13 == 0 && i != 0 && i != 1) {

                    pages.Add(searchResults[i]);
                    pageIndex++;
                }
                else {
                    pages[pageIndex] += $"\n{searchResults[i]}";
                }
            }

            var message = new PaginatedMessage(pages, $"{searchSize} Search Results", new Color(250, 140, 73), Context.User);

            await paginator.SendPaginatedMessageAsync(Context.Channel, message);


        }

    }

}