using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using System.Net;

// JSON BASICS - from a string 
// A JSON file starts with either
// - a square bracket, indicating a list,
// - or a curly bracket, indicating a dictionary or object
string json = @"{""key1"":""value1"",""key2"":""value2""}";

// a curly bracket could just be a simple collection matching keys to values, both strings 
IDictionary<string, string> values = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);

foreach (string k in values.Keys)
    Console.WriteLine($"{k}: {values[k]}");


// step two - a dictionary from a text file 
// if the JSON isn't just as simple as string-string we can parse it with the JObject.Parse method 
// and convert with the ToObject types 
JObject o1 = JObject.Parse(File.ReadAllText("/Users/alexwan/Projects/introduction-to-json-Smalltalk1357/JsonIntroduction.app/data.json"));
Debug.Assert("json" == o1["name"].ToObject<string>());
Debug.Assert(o1["entry"].ToObject<bool>());
Debug.Assert(7 == o1["value"].ToObject<int>());

PressAKey();

// step - automatically unpack an object from a json dictionary 
// e.g. https://www.newtonsoft.com/json/help/html/DeserializeObject.htm 
string json2 = @"{
'Email': 'james@example.com',
'Active': true,
'CreatedDate': '2021-01-20T00:00:00Z',
'Roles': [
    'User',
    'Admin'
]
}";

// if all the keys map to an object that already exists (note that these need to have public setters) 
// this is safest when we control the JSON, so we are confident that it won't change 
Account account = JsonConvert.DeserializeObject<Account>(json2);

Console.WriteLine(account.Email);

Account a2 = new Account("a@b.com", false, DateTime.UtcNow, new List<string>{"User"});
string j3 = JsonConvert.SerializeObject(a2);
Console.WriteLine(j3);

PressAKey();

string msg = "Downloading Data";

Console.Write(msg);
// step four - downloading data from a web URI
// For examples, this JSON example is a list of objects.
// If you click on the list you'll see that it is actually a list with two items, the first item is an object, the second actually another list 
string jsonDownload;
using(var wc = new WebClient())
{
    jsonDownload = wc.DownloadString("http://api.worldbank.org/v2/countries/USA/indicators/NY.GDP.MKTP.CD?per_page=5000&format=json");

}
Console.Write($"\r{"".PadLeft(msg.Length+1, ' ')}\r");

// because it's an object we can parse it with JArray - this will return a collection. 
// Using the dynamic keyword allows us to wave our hands a little.
dynamic j = JArray.Parse(jsonDownload);

// so we ignore the first item and the second item is assumed to be a list. 
// we choose to use dynamic because the underlying JSON is too complex (or liable to change) 
// unless something has gone wrong, let's grab the second item from the list and treat it like a collection 
dynamic l = j[1];
Console.WriteLine($"{l.Count} years of data");

// the dynamic allows us to make assumptions about the properties on the object. 
foreach(dynamic row in l)
{
    // note that we can treat row like a dictionary, with [], or as an object, with .property 
    Console.WriteLine($"{row["date"]}: {row.countryiso3code}: ${row.value / 1000000000:F3}bn");
}

PressAKey();
Console.Write(msg);

// this next feed though, is not a collection, but a {} so we should use the JObject.Parse method 
for (int i = 0; i < 10; ++i)
{
    string nasaDownload;
    using (var wc = new WebClient())
    {
        nasaDownload = wc.DownloadString("http://api.open-notify.org/iss-now.json");
    }
    if(i == 0)
        Console.Write($"\r{"".PadLeft(msg.Length + 1, ' ')}\r");

    dynamic o = JObject.Parse(nasaDownload);
    // https://stackoverflow.com/questions/249760/how-can-i-convert-a-unix-timestamp-to-datetime-and-vice-versa 
    DateTime dt = DateTimeOffset.FromUnixTimeSeconds((long)o.timestamp).LocalDateTime;
    Console.WriteLine($"{dt:T} : ({o.iss_position.latitude}, {o.iss_position.longitude})");
    Thread.Sleep(2000);
}

// and relax


void PressAKey()
{
    Console.WriteLine("\nPress a key");
    Console.Read();
}

public class Account
{
    string _email;
    bool _active;
    DateTime _created;
    IList<string> _roles;

    public string Email { get => _email; set => _email = value; }
    public bool Active { get => _active; set => _active = value; }
    public DateTime CreatedDate { get => _created; set => _created = value; }
    public IList<string> Roles { get => _roles; set => _roles = value; }

    public Account(string e, bool a, DateTime c, IList<string> r)
    {
        _email = e;
        _active = a;
        _created = c;
        _roles = r;
    }
}
