namespace SlickProxyLib
{
    public class QueryStringParam
    {
        public QueryStringParam(string name, string value)
        {
            this.Name = name;
            this.Value = value;
        }

        public string Name { get; }

        public string Value { get; }
    }
}