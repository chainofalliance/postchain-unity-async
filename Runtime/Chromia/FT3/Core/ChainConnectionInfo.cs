namespace Chromia.Postchain.Ft3
{
    public class ChainConnectionInfo
    {
        public readonly string Url;
        public readonly string ChainId;

        public ChainConnectionInfo(string chainId, string url)
        {
            this.ChainId = chainId;
            this.Url = url;
        }
    }
}