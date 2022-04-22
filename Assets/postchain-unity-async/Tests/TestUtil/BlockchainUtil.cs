using Chromia.Postchain.Ft3;
using System.Collections;
using System;

public class BlockchainUtil
{
    const string CHAINID = "5759EB34C39B4D34744EC324DFEFAC61526DCEB37FB05D22EB7C95A184380205";
    const string NODEURL = "http://localhost:7740";

    public static IEnumerator GetDefaultBlockchain(Action<Blockchain> onSuccess)
    {
        yield return Blockchain.Initialize(
            CHAINID,
            DirectoryServiceUtil.GetDefaultDirectoryService(CHAINID, NODEURL),
            onSuccess,
            (string error) => { throw new Exception(error); }
        );
    }
}
