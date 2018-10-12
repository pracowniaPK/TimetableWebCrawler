using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.IO;

namespace TimetableWebCrawler
{
    class Timetable
    {
        public class TimetableItem
        {
            public TimetableItem(string name, string time, string room, string type = "ćw", string teacher = "", string comments = "")
            {
                this.name = name;
                this.type = type;
                this.time = time;
                this.room = room;
                this.teacher = teacher;
                this.comments = comments;
            }

            string name;
            string type;
            string time;
            string room;
            string teacher;
            string comments;

            public override string ToString()
            {
                return name + " (" + type + ")\n" + time + "\n" + room + "\n" + teacher + "\nU: " + comments + "\n------------------------------------------------";
            }
        }

        public Timetable(string header = "")
        {
            week = new List<TimetableItem>[5];
            for (int i = 0; i < 5; i++)
            {
                week[i] = new List<TimetableItem>();
            }
            this.header = header;
        }

        public string header;

        public List<TimetableItem>[] week;

        public override string ToString()
        {
            string result = "";
            result += DateTime.Now.ToString("dd.MM.yyyy HH:mm");
            result += "\n";
            result += header;
            result += "\n------------------------------------------------";
            int counter = 1;
            foreach (var day in week)
            {
                foreach (var item in day)
                {
                    result += "\nZJ_" + counter + ":\n";
                    counter += 1;
                    result += item.ToString();
                }
            }
            result += "\nend.";

            return result;
        }
    }

    class Program
    {
        static string UrlTimetableSource = @"http://arkadiusz_gendek.users.sggw.pl/dzienne/dzienne.html";
        static string TTDirectory = @"E:\tt\";

        static void Main(string[] args)
        {
            List<Timetable> timetables = new List<Timetable>();
            foreach (string url in GetTimetablesUrls(UrlTimetableSource))
            {
                timetables.Add(GetTimetable(url));
            }
            Console.WriteLine("scrapping finished");
            foreach (var timetable in timetables)
            {
                string path = TTDirectory + timetable.header.Replace(",", "").Replace(" ", "").Replace("\\", "").Replace("/", "").Replace("ż","z") + ".txt";
                if (!File.Exists(path)) { var file = File.Create(path); file.Close(); }
                using (var sw = new StreamWriter(path, false))
                {
                    sw.Write(timetable.ToString().Replace("\n", Environment.NewLine));
                }
            }
            Console.WriteLine("files saved");

            Console.WriteLine("\npress any key to exit");
            Console.ReadKey();
        }

        static List<string> GetTimetablesUrls(string pageUrl)
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

        static Timetable GetTimetable(string pageUrl)
        {
            Timetable result;

            HtmlWeb web = new HtmlWeb() { AutoDetectEncoding = false, OverrideEncoding = Encoding.GetEncoding("iso-8859-2") };
            var htmlDoc = web.Load(pageUrl);
            var rows = htmlDoc.DocumentNode.SelectNodes("//tr");
            rows.RemoveAt(0);

            var body = htmlDoc.DocumentNode.SelectNodes("//body");
            string[] stuff = Regex.Split(body[0].InnerHtml, "<br>");
            string[] line1 = stuff[0].Replace("\n", "").Split(' ');
            // header definition !!
            string header = "WIP, 2018, Zima, ST, ";
            if (line1.Length == 5)
            {
                header += line1[3] + line1[4] + ", ";
            } else
            {
                header += line1[3] + ", ";
            }
            header += "inż, ";
            if (line1[2] == "s.1.")
                header += "R1, S1, ";
            else if (line1[2] == "s.3.")
                header += "R2, S3, ";
            else if (line1[2] == "s.5.")
                header += "R3, S5, ";
            else if (line1[2] == "s.6.")
                header += "R4, S7, ";
            header += ("gr" + body[0].InnerText[2]);
            result = new Timetable(header);

            foreach (var row in rows)
            {
                for (int i = 0; i < 5; i++)
                {
                    var cell = row.SelectNodes("th|td")[i + 1];
                    if (cell.InnerText != "&nbsp")
                    {
                        Timetable.TimetableItem item;
                        string[] lines = Regex.Split(cell.InnerHtml, "<br>");
                        string name = lines[0];
                        string time = row.SelectNodes("th|td")[0].InnerText.Replace(" ", "");
                        time = "d" + (i+1) + ", " + time;
                        if (lines.Length == 2) {
                            item = new Timetable.TimetableItem(name, time, lines[1]);
                        } else if(lines.Length == 3) {
                            item = new Timetable.TimetableItem(name, time, lines[2], comments: lines[1]);
                        } else if(lines.Length == 4) {
                            item = new Timetable.TimetableItem(name, time, lines[2] + " " + lines[3], comments: lines[1]);
                        } else {
                            item = new Timetable.TimetableItem(name, time, "");
                        }
                        result.week[i].Add(item);
                    }
                }
            }

            return result;
        }
    }
}
