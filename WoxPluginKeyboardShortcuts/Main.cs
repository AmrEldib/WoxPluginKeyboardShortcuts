using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Wox.Plugin;

namespace WoxPluginKeyboardShortcuts
{
    public class Main : IPlugin
    {
        private string _PluginFolder;
        
        public string PluginFolder
        {
            get { return _PluginFolder; }
            set { _PluginFolder = value; }
        }
        
        public string ImageFolder
        {
            get { return @"Images\"; }
        }

        public string DataFolder    
        {
            get { return Path.Combine(this.PluginFolder, @"Data\"); }
        }
        
        public void Init(PluginInitContext context)
        {
            this.PluginFolder = context.CurrentPluginMetadata.PluginDirectory;
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
                
                var dataFiles = from file in Directory.GetFiles(this.DataFolder, "*" + query.Terms[1] + "*.json")
                            select new FileInfo(file);

                foreach (FileInfo dataFile in dataFiles)
                {
                    var result = new Result
                    {
                        Title = Path.GetFileNameWithoutExtension(dataFile.Name),
                        SubTitle = "Search its shortcuts",
                        IcoPath = this.ImageFolder + Path.GetFileNameWithoutExtension(dataFile.Name) + ".png"
                    };

                    results.Add(result);
                }
            }
            // if user entered more than one query word
            else
            {
                var dataFiles = from file in Directory.GetFiles(this.DataFolder, "*" + query.Terms[1] + "*.json")
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
                                Title = sc.key,
                                SubTitle = Path.GetFileNameWithoutExtension(dataFile.Name) + ": " + sc.val,
                                IcoPath = this.ImageFolder + Path.GetFileNameWithoutExtension(dataFile.Name) + ".png"
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
