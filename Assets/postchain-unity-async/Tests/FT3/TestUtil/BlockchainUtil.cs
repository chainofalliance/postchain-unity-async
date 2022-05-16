using Chromia.Postchain.Ft3;
using Cysharp.Threading.Tasks;

public class BlockchainUtil
{
    const string NODEURL = "http://localhost:7740";

    public async static UniTask<Blockchain> GetDefaultBlockchain()
    {
        Postchain postchain = new Postchain(NODEURL);
        return await postchain.Blockchain(1);
    }
}
