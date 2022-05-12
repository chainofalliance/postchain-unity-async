using System.Collections.Generic;
using System.Threading.Tasks;
using Chromia.Postchain.Ft3;
using NUnit.Framework;
using System;

using Chromia.Postchain.Client;
using Cysharp.Threading.Tasks;

public class AuthDescriptorRuleTest
{
    private UniTask<PostchainResponse<string>> AddAuthDescriptorTo(Account account, User adminUser, User user, Blockchain blockchain)
    {
        var signers = new List<byte[]>();
        signers.AddRange(adminUser.AuthDescriptor.Signers);
        signers.AddRange(user.AuthDescriptor.Signers);

        return blockchain.TransactionBuilder()
            .Add(AccountOperations.AddAuthDescriptor(account.Id, adminUser.AuthDescriptor.ID, user.AuthDescriptor))
            .Build(signers.ToArray())
            .Sign(adminUser.KeyPair)
            .Sign(user.KeyPair)
            .PostAndWait()
        ;
    }

    public UniTask<PostchainResponse<Account>> SourceAccount(Blockchain blockchain, User user, Asset asset)
    {
        AccountBuilder builder = AccountBuilder.CreateAccountBuilder(blockchain, user);
        builder.WithBalance(asset, 200);
        builder.WithPoints(5);
        return builder.Build();
    }

    public UniTask<PostchainResponse<Account>> DestinationAccount(Blockchain blockchain)
    {
        AccountBuilder builder = AccountBuilder.CreateAccountBuilder(blockchain);
        return builder.Build();
    }

    public UniTask<PostchainResponse<Asset>> CreateAsset(Blockchain blockchain)
    {
        return Asset.Register(
            TestUtil.GenerateAssetName(),
            TestUtil.GenerateId(),
            blockchain
        );
    }

    // should succeed when calling operations, number of times less than or equal to value set by operation count rule
    [Test]
    public async Task AuthDescriptorRuleTestRun1()
    {
        var blockchain = await BlockchainUtil.GetDefaultBlockchain();
        var asset = await CreateAsset(blockchain);

        User user = TestUser.SingleSig(Rules.OperationCount().LessOrEqual(2));
        var account1 = await SourceAccount(blockchain, user, asset.Content);
        var account2 = await DestinationAccount(blockchain);

        var res1 = await account1.Content.Transfer(account2.Content.GetID(), asset.Content.Id, 10);
        Assert.False(res1.Error);

        var res2 = await account1.Content.Transfer(account2.Content.GetID(), asset.Content.Id, 20);
        Assert.False(res2.Error);
    }

    // should fail when calling operations, number of times more than value set by operation count rule
    [Test]
    public async Task AuthDescriptorRuleTestRun2()
    {
        var blockchain = await BlockchainUtil.GetDefaultBlockchain();
        var asset = await CreateAsset(blockchain);

        User user = TestUser.SingleSig(Rules.OperationCount().LessThan(2));
        var account1 = await SourceAccount(blockchain, user, asset.Content);
        var account2 = await DestinationAccount(blockchain);

        var res = await account1.Content.Transfer(account2.Content.GetID(), asset.Content.Id, 10);
        Assert.False(res.Error);

        res = await account1.Content.Transfer(account2.Content.GetID(), asset.Content.Id, 20);
        Assert.True(res.Error);
    }

    // should fail when current time is greater than time defined by 'less than' block time rule
    [Test]
    public async Task AuthDescriptorRuleTestRun3()
    {
        var blockchain = await BlockchainUtil.GetDefaultBlockchain();
        var asset = await CreateAsset(blockchain);

        User user = TestUser.SingleSig(Rules.BlockTime().LessThan(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - 10000));
        var account1 = await SourceAccount(blockchain, user, asset.Content);
        var account2 = await DestinationAccount(blockchain);

        var res = await account1.Content.Transfer(account2.Content.GetID(), asset.Content.Id, 10);
        Assert.True(res.Error);
    }

    // should succeed when current time is less than time defined by 'less than' block time rule
    [Test]
    public async Task AuthDescriptorRuleTestRun4()
    {
        var blockchain = await BlockchainUtil.GetDefaultBlockchain();
        var asset = await CreateAsset(blockchain);
        User user = TestUser.SingleSig(Rules.BlockTime().LessThan(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() + 10000));
        var account1 = await SourceAccount(blockchain, user, asset.Content);
        var account2 = await DestinationAccount(blockchain);

        var res = await account1.Content.Transfer(account2.Content.GetID(), asset.Content.Id, 10);
        Assert.False(res.Error);
    }

    // should succeed when current block height is less than value defined by 'less than' block height rule
    [Test]
    public async Task AuthDescriptorRuleTestRun5()
    {
        var blockchain = await BlockchainUtil.GetDefaultBlockchain();

        var asset = await CreateAsset(blockchain);

        User user = TestUser.SingleSig(Rules.BlockHeight().LessThan(10000));

        var account1 = await SourceAccount(blockchain, user, asset.Content);

        var account2 = await DestinationAccount(blockchain);

        var res = await account1.Content.Transfer(account2.Content.GetID(), asset.Content.Id, 10);
        Assert.False(res.Error);
    }

    // should fail when current block height is greater than value defined by 'less than' block height rule
    [Test]
    public async Task AuthDescriptorRuleTestRun6()
    {
        var blockchain = await BlockchainUtil.GetDefaultBlockchain();
        var asset = await CreateAsset(blockchain);

        User user = TestUser.SingleSig(Rules.BlockHeight().LessThan(1));
        var account1 = await SourceAccount(blockchain, user, asset.Content);
        var account2 = await DestinationAccount(blockchain);

        var res = await account1.Content.Transfer(account2.Content.GetID(), asset.Content.Id, 10);
        Assert.True(res.Error);
    }

    // should fail if operation is executed before timestamp defined by 'greater than' block time rule
    [Test]
    public async Task AuthDescriptorRuleTestRun7()
    {
        var blockchain = await BlockchainUtil.GetDefaultBlockchain();
        var asset = await CreateAsset(blockchain);

        User user = TestUser.SingleSig(Rules.BlockTime().GreaterThan(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() + 10000));
        var account1 = await SourceAccount(blockchain, user, asset.Content);
        var account2 = await DestinationAccount(blockchain);


        var res = await account1.Content.Transfer(account2.Content.GetID(), asset.Content.Id, 10);
        Assert.True(res.Error);
    }

    // should succeed if operation is executed after timestamp defined by 'greater than' block time rule
    [Test]
    public async Task AuthDescriptorRuleTestRun8()
    {
        var blockchain = await BlockchainUtil.GetDefaultBlockchain();

        var asset = await CreateAsset(blockchain);

        User user = TestUser.SingleSig(Rules.BlockTime().GreaterThan(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - 10000));

        var account1 = await SourceAccount(blockchain, user, asset.Content);

        var account2 = await DestinationAccount(blockchain);


        var res = await account1.Content.Transfer(account2.Content.GetID(), asset.Content.Id, 10);
        Assert.False(res.Error);
    }

    // should fail if operation is executed before block defined by 'greater than' block height rule
    [Test]
    public async Task AuthDescriptorRuleTestRun9()
    {
        var blockchain = await BlockchainUtil.GetDefaultBlockchain();

        var asset = await CreateAsset(blockchain);

        User user = TestUser.SingleSig(Rules.BlockHeight().GreaterThan(10000));

        var account1 = await SourceAccount(blockchain, user, asset.Content);

        var account2 = await DestinationAccount(blockchain);


        var res = await account1.Content.Transfer(account2.Content.GetID(), asset.Content.Id, 10);
        Assert.True(res.Error);
    }

    // should succeed if operation is executed after block defined by 'greater than' block height rule
    [Test]
    public async Task AuthDescriptorRuleTestRun10()
    {
        var blockchain = await BlockchainUtil.GetDefaultBlockchain();

        var asset = await CreateAsset(blockchain);

        User user = TestUser.SingleSig(Rules.BlockHeight().GreaterThan(1));

        var account1 = await SourceAccount(blockchain, user, asset.Content);

        var account2 = await DestinationAccount(blockchain);


        var res = await account1.Content.Transfer(account2.Content.GetID(), asset.Content.Id, 10);
        Assert.False(res.Error);
    }

    // should be able to create complex rules
    [Test]
    public async Task AuthDescriptorRuleTestRun11()
    {
        var blockchain = await BlockchainUtil.GetDefaultBlockchain();

        var asset = await CreateAsset(blockchain);

        User user = TestUser.SingleSig(Rules.BlockHeight().GreaterThan(1).And().BlockHeight().LessThan(10000));

        var account1 = await SourceAccount(blockchain, user, asset.Content);

        var account2 = await DestinationAccount(blockchain);


        var res = await account1.Content.Transfer(account2.Content.GetID(), asset.Content.Id, 10);
        Assert.False(res.Error);
    }

    // should fail if block heights defined by 'greater than' and 'less than' block height rules are less than current block height
    [Test]
    public async Task AuthDescriptorRuleTestRun12()
    {
        var blockchain = await BlockchainUtil.GetDefaultBlockchain();

        var asset = await CreateAsset(blockchain);

        User user = TestUser.SingleSig(Rules.BlockHeight().GreaterThan(1).And().BlockHeight().LessThan(10));

        var account1 = await SourceAccount(blockchain, user, asset.Content);

        var account2 = await DestinationAccount(blockchain);


        var res = await account1.Content.Transfer(account2.Content.GetID(), asset.Content.Id, 10);
        Assert.True(res.Error);
    }

    // should fail if block times defined by 'greater than' and 'less than' block time rules are in the past
    [Test]
    public async Task AuthDescriptorRuleTestRun13()
    {
        var blockchain = await BlockchainUtil.GetDefaultBlockchain();

        var asset = await CreateAsset(blockchain);

        User user = TestUser.SingleSig(
            Rules.BlockTime().GreaterThan(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - 20000).
            And().
            BlockTime().LessThan(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - 10000)
            );

        var account1 = await SourceAccount(blockchain, user, asset.Content);

        var account2 = await DestinationAccount(blockchain);


        var res = await account1.Content.Transfer(account2.Content.GetID(), asset.Content.Id, 10);
        Assert.True(res.Error);
    }

    // should succeed if current time is within period defined by 'greater than' and 'less than' block time rules
    [Test]
    public async Task AuthDescriptorRuleTestRun14()
    {
        var blockchain = await BlockchainUtil.GetDefaultBlockchain();

        var asset = await CreateAsset(blockchain);

        User user = TestUser.SingleSig(
            Rules.BlockTime().GreaterThan(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - 10000).
            And().
            BlockTime().LessThan(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() + 10000)
            );

        var account1 = await SourceAccount(blockchain, user, asset.Content);

        var account2 = await DestinationAccount(blockchain);


        var res = await account1.Content.Transfer(account2.Content.GetID(), asset.Content.Id, 10);
        Assert.False(res.Error);
    }

    // should succeed if current time is within period defined by 'greater than' and 'less than' block time rules
    [Test]
    public async Task AuthDescriptorRuleTestRun15()
    {
        var blockchain = await BlockchainUtil.GetDefaultBlockchain();

        var asset = await CreateAsset(blockchain);

        User user1 = TestUser.SingleSig();
        User user2 = TestUser.SingleSig(Rules.OperationCount().LessThan(2));

        var srcAccount1 = await SourceAccount(blockchain, user1, asset.Content);
        var destAccount = await DestinationAccount(blockchain);

        // add expiring auth descriptor to the account
        await srcAccount1.Content.AddAuthDescriptor(user2.AuthDescriptor);

        // get the same account, but initialized with user2
        // object which contains expiring auth descriptor
        var srcAccount2 = await blockchain.NewSession(user2).GetAccountById(srcAccount1.Content.GetID());

        await srcAccount2.Content.Transfer(destAccount.Content.GetID(), asset.Content.Id, 10);

        // account descriptor used by user2 object has expired.
        // this operation call will delete it.
        // any other operation, which calls require_auth internally
        // would also delete expired auth descriptor.
        await srcAccount1.Content.Transfer(destAccount.Content.GetID(), asset.Content.Id, 30);

        await srcAccount1.Content.Sync();

        Assert.AreEqual(1, srcAccount1.Content.AuthDescriptor.Count);
    }

    // shouldn't delete non-expired auth descriptor
    [Test]
    public async Task AuthDescriptorRuleTestRun16()
    {
        var blockchain = await BlockchainUtil.GetDefaultBlockchain();

        var asset = await CreateAsset(blockchain);

        User user1 = TestUser.SingleSig();
        User user2 = TestUser.SingleSig(Rules.OperationCount().LessThan(10));

        var srcAccount1 = await SourceAccount(blockchain, user1, asset.Content);
        var destAccount = await DestinationAccount(blockchain);

        // add expiring auth descriptor to the account
        await AddAuthDescriptorTo(srcAccount1.Content, user1, user2, blockchain);

        // get the same account, but initialized with user2
        // object which contains expiring auth descriptor
        var srcAccount2 = await blockchain.NewSession(user2).GetAccountById(srcAccount1.Content.GetID());

        // perform transfer with expiring auth descriptor.
        // auth descriptor didn't expire, because it's only used 1 out of 10 times.
        await srcAccount2.Content.Transfer(destAccount.Content.GetID(), asset.Content.Id, 10);

        // perform transfer using auth descriptor without rules
        await srcAccount1.Content.Transfer(destAccount.Content.GetID(), asset.Content.Id, 10);

        await srcAccount1.Content.Sync();

        Assert.AreEqual(2, srcAccount1.Content.AuthDescriptor.Count);
    }

    // should delete only expired auth descriptor if multiple expiring descriptors exist
    [Test]
    public async Task AuthDescriptorRuleTestRun17()
    {
        var blockchain = await BlockchainUtil.GetDefaultBlockchain();

        var asset = await CreateAsset(blockchain);

        User user1 = TestUser.SingleSig();
        User user2 = TestUser.SingleSig(Rules.OperationCount().LessOrEqual(1));
        User user3 = TestUser.SingleSig(Rules.OperationCount().LessOrEqual(1));


        var srcAccount1 = await SourceAccount(blockchain, user1, asset.Content);

        var destAccount = await DestinationAccount(blockchain);

        await AddAuthDescriptorTo(srcAccount1.Content, user1, user2, blockchain);
        await AddAuthDescriptorTo(srcAccount1.Content, user1, user3, blockchain);

        var srcAccount2 = await blockchain.NewSession(user2).GetAccountById(srcAccount1.Content.GetID());

        await srcAccount2.Content.Transfer(destAccount.Content.GetID(), asset.Content.Id, 50);

        // this call will trigger deletion of expired auth descriptor (attached to user2)
        await srcAccount1.Content.Transfer(destAccount.Content.GetID(), asset.Content.Id, 100);

        await srcAccount1.Content.Sync();

        Assert.AreEqual(2, srcAccount1.Content.AuthDescriptor.Count);
    }

    // should add auth descriptors
    [Test]
    public async Task AuthDescriptorRuleTestRun18()
    {
        var blockchain = await BlockchainUtil.GetDefaultBlockchain();

        var asset = await CreateAsset(blockchain);

        User user1 = TestUser.SingleSig();
        User user2 = TestUser.SingleSig(Rules.OperationCount().LessOrEqual(1));
        User user3 = TestUser.SingleSig(Rules.OperationCount().LessOrEqual(1));

        var account = await SourceAccount(blockchain, user1, asset.Content);

        await AddAuthDescriptorTo(account.Content, user1, user2, blockchain);
        await AddAuthDescriptorTo(account.Content, user1, user3, blockchain);

        await account.Content.Sync();

        Assert.AreEqual(3, account.Content.AuthDescriptor.Count);
    }

    // should delete auth descriptors
    [Test]
    public async Task AuthDescriptorRuleTestRun19()
    {
        var blockchain = await BlockchainUtil.GetDefaultBlockchain();

        var asset = await CreateAsset(blockchain);

        User user1 = TestUser.SingleSig();
        User user2 = TestUser.SingleSig(Rules.OperationCount().LessOrEqual(1));
        User user3 = TestUser.SingleSig(Rules.OperationCount().LessOrEqual(1));

        var account = await SourceAccount(blockchain, user1, asset.Content);

        await account.Content.AddAuthDescriptor(user2.AuthDescriptor);
        await account.Content.AddAuthDescriptor(user3.AuthDescriptor);

        await account.Content.DeleteAllAuthDescriptorsExclude(user1.AuthDescriptor);
        Assert.AreEqual(1, account.Content.AuthDescriptor.Count);

        await account.Content.Sync();
        Assert.AreEqual(1, account.Content.AuthDescriptor.Count);
    }

    // should fail when deleting an auth descriptor which is not owned by the account
    [Test]
    public async Task AuthDescriptorRuleTestRun20()
    {
        var blockchain = await BlockchainUtil.GetDefaultBlockchain();

        var asset = await CreateAsset(blockchain);

        User user1 = TestUser.SingleSig();
        User user2 = TestUser.SingleSig();

        var account1 = await SourceAccount(blockchain, user1, asset.Content);
        var account2 = await SourceAccount(blockchain, user2, asset.Content);

        var res = await account1.Content.DeleteAuthDescriptor(user2.AuthDescriptor);
        Assert.True(res.Error);
    }

    // should delete auth descriptor
    [Test]
    public async Task AuthDescriptorRuleTestRun21()
    {
        var blockchain = await BlockchainUtil.GetDefaultBlockchain();

        var asset = await CreateAsset(blockchain);

        User user1 = TestUser.SingleSig();
        User user2 = TestUser.SingleSig();


        var account = await SourceAccount(blockchain, user1, asset.Content);
        await account.Content.AddAuthDescriptor(user2.AuthDescriptor);
        await account.Content.DeleteAuthDescriptor(user2.AuthDescriptor);

        Assert.AreEqual(1, account.Content.AuthDescriptor.Count);
    }

    // Should be able to create same rules with different value
    [Test]
    public async Task AuthDescriptorRuleTestRun22()
    {
        var blockchain = await BlockchainUtil.GetDefaultBlockchain();

        var asset = await CreateAsset(blockchain);

        var rules = Rules.BlockHeight().GreaterThan(1).And().BlockHeight().GreaterThan(10000).And().BlockTime().GreaterOrEqual(122222999);
        User user = TestUser.SingleSig(rules);

        var account = await SourceAccount(blockchain, user, asset.Content);

        Assert.AreEqual(1, account.Content.AuthDescriptor.Count);
    }

    // shouldn't be able to create too many rules
    [Test]
    public async Task AuthDescriptorRuleTestRun23()
    {
        var blockchain = await BlockchainUtil.GetDefaultBlockchain();

        var asset = await CreateAsset(blockchain);

        var rules = Rules.BlockHeight().GreaterThan(1).And().BlockHeight().GreaterThan(10000).And().BlockTime().GreaterOrEqual(122222999);

        for (int i = 0; i < 400; i++)
        {
            rules = rules.And().BlockHeight().GreaterOrEqual(i);
        }

        User user = TestUser.SingleSig(rules);

        var account = await SourceAccount(blockchain, user, asset.Content);
        Assert.AreEqual(null, account.Content);
    }
}
