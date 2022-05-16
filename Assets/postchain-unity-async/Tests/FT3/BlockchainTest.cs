using System.Collections.Generic;
using System.Threading.Tasks;
using Chromia.Postchain.Ft3;
using NUnit.Framework;

using Cysharp.Threading.Tasks;

public class BlockchainTest
{
    // should provide info
    [Test]
    public async Task BlockchainTestRun1()
    {
        var blockchain = await BlockchainUtil.GetDefaultBlockchain();
        var info = await BlockchainInfo.GetInfo(blockchain.Connection);

        Assert.AreEqual(info.Content.Name, "Unity FT3");
    }

    // should be able to register an account
    [Test]
    public async Task BlockchainTestRun2()
    {
        var blockchain = await BlockchainUtil.GetDefaultBlockchain();

        User user = TestUser.SingleSig();
        BlockchainSession session = blockchain.NewSession(user);

        var account = await blockchain.RegisterAccount(user.AuthDescriptor, user);
        var foundAccount = await session.GetAccountById(account.Content.Id);

        Assert.AreEqual(account.Content.Id.ToUpper(), foundAccount.Content.Id.ToUpper());
    }

    // should return account by participant id
    [Test]
    public async Task BlockchainTestRun3()
    {
        var blockchain = await BlockchainUtil.GetDefaultBlockchain();
        User user = TestUser.SingleSig();

        AccountBuilder accountBuilder = AccountBuilder.CreateAccountBuilder(blockchain, user);
        accountBuilder.WithParticipants(new List<KeyPair>() { user.KeyPair });

        var account = await accountBuilder.Build();
        var foundAccounts = await blockchain.GetAccountsByParticipantId(Util.ByteArrayToString(user.KeyPair.PubKey), user);

        Assert.AreEqual(1, foundAccounts.Content.Length);
        Assert.AreEqual(account.Content.Id.ToUpper(), foundAccounts.Content[0].Id.ToUpper());
    }

    // should return account by auth descriptor id
    [Test]
    public async Task BlockchainTestRun4()
    {
        var blockchain = await BlockchainUtil.GetDefaultBlockchain();
        User user = TestUser.SingleSig();

        AccountBuilder accountBuilder = AccountBuilder.CreateAccountBuilder(blockchain, user);
        accountBuilder.WithParticipants(new List<KeyPair>() { user.KeyPair });

        var account = await accountBuilder.Build();
        var foundAccounts = await blockchain.GetAccountsByAuthDescriptorId(user.AuthDescriptor.ID, user);

        Assert.AreEqual(1, foundAccounts.Content.Length);
        Assert.AreEqual(account.Content.Id.ToUpper(), foundAccounts.Content[0].Id.ToUpper());
    }

    // should be able to link other chain
    [Test]
    public async Task BlockchainTestRun5()
    {
        var blockchain = await BlockchainUtil.GetDefaultBlockchain();

        var chainId1 = TestUtil.GenerateId();
        var res = await blockchain.LinkChain(chainId1);
        Assert.False(res.Error);

        var isLinked = await blockchain.IsLinkedWithChain(chainId1.ToUpper());

        Assert.False(isLinked.Error);
        Assert.True(isLinked.Content);
    }

    // should be able to link multiple chains
    [Test]
    public async Task BlockchainTestRun6()
    {
        var blockchain = await BlockchainUtil.GetDefaultBlockchain();
        var chainId1 = TestUtil.GenerateId();
        var chainId2 = TestUtil.GenerateId();

        await blockchain.LinkChain(chainId1);
        await blockchain.LinkChain(chainId2);

        var linkedChains = await blockchain.GetLinkedChainsIds();

        Assert.Contains(chainId1.ToUpper(), linkedChains.Content);
        Assert.Contains(chainId2.ToUpper(), linkedChains.Content);
    }

    // should return false when isLinkedWithChain is called for unknown chain id
    [Test]
    public async Task BlockchainTestRun7()
    {
        var blockchain = await BlockchainUtil.GetDefaultBlockchain();
        var isLinked = await blockchain.IsLinkedWithChain(TestUtil.GenerateId());

        Assert.False(isLinked.Content);
    }

    // should return asset queried by id
    [Test]
    public async Task BlockchainTestRun8()
    {
        var blockchain = await BlockchainUtil.GetDefaultBlockchain();
        var asset = await Asset.Register(TestUtil.GenerateAssetName(), TestUtil.GenerateId(), blockchain);

        var found = await blockchain.GetAssetById(asset.Content.Id);
        Assert.AreEqual(asset.Content.Id.ToUpper(), found.Content.Id.ToUpper());
    }
}
