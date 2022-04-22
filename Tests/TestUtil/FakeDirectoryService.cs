using Chromia.Postchain.Ft3;
using System.Linq;
public class FakeDirectoryService : DirectoryService
{
    private readonly ChainConnectionInfo[] _chainInfos;

    public FakeDirectoryService(ChainConnectionInfo[] chainInfos)
    {
        this._chainInfos = chainInfos;
    }

    public ChainConnectionInfo GetChainConnectionInfo(string id)
    {
        return this._chainInfos.ToList().Find(info => info.ChainId.Equals(id));
    }
}
