using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using System.Threading.Tasks;
using Chromia.Postchain.Ft3;
using NUnit.Framework;

public class XCTransferTest
{
    // Cross-chain transfer
    [Test]
    public async Task XcTransferTestRun1()
    {
        var blockchain = await BlockchainUtil.GetDefaultBlockchain();

        var asset = await Asset.Register(TestUtil.GenerateAssetName(), TestUtil.GenerateId(), blockchain);

        var destinationChainId = TestUtil.GenerateId();
        var destinationAccountId = TestUtil.GenerateId();
        User user = TestUser.SingleSig();

        AccountBuilder accountBuilder = AccountBuilder.CreateAccountBuilder(blockchain, user);
        accountBuilder.WithParticipants(new List<KeyPair>() { user.KeyPair });
        accountBuilder.WithBalance(asset.Content, 100);
        accountBuilder.WithPoints(1);
        var account = await accountBuilder.Build();

        await account.Content.XcTransfer(destinationChainId, destinationAccountId, asset.Content.Id, 10);

        var accountBalance = await AssetBalance.GetByAccountAndAssetId(account.Content.Id, asset.Content.Id, blockchain);

        var chainBalance = await AssetBalance.GetByAccountAndAssetId(
            TestUtil.BlockchainAccountId(destinationChainId),
            asset.Content.Id,
            blockchain);

        Assert.AreEqual(90, accountBalance.Content.Amount);
        Assert.AreEqual(10, chainBalance.Content.Amount);
    }
}
