using AM.Elasticsearch.TraceListener;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace ElasticSearch.Diagnostics.Tests
{
    public class esinsideTests : IClassFixture<Fixture>
    {
        private readonly ITestOutputHelper _output;

        private readonly Fixture _fixture;
        //private readonly ElasticsearchInside.Elasticsearch _elasticsearch;

        public esinsideTests(ITestOutputHelper output, Fixture fixture)
        {
            _output = output;
            _fixture = fixture;
        }



        [Fact]
        public async Task SimpleWrite()
        {

            var x = new ElasticSearchTraceListener("tester");
            x.ElasticSearchUri = _fixture.Elasticsearch.Url.ToString();
            x.ElasticSearchTraceIndex = "trace";

            x.Write(4);
        }

        [Fact]
        public async Task WriteObjectTest()
        {
            var x = new ElasticSearchTraceListener("tester");
            x.ElasticSearchUri = _fixture.Elasticsearch.Url.ToString();
            x.ElasticSearchTraceIndex = "trace";


            x.Write(new
            {
                thing = "ggg",
                morethings = 11111,
                anotherthing = "yyyy"
            });
        }

        [Fact]
        public async Task WriteExceptionest()
        {
            var x = new ElasticSearchTraceListener("tester");
            x.ElasticSearchUri = _fixture.Elasticsearch.Url.ToString();
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
        public async Task ALOTofExmsgs()
        {
            var x = new ElasticSearchTraceListener("tester");
            x.ElasticSearchUri = _fixture.Elasticsearch.Url.ToString();
            x.ElasticSearchTraceIndex = "trace";

            for (int i = 0; i < 100; i++)
            {
                x.Write(new Exception());
            }

            x.Flush();
        }

        [Fact]
        public async Task WriteManySimpleStringsTest()
        {
            var x = new ElasticSearchTraceListener("tester");
            x.ElasticSearchUri = _fixture.Elasticsearch.Url.ToString();
            x.ElasticSearchTraceIndex = "trace";

            for (int i = 0; i < 10; i++)
            {
                x.Write("xxxxx" + i);
            }
            x.Flush();
        }

        [Fact]
        public async Task TraceSourceManySimpleStringsTest()
        {
            var x = new ElasticSearchTraceListener("tester");
            x.ElasticSearchUri =_fixture.Elasticsearch.Url.ToString();
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
        public async Task TSTestTimeIds()
        {
            var x = new ElasticSearchTraceListener("tester");
            x.ElasticSearchUri = _fixture.Elasticsearch.Url.ToString();
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
        public async Task TSManyWriteExceptionsTest()
        {
            var x = new ElasticSearchTraceListener("tester");
            x.ElasticSearchUri = _fixture.Elasticsearch.Url.ToString();
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
        public async Task TraceDataWithString()
        {
            var x = new ElasticSearchTraceListener("tester");
            x.ElasticSearchUri = _fixture.Elasticsearch.Url.ToString();
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
        public async Task CauseFailedSerialization()
        {
            var x = new ElasticSearchTraceListener("tester");
            x.ElasticSearchUri = _fixture.Elasticsearch.Url.ToString();
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
        //public async Task IdentityTest()
        //{
        //    IPrincipal principal = Thread.CurrentPrincipal;
        //    IIdentity identity = principal == null ? null : principal.Identity;
        //    string name = identity == null ? "" : identity.Name;

        //    Assert.False(string.IsNullOrWhiteSpace(name));
        //}

        [Fact]
        public async Task UserNameTest()
        {
            string name = Environment.UserDomainName + "\\" + Environment.UserName;

            _output.WriteLine(name);

            Assert.False(string.IsNullOrWhiteSpace(name));
        }

        [Fact]
        public async Task SometimesExceptionsThrowWhenSerialized()
        {
            var x = new ElasticSearchTraceListener("tester");
            x.ElasticSearchUri = _fixture.Elasticsearch.Url.ToString();
            x.ElasticSearchTraceIndex = "trace";

            var ts = new TraceSource("exxxxx", SourceLevels.All);
            ts.Listeners.Add(x);

            ts.TraceData(TraceEventType.Error, 99, new ObjectWithPropertyThatThrows());
            Assert.True(true);  //we did not blow up trying to serialize "ObjectThatThrows"

            x.Flush();
        }
    }
}
