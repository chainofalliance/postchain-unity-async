using Chromia.Postchain.Ft3;
using NUnit.Framework;

public class PostchainTest
{
    // should instantiate blockchain by passing chain id as a string
    [Test]
    public async void PostchainTestRun1()
    {
        string NODEURL = "http://localhost:7740";

        Postchain postchain = new Postchain(NODEURL);
        var blockchain = await postchain.Blockchain(1);

        Assert.NotNull(blockchain);
    }
}
