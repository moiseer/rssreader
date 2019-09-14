using System.Collections.Generic;

namespace RssReader
{
    public class Settings
    {
        public List<string> Channels { get; set; }

        public Settings()
        {
            Channels = new List<string>();
        }
    }
}
