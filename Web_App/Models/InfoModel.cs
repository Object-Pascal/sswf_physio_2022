using System.Collections.Generic;

namespace Web_App.Models
{
    public class InfoModel
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string ReturnAction { get; set; }
        public string ReturnController { get; set; }
        public bool HasImportantData { get; set; }
        public Dictionary<string, string> TableEntries { get; set; }

        public InfoModel()
        {
            HasImportantData = false;
            TableEntries = new Dictionary<string, string>();
        }
    }
}