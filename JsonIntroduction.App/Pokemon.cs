using Newtonsoft.Json.Linq;

namespace JsonIntroduction.App
{
    public class Pokemon
    {
        public static void Run()
        {
            string index = GetIndex();
            string url = $"https://pokeapi.co/api/v2/pokemon/{index}/";
            
            string jsonDownload;
            using (HttpClient client = new())
                jsonDownload = client.GetStringAsync(url).Result;
            JObject o = JObject.Parse(jsonDownload);
            
            Dictionary<string, JToken?> attributes = new();
            Dictionary<string, JToken?> lists = new();
            string[] wantedAttributes = ["name", "id", "height", "weight"];
            string[] wantedLists = ["stats", "moves", "abilities"];
            
            foreach (KeyValuePair<string, JToken?> item in o)
            {
                if (wantedAttributes.Contains(item.Key))
                    attributes[item.Key] = item.Value;
                if (wantedLists.Contains(item.Key))
                    lists[item.Key] = item.Value;
            }

            foreach (KeyValuePair<string, JToken?> item in attributes)
            {
                Console.WriteLine($"{item.Key}: {item.Value}");
            }
            
        }

        private static string GetIndex()
        {
            while (true)
            {
                Console.Write("Enter pokemon number: ");
                string input = Console.ReadLine()!;
                if (int.TryParse(input, out int index))
                {
                    if (index is > 0 and < 1026)
                        return input;
                }
                Console.WriteLine("Invalid input\n\n");
            }
        }
    }
}
