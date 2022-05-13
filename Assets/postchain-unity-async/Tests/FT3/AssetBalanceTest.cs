using System.Threading.Tasks;
using Chromia.Postchain.Ft3;
using NUnit.Framework;

public class AssetBalanceTest
{
    [Test]
    public async Task AssetBalanceTestRun()
    {
        var blockchain = await BlockchainUtil.GetDefaultBlockchain();

        var asset1 = await Asset.Register(TestUtil.GenerateAssetName(), TestUtil.GenerateId(), blockchain);
        var asset2 = await Asset.Register(TestUtil.GenerateAssetName(), TestUtil.GenerateId(), blockchain);

        AccountBuilder accountBuilder = AccountBuilder.CreateAccountBuilder(blockchain);
        var account = await accountBuilder.Build();

        Assert.False(asset1.Error);
        Assert.False(asset2.Error);
        Assert.False(account.Error);

        await AssetBalance.GiveBalance(account.Content.Id, asset1.Content.Id, 10, blockchain);
        await AssetBalance.GiveBalance(account.Content.Id, asset2.Content.Id, 20, blockchain);

        var balances = await AssetBalance.GetByAccountId(account.Content.Id, blockchain);
        Assert.False(balances.Error);

        Assert.AreEqual(2, balances.Content.Length);
    }
}
