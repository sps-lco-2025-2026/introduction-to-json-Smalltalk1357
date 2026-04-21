using System.Net.ServerSentEvents;
using Newtonsoft.Json.Linq;
using FuzzySharp;

namespace JsonIntroduction.App
{
    public class Pokemon
    {
        public static void Run()
        {
            bool repeat = true;
            int index = 25;
            while (repeat)
                (index, repeat) = GetIndex();
            
            JObject jsonObject = GetJsonObject($"pokemon/{index}");
            
            Console.Clear();
            
            // Basic information
            string name = Capitalise(jsonObject["name"]!.ToString());
            Console.WriteLine($"---------- Pokemon #{index}: {name} ----------");
            Console.WriteLine($"Height: {jsonObject["height"]}");
            Console.WriteLine($"Weight: {jsonObject["weight"]}");
            Console.WriteLine($"Type: {string.Join(", ", UnpackTypes(jsonObject["types"]))}");
            
            // Main stats
            Console.WriteLine("\nBase Stats:");
            List<int> stats = UnpackStats(jsonObject["stats"]);
            string[] statName = ["HP", "Attack", "Defense", "Special Attack", "Special Defense", "Speed"];
            for (int i = 0; i < stats.Count; i++)
            {
                // maximum stat = 255, scales chart down to 30 chars
                int stat = stats[i];
                Console.WriteLine($"{statName[i], 15}|{string.Concat(Enumerable.Repeat("▋", 30 * stat / 255)), -30}|{stat}");
            }
            Console.WriteLine(string.Concat(Enumerable.Repeat("-", 50)));
            int total = stats.Sum();
            Console.WriteLine($"{"Total", 15}|{string.Concat(Enumerable.Repeat("▋", 30 * total / 800)), -30}|{total}");
            
            // Abilities
            Console.WriteLine("\nAbilities:");
            List<Tuple<string, string>> abilities = UnpackAbilities(jsonObject["abilities"]);
            for (int i = 0; i < abilities.Count; i++)
            {
                Console.WriteLine($"{i+1}: {abilities[i].Item1} (Slots: {abilities[i].Item2})");
            }
            
            // Moves
            Console.Write("\nView moves? (y/n): ");
            if (Console.ReadLine() == "y")
            {
                List<(string, int)> moves = UnpackMoves(jsonObject["moves"]);
                Console.WriteLine($"\nMoves (Based off Pokemon Red-Blue): (Count: {moves.Count})");

                for (int i = 0; i < moves.Count; i++)
                {
                    Console.WriteLine($"{i + 1,3}: {moves[i].Item1,-20} (Learnt at level {moves[i].Item2})");
                }
            }
        }

        private static JObject GetJsonObject(string url)
        {
            url = $"https://pokeapi.co/api/v2/{url}";

            string jsonDownload;
            using (HttpClient client = new())
                jsonDownload = client.GetStringAsync(url).Result;
            JObject o = JObject.Parse(jsonDownload);

            return o;
        }
        
        // returns index of pokemon and whether it needs to be repeated (instead of recursion)
        private static (int, bool) GetIndex()
        {
            JObject o = GetJsonObject("pokemon-species?limit=10000&offset=0");
            int count = (int)o["count"]!;

            List<string> species = File.Exists("Species.txt") ? File.ReadAllLines("Species.txt").ToList() : [];
            if (species.Count != count || species.Count == 0)
            {
                species.AddRange(o["results"]!.Select(token => (string)token["name"]!));
                File.WriteAllLines("Species.txt", species);
            }

            Console.WriteLine($"Current total number of pokemon: {count}");
            Console.Write("Enter pokemon name or ID (enter for random): ");

            string input = Console.ReadLine()!.ToLower();
            
            Random r = new();
            int rNum = r.Next(0, count) + 1;
            
            if (input == "") // Random
            {
                return (rNum, false);
            }
            
            if (int.TryParse(input, out int index)) // Search by ID
            {
                if (index > 0 && index <= count)
                    return (index, false);
            }
            
            if (species.Contains(input)) // Search by name
            {
                index = species.IndexOf(input) + 1;
                return (index, false);
            }

            // Fuzzy search by name - uses Levenshtein distance (via FuzzySharp)
            List<(string, int)> results = [];
            foreach (string name in species)
            {
                int score = Fuzz.PartialRatio(input, name);
                if (score > 70) 
                    results.Add((name, score));
            }
            results.Sort((n, s) => s.Item2.CompareTo(n.Item2));

            if (results.Count == 0)
            {
                Console.WriteLine("Not matches found - try again.");
                return (0, true);
            }
            
            int num = results.Count < 5 ? results.Count : 5;
            Console.WriteLine("Not found. Similar matches:");
            for (int i = 0; i < num; i++)
            {
                Console.WriteLine($"{Capitalise(results[i].Item1)}");
            }
            Console.WriteLine();
            
            return (0, true);
        }
        
        // To unpack the lists of a Pokémon's stats requires knowledge of how the JSON is structured

        private static List<Tuple<string, string>> UnpackAbilities(JToken? lists)
        {
            List<Tuple<string, string>> output = new();
            
            foreach (JToken temp in lists!)
            {
                string ability = temp["ability"]!["name"]!.ToString();
                string slots = temp["slot"]!.ToString();
                
                output.Add(new(Capitalise(ability), slots));
            }
            return output;
        }

        private static List<int> UnpackStats(JToken? lists)
        {
            List<int> output = [];
            
            foreach (JToken temp in lists!)
            {
                int baseStat = (int)temp["base_stat"]!;
                output.Add(baseStat);
            }
            
            return output;
        }

        private static List<string> UnpackTypes(JToken? lists)
        {
            List<string> output = [];

            foreach (JToken temp in lists!)
            {
                string type = temp["type"]!["name"]!.ToString();
                output.Add(Capitalise(type));
            }

            return output;
        }

        private static List<(string, int)> UnpackMoves(JToken? lists)
        {
            List<(string, int)> output = [];
            
            foreach (JToken temp in lists!)
            {
                string move = temp["move"]!["name"]!.ToString();
                int levelLearnt = (int)temp["version_group_details"]![0]!["level_learned_at"]!;
                output.Add((Capitalise(move), levelLearnt));
            }
            
            return output;
        }
        
        private static string Capitalise(string word) => char.ToUpper(word[0]) + word[1..];
        
    }
}
