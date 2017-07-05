using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.Addons.Paginator;
using SaintCoinach;

namespace Manderville.Modules {

    [Group("Items")]
    [Alias("I")]
    public class Items : ModuleBase {
        private readonly PaginationService paginator;
        private readonly ARealmReversed _realm;
        const string GameDirectory = @"C:\FFXIV\SquareEnix\FINAL FANTASY XIV - A Realm Reborn";

        public Items(PaginationService paginationService) {
            paginator = paginationService;

            _realm = new ARealmReversed(GameDirectory, SaintCoinach.Ex.Language.English);

            if (!_realm.IsCurrentVersion) {
                const bool IncludeDataChanges = true;
                var updateReport = _realm.Update(IncludeDataChanges);
            }
        }

        [Command("Search")]
        [Alias("s")]
        [Summary("Searches for items by name")]
        public async Task SearchGeneral ([Remainder] string input) {
            var x = input.Split(' ');
            int y = 0;
            if (x[0].Length == 3 && Int32.TryParse(x[0], out y) && x.Length == 1) {
                await ItemLevelSearch(Int32.Parse(x[0]));
            } else if (x[0].Length == 3 && Int32.TryParse(x[0], out y) && x.Length > 1) {
                var rep = "";
                for (var i=1;i<x.Length;i++) {
                    rep += x[i] + " ";
                }
                await ItemLevelNameSearch(Int32.Parse(x[0]), rep);
            } else {
                await ItemSearch(input);
            }
        }

        public async Task ItemSearch(string input) {
            Console.WriteLine($"searching for: {input}");
            var items = _realm.GameData.GetSheet<SaintCoinach.Xiv.Item>();
            var searchResultsIEnumerable = from item in items
                                           where item.Name.ToString().ToLower().Contains(input.ToLower())
                                           select item;

            var pages = new List<string>();
            pages.Add("");
            var searchSize = searchResultsIEnumerable.Count();
            string[] searchResults = new String[searchSize];

            for (var i = 0; i < searchSize; i++) {
                var itemTemp = searchResultsIEnumerable.ElementAt(i);
                searchResults[i] = $"{itemTemp.Name}";
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




        public async Task ItemLevelSearch(int ilvl) {
            Console.WriteLine($"searching for: {ilvl}");
            var items = _realm.GameData.GetSheet<SaintCoinach.Xiv.Item>();
            var searchResultsIEnumerable = from item in items
                                           where item.ItemLevel.Key == ilvl
                                           select item;

            var pages = new List<string>();
            pages.Add("");
            var searchSize = searchResultsIEnumerable.Count();
            string[] searchResults = new String[searchSize];

            for (var i = 0; i < searchSize; i++) {
                var itemTemp = searchResultsIEnumerable.ElementAt(i);
                searchResults[i] = $"{itemTemp.Name}";
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

        public async Task ItemLevelNameSearch(int ilvl, [Remainder] string input) {
            Console.WriteLine($"searching for: {input}");
            var items = _realm.GameData.GetSheet<SaintCoinach.Xiv.Item>();
            var searchResultsIEnumerable = from item in items
                                           where item.Name.ToString().ToLower().Contains(input.ToLower()) && item.ItemLevel.Key == ilvl
                                           select item;

            var pages = new List<string>();
            pages.Add("");
            var searchSize = searchResultsIEnumerable.Count();
            string[] searchResults = new String[searchSize];

            for (var i = 0; i < searchSize; i++) {
                var itemTemp = searchResultsIEnumerable.ElementAt(i);
                searchResults[i] = $"{itemTemp.Name}";
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

        [Command("Info")]
        [Alias("i")]
        [Summary("Lists info about the specified item")]
        public async Task ItemInfo([Remainder]string input) {
            var items = _realm.GameData.GetSheet<SaintCoinach.Xiv.Item>();

            var itemResult = from it in items
                             where it.Name.ToString().ToLower().Contains(input.ToLower())
                             select it;

            SaintCoinach.Xiv.Item item;

            try {
                item = itemResult.First();
            } catch {
                await ReplyAsync("No results");
                return;
            }

            var itemType = item.GetType();
            
            Console.WriteLine($"Item type: {item.GetType()}");
            string reply;
            var Embed = new EmbedBuilder()
                    .WithTitle(item.Name)
                    .WithColor(new Color(250, 140, 73));

            if (itemType.ToString() == "SaintCoinach.Xiv.Items.PhysicalWeapon") { // Physical Weapon
                Console.WriteLine($"Physical Weapon: {item.Name}");
                
                //var physicalWeapons = _realm.GameData
                

                //var physicalSearch = from phys in physicalWeapons
                //                    where phys.Name.ToString().ToLower() == item.Name.ToString().ToLower()
                //                     select phys;

                //var realItem = physicalSearch.First();



                //reply = $"__**Stats**__\n" +
                //$"Item Level: {realItem.ItemLevel.Key}\n";

                //Console.WriteLine(realItem.AllParameters);

            } else if (itemType.ToString() == "SaintCoinach.Xiv.Items.Armour") {
                Console.WriteLine("Armour");
            } else if (itemType.ToString() == "SaintCoinach.Xiv.Item") { // Generic Item
                reply = "";

                if (!item.Description.IsEmpty) {
                    reply += $"__**Description**__\n" +
                    $"```{item.Description}```\n";
                }


                Embed.WithDescription(reply);
                    

                await ReplyAsync("", embed: Embed);
            }

            Embed.Build();
            
        }
    }
}
