using Newtonsoft.Json.Linq;

namespace JsonIntroduction.App
{
    public class Pokemon
    {
        public static void Run()
        {
            string index = GetIndex();
            (Dictionary<string, string> attributes, Dictionary<string, JToken?> lists) = GetFormattedJson(index);
            
            attributes["name"] = Capitalise(attributes["name"]);
            Console.WriteLine($"\n---------- Pokemon #{index}: {attributes["name"]} ----------");
            Console.WriteLine($"Height: {attributes["height"]}");
            Console.WriteLine($"Weight: {attributes["weight"]}");
            
            Console.WriteLine("\nStatistics:");
            List<Tuple<string, string>> stat = UnpackStats(lists["abilities"]);
            for (int i = 0; i < stat.Count; i++)
            {
                Console.WriteLine($"{stat[i].Item1}: {stat[i].Item2}");
            }
            
            Console.WriteLine("\nAbilities:");
            List<Tuple<string, string>> abilities = UnpackAbilities(lists["abilities"]);
            for (int i = 0; i < abilities.Count; i++)
            {
                Console.WriteLine($"{i+1}: {abilities[i].Item1} (Slots: {abilities[i].Item2})");
            }
        }

        private static Tuple<Dictionary<string, string>, Dictionary<string, JToken?>> GetFormattedJson(string index)
        {
            string url = $"https://pokeapi.co/api/v2/pokemon/{index}/";

            string jsonDownload;
            using (HttpClient client = new())
                jsonDownload = client.GetStringAsync(url).Result;
            JObject o = JObject.Parse(jsonDownload);
            
            Dictionary<string, string> attributes = new();
            Dictionary<string, JToken?> lists = new();
            string[] wantedAttributes = ["name", "height", "weight"];
            string[] wantedLists = ["stats", "moves", "abilities", "types"];
            
            foreach (KeyValuePair<string, JToken?> item in o)
            {
                if (wantedAttributes.Contains(item.Key))
                    attributes[item.Key] = item.Value!.ToString();
                if (wantedLists.Contains(item.Key))
                    lists[item.Key] = item.Value;
            }
            
            return new Tuple<Dictionary<string, string>, Dictionary<string, JToken?>>(attributes, lists);
        }

        private static string GetIndex()
        {
            Console.Write("Enter pokemon number (Return for random): ");
            string input = Console.ReadLine()!;
            if (int.TryParse(input, out int index))
            {
                if (index is > 0 and <= 1350)
                    return input;
            }
            Random r = new();
            input = r.Next(1, 1026).ToString();
            Console.WriteLine($"Choosing random pokemon number: {input}");
            return input;
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

        private static List<Tuple<string, string>> UnpackStats(JToken? lists)
        {
            List<Tuple<string, string>> output = new();
            
            foreach (JToken temp in lists!)
            {
                string stat = temp["stat"]!["name"]!.ToString();
                string baseStat = temp["base_stat"]!.ToString();
                
                output.Add(new(Capitalise(stat), baseStat));
            }
            
            return output;
        }
        
        private static string Capitalise(string word) => char.ToUpper(word[0]) + word[1..];
        
    }
}
