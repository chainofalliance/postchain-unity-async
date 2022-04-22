using System.Collections.Generic;
using System.Linq;

namespace Chromia.Postchain.Ft3
{
    public class DirectoryServiceBase : DirectoryService
    {
        private List<ChainConnectionInfo> _chainInfos;

        public DirectoryServiceBase(ChainConnectionInfo[] chainInfos)
        {
            this._chainInfos = chainInfos.ToList();
        }

        public ChainConnectionInfo GetChainConnectionInfo(string id)
        {
            return this._chainInfos.Find(info => info.ChainId.Equals(id));
        }
    }
}