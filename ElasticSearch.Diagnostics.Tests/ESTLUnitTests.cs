using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Xunit.Abstractions;
using Xunit;
using System.Net;
using System.Security.Principal;

namespace ElasticSearch.Diagnostics.Tests
{
    public class ESTLUnitTests
    {
        private readonly ITestOutputHelper output;

        public ESTLUnitTests(ITestOutputHelper output)
        {
            this.output = output;
        }


        //[ClassInitialize()]        //Use ClassInitialize to run code before you run the first test in the class.
        //public static void ci(TestContext tc)
        //{
        //    TaskScheduler.UnobservedTaskException += (object sender, UnobservedTaskExceptionEventArgs eventArgs) =>
        //    {
        //        eventArgs.SetObserved();
        //        ((AggregateException)eventArgs.Exception).Handle(ex =>
        //        {
        //            Debug.WriteLine("Exception type: {0}", ex.GetType());
        //            return true;
        //        });
        //    };
        //}

        //[ClassCleanup()]//        Use ClassCleanup to run code after all tests in a class have run.
        //public static void cc()
        //{ }

        //[TestInitialize()] //Use TestInitialize to run code before you run each test.
        //public void ti()
        //{ }

        //[TestCleanup()] //Use TestCleanup to run code after each test has run.
        //public void tc()
        //{
        //    Thread.Sleep(500);
        //}

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




        class Junk
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
