using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace ElasticSearch.Diagnostics.Tests
{
    [TestClass]
    public class ESTLUnitTests
    {
        public TestContext TestContext { get; set; }



        [ClassInitialize()]        //Use ClassInitialize to run code before you run the first test in the class.
        public static void ci(TestContext tc)
        {
            TaskScheduler.UnobservedTaskException += (object sender, UnobservedTaskExceptionEventArgs eventArgs) =>
            {
                eventArgs.SetObserved();
                ((AggregateException)eventArgs.Exception).Handle(ex =>
                {
                    Debug.WriteLine("Exception type: {0}", ex.GetType());
                    return true;
                });
            };
        }

        [ClassCleanup()]//        Use ClassCleanup to run code after all tests in a class have run.
        public static void cc()
        { }

        [TestInitialize()] //Use TestInitialize to run code before you run each test.
        public void ti()
        { }

        [TestCleanup()] //Use TestCleanup to run code after each test has run.
        public void tc()
        {
            Thread.Sleep(500);
        }

        [TestMethod]
        public void SimpleWrite()
        {
            var x = new ElasticSearchTraceListener("tester");
            x.ElasticSearchUri = "http://192.168.2.50:9200";
            x.ElasticSearchIndex = "trace";
            x.ElasticSearchTraceIndex = "trace";

            x.Write(4);
        }

        [TestMethod]
        public void WriteObjectTest()
        {
            var x = new ElasticSearchTraceListener("tester");
            x.ElasticSearchUri = "http://192.168.2.50:9200";
            x.ElasticSearchIndex = "trace";
            x.ElasticSearchTraceIndex = "trace";


            x.Write(new
            {
                thing = "ggg",
                morethings = 11111,
                anotherthing = "yyyy"
            });
        }

        [TestMethod]
        public void WriteExceptionest()
        {
            var x = new ElasticSearchTraceListener("tester");
            x.ElasticSearchUri = "http://192.168.2.50:9200";
            x.ElasticSearchIndex = "trace";
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





        [TestMethod]
        public void ALOTofExmsgs()
        {
            var x = new ElasticSearchTraceListener("tester");
            x.ElasticSearchUri = "http://192.168.2.50:9200";
            x.ElasticSearchIndex = "trace";
            x.ElasticSearchTraceIndex = "trace";

            for (int i = 0; i < 100; i++)
            {
                x.Write(new Exception());
            }

            x.Flush();
        }

        [TestMethod]
        public void WriteManySimpleStringsTest()
        {
            var x = new ElasticSearchTraceListener("tester");
            x.ElasticSearchUri = "http://192.168.2.50:9200";
            x.ElasticSearchIndex = "trace";
            x.ElasticSearchTraceIndex = "trace";

            for (int i = 0; i < 10; i++)
            {
                x.Write("xxxxx" + i);
            }
            x.Flush();
        }

        [TestMethod]
        public void TraceSourceManySimpleStringsTest()
        {
            var x = new ElasticSearchTraceListener("tester");
            x.ElasticSearchUri = "http://192.168.2.50:9200";
            x.ElasticSearchIndex = "trace";
            x.ElasticSearchTraceIndex = "trace";

            var ts = new TraceSource("x", SourceLevels.All);
            ts.Listeners.Add(x);

            for (int i = 0; i < 10; i++)
            {
                ts.TraceInformation("xxxxx" + i);
            }
            x.Flush();

        }

        [TestMethod]
        public void TSTestTimeIds()
        {
            var x = new ElasticSearchTraceListener("tester");
            x.ElasticSearchUri = "http://192.168.2.50:9200";
            x.ElasticSearchIndex = "trace";
            x.ElasticSearchTraceIndex = "trace";

            var ts = new TraceSource("x", SourceLevels.All);
            ts.Listeners.Add(x);

            for (int i = 0; i < 10; i++)
            {
                ts.TraceEvent(TraceEventType.Error, 1000, DateTime.Now.ToString());
            }
            x.Flush();

        }


    }
}
