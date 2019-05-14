namespace ElasticSearch.Diagnostics.Tests
{
    internal class ObjectWithPropertyThatThrows
    {
        public string PropertyThatThrows
        {
            get
            {
                throw new System.Exception("You cannot use \"get\" on PropertyThatThrows - this is going to make Newtonsoft.JSON blow up");
            }
        }
    }
}
