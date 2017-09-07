using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Wox.Plugin;

namespace WoxPluginKeyboardShortcuts
{
    public class Main : IPlugin
    {
        public void Init(PluginInitContext context)
        {
            
        }
        
        public List<Result> Query(Query query)
        {
            List<Result> results = new List<Result>();
            
            // if user entered only one query word, it should be the invocation word for the plugin
            if (query.Terms.Length < 2)
            {
                return new List<Result>();
            }
            // if user entered only one query word
            else if (query.Terms.Length <= 2)
            {
                var dataFiles = from file in Directory.GetFiles(@"Plugins\WoxPluginKeyboardShortcuts\Data\", "*" + query.Terms[1] + "*.json")
                            select new FileInfo(file);

                foreach (FileInfo dataFile in dataFiles)
                {
                    var result = new Result
                    {
                        Title = Path.GetFileNameWithoutExtension(dataFile.Name),
                        SubTitle = "Search its shortcuts",
                        IcoPath = @"Images\" + Path.GetFileNameWithoutExtension(dataFile.Name) + ".png"
                    };

                    results.Add(result);
                }
            }
            // if user entered more than one query word
            else
            {
                var dataFiles = from file in Directory.GetFiles(@"Plugins\WoxPluginKeyboardShortcuts\Data\", "*" + query.Terms[1] + "*.json")
                                select new FileInfo(file);

                foreach (FileInfo dataFile in dataFiles)
                {
                    using (StreamReader reader = File.OpenText(dataFile.FullName))
                    {
                        JObject o = (JObject)JToken.ReadFrom(new JsonTextReader(reader));
                        
                        var selected = o.SelectTokens("$.sections..*[?(@.val)]")
                            .Select(s => JsonConvert.DeserializeObject<Shortcut>(s.ToString()))
                            .Where(s => s.val.ToLower().Contains(query.SecondToEndSearch.ToLower()))
                            .ToList();
                        
                        foreach (Shortcut sc in selected)
                        {
                            var result = new Result
                            {
                                Title = Path.GetFileNameWithoutExtension(dataFile.Name) + ": " + sc.val + ": " + sc.key,
                                SubTitle = sc.key,
                                IcoPath = @"Images\" + Path.GetFileNameWithoutExtension(dataFile.Name) + ".png"
                            };

                            results.Add(result);
                        }
                    }
                }
                
            }

            return results;
        }
    }
}
