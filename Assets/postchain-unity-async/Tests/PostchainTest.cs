using System.Collections;
using UnityEngine.TestTools;
using Chromia.Postchain.Ft3;
using NUnit.Framework;

public class PostchainTest
{
    private Blockchain blockchain;

    private IEnumerator SetupBlockchain()
    {
        yield return BlockchainUtil.GetDefaultBlockchain((Blockchain _blockchain) => { blockchain = _blockchain; });
    }
    private void DefaultErrorHandler(string error) { }

    // should instantiate blockchain by passing chain id as a string
    [UnityTest]
    public IEnumerator PostchainTestRun1()
    {
        string CHAINID = "5759EB34C39B4D34744EC324DFEFAC61526DCEB37FB05D22EB7C95A184380205";
        string NODEURL = "http://localhost:7740";

        Blockchain blockchain = null;
        Postchain postchain = new Postchain(NODEURL);
        yield return postchain.Blockchain(CHAINID, (Blockchain _blockchain) => blockchain = _blockchain, DefaultErrorHandler);

        Assert.NotNull(blockchain);
    }
}
