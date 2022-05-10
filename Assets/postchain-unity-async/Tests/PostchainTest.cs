using UnityEngine.TestTools;
using Chromia.Postchain.Ft3;
using NUnit.Framework;
using Cysharp.Threading.Tasks;

public class PostchainTest
{
    // should instantiate blockchain by passing chain id as a string
    [UnityTest]
    public async UniTask PostchainTestRun1()
    {
        string CHAINID = "5759EB34C39B4D34744EC324DFEFAC61526DCEB37FB05D22EB7C95A184380205";
        string NODEURL = "http://localhost:7740";

        Postchain postchain = new Postchain(NODEURL);
        var blockchain = await postchain.Blockchain(CHAINID);

        Assert.NotNull(blockchain);
    }
}
