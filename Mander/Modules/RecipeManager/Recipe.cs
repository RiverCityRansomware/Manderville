using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net;
using Discord;
using Discord.Addons.Paginator;
using Discord.Commands;
using Manderville.Common;
using Newtonsoft.Json;
using SaintCoinach;
using System.Text.RegularExpressions;

namespace Manderville.Modules {

    [Group("Recipes")]
    [Alias("R")]
    public class Recipes : ModuleBase {

        private readonly PaginationService paginator;
        private readonly ARealmReversed _realm;
        const string GameDirectory = @"C:\FFXIV\SquareEnix\FINAL FANTASY XIV - A Realm Reborn";

        public Recipes(PaginationService paginationService) {
            paginator = paginationService;
            _realm = new ARealmReversed(GameDirectory, SaintCoinach.Ex.Language.English);

            if (!_realm.IsCurrentVersion) {
                const bool IncludeDataChanges = true;
                var updateReport = _realm.Update(IncludeDataChanges);
            }
        }
       
        

        public bool isJob(string x) {
            string s = x.ToLower();
            if ((s == "alc" || s == "bsm" || s == "arm" ||
                s == "crp" || s == "cul" || s == "gsm" ||
                s == "ltw" || s == "wvr") || (s == "alchemist" ||
                s == "blacksmith" || s == "armorer" || s == "carpenter" ||
                s == "culinarian" || s == "goldsmith" || s == "leatherworker" ||
                s == "weaver" || s == "armourer")) {
                return true;
            } else {
                return false;
            }
        }

        public bool isLevel(string s) {
            int x = 0;
            if (Int32.TryParse(s, out x)) {
                return true;
            } else {
                return false;
            }
        }

        [Command("Search")]
        [Alias("s")]
        [Remarks("[Job recipe] or [Job Level] or [Job Level recipe]")]
        [Summary("Searches for recipes by level and crafting job abbr[alc, crp, etc]\nIf level and/or job are not specified searches by name")]
        public async Task SearchCommand([Remainder] string input) {
            string[] args = input.Split(' ');

            foreach (var arg in args) {
                Console.WriteLine($"arg: {arg}");
            }

            var isJobFirst = false;
            var isJobSecond = false;
            var isRemainder = false;
            var isLevelFirst = false;
            var isLevelSecond = false;
            string remainder = "";

            if (args.Length >= 1) {
                isJobFirst = isJob(args[0]);
                isLevelFirst = isLevel(args[0]);
            } else {
                await ReplyAsync("At least one argument must be supplied for this command");
            }

            if (args.Length >= 2) {
                isJobSecond = isJob(args[1]);
                isLevelSecond = isLevel(args[1]);
                if ((isJobFirst == true && isJobSecond == false) && (isLevelFirst == false && isLevelSecond == false)) {
                    isRemainder = true;
                    for (var i = 1; i < args.Length; i++) {
                        remainder += args[i];
                    }
                }
            }

            if (args.Length >= 3 && remainder == "") {
                isRemainder = true;
                for (var i = 2; i < args.Length; i++) {
                    remainder += args[i];
                }
            }

            Console.WriteLine($"isJobFirst: {isJobFirst}\n" +
                $"isJobSecond: {isJobSecond}\n" +
                $"isLevelFirst: {isLevelFirst}\n" +
                $"isLevelSecond: {isLevelSecond}\n" +
                $"isRemainder: {isRemainder}");

            if (isJobFirst != true && isLevelFirst != true) {
                await RecipeSearchGeneral(input);
            } else if ((isJobFirst == true && isJobSecond == false) && (isLevelSecond == true && isLevelFirst == false) && isRemainder == false) {
                await RecipeSearchJobLevel(args[0], args[1]);
            } else if ((isJobFirst == true && isJobSecond == false) && (isLevelSecond == true && isLevelFirst == false) && isRemainder == true) {
                await RecipeSearchJobLevelRemainder(args[0], args[1], remainder);
            } else if ((isJobFirst == false && isJobSecond == true) && (isLevelFirst == true && isLevelSecond == false) && isRemainder == false) {
                await RecipeSearchJobLevel(args[1], args[0]);
            } else if ((isJobFirst == false && isJobSecond == true) && (isLevelFirst == true && isLevelSecond == false) && isRemainder == true) {
                await RecipeSearchJobLevelRemainder(args[1], args[0], remainder);
            } else if ((isJobFirst == true && isJobSecond == false) && (isLevelFirst == false && isLevelSecond == false) && isRemainder == true) {
                await RecipeSearchJob(args[0], remainder);
            }
        }

        // Saint Coinach
        // Searches by name only
        public async Task RecipeSearchGeneral([Remainder] string input) {
            Console.WriteLine("searching by remainder: " + input);
            var recipes = _realm.GameData.GetSheet<SaintCoinach.Xiv.Recipe>();
            var searchResultsIEnumerable = from recipe in recipes
                                           where recipe.ResultItem.Name.ToString().ToLower().Contains(input.ToLower())
                                           select recipe;

            var pages = new List<string>();
            pages.Add("");
            var searchSize = searchResultsIEnumerable.Count();
            Console.WriteLine("Size of search: " + searchSize);

            string[] searchResults = new String[searchSize];
            for (var i = 0; i < searchSize; i++) {
                var recipeTemp = searchResultsIEnumerable.ElementAt(i);
                searchResults[i] = $"{recipeTemp.ClassJob.Abbreviation} {recipeTemp.RecipeLevelTable.CharacterLevel}: {recipeTemp.ResultItem.Name}";
            }

            Array.Sort(searchResults, new AlphanumComparatorFast());

            int pageIndex = 0;
            for (var i = 0; i < searchSize; i++) {

                if (i % 13 == 0 && i != 0 && i != 1) {

                    pages.Add(searchResults[i]);
                    pageIndex++;
                } else {
                    pages[pageIndex] += $"\n{searchResults[i]}";
                }
            }

            var message = new PaginatedMessage(pages, $"{searchSize} Search Results", new Color(250, 140, 73), Context.User);

            await paginator.SendPaginatedMessageAsync(Context.Channel, message);
        }

        // Saint Coinach
        // Searches by Job and Level
        public async Task RecipeSearchJobLevel(string job, string level) {
            Console.WriteLine("searching by job level");
            var recipes = _realm.GameData.GetSheet<SaintCoinach.Xiv.Recipe>();
            var searchResultsIEnumerable = from recipe in recipes
                                           where recipe.RecipeLevelTable.CharacterLevel.ToString() == level &&
                                           (recipe.ClassJob.Name.ToString().ToLower() == job.ToLower() ||
                                           recipe.ClassJob.Abbreviation.ToString().ToLower() == job.ToLower())
                                           select recipe;

            var searchSize = searchResultsIEnumerable.Count();
            string[] searchResults = new string[searchSize];

            for (var i = 0; i < searchSize; i++) {
                var recipeTemp = searchResultsIEnumerable.ElementAt(i);
                searchResults[i] = $"{recipeTemp.ResultItem.Name}";
            }

            Array.Sort(searchResults, new AlphanumComparatorFast());


            var pages = new List<string>();
            pages.Add("");

            int pageIndex = 0;
            int pageLength = 0;
            for (var i = 0; i < searchSize; i++) {
                if (pageLength <= 12) {
                    pages[pageIndex] += $"\n{searchResults[i]}";
                    pageLength++;
                } else {
                    pages.Add($"{searchResults[i]}");
                    pageLength = 0;
                    pageIndex++;
                }
            }
            var message = new PaginatedMessage(pages, $"{searchSize} Search Results", new Color(250, 140, 73), Context.User);
            await paginator.SendPaginatedMessageAsync(Context.Channel, message);
        }

        // Saint Coinach
        // Searches by job and level : with remainder
        public async Task RecipeSearchJobLevelRemainder(string job, string level, [Remainder] string input) {
            Console.WriteLine("searching by job level remainder");
            var recipes = _realm.GameData.GetSheet<SaintCoinach.Xiv.Recipe>();
            var searchResultsIEnumerable = from recipe in recipes
                                           where recipe.RecipeLevelTable.CharacterLevel.ToString() == level &&
                                           (recipe.ClassJob.Name.ToString().ToLower() == job.ToLower() ||
                                           recipe.ClassJob.Abbreviation.ToString().ToLower() == job.ToLower()) &&
                                           (recipe.ResultItem.Name.ToString().ToLower().Contains(input))
                                           select recipe;

            var searchSize = searchResultsIEnumerable.Count();
            string[] searchResults = new string[searchSize];

            for (var i = 0; i < searchSize; i++) {
                var recipeTemp = searchResultsIEnumerable.ElementAt(i);
                searchResults[i] = $"{recipeTemp.ResultItem.Name}";
            }

            Array.Sort(searchResults, new AlphanumComparatorFast());


            var pages = new List<string>();
            pages.Add("");

            int pageIndex = 0;
            int pageLength = 0;
            for (var i = 0; i < searchSize; i++) {
                if (pageLength <= 12) {
                    pages[pageIndex] += $"\n{searchResults[i]}";
                    pageLength++;
                } else {
                    pages.Add($"{searchResults[i]}");
                    pageLength = 0;
                    pageIndex++;
                }
            }
            var message = new PaginatedMessage(pages, $"{searchSize} Search Results", new Color(250, 140, 73), Context.User);
            await paginator.SendPaginatedMessageAsync(Context.Channel, message);
        }

        // Search by job and remainder
        public async Task RecipeSearchJob(string job, [Remainder] string input) {
            Console.WriteLine("searching by job");
            var recipes = _realm.GameData.GetSheet<SaintCoinach.Xiv.Recipe>();
            var searchResultsIEnumerable = from recipe in recipes
                                           where (recipe.ClassJob.Name.ToString().ToLower() == job.ToLower() ||
                                           recipe.ClassJob.Abbreviation.ToString().ToLower() == job.ToLower()) &&
                                           (recipe.ResultItem.Name.ToString().ToLower().Contains(input))
                                           select recipe;

            var searchSize = searchResultsIEnumerable.Count();
            string[] searchResults = new string[searchSize];

            for (var i = 0; i < searchSize; i++) {
                var recipeTemp = searchResultsIEnumerable.ElementAt(i);
                searchResults[i] = $"{recipeTemp.RecipeLevelTable.CharacterLevel}: {recipeTemp.ResultItem.Name}";
            }

            Array.Sort(searchResults, new AlphanumComparatorFast());


            var pages = new List<string>();
            pages.Add("");

            int pageIndex = 0;
            int pageLength = 0;
            for (var i = 0; i < searchSize; i++) {
                if (pageLength <= 12) {
                    pages[pageIndex] += $"\n{searchResults[i]}";
                    pageLength++;
                } else {
                    pages.Add($"{searchResults[i]}");
                    pageLength = 0;
                    pageIndex++;
                }
            }
            var message = new PaginatedMessage(pages, $"{searchSize} Search Results", new Color(250, 140, 73), Context.User);
            await paginator.SendPaginatedMessageAsync(Context.Channel, message);
        }


        // TODO: Shopping cart here, maybe have it be strings seperated by commas?
        [Command("ShoppingCart")]
        [Alias("sc")]
        [Summary("")]
        public async Task ShoppingCart([Remainder] string input) {
            // Splits on comma and space
            var args = Regex.Split(input, ", +");


            // Splits numbers into a string array
            string[] numberString = new string[args.Count()];
            // Creates a int array of the same size of numberString
            int[] numbers = new int[args.Count()];

            for (int j=0;j<args.Count();j++) {
                string[] temp = Regex.Split(args[j], @"[\D+]");
                for (var i = 0; i < temp.Count(); i++) {
                    if (temp[i] != null && temp[i] != "") {
                        Console.WriteLine($"number: {temp[i]}");
                        numberString[j] = temp[i];
                    }
                }
            }

            var recipes = _realm.GameData.GetSheet<SaintCoinach.Xiv.Recipe>();

            SaintCoinach.Xiv.Recipe[] searchResults = new SaintCoinach.Xiv.Recipe[args.Count()];

            Dictionary<string, int> matTotals = new Dictionary<string, int>();

            foreach(var num in numberString) {
                Console.WriteLine($"num: {num}");
            }

            for (var i=0; i<args.Length; i++) {
                // Grabs the non number characters from the argument
                var argRecipes = Regex.Split(args[i], "^\\d+\\s+");
                
                // Recipe name
                string mat = "";
                //Console.Write("arg:_");
                for (var j=0;j<argRecipes.Count();j++) {
                    //Console.Write($"{argRecipes[j]}");
                        mat += argRecipes[j];
                    
                }
                
                int x = 0;
               
                // If number is not null or empty parse it and add it to numbers[] else add 1 to numbers
                if (numberString[i] != null && (Int32.TryParse(numberString[i], out x) == true)) {
                    Console.WriteLine($"number to parse: {numberString[i]}");
                    numbers[i] = Int32.Parse(numberString[i]);
                } else {
                    numbers[i] = 1;
                }

                Console.WriteLine($"mat: {mat}");

                var recipeResult = from rec in recipes
                                   where rec.ResultItem.Name.ToString().ToLower().Contains(mat.ToString().ToLower())
                                   select rec;

                

                SaintCoinach.Xiv.Recipe recipe;
                try {
                    recipe = recipeResult.First();
                } catch {
                    await ReplyAsync("That doesn't have a recipe");
                    return;
                }

                Console.WriteLine($"recipe: {numbers[i]}× {recipe.ResultItem.Name}");


               for (int k=1;k<=numbers[i];k++) {
                    TotalMats(matTotals, recipe);
               }

            }



            string[] matCountArray = new string[matTotals.Count];
            string matCount = "";

            for (int i = 0; i < matTotals.Count; i++) {
                matCountArray[i] += $"{matTotals.ElementAt(i).Value}× {matTotals.ElementAt(i).Key}\n";
            }

            Array.Sort(matCountArray, new AlphanumComparatorFast());

            foreach (var keyv in matCountArray) {
                matCount += $" - `{keyv}`\n";
            }
            string reply = $"__**Total Mats**__\n" +
                $"{matCount}";


            var embed = new EmbedBuilder()
                .WithColor(new Color(250, 140, 73))
                .WithDescription(reply)
                .Build();



            await ReplyAsync("", embed: embed);

        }

        public string ListMats(SaintCoinach.Xiv.Recipe recipe){
            var ingredients = "";
            foreach (var ingredient in recipe.Ingredients) {
                ingredients += $"**{ingredient.Count}× {ingredient.Item.Name}**\n";
                if (ingredient.Item.RecipesAsResult != null) {
                    foreach (var ing in ingredient.Item.RecipesAsResult) {
                        foreach (var subIngredient in ing.Ingredients) {
                            ingredients += $"  - {subIngredient.Count}× {subIngredient.Item.Name}\n";
                            if (subIngredient.Item.RecipesAsResult != null && subIngredient.Item.RecipesAsResult.Count() > 0) {
                                int i = 1;
                                ingredients += ListSubMats(subIngredient, i);
                            }
                        }
                    }
                }
            }
            return ingredients;
        }

        public string ListSubMats(SaintCoinach.Xiv.RecipeIngredient ingredients, int i) {
            var ingredientsString = "";
            if (ingredients.Item.RecipesAsResult != null && ingredients.Item.RecipesAsResult.Count() > 0) {
                var ing = ingredients.Item.RecipesAsResult.First();

                foreach (var subIngredient in ing.Ingredients) {
                    for (int j = 0; j <= i; j++) {
                        ingredientsString += "   ";
                    }

                    
                    if (i == 1) {
                        ingredientsString += $"* *{subIngredient.Count}× {subIngredient.Item.Name}*\n";
                    }else if (i == 2) {
                        ingredientsString += $"° `{subIngredient.Count}× {subIngredient.Item.Name}`\n";
                    } else if (i == 3) {
                        ingredientsString += $"- {subIngredient.Count}× {subIngredient.Item.Name}\n";
                    }

                    if (subIngredient.Item.RecipesAsResult != null) {
                        i++;
                        ingredientsString += ListSubMats(subIngredient, i);
                        i--;
                    }
                }
            }
            
            
            

            return ingredientsString;
        }

        public Dictionary<string, int> TotalMats(Dictionary<string, int> matTotals, SaintCoinach.Xiv.Recipe recipe) {
            //Console.WriteLine($"Recipe: {recipe.ResultItem.Name}");
            foreach (var ingredient in recipe.Ingredients) {
                //Console.WriteLine($" - {ingredient.Count}× {ingredient.Item.Name}");
                if (matTotals.ContainsKey(ingredient.Item.Name)) {
                    matTotals[ingredient.Item.Name] += ingredient.Count ;
                } else if (ingredient.Item.RecipesAsResult != null && ingredient.Item.RecipesAsResult.Count() < 1){
                    matTotals.Add(ingredient.Item.Name, ingredient.Count);
                }

                if (ingredient.Item.RecipesAsResult != null && ingredient.Item.RecipesAsResult.Count() > 0) {
                    foreach (var ing in ingredient.Item.RecipesAsResult.First().Ingredients) {
                        TotalSubMats(matTotals, ing, ingredient, ingredient.Count);
                    }
                }
            }
            return matTotals;
       }

        public Dictionary<string, int> TotalSubMats(Dictionary<string, int> matTotals, SaintCoinach.Xiv.RecipeIngredient ingred, SaintCoinach.Xiv.RecipeIngredient result, int countt) {
            //Console.WriteLine($"   * {ingred.Count}× {ingred.Item.Name} result: {result.Count}× {result.Item.Name} ");
            var count = ingred.Count * countt;
            if (matTotals.ContainsKey(ingred.Item.Name)) {
                matTotals[ingred.Item.Name] += ingred.Count * countt;
            } else {       
                matTotals.Add(ingred.Item.Name, count);
            } 

            if (ingred.Item.RecipesAsResult != null && ingred.Item.RecipesAsResult.Count() > 0) {
                foreach (var ing in ingred.Item.RecipesAsResult.First().Ingredients) {
                    TotalSubMats(matTotals, ing, ingred, count);
                }
            }

            return matTotals;
        }

        [Command("InfoAll")]
        [Alias("ia")]
        [Remarks("recipe")]
        [Summary("Lists required crafting materials for specified recipe")]
        public async Task RecipeInfoAll([Remainder] string input) {
            var recipes = _realm.GameData.GetSheet<SaintCoinach.Xiv.Recipe>();

            var recipeResult = from rec in recipes
                               where rec.ResultItem.Name.ToString().ToLower().Contains(input.ToLower())
                               select rec;

            SaintCoinach.Xiv.Recipe recipe;
            try {
                recipe = recipeResult.First();
            } catch {
                await ReplyAsync("That doesn't have a recipe");
                return;
            }

            string ingredients = ListMats(recipe);
            Dictionary<string, int> matTotals = new Dictionary<string, int>();
            TotalMats(matTotals, recipe);

            string[] matCountArray = new string[matTotals.Count];
            string matCount = "";

            for (int i = 0; i < matTotals.Count; i++) {
                matCountArray[i] += $"{matTotals.ElementAt(i).Value}× {matTotals.ElementAt(i).Key}\n";
            }

            Array.Sort(matCountArray, new AlphanumComparatorFast());

            foreach (var keyv in matCountArray) {
                matCount += $" - `{keyv}`\n";
            }

            char canBeHQ = recipe.ResultItem.CanBeHq ? '\u2713' : '×';
            char canBeDesynthed = recipe.ResultItem.IsDesynthesizable ? '\u2713' : '×';
            char canBeConverted = recipe.ResultItem.IsConvertable ? '\u2713' : '×';
            char canBeCollectable = recipe.ResultItem.IsCollectable ? '\u2713' : '×';
            char canBeDyed = recipe.ResultItem.IsDyeable ? '\u2713' : '×';
            char canBeAdvMelded = recipe.ResultItem.IsAdvancedMeldingPermitted ? '\u2713' : '×';
            char canBeQuickSynthed = recipe.CanQuickSynth ? '\u2713' : '×';



            string reply;

            if (!recipe.ResultItem.Description.IsEmpty) {
                reply = $"__Crafting__\n" +
                $"  -  Craftsmanship: `{recipe.RequiredCraftsmanship}`\n" +
                $"  -  Control: `{recipe.RequiredControl}`\n" +
                $"  -  Difficulty: `{recipe.DifficultyFactor}`\n" +
                $"  -  Durability: `{recipe.DurabilityFactor}`\n" +
                $"__**Ingredients:**__\n" +
                $"{ingredients}\n" +
                $"__**Mat Total:**__\n" +
                $"{matCount}\n" +
                $"__**Misc**__\n" +
                $"  -  Can be Advanced Melded: `{canBeAdvMelded}`\n" +
                $"  -  Can be Collectable: {canBeCollectable}\n" +
                $"  -  Can be Converted: `{canBeConverted}`\n" +
                $"  -  Can be Desynthed: `{canBeDesynthed}`\n" +
                $"  -  Can be dyed: `{canBeDyed}`\n" +
                $"  -  Can be Quick Synthed: `{canBeQuickSynthed}`\n" +
                $"__**Description**__\n" +
                $"```{recipe.ResultItem.Description}```\n";
            } else {
                reply = $"__Crafting__\n" +
                $"  -  Craftsmanship: `{recipe.RequiredCraftsmanship}`\n" +
                $"  -  Control: `{recipe.RequiredControl}`\n" +
                $"  -  Difficulty: `{recipe.DifficultyFactor}`\n" +
                $"  -  Durability: `{recipe.DurabilityFactor}`\n" +
                $"__**Ingredients:**__\n" +
                $"{ingredients}\n" +
                $"__**Mat Total:**__\n" +
                $"{matCount}\n" +
                $"__**Misc**__\n" +
                $"  -  Can be Advanced Melded: `{canBeAdvMelded}`\n" +
                $"  -  Can be Collectable: {canBeCollectable}\n" +
                $"  -  Can be Converted: `{canBeConverted}`\n" +
                $"  -  Can be Desynthed: `{canBeDesynthed}`\n" +
                $"  -  Can be dyed: `{canBeDyed}`\n" +
                $"  -  Can be Quick Synthed: `{canBeQuickSynthed}`\n";
            }

            

            Console.WriteLine($"description: {recipe.ResultItem.Description.IsEmpty}");
            var embed = new EmbedBuilder()
                .WithTitle($"{recipe.ResultItem.Name}" + $"- High Quality {canBeHQ}")
                .WithColor(new Color(250, 140, 73))
                .WithThumbnailUrl($"https://secure.xivdb.com/img/game_local/{ recipe.ResultItem.Key.ToString().First() + "/" + recipe.ResultItem.Key}.jpg")
                .WithUrl($"https://xivdb.com/recipe/{recipe.Key}")
                .WithDescription(reply)
                .WithFooter(new EmbedFooterBuilder()
                .WithText(recipe.ClassJob.Name.ToString().ToUpperInvariant() + $" - {recipe.CraftType.Name} - Level {recipe.RecipeLevelTable.CharacterLevel} - Ilvl {recipe.ResultItem.ItemLevel.Key}"))
                .Build();

            

            await ReplyAsync("", embed: embed);
        }


        [Command("Info")]
        [Alias("i")]
        [Remarks("recipe")]
        [Summary("Lists required crafting materials for specified recipe")]
        public async Task RecipeInfo([Remainder] string input) {
            var recipes = _realm.GameData.GetSheet<SaintCoinach.Xiv.Recipe>();

            var recipeResult = from rec in recipes
                               where rec.ResultItem.Name.ToString().ToLower().Contains(input.ToLower())
                               select rec;

            SaintCoinach.Xiv.Recipe recipe;
            try {
                recipe = recipeResult.First();
            } catch {
                await ReplyAsync("That doesn't have a recipe");
                return;
            }

            Dictionary<string, int> matTotals = new Dictionary<string, int>();
            TotalMats(matTotals, recipe);

            string[] matCountArray = new string[matTotals.Count];
            string matCount = "";

            for (int i=0;i<matTotals.Count;i++) {
                matCountArray[i] += $"{matTotals.ElementAt(i).Value}× {matTotals.ElementAt(i).Key}\n";
            }

            Array.Sort(matCountArray, new AlphanumComparatorFast());

            foreach (var keyv in matCountArray) {
                matCount += $" - `{keyv}`\n";
            }

            char canBeHQ = recipe.ResultItem.CanBeHq ? '\u2713' : '×';
            char canBeDesynthed = recipe.ResultItem.IsDesynthesizable ? '\u2713' : '×';
            char canBeConverted = recipe.ResultItem.IsConvertable ? '\u2713' : '×';
            char canBeCollectable = recipe.ResultItem.IsCollectable ? '\u2713' : '×';
            char canBeDyed = recipe.ResultItem.IsDyeable ? '\u2713' : '×';
            char canBeAdvMelded = recipe.ResultItem.IsAdvancedMeldingPermitted ? '\u2713' : '×';
            char canBeQuickSynthed = recipe.CanQuickSynth ? '\u2713' : '×';

            string reply = "";

            if (recipe.ResultItem.ItemLevel != null) {
                reply += $"__**Stats**__\n" +
                $"Ilvl: {recipe.ResultItem.ItemLevel}\n";
            }

            if (!recipe.ResultItem.Description.IsEmpty) {
                reply = $"__**Mat Total:**__\n" +
                $"{matCount}\n" +
                $"__**Misc**__\n" +
                $"  -  Can be Collectable: `{canBeCollectable}`\n" +
                $"  -  Can be Desynthed: `{canBeDesynthed}`\n" +
                $"  -  Can be dyed: `{canBeDyed}`\n" +
                $"__**Description**__\n" +
                $"```{recipe.ResultItem.Description}```\n";
            } else {
                reply = $"__**Mat Total:**__\n" +
                $"{matCount}\n" +
                $"__**Misc**__\n" +
                $"  -  Can be Collectable: `{canBeCollectable}`\n" +
                $"  -  Can be Desynthed: `{canBeDesynthed}`\n" +
                $"  -  Can be dyed: `{canBeDyed}`\n";
            }

            

            var embed = new EmbedBuilder()
                .WithTitle($"{recipe.ResultItem.Name}" + $"- High Quality {canBeHQ}")
                .WithColor(new Color(250, 140, 73))
                .WithThumbnailUrl($"https://secure.xivdb.com/img/game_local/{ recipe.ResultItem.Key.ToString().First() + "/" + recipe.ResultItem.Key}.jpg")
                .WithUrl($"https://xivdb.com/recipe/{recipe.Key}")
                .WithDescription(reply)
                .WithFooter(new EmbedFooterBuilder()
                .WithText(recipe.ClassJob.Name.ToString().ToUpperInvariant() + $" - {recipe.CraftType.Name} - Level {recipe.RecipeLevelTable.CharacterLevel} - Ilvl {recipe.ResultItem.ItemLevel.Key}"))
                .Build();



            await ReplyAsync("", embed: embed);
        }


    }


}