using Chromia.Postchain.Client;
using System;

public class TestUtil
{
    public static int GenerateNumber(int max = 10000)
    {
        return (int)Math.Round((double)new Random().Next());
    }

    public static string GenerateAssetName(string prefix = "CHROMA")
    {
        return prefix + "_" + GenerateNumber();
    }

    public static string GenerateId()
    {
        var id = PostchainUtil.Sha256(
            BitConverter.GetBytes(GenerateNumber())
        );
        return Chromia.Postchain.Ft3.Util.ByteArrayToString(id);
    }

    public static string BlockchainAccountId(string chainId)
    {
        var gtv = new object[] {
            "B",
            Chromia.Postchain.Ft3.Util.HexStringToBuffer(chainId)
        };
        var hash = PostchainUtil.HashGTV(gtv);
        return Chromia.Postchain.Ft3.Util.ByteArrayToString(hash);
    }
}
