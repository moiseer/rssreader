using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RssReader
{
    class Program
    {
        static void Main(string[] args)
        {
            RssReader rssReader = new RssReader();
            rssReader.StartMessage();
            rssReader.Run();
        }
    }
}
