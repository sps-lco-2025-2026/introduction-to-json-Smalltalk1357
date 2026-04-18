using System.Net.ServerSentEvents;
using Newtonsoft.Json.Linq;

namespace JsonIntroduction.App
{
    public class Pokemon
    {
        public static void Run()
        {
            (int index, bool random) = GetIndex();
            JObject jsonObject = GetJsonObject($"pokemon/{index}");
            
            Console.Clear();
            
            string name = Capitalise(jsonObject["name"]!.ToString());
            Console.WriteLine($"---------- Pokemon #{index}: {name} ----------");
            Console.WriteLine($"Height: {jsonObject["height"]}");
            Console.WriteLine($"Weight: {jsonObject["weight"]}");
            Console.WriteLine($"Type: {string.Join(", ", UnpackTypes(jsonObject["types"]))}");
            
            Console.WriteLine("\nMain Stats:");
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
            
            Console.WriteLine("\nAbilities:");
            List<Tuple<string, string>> abilities = UnpackAbilities(jsonObject["abilities"]);
            for (int i = 0; i < abilities.Count; i++)
            {
                Console.WriteLine($"{i+1}: {abilities[i].Item1} (Slots: {abilities[i].Item2})");
            }

            if (random)
            {
                Console.WriteLine("\nRandomly selected Pokemon. If not correct pokemon, check spelling/ID");
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
            if (int.TryParse(input, out int index))
            {
                if (index > 0 && index <= count)
                    return (index, false);
            }
            if (species.Contains(input))
            {
                index = species.IndexOf(input) + 1;
                return (index, false);
            }
            Random r = new();
            index = r.Next(0, count) + 1;
            return (index, false);
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
            List<int> output = new();
            
            foreach (JToken temp in lists!)
            {
                int baseStat = (int)temp["base_stat"]!;
                output.Add(baseStat);
            }
            
            return output;
        }

        private static List<string> UnpackTypes(JToken? lists)
        {
            List<string> output = new();

            foreach (JToken temp in lists!)
            {
                string type = temp["type"]!["name"]!.ToString();
                output.Add(Capitalise(type));
            }

            return output;
        }
        
        private static string Capitalise(string word) => char.ToUpper(word[0]) + word[1..];
        
    }
}
