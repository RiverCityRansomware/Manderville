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

            
            

            string reply;
            var Embed = new EmbedBuilder()
                    .WithTitle(item.Name)
                    .WithUrl($"https://xivdb.com/item/{item.Key}")
                    .WithColor(new Color(250, 140, 73));

            // --------------
            // PhysicalWeapon
            // --------------
            if (itemType.ToString() == "SaintCoinach.Xiv.Items.PhysicalWeapon") {
                var physicalWeapon = item as SaintCoinach.Xiv.Items.PhysicalWeapon;

                var parameters = "";

                foreach (var param in physicalWeapon.AllParameters) {
                    parameters += $"{param.BaseParam.Name}: `{param.Values.First()}`\n";
                }

                char canBeDyed = physicalWeapon.IsDyeable ? '\u2713' : '×';
                char canbeConverted = physicalWeapon.IsConvertable ? '\u2713' : '×';
                char isPvP = physicalWeapon.IsPvP ? '\u2713' : '×';

                reply = $"__**Stats**__\n" +
                $"Item Level: `{physicalWeapon.ItemLevel.Key}`\n" +
                $"{parameters}\n" +
                $"__**Misc**__\n" +
                $"Convertable: {canbeConverted}\n" +
                $"Dyeable: {canBeDyed}\n" +
                $"PvP: {isPvP}\n\n";

                //http://img.finalfantasyxiv.com/lds/pc/global/images/itemicon/17/1780833b30b3d7a680c5ee89c79c9240214806a9.png?20170711

                Console.WriteLine($"{physicalWeapon.Key}");

                Embed.WithDescription(reply)
                    .WithThumbnailUrl($"https://secure.xivdb.com/img/game_local/{item.Key.ToString().First()}/{item.Key.ToString()}.jpg")
                    .WithFooter(new EmbedFooterBuilder()
                    .WithText($"{physicalWeapon.EquipmentLevel.ToString()} {physicalWeapon.ClassJobCategory.Name}"))
                    .Build();
                await ReplyAsync("", embed: Embed);

            }
            // -----------
            // MagicWeapon
            // -----------
            else if (itemType.ToString() == "SaintCoinach.Xiv.Items.MagicWeapon") {
                var magicWeapon = item as SaintCoinach.Xiv.Items.MagicWeapon;

                var parameters = "";

                foreach (var param in magicWeapon.AllParameters) {
                    parameters += $"{param.BaseParam.Name}: `{param.Values.First()}`\n";
                }

                char canBeDyed = magicWeapon.IsDyeable ? '\u2713' : '×';
                char canbeConverted = magicWeapon.IsConvertable ? '\u2713' : '×';
                char isPvP = magicWeapon.IsPvP ? '\u2713' : '×';

                reply = $"__**Stats**__\n" +
                $"Item Level: `{magicWeapon.ItemLevel.Key}`\n" +
                $"{parameters}\n" +
                $"__**Misc**__\n" +
                $"Convertable: {canbeConverted}\n" +
                $"Dyeable: {canBeDyed}\n" +
                $"PvP: {isPvP}\n\n";

                Embed.WithDescription(reply)
                    .WithThumbnailUrl($"https://secure.xivdb.com/img/game_local/{item.Key.ToString().First()}/{item.Key.ToString()}.jpg")
                    .WithFooter(new EmbedFooterBuilder()
                    .WithText($"{magicWeapon.EquipmentLevel.ToString()} {magicWeapon.ClassJobCategory.Name}"))
                    .Build();
                await ReplyAsync("", embed: Embed);
            }
            // ------
            // Weapon
            // ------
            else if (itemType.ToString() == "SaintCoinach.Xiv.Items.Weapon") {
                var weapon = item as SaintCoinach.Xiv.Items.Weapon;

                var parameters = "";

                foreach (var param in weapon.AllParameters) {
                    parameters += $"{param.BaseParam.Name}: `{param.Values.First()}`\n";
                }

                char canBeDyed = weapon.IsDyeable ? '\u2713' : '×';
                char canbeConverted = weapon.IsConvertable ? '\u2713' : '×';
                char isPvP = weapon.IsPvP ? '\u2713' : '×';

                reply = $"__**Stats**__\n" +
                $"Item Level: `{weapon.ItemLevel.Key}`\n" +
                $"{parameters}\n" +
                $"__**Misc**__\n" +
                $"Convertable: {canbeConverted}\n" +
                $"Dyeable: {canBeDyed}\n" +
                $"PvP: {isPvP}\n\n";

                Embed.WithDescription(reply)
                    .WithThumbnailUrl($"https://secure.xivdb.com/img/game_local/{item.Key.ToString().First()}/{item.Key.ToString()}.jpg")
                    .WithFooter(new EmbedFooterBuilder()
                    .WithText($"{weapon.EquipmentLevel.ToString()} {weapon.ClassJobCategory.Name}"))
                    .Build();
                await ReplyAsync("", embed: Embed);
            }
            // ------
            // Shield
            // ------
            else if (itemType.ToString() == "SaintCoinach.Xiv.Items.Shield") {
                var weapon = item as SaintCoinach.Xiv.Items.Weapon;

                var parameters = "";

                foreach (var param in weapon.AllParameters) {
                    parameters += $"{param.BaseParam.Name}: `{param.Values.First()}`\n";
                }

                char canBeDyed = weapon.IsDyeable ? '\u2713' : '×';
                char canbeConverted = weapon.IsConvertable ? '\u2713' : '×';
                char isPvP = weapon.IsPvP ? '\u2713' : '×';

                reply = $"__**Stats**__\n" +
                $"Item Level: `{weapon.ItemLevel.Key}`\n" +
                $"{parameters}\n" +
                $"__**Misc**__\n" +
                $"Convertable: {canbeConverted}\n" +
                $"Dyeable: {canBeDyed}\n" +
                $"PvP: {isPvP}\n\n";

                Embed.WithDescription(reply)
                    .WithThumbnailUrl($"https://secure.xivdb.com/img/game_local/{item.Key.ToString().First()}/{item.Key.ToString()}.jpg")
                    .WithFooter(new EmbedFooterBuilder()
                    .WithText($"{weapon.EquipmentLevel.ToString()} {weapon.ClassJobCategory.Name}"))
                    .Build();
                await ReplyAsync("", embed: Embed);
            }
            // ---------
            // Equipment
            // ---------
            else if (itemType.ToString() == "SaintCoinach.Xiv.Items.Equipment") {
                var equipment = item as SaintCoinach.Xiv.Items.Equipment;

                var parameters = "";

                foreach (var param in equipment.AllParameters) {
                    parameters += $"{param.BaseParam.Name}: `{param.Values.First()}`\n";
                }

                char canBeDyed = equipment.IsDyeable ? '\u2713' : '×';
                char canbeConverted = equipment.IsConvertable ? '\u2713' : '×';
                char isPvP = equipment.IsPvP ? '\u2713' : '×';


                reply = $"__**Stats**__\n" +
                $"Item Level: `{equipment.ItemLevel.Key}`\n" +
                $"{parameters}\n" +
                $"__**Misc**__\n" +
                $"Convertable: {canbeConverted}\n" +
                $"Dyeable: {canBeDyed}\n" +
                $"PvP: {isPvP}\n\n";

                Embed.WithDescription(reply)
                    .WithThumbnailUrl($"https://secure.xivdb.com/img/game_local/{item.Key.ToString().First()}/{item.Key.ToString()}.jpg")
                    .WithFooter(new EmbedFooterBuilder()
                    .WithText($"{equipment.EquipmentLevel.ToString()} {equipment.ClassJobCategory.Name}"))
                    .Build();
                await ReplyAsync("", embed: Embed);
            }
            // ------
            // Armour
            // ------
            else if (itemType.ToString() == "SaintCoinach.Xiv.Items.Armour") {
                var armour = item as SaintCoinach.Xiv.Items.Armour;

                var parameters = "";

                foreach (var param in armour.AllParameters) {
                    parameters += $"{param.BaseParam.Name}: `{param.Values.First()}`\n";
                }

                char canBeDyed = armour.IsDyeable ? '\u2713' : '×';
                char canbeConverted = armour.IsConvertable ? '\u2713' : '×';
                char isPvP = armour.IsPvP ? '\u2713' : '×';


                reply = $"__**Stats**__\n" +
                $"Item Level: `{armour.ItemLevel.Key}`\n" +
                $"{parameters}\n" +
                $"__**Misc**__\n" +
                $"Convertable: {canbeConverted}\n" +
                $"Dyeable: {canBeDyed}\n" +
                $"PvP: {isPvP}\n\n";

                Embed.WithDescription(reply)
                    .WithThumbnailUrl($"https://secure.xivdb.com/img/game_local/{item.Key.ToString().First()}/{item.Key.ToString()}.jpg")
                    .WithFooter(new EmbedFooterBuilder()
                    .WithText($"{armour.EquipmentLevel.ToString()} {armour.ClassJobCategory.Name}"))
                    .Build();
                await ReplyAsync("", embed: Embed);

            }
            // ------
            // Accessory
            // ------
            else if (itemType.ToString() == "SaintCoinach.Xiv.Items.Accessory") {
                var accessory = item as SaintCoinach.Xiv.Items.Accessory;
                var parameters = "";

                foreach (var param in accessory.AllParameters) {
                    parameters += $"{param.BaseParam.Name}: `{param.Values.First()}`\n";
                }

                char canBeDyed = accessory.IsDyeable ? '\u2713' : '×';
                char canbeConverted = accessory.IsConvertable ? '\u2713' : '×';
                char isPvP = accessory.IsPvP ? '\u2713' : '×';

                reply = $"__**Stats**__\n" +
                $"Item Level: `{accessory.ItemLevel.Key}`\n" +
                $"{parameters}\n" +
                $"__**Misc**__\n" +
                $"Convertable: {canbeConverted}\n" +
                $"Dyeable: {canBeDyed}\n" +
                $"PvP: {isPvP}\n\n";

                Embed.WithDescription(reply)
                    .WithThumbnailUrl($"https://secure.xivdb.com/img/game_local/{item.Key.ToString().First()}/{item.Key.ToString()}.jpg")
                    .WithFooter(new EmbedFooterBuilder()
                    .WithText($"{accessory.EquipmentLevel.ToString()} {accessory.ClassJobCategory.Name}"))
                    .Build();
                await ReplyAsync("", embed: Embed);
            }
            // ----
            // SoulCrystal
            // ----
            else if (itemType.ToString() == "SoulCrystal") {
                var soulCrystal = item as SaintCoinach.Xiv.Items.SoulCrystal;
                var parameters = "";

                foreach (var param in soulCrystal.AllParameters) {
                    parameters += $"{param.BaseParam.Name}: `{param.Values.First()}`\n";
                }

                reply = $"__**Stats**__\n" +
                $"Item Level: `{soulCrystal.ItemLevel.Key}`\n" +
                $"{parameters}\n" +
                $"{soulCrystal.Description}";

                Embed.WithDescription(reply)
                    .WithThumbnailUrl($"https://secure.xivdb.com/img/game_local/{item.Key.ToString().First()}/{item.Key.ToString()}.jpg")
                    .WithFooter(new EmbedFooterBuilder()
                    .WithText($"{soulCrystal.EquipmentLevel.ToString()} {soulCrystal.ClassJobCategory.Name}"))
                    .Build();
                await ReplyAsync("", embed: Embed);
            }
            // ------------
            // CraftingTool
            // ------------
            else if (itemType.ToString() == "SaintCoinach.Xiv.Items.CraftingTool") {
                var craftingTool = item as SaintCoinach.Xiv.Items.CraftingTool;
                var parameters = "";

                foreach (var param in craftingTool.AllParameters) {
                    parameters += $"{param.BaseParam.Name}: `{param.Values.First()}`\n";
                }

                char canBeDyed = craftingTool.IsDyeable ? '\u2713' : '×';
                char canbeConverted = craftingTool.IsConvertable ? '\u2713' : '×';
                char isPvP = craftingTool.IsPvP ? '\u2713' : '×';

                reply = $"__**Stats**__\n" +
                $"Item Level: `{craftingTool.ItemLevel.Key}`\n" +
                $"{parameters}\n" +
                $"__**Misc**__\n" +
                $"Convertable: {canbeConverted}\n" +
                $"Dyeable: {canBeDyed}\n" +
                $"PvP: {isPvP}\n\n";

                Embed.WithDescription(reply)
                    .WithThumbnailUrl($"https://secure.xivdb.com/img/game_local/{item.Key.ToString().First()}/{item.Key.ToString()}.jpg")
                    .WithFooter(new EmbedFooterBuilder()
                    .WithText($"{craftingTool.EquipmentLevel.ToString()} {craftingTool.ClassJobCategory.Name}"))
                    .Build();
                await ReplyAsync("", embed: Embed);
            }
            // ----
            // GatheringTool
            // ----
            else if (itemType.ToString() == "SaintCoinach.Xiv.Items.GatheringTool") {
                var gatheringTool = item as SaintCoinach.Xiv.Items.GatheringTool;
                var parameters = "";

                foreach (var param in gatheringTool.AllParameters) {
                    parameters += $"{param.BaseParam.Name}: `{param.Values.First()}`\n";
                }

                char canBeDyed = gatheringTool.IsDyeable ? '\u2713' : '×';
                char canbeConverted = gatheringTool.IsConvertable ? '\u2713' : '×';
                char isPvP = gatheringTool.IsPvP ? '\u2713' : '×';

                reply = $"__**Stats**__\n" +
                $"Item Level: `{gatheringTool.ItemLevel.Key}`\n" +
                $"{parameters}\n" +
                $"__**Misc**__\n" +
                $"Convertable: {canbeConverted}\n" +
                $"Dyeable: {canBeDyed}\n" +
                $"PvP: {isPvP}\n\n";

                Embed.WithDescription(reply)
                    .WithThumbnailUrl($"https://secure.xivdb.com/img/game_local/{item.Key.ToString().First()}/{item.Key.ToString()}.jpg")
                    .WithFooter(new EmbedFooterBuilder()
                    .WithText($"{gatheringTool.EquipmentLevel.ToString()} {gatheringTool.ClassJobCategory.Name}"))
                    .Build();
                await ReplyAsync("", embed: Embed);
            }
            // -------
            // Housing
            // -------
            else if (itemType.ToString() == "SaintCoinach.Xiv.Items.Housing") {
                var housingItem = item as SaintCoinach.Xiv.Items.Housing;
                reply = "";

                if (!housingItem.Description.IsEmpty) {
                    reply += $"__**Description**__\n" +
                    $"```{housingItem.Description}```\n";
                }

                Embed
                    .WithThumbnailUrl($"https://secure.xivdb.com/img/game_local/{item.Key.ToString().First()}/{item.Key.ToString()}.jpg")
                    .WithDescription(reply)
                    .Build();
                await ReplyAsync("", embed: Embed);
            }
            // ------------
            // MaterialItem
            // ------------
            else if (itemType.ToString() == "SaintCoinach.Xiv.Items.MateriaItem") {
                var materiaItem = item as SaintCoinach.Xiv.Items.MateriaItem;
                reply = "";

                reply += $"__**Stats**__\n" +
                    $"{materiaItem.BaseParam}: {materiaItem.Value}\n\n";

                if (!materiaItem.Description.IsEmpty) {
                    reply += $"__**Description**__\n" +
                    $"```{materiaItem.Description}```\n";
                }

                Embed
                    .WithThumbnailUrl($"https://secure.xivdb.com/img/game_local/{item.Key.ToString().First()}/{item.Key.ToString()}.jpg")
                    .WithDescription(reply)
                    .Build();
                await ReplyAsync("", embed: Embed);
            }
            // ------
            // Usable
            // ------
            else if (itemType.ToString() == "SaintCoinach.Xiv.Items.Usable") {
                var usableItem = item as SaintCoinach.Xiv.Items.Usable;
                reply = "";
                reply = $"Cooldown: {usableItem.Cooldown}\n" +
                    $"__**Description**__\n" +
                    $"```{usableItem.Description}```\n";

                Embed
                    .WithThumbnailUrl($"https://secure.xivdb.com/img/game_local/{item.Key.ToString().First()}/{item.Key.ToString()}.jpg")
                    .WithDescription(reply)
                    .Build();
                await ReplyAsync("", embed: Embed);
            }
            // ------------
            // Generic item
            // ------------
            else if (itemType.ToString() == "SaintCoinach.Xiv.Item") {
                reply = "";

                if (!item.Description.IsEmpty) {
                    reply += $"__**Description**__\n" +
                    $"```{item.Description}```\n";
                }

                Embed
                    .WithThumbnailUrl($"https://secure.xivdb.com/img/game_local/{item.Key.ToString().First()}/{item.Key.ToString()}.jpg")
                    .WithDescription(reply)
                    .Build();
                await ReplyAsync("", embed: Embed);
            }

            Embed.Build();
            
        }
    }
}
