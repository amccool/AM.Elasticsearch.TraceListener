using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Xunit.Abstractions;
using Xunit;
using System.Net;
using System.Security.Principal;
using AM.Elasticsearch.TraceListener;

namespace ElasticSearch.Diagnostics.Tests
{
    public class ESTLUnitTests
    {
        private readonly ITestOutputHelper output;

        public ESTLUnitTests(ITestOutputHelper output)
        {
            this.output = output;
        }

        [Fact]
        public void SimpleWrite()
        {
            var x = new ElasticSearchTraceListener("tester");
            x.ElasticSearchUri = "http://192.168.204.198:9200";
            x.ElasticSearchTraceIndex = "trace";

            x.Write(4);
        }

        [Fact]
        public void WriteObjectTest()
        {
            var x = new ElasticSearchTraceListener("tester");
            x.ElasticSearchUri = "http://192.168.204.198:9200";
            x.ElasticSearchTraceIndex = "trace";


            x.Write(new
            {
                thing = "ggg",
                morethings = 11111,
                anotherthing = "yyyy"
            });
        }

        [Fact]
        public void WriteExceptionest()
        {
            var x = new ElasticSearchTraceListener("tester");
            x.ElasticSearchUri = "http://192.168.204.198:9200";
            x.ElasticSearchTraceIndex = "trace";

            try
            {
                var n = 0;
                var y = 100000 / n;
            }
            catch (Exception ex)
            {
                x.Write(ex);
                //dont throw we want to see that the exception got writ
                //throw;
            }

        }





        [Fact]
        public void ALOTofExmsgs()
        {
            var x = new ElasticSearchTraceListener("tester");
            x.ElasticSearchUri = "http://192.168.204.198:9200";
            x.ElasticSearchTraceIndex = "trace";

            for (int i = 0; i < 100; i++)
            {
                x.Write(new Exception());
            }

            x.Flush();
        }

        [Fact]
        public void WriteManySimpleStringsTest()
        {
            var x = new ElasticSearchTraceListener("tester");
            x.ElasticSearchUri = "http://192.168.204.198:9200";
            x.ElasticSearchTraceIndex = "trace";

            for (int i = 0; i < 10; i++)
            {
                x.Write("xxxxx" + i);
            }
            x.Flush();
        }

        [Fact]
        public void TraceSourceManySimpleStringsTest()
        {
            var x = new ElasticSearchTraceListener("tester");
            x.ElasticSearchUri = "http://192.168.204.198:9200";
            x.ElasticSearchTraceIndex = "trace";

            var ts = new TraceSource("x", SourceLevels.All);
            ts.Listeners.Add(x);

            for (int i = 0; i < 10; i++)
            {
                ts.TraceInformation("xxxxx" + i);
            }
            x.Flush();

        }

        [Fact]
        public void TSTestTimeIds()
        {
            var x = new ElasticSearchTraceListener("tester");
            x.ElasticSearchUri = "http://192.168.204.198:9200";
            x.ElasticSearchTraceIndex = "trace";

            var ts = new TraceSource("x", SourceLevels.All);
            ts.Listeners.Add(x);

            for (int i = 0; i < 10000; i++)
            {
                ts.TraceEvent(TraceEventType.Error, 1000, DateTime.Now.ToString());
            }
            x.Flush();

        }

        [Fact]
        public void TSManyWriteExceptionsTest()
        {
            var x = new ElasticSearchTraceListener("tester");
            x.ElasticSearchUri = "http://192.168.204.198:9200";
            x.ElasticSearchTraceIndex = "trace";

            var ts = new TraceSource("exxxxx", SourceLevels.All);
            ts.Listeners.Add(x);

            try
            {
                var n = 0;
                var y = 100000 / n;
            }
            catch (Exception ex)
            {
                for (int i = 0; i < 10000; i++)
                {
                    ts.TraceData(TraceEventType.Error, 99, ex);
                }
            }
            x.Flush();
        }

        [Fact]
        public void TraceDataWithString()
        {
            var x = new ElasticSearchTraceListener("tester");
            x.ElasticSearchUri = "http://192.168.204.198:9200";
            x.ElasticSearchTraceIndex = "trace";

            var ts = new TraceSource("exxxxx", SourceLevels.All);
            ts.Listeners.Add(x);

            ts.TraceData(TraceEventType.Error, 99, "ggggggggggggggggg");

            x.Flush();
        }




        public class Junk
        {
            public IPAddress ipaddr { get; set; }
        }

        [Fact]
        public void CauseFailedSerialization()
        {
            var x = new ElasticSearchTraceListener("tester");
            x.ElasticSearchUri = "http://192.168.204.198:9200";
            x.ElasticSearchTraceIndex = "trace";

            var ts = new TraceSource("exxxxx", SourceLevels.All);
            ts.Listeners.Add(x);

            for (int i = 0; i < 50; i++)
            {

            ts.TraceData(TraceEventType.Information, 99, new Junk()
            {
                ipaddr = IPAddress.Parse("1.1.1.1")
            });
            }

            ts.Flush();
        }

        //[Fact]
        //public void IdentityTest()
        //{
        //    IPrincipal principal = Thread.CurrentPrincipal;
        //    IIdentity identity = principal == null ? null : principal.Identity;
        //    string name = identity == null ? "" : identity.Name;

        //    Assert.False(string.IsNullOrWhiteSpace(name));
        //}

        [Fact]
        public void UserNameTest()
        {
            string name = Environment.UserDomainName + "\\" + Environment.UserName;

            this.output.WriteLine(name);

            Assert.False(string.IsNullOrWhiteSpace(name));
        }


    }
}
