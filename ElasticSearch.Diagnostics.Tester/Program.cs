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
