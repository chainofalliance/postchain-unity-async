using System.Collections.Generic;
using Chromia.Postchain.Ft3;
using NUnit.Framework;

using Cysharp.Threading.Tasks;

public class TransferTest
{
    // should succeed when balance is higher than amount to transfer
    [Test]
    public async void TransferTestRun1()
    {
        var blockchain = await BlockchainUtil.GetDefaultBlockchain();
        var asset = await Asset.Register(TestUtil.GenerateAssetName(), TestUtil.GenerateId(), blockchain);

        User user = TestUser.SingleSig();

        AccountBuilder accountBuilder = AccountBuilder.CreateAccountBuilder(blockchain, user);
        accountBuilder.WithParticipants(new List<KeyPair>() { user.KeyPair });
        accountBuilder.WithBalance(asset.Content, 200);
        accountBuilder.WithPoints(1);
        var account1 = await accountBuilder.Build();

        AccountBuilder accountBuilder2 = AccountBuilder.CreateAccountBuilder(blockchain);
        var account2 = await accountBuilder2.Build();

        await account1.Content.Transfer(account2.Content.Id, asset.Content.Id, 10);

        var assetBalance1 = await AssetBalance.GetByAccountAndAssetId(account1.Content.Id, asset.Content.Id, blockchain);
        var assetBalance2 = await AssetBalance.GetByAccountAndAssetId(account2.Content.Id, asset.Content.Id, blockchain);

        Assert.AreEqual(190, assetBalance1.Content.Amount);
        Assert.AreEqual(10, assetBalance2.Content.Amount);
    }

    // should fail when balance is lower than amount to transfer
    [Test]
    public async void TransferTestRun2()
    {
        var blockchain = await BlockchainUtil.GetDefaultBlockchain();
        var asset = await Asset.Register(TestUtil.GenerateAssetName(), TestUtil.GenerateId(), blockchain);
        User user = TestUser.SingleSig();

        AccountBuilder accountBuilder = AccountBuilder.CreateAccountBuilder(blockchain, user);
        accountBuilder.WithParticipants(new List<KeyPair>() { user.KeyPair });
        accountBuilder.WithBalance(asset.Content, 5);
        var account1 = await accountBuilder.Build();

        AccountBuilder accountBuilder2 = AccountBuilder.CreateAccountBuilder(blockchain);
        var account2 = await accountBuilder2.Build();

        var res = await account1.Content.Transfer(account2.Content.Id, asset.Content.Id, 10);
        Assert.True(res.Error);
    }

    // should fail if auth descriptor doesn't have transfer rights
    [Test]
    public async void TransferTestRun3()
    {
        var blockchain = await BlockchainUtil.GetDefaultBlockchain();
        var asset = await Asset.Register(TestUtil.GenerateAssetName(), TestUtil.GenerateId(), blockchain);
        User user = TestUser.SingleSig();

        AccountBuilder accountBuilder = AccountBuilder.CreateAccountBuilder(blockchain, user);
        accountBuilder.WithAuthFlags(new List<FlagsType>() { FlagsType.Account });
        accountBuilder.WithParticipants(new List<KeyPair>() { user.KeyPair });
        accountBuilder.WithBalance(asset.Content, 200);
        accountBuilder.WithPoints(1);
        var account1 = await accountBuilder.Build();

        AccountBuilder accountBuilder2 = AccountBuilder.CreateAccountBuilder(blockchain);
        var account2 = await accountBuilder2.Build();

        var res = await account1.Content.Transfer(account2.Content.Id, asset.Content.Id, 10);
        Assert.True(res.Error);
    }

    // should succeed if transferring tokens to a multisig account
    [Test]
    public async void TransferTestRun4()
    {
        var blockchain = await BlockchainUtil.GetDefaultBlockchain();
        var asset = await Asset.Register(TestUtil.GenerateAssetName(), TestUtil.GenerateId(), blockchain);

        User user = TestUser.SingleSig();
        User user2 = TestUser.SingleSig();
        User user3 = TestUser.SingleSig();

        AccountBuilder accountBuilder = AccountBuilder.CreateAccountBuilder(blockchain, user);
        accountBuilder.WithParticipants(new List<KeyPair>() { user.KeyPair });
        accountBuilder.WithBalance(asset.Content, 200);
        accountBuilder.WithPoints(0);
        var account1 = await accountBuilder.Build();


        AuthDescriptor multiSig = new MultiSignatureAuthDescriptor(
            new List<byte[]>(){
                user2.KeyPair.PubKey, user3.KeyPair.PubKey
            },
            2,
            new List<FlagsType>() { FlagsType.Account, FlagsType.Transfer }.ToArray()
        );

        await blockchain.TransactionBuilder()
            .Add(AccountDevOperations.Register(multiSig))
            .Build(multiSig.Signers.ToArray())
            .Sign(user2.KeyPair)
            .Sign(user3.KeyPair)
            .PostAndWait();


        await account1.Content.Transfer(multiSig.ID, asset.Content.Id, 10);

        var assetBalance1 = await AssetBalance.GetByAccountAndAssetId(account1.Content.Id, asset.Content.Id, blockchain);
        var assetBalance2 = await AssetBalance.GetByAccountAndAssetId(multiSig.ID, asset.Content.Id, blockchain);

        Assert.AreEqual(190, assetBalance1.Content.Amount);
        Assert.AreEqual(10, assetBalance2.Content.Amount);
    }

    // should succeed burning tokens
    [Test]
    public async void TransferTestRun5()
    {
        var blockchain = await BlockchainUtil.GetDefaultBlockchain();
        var asset = await Asset.Register(TestUtil.GenerateAssetName(), TestUtil.GenerateId(), blockchain);
        User user = TestUser.SingleSig();

        AccountBuilder accountBuilder = AccountBuilder.CreateAccountBuilder(blockchain, user);
        accountBuilder.WithParticipants(new List<KeyPair>() { user.KeyPair });
        accountBuilder.WithBalance(asset.Content, 200);
        accountBuilder.WithPoints(1);
        var account = await accountBuilder.Build();

        await account.Content.BurnTokens(asset.Content.Id, 10);
        var assetBalance = account.Content.GetAssetById(asset.Content.Id);

        Assert.AreEqual(190, assetBalance.Amount);
    }
}
