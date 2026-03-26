using Newtonsoft.Json.Linq;

namespace JsonIntroduction.App
{
    public class Pokemon
    {
        public static void Run()
        {
            string jsonDownload;
            using (HttpClient client = new())
                jsonDownload = client.GetStringAsync("https://pokeapi.co/api/v2/pokemon-species/25/").Result;
            
            JObject o = JObject.Parse(jsonDownload);

            foreach (KeyValuePair<string, JToken?> item in o)
            {
                if (item.Key != "flavor_text_entries" || item.Key != "names")
                {
                    if (item.Value!.Type == JTokenType.Array)
                    {
                        Console.WriteLine(item.Key.Replace('_', ' ') + ": ");
                        foreach (JToken jt in item.Value)
                        {
                            Console.WriteLine(jt);
                        }
                    }
                    else if (item.Value.Type == JTokenType.Object)
                    {
                        // Console.WriteLine(item.Key.Replace('_', ' ') + ": ");
                        // foreach (KeyValuePair<string, JToken> kvp in item.Value)
                        // {
                        //     Console.WriteLine(kvp.Value);
                        // }
                    }
                    else
                    {
                        Console.WriteLine(item.Key.Replace('_', ' ') + ": " + item.Value);
                    }
                }
            }
        }
    }
}
