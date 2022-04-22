using Chromia.Postchain.Ft3;
using System.Collections.Generic;

public class DirectoryServiceUtil
{
    public static DirectoryService GetDefaultDirectoryService(string chainId, string nodeUrl)
    {
        var connection = new ChainConnectionInfo(
            chainId,
            nodeUrl
        );

        return new FakeDirectoryService(new List<ChainConnectionInfo>() { connection }.ToArray());
    }
}
