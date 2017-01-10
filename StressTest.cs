using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;

namespace TestGoogleSearch
{
    class Program
    {

        static void Main()
        {

            var query = GetGoogleResults("cape techs 63701");

            //Normally it would be query = await GetGoogleResults(...); but since its in static void Main() it won't let you use async methods :(
            Task.WaitAll(query);

            foreach (var result in query.Result)
            {
                Console.WriteLine(result);
            }

            Console.WriteLine("\r\n----Push Any Key To Start Stress Test----");
            Console.ReadKey();

            Console.Clear();
            Console.WriteLine("----Stress Test----");
            var tasks = new List<Task<List<string>>>();
            for (int i = 0; i < 100; i++)
            {
                var qu = GetGoogleResults("cape techs");
                tasks.Add(qu);

            }


            Task.WaitAll(tasks.ToArray());

            foreach (var task in tasks)
            {
                //Console.WriteLine("Returned {0} results in query", task.Result.Count);
                foreach(var res in task.Result)
                {
                    Console.WriteLine(res);
                }
            }

            Console.ReadKey();
        }


        static async Task<List<string>> GetGoogleResults(string whatToSearch)
        {
            var results = new List<string>();
            whatToSearch = HttpUtility.HtmlEncode(whatToSearch);
            using (var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Accept", "text/html,application/xhtml+xml,application/xml");
                httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Accept-Encoding", "gzip, deflate");
                httpClient.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "Mozilla/5.0 (Windows NT 6.2; WOW64; rv:19.0) Gecko/20100101 Firefox/19.0");
                httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Accept-Charset", "ISO-8859-1");

                var response = await httpClient.GetAsync(new Uri("https://www.google.com/search?q=" + whatToSearch));

                response.EnsureSuccessStatusCode();
                using (var responseStream = await response.Content.ReadAsStreamAsync())
                using (var decompressedStream = new GZipStream(responseStream, CompressionMode.Decompress))
                using (var streamReader = new StreamReader(decompressedStream))
                {
                    string result = streamReader.ReadToEnd();

                    var htmlDocument = new HtmlDocument();
                    htmlDocument.LoadHtml(result);


                    var origin = htmlDocument.DocumentNode.SelectSingleNode("//div[@id='ires']");

                    var n1 = origin.SelectSingleNode("//div[@class='rc']");
                    var n2 = n1.SelectSingleNode("//h3[@class='r']");

                    foreach (var node3 in n2.SelectNodes("//a").Where(x => x.Attributes.Contains("href")))
                    {
                        var res = node3.Attributes["href"].Value;
                        if (res.Contains("http://") && !res.Contains("google.com") && !res.Contains("googleusercontent.com"))
                        {
                            results.Add(res);
                        }
                    }          
                }
                return results;
            }

        }

    }




}
