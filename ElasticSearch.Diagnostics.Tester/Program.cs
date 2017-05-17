using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace ElasticSearch.Diagnostics.Tester
{
    class Stuff
    {
        public int sdghhhhfghfh;
        public string kjdfvjkfnv;
        public long kdsflsjklkg;
        public DateTime ddd;
    }


    class Program
    {
        private static readonly TraceSource _traceSource = new TraceSource("alextrace", SourceLevels.Error);
        static void Main(string[] args)
        {
	        var ub = new UriBuilder("http://rb-es.northcentralus.cloudapp.azure.com:9200");
	        ub.UserName = HttpUtility.UrlEncode("elastic");
	        ub.Password = HttpUtility.UrlEncode("sY,H]J#nG9/hB4}4");

			Console.WriteLine(ub.Uri);





            var s = new Stuff()
            {
                kdsflsjklkg = 83465983,
                kjdfvjkfnv = "sdgghshshhdhdhdhdh",
                sdghhhhfghfh = 0,
                 ddd = DateTime.Now
            };

            Trace.Write(s);

            Trace.Write(DateTime.UtcNow);




            try
            {
                Test();
            }
            catch (Exception ex)
            {
                Trace.Write(ex);
            }


            try
            {
                using (weatherService.WeatherSoapClient client = new weatherService.WeatherSoapClient())
                {
                    var x = client.GetCityWeatherByZIP("92691");
                }
            }
            catch (Exception ex)
            {
                _traceSource.TraceData(TraceEventType.Error, 99999, ex);

                //throw;
            }




            try
            {
                int x = 0;
                int y = 200 / x;

            }
            catch (Exception ex)
            {
                _traceSource.TraceData(TraceEventType.Error, 119999911, ex);
                //throw;
            }


			//log a ALOT of entries for a preformance test
			Stopwatch sw = Stopwatch.StartNew();
	        for (int i = 0; i < 50000; i++)
	        {
		        _traceSource.TraceEvent( TraceEventType.Information, i, $"an event happened! at {sw.ElapsedMilliseconds}ms");
	        }
			sw.Stop();
	        _traceSource.TraceEvent(TraceEventType.Information, 0, $"total in program time for {5000} evnts tooks {sw.ElapsedMilliseconds}ms");


			Console.ReadLine();
        }


        public static void Test()
        {
            try
            {
                Trace.Write("Entering");

                throw new HttpException(404, "Not Found!");


            }
            catch (Exception ex)
            {
                _traceSource.TraceData(TraceEventType.Error, 99999, ex);
                //throw;
            }

            try
            {
                throw new HttpParseException("Not Found!");

            }
            catch (Exception ex)
            {
                _traceSource.TraceData(TraceEventType.Error, 99999, ex);
                //throw;
            }

            Trace.Write("Exiting");
        }
    }
}
