using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimetableWebCrawler
{
    class Program
    {
        static string UrlTimetableSource = @"http://arkadiusz_gendek.users.sggw.pl/dzienne/dzienne.html";

        static void Main(string[] args)
        {
            foreach (string url in GetTimetablesUrls(UrlTimetableSource))
            {
                Console.WriteLine(url);
            }



            Console.WriteLine("\npress any key to exit");
            Console.ReadKey();
        }

        private static List<string> GetTimetablesUrls(string pageUrl)
        {
            List<string> result = new List<string>();

            HtmlWeb web = new HtmlWeb();
            var htmlDoc = web.Load(pageUrl);
            var nodes = htmlDoc.DocumentNode.SelectNodes("//a[@href]");
            nodes.RemoveAt(nodes.Count - 1);

            string[] parts = pageUrl.Split('/');
            Array.Resize(ref parts, parts.Length - 1);
            string rootUrl = String.Join("/", parts) + "/";

            foreach (var node in nodes)
            {
                result.Add(rootUrl + node.Attributes["href"].Value);
            }

            return result;
        }
    }
}
