using System.Collections.Generic;
using System.Collections;
using UnityEngine.TestTools;
using Chromia.Postchain.Ft3;
using NUnit.Framework;
using System;

public class AuthDescriptorRuleTest
{
    private Blockchain blockchain;

    private IEnumerator SetupBlockchain()
    {
        yield return BlockchainUtil.GetDefaultBlockchain((Blockchain _blockchain) => { blockchain = _blockchain; });
    }

    private void DefaultErrorHandler(string error) { UnityEngine.Debug.Log(error); }
    private void EmptyCallback() { }

    private IEnumerator AddAuthDescriptorTo(Account account, User adminUser, User user, Action onSuccess)
    {
        var signers = new List<byte[]>();
        signers.AddRange(adminUser.AuthDescriptor.Signers);
        signers.AddRange(user.AuthDescriptor.Signers);

        yield return blockchain.TransactionBuilder()
            .Add(AccountOperations.AddAuthDescriptor(account.Id, adminUser.AuthDescriptor.ID, user.AuthDescriptor))
            .Build(signers.ToArray(), DefaultErrorHandler)
            .Sign(adminUser.KeyPair)
            .Sign(user.KeyPair)
            .PostAndWait(onSuccess)
        ;
    }

    public IEnumerator SourceAccount(Blockchain blockchain, User user, Asset asset, Action<Account> onSuccess)
    {
        AccountBuilder builder = AccountBuilder.CreateAccountBuilder(blockchain, user);
        builder.WithBalance(asset, 200);
        builder.WithPoints(5);
        yield return builder.Build(onSuccess);
    }

    public IEnumerator DestinationAccount(Blockchain blockchain, Action<Account> onSuccess)
    {
        AccountBuilder builder = AccountBuilder.CreateAccountBuilder(blockchain);
        yield return builder.Build(onSuccess);
    }

    public IEnumerator CreateAsset(Blockchain blockchain, Action<Asset> onSuccess)
    {
        yield return Asset.Register(
            TestUtil.GenerateAssetName(),
            TestUtil.GenerateId(),
            blockchain, onSuccess,
            DefaultErrorHandler
        );
    }

    // should succeed when calling operations, number of times less than or equal to value set by operation count rule
    [UnityTest]
    public IEnumerator AuthDescriptorRuleTestRun1()
    {
        yield return SetupBlockchain();
        Asset asset = null;
        yield return CreateAsset(blockchain, (Asset _asset) => asset = _asset);

        User user = TestUser.SingleSig(Rules.OperationCount().LessOrEqual(2));
        Account account1 = null;
        yield return SourceAccount(blockchain, user, asset, (Account _account) => account1 = _account);
        Account account2 = null;
        yield return DestinationAccount(blockchain, (Account _account) => account2 = _account);

        bool successful = false;
        yield return account1.Transfer(account2.GetID(), asset.Id, 10, () => successful = true, DefaultErrorHandler);
        Assert.True(successful);

        successful = false;
        yield return account1.Transfer(account2.GetID(), asset.Id, 20, () => successful = true, DefaultErrorHandler);
        Assert.True(successful);
    }

    // should fail when calling operations, number of times more than value set by operation count rule
    [UnityTest]
    public IEnumerator AuthDescriptorRuleTestRun2()
    {
        yield return SetupBlockchain();
        Asset asset = null;
        yield return CreateAsset(blockchain, (Asset _asset) => asset = _asset);

        User user = TestUser.SingleSig(Rules.OperationCount().LessThan(2));
        Account account1 = null;
        yield return SourceAccount(blockchain, user, asset, (Account _account) => account1 = _account);
        Account account2 = null;
        yield return DestinationAccount(blockchain, (Account _account) => account2 = _account);

        bool successful = false;
        yield return account1.Transfer(account2.GetID(), asset.Id, 10, () => successful = true, DefaultErrorHandler);
        Assert.True(successful);

        successful = false;
        yield return account1.Transfer(account2.GetID(), asset.Id, 20, () => successful = true, DefaultErrorHandler);
        Assert.False(successful);
    }

    // should fail when current time is greater than time defined by 'less than' block time rule
    [UnityTest]
    public IEnumerator AuthDescriptorRuleTestRun3()
    {
        yield return SetupBlockchain();
        Asset asset = null;
        yield return CreateAsset(blockchain, (Asset _asset) => asset = _asset);

        User user = TestUser.SingleSig(Rules.BlockTime().LessThan(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - 10000));
        Account account1 = null;
        yield return SourceAccount(blockchain, user, asset, (Account _account) => account1 = _account);
        Account account2 = null;
        yield return DestinationAccount(blockchain, (Account _account) => account2 = _account);

        bool successful = false;
        yield return account1.Transfer(account2.GetID(), asset.Id, 10, () => successful = true, DefaultErrorHandler);
        Assert.False(successful);
    }

    // should succeed when current time is less than time defined by 'less than' block time rule
    [UnityTest]
    public IEnumerator AuthDescriptorRuleTestRun4()
    {
        yield return SetupBlockchain();
        Asset asset = null;
        yield return CreateAsset(blockchain, (Asset _asset) => asset = _asset);
        User user = TestUser.SingleSig(Rules.BlockTime().LessThan(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() + 10000));
        Account account1 = null;
        yield return SourceAccount(blockchain, user, asset, (Account _account) => account1 = _account);
        Account account2 = null;
        yield return DestinationAccount(blockchain, (Account _account) => account2 = _account);

        bool successful = false;
        yield return account1.Transfer(account2.GetID(), asset.Id, 10, () => successful = true, DefaultErrorHandler);
        Assert.True(successful);
    }

    // should succeed when current block height is less than value defined by 'less than' block height rule
    [UnityTest]
    public IEnumerator AuthDescriptorRuleTestRun5()
    {
        yield return SetupBlockchain();
        Asset asset = null;
        yield return CreateAsset(blockchain, (Asset _asset) => asset = _asset);

        User user = TestUser.SingleSig(Rules.BlockHeight().LessThan(10000));
        Account account1 = null;
        yield return SourceAccount(blockchain, user, asset, (Account _account) => account1 = _account);
        Account account2 = null;
        yield return DestinationAccount(blockchain, (Account _account) => account2 = _account);

        bool successful = false;
        yield return account1.Transfer(account2.GetID(), asset.Id, 10, () => successful = true, DefaultErrorHandler);
        Assert.True(successful);
    }

    // should fail when current block height is greater than value defined by 'less than' block height rule
    [UnityTest]
    public IEnumerator AuthDescriptorRuleTestRun6()
    {
        yield return SetupBlockchain();
        Asset asset = null;
        yield return CreateAsset(blockchain, (Asset _asset) => asset = _asset);

        User user = TestUser.SingleSig(Rules.BlockHeight().LessThan(1));
        Account account1 = null;
        yield return SourceAccount(blockchain, user, asset, (Account _account) => account1 = _account);
        Account account2 = null;
        yield return DestinationAccount(blockchain, (Account _account) => account2 = _account);

        bool successful = false;
        yield return account1.Transfer(account2.GetID(), asset.Id, 10, () => successful = true, DefaultErrorHandler);
        Assert.False(successful);
    }

    // should fail if operation is executed before timestamp defined by 'greater than' block time rule
    [UnityTest]
    public IEnumerator AuthDescriptorRuleTestRun7()
    {
        yield return SetupBlockchain();
        Asset asset = null;
        yield return CreateAsset(blockchain, (Asset _asset) => asset = _asset);

        User user = TestUser.SingleSig(Rules.BlockTime().GreaterThan(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() + 10000));
        Account account1 = null;
        yield return SourceAccount(blockchain, user, asset, (Account _account) => account1 = _account);
        Account account2 = null;
        yield return DestinationAccount(blockchain, (Account _account) => account2 = _account);

        bool successful = false;
        yield return account1.Transfer(account2.GetID(), asset.Id, 10, () => successful = true, DefaultErrorHandler);
        Assert.False(successful);
    }

    // should succeed if operation is executed after timestamp defined by 'greater than' block time rule
    [UnityTest]
    public IEnumerator AuthDescriptorRuleTestRun8()
    {
        yield return SetupBlockchain();
        Asset asset = null;
        yield return CreateAsset(blockchain, (Asset _asset) => asset = _asset);

        User user = TestUser.SingleSig(Rules.BlockTime().GreaterThan(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - 10000));
        Account account1 = null;
        yield return SourceAccount(blockchain, user, asset, (Account _account) => account1 = _account);
        Account account2 = null;
        yield return DestinationAccount(blockchain, (Account _account) => account2 = _account);

        bool successful = false;
        yield return account1.Transfer(account2.GetID(), asset.Id, 10, () => successful = true, DefaultErrorHandler);
        Assert.True(successful);
    }

    // should fail if operation is executed before block defined by 'greater than' block height rule
    [UnityTest]
    public IEnumerator AuthDescriptorRuleTestRun9()
    {
        yield return SetupBlockchain();
        Asset asset = null;
        yield return CreateAsset(blockchain, (Asset _asset) => asset = _asset);

        User user = TestUser.SingleSig(Rules.BlockHeight().GreaterThan(10000));
        Account account1 = null;
        yield return SourceAccount(blockchain, user, asset, (Account _account) => account1 = _account);
        Account account2 = null;
        yield return DestinationAccount(blockchain, (Account _account) => account2 = _account);

        bool successful = false;
        yield return account1.Transfer(account2.GetID(), asset.Id, 10, () => successful = true, DefaultErrorHandler);
        Assert.False(successful);
    }

    // should succeed if operation is executed after block defined by 'greater than' block height rule
    [UnityTest]
    public IEnumerator AuthDescriptorRuleTestRun10()
    {
        yield return SetupBlockchain();
        Asset asset = null;
        yield return CreateAsset(blockchain, (Asset _asset) => asset = _asset);

        User user = TestUser.SingleSig(Rules.BlockHeight().GreaterThan(1));
        Account account1 = null;
        yield return SourceAccount(blockchain, user, asset, (Account _account) => account1 = _account);
        Account account2 = null;
        yield return DestinationAccount(blockchain, (Account _account) => account2 = _account);

        bool successful = false;
        yield return account1.Transfer(account2.GetID(), asset.Id, 10, () => successful = true, DefaultErrorHandler);
        Assert.True(successful);
    }

    // should be able to create complex rules
    [UnityTest]
    public IEnumerator AuthDescriptorRuleTestRun11()
    {
        yield return SetupBlockchain();
        Asset asset = null;
        yield return CreateAsset(blockchain, (Asset _asset) => asset = _asset);

        User user = TestUser.SingleSig(Rules.BlockHeight().GreaterThan(1).And().BlockHeight().LessThan(10000));
        Account account1 = null;
        yield return SourceAccount(blockchain, user, asset, (Account _account) => account1 = _account);
        Account account2 = null;
        yield return DestinationAccount(blockchain, (Account _account) => account2 = _account);

        bool successful = false;
        yield return account1.Transfer(account2.GetID(), asset.Id, 10, () => successful = true, DefaultErrorHandler);
        Assert.True(successful);
    }

    // should fail if block heights defined by 'greater than' and 'less than' block height rules are less than current block height
    [UnityTest]
    public IEnumerator AuthDescriptorRuleTestRun12()
    {
        yield return SetupBlockchain();
        Asset asset = null;
        yield return CreateAsset(blockchain, (Asset _asset) => asset = _asset);

        User user = TestUser.SingleSig(Rules.BlockHeight().GreaterThan(1).And().BlockHeight().LessThan(10));
        Account account1 = null;
        yield return SourceAccount(blockchain, user, asset, (Account _account) => account1 = _account);
        Account account2 = null;
        yield return DestinationAccount(blockchain, (Account _account) => account2 = _account);

        bool successful = false;
        yield return account1.Transfer(account2.GetID(), asset.Id, 10, () => successful = true, DefaultErrorHandler);
        Assert.False(successful);
    }

    // should fail if block times defined by 'greater than' and 'less than' block time rules are in the past
    [UnityTest]
    public IEnumerator AuthDescriptorRuleTestRun13()
    {
        yield return SetupBlockchain();
        Asset asset = null;
        yield return CreateAsset(blockchain, (Asset _asset) => asset = _asset);

        User user = TestUser.SingleSig(
            Rules.BlockTime().GreaterThan(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - 20000).
            And().
            BlockTime().LessThan(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - 10000)
            );
        Account account1 = null;
        yield return SourceAccount(blockchain, user, asset, (Account _account) => account1 = _account);
        Account account2 = null;
        yield return DestinationAccount(blockchain, (Account _account) => account2 = _account);

        bool successful = false;
        yield return account1.Transfer(account2.GetID(), asset.Id, 10, () => successful = true, DefaultErrorHandler);
        Assert.False(successful);
    }

    // should succeed if current time is within period defined by 'greater than' and 'less than' block time rules
    [UnityTest]
    public IEnumerator AuthDescriptorRuleTestRun14()
    {
        yield return SetupBlockchain();
        Asset asset = null;
        yield return CreateAsset(blockchain, (Asset _asset) => asset = _asset);

        User user = TestUser.SingleSig(
            Rules.BlockTime().GreaterThan(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - 10000).
            And().
            BlockTime().LessThan(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() + 10000)
            );
        Account account1 = null;
        yield return SourceAccount(blockchain, user, asset, (Account _account) => account1 = _account);
        Account account2 = null;
        yield return DestinationAccount(blockchain, (Account _account) => account2 = _account);

        bool successful = false;
        yield return account1.Transfer(account2.GetID(), asset.Id, 10, () => successful = true, DefaultErrorHandler);
        Assert.True(successful);
    }

    // should succeed if current time is within period defined by 'greater than' and 'less than' block time rules
    [UnityTest]
    public IEnumerator AuthDescriptorRuleTestRun15()
    {
        yield return SetupBlockchain();
        Asset asset = null;
        yield return CreateAsset(blockchain, (Asset _asset) => asset = _asset);

        User user1 = TestUser.SingleSig();
        User user2 = TestUser.SingleSig(Rules.OperationCount().LessThan(2));

        Account srcAccount1 = null;
        yield return SourceAccount(blockchain, user1, asset, (Account _account) => srcAccount1 = _account);
        Account destAccount = null;
        yield return DestinationAccount(blockchain, (Account _account) => destAccount = _account);

        // add expiring auth descriptor to the account
        yield return srcAccount1.AddAuthDescriptor(user2.AuthDescriptor, EmptyCallback, DefaultErrorHandler);

        // get the same account, but initialized with user2
        // object which contains expiring auth descriptor
        Account srcAccount2 = null;
        yield return blockchain.NewSession(user2).GetAccountById(srcAccount1.GetID(), (Account _account) => srcAccount2 = _account, DefaultErrorHandler);

        yield return srcAccount2.Transfer(destAccount.GetID(), asset.Id, 10, EmptyCallback, DefaultErrorHandler);

        // account descriptor used by user2 object has expired.
        // this operation call will delete it.
        // any other operation, which calls require_auth internally
        // would also delete expired auth descriptor.
        yield return srcAccount1.Transfer(destAccount.GetID(), asset.Id, 30, EmptyCallback, DefaultErrorHandler);

        yield return srcAccount1.Sync(EmptyCallback, DefaultErrorHandler);

        Assert.AreEqual(1, srcAccount1.AuthDescriptor.Count);
    }

    // shouldn't delete non-expired auth descriptor
    [UnityTest]
    public IEnumerator AuthDescriptorRuleTestRun16()
    {
        yield return SetupBlockchain();
        Asset asset = null;
        yield return CreateAsset(blockchain, (Asset _asset) => asset = _asset);

        User user1 = TestUser.SingleSig();
        User user2 = TestUser.SingleSig(Rules.OperationCount().LessThan(10));

        Account srcAccount1 = null;
        yield return SourceAccount(blockchain, user1, asset, (Account _account) => srcAccount1 = _account);
        Account destAccount = null;
        yield return DestinationAccount(blockchain, (Account _account) => destAccount = _account);

        // add expiring auth descriptor to the account
        yield return AddAuthDescriptorTo(srcAccount1, user1, user2, EmptyCallback);

        // get the same account, but initialized with user2
        // object which contains expiring auth descriptor
        Account srcAccount2 = null;
        yield return blockchain.NewSession(user2).GetAccountById(srcAccount1.GetID(), (Account _account) => srcAccount2 = _account, DefaultErrorHandler);

        // perform transfer with expiring auth descriptor.
        // auth descriptor didn't expire, because it's only used 1 out of 10 times.
        yield return srcAccount2.Transfer(destAccount.GetID(), asset.Id, 10, EmptyCallback, DefaultErrorHandler);

        // perform transfer using auth descriptor without rules
        yield return srcAccount1.Transfer(destAccount.GetID(), asset.Id, 10, EmptyCallback, DefaultErrorHandler);

        yield return srcAccount1.Sync(EmptyCallback, DefaultErrorHandler);

        Assert.AreEqual(2, srcAccount1.AuthDescriptor.Count);
    }

    // should delete only expired auth descriptor if multiple expiring descriptors exist
    [UnityTest]
    public IEnumerator AuthDescriptorRuleTestRun17()
    {
        yield return SetupBlockchain();
        Asset asset = null;
        yield return CreateAsset(blockchain, (Asset _asset) => asset = _asset);

        User user1 = TestUser.SingleSig();
        User user2 = TestUser.SingleSig(Rules.OperationCount().LessOrEqual(1));
        User user3 = TestUser.SingleSig(Rules.OperationCount().LessOrEqual(1));

        Account srcAccount1 = null;
        yield return SourceAccount(blockchain, user1, asset, (Account _account) => srcAccount1 = _account);
        Account destAccount = null;
        yield return DestinationAccount(blockchain, (Account _account) => destAccount = _account);

        yield return AddAuthDescriptorTo(srcAccount1, user1, user2, EmptyCallback);
        yield return AddAuthDescriptorTo(srcAccount1, user1, user3, EmptyCallback);

        Account srcAccount2 = null;
        yield return blockchain.NewSession(user2).GetAccountById(srcAccount1.GetID(), (Account _account) => srcAccount2 = _account, DefaultErrorHandler);

        yield return srcAccount2.Transfer(destAccount.GetID(), asset.Id, 50, EmptyCallback, DefaultErrorHandler);

        // this call will trigger deletion of expired auth descriptor (attached to user2)
        yield return srcAccount1.Transfer(destAccount.GetID(), asset.Id, 100, EmptyCallback, DefaultErrorHandler);

        yield return srcAccount1.Sync(EmptyCallback, DefaultErrorHandler);

        Assert.AreEqual(2, srcAccount1.AuthDescriptor.Count);
    }

    // should add auth descriptors
    [UnityTest]
    public IEnumerator AuthDescriptorRuleTestRun18()
    {
        yield return SetupBlockchain();
        Asset asset = null;
        yield return CreateAsset(blockchain, (Asset _asset) => asset = _asset);

        User user1 = TestUser.SingleSig();
        User user2 = TestUser.SingleSig(Rules.OperationCount().LessOrEqual(1));
        User user3 = TestUser.SingleSig(Rules.OperationCount().LessOrEqual(1));

        Account account = null;
        yield return SourceAccount(blockchain, user1, asset, (Account _account) => account = _account);

        yield return AddAuthDescriptorTo(account, user1, user2, EmptyCallback);
        yield return AddAuthDescriptorTo(account, user1, user3, EmptyCallback);

        yield return account.Sync(EmptyCallback, DefaultErrorHandler);

        Assert.AreEqual(3, account.AuthDescriptor.Count);
    }

    // should delete auth descriptors
    [UnityTest]
    public IEnumerator AuthDescriptorRuleTestRun19()
    {
        yield return SetupBlockchain();
        Asset asset = null;
        yield return CreateAsset(blockchain, (Asset _asset) => asset = _asset);

        User user1 = TestUser.SingleSig();
        User user2 = TestUser.SingleSig(Rules.OperationCount().LessOrEqual(1));
        User user3 = TestUser.SingleSig(Rules.OperationCount().LessOrEqual(1));

        Account account = null;
        yield return SourceAccount(blockchain, user1, asset, (Account _account) => account = _account);

        yield return account.AddAuthDescriptor(user2.AuthDescriptor, EmptyCallback, DefaultErrorHandler);
        yield return account.AddAuthDescriptor(user3.AuthDescriptor, EmptyCallback, DefaultErrorHandler);

        yield return account.DeleteAllAuthDescriptorsExclude(user1.AuthDescriptor, EmptyCallback, DefaultErrorHandler);
        Assert.AreEqual(1, account.AuthDescriptor.Count);

        yield return account.Sync(EmptyCallback, DefaultErrorHandler);

        Assert.AreEqual(1, account.AuthDescriptor.Count);
    }

    // should fail when deleting an auth descriptor which is not owned by the account
    [UnityTest]
    public IEnumerator AuthDescriptorRuleTestRun20()
    {
        yield return SetupBlockchain();
        Asset asset = null;
        yield return CreateAsset(blockchain, (Asset _asset) => asset = _asset);

        User user1 = TestUser.SingleSig();
        User user2 = TestUser.SingleSig();

        Account account1 = null;
        yield return SourceAccount(blockchain, user1, asset, (Account _account) => account1 = _account);
        Account account2 = null;
        yield return SourceAccount(blockchain, user2, asset, (Account _account) => account2 = _account);

        bool successful = false;
        yield return account1.DeleteAuthDescriptor(user2.AuthDescriptor, () => successful = true, DefaultErrorHandler);
        Assert.False(successful);
    }

    // should delete auth descriptor
    [UnityTest]
    public IEnumerator AuthDescriptorRuleTestRun21()
    {
        yield return SetupBlockchain();
        Asset asset = null;
        yield return CreateAsset(blockchain, (Asset _asset) => asset = _asset);

        User user1 = TestUser.SingleSig();
        User user2 = TestUser.SingleSig();

        Account account = null;
        yield return SourceAccount(blockchain, user1, asset, (Account _account) => account = _account);
        yield return account.AddAuthDescriptor(user2.AuthDescriptor, EmptyCallback, DefaultErrorHandler);
        yield return account.DeleteAuthDescriptor(user2.AuthDescriptor, EmptyCallback, DefaultErrorHandler);

        Assert.AreEqual(1, account.AuthDescriptor.Count);
    }

    // Should be able to create same rules with different value
    [UnityTest]
    public IEnumerator AuthDescriptorRuleTestRun22()
    {
        yield return SetupBlockchain();
        Asset asset = null;
        yield return CreateAsset(blockchain, (Asset _asset) => asset = _asset);

        var rules = Rules.BlockHeight().GreaterThan(1).And().BlockHeight().GreaterThan(10000).And().BlockTime().GreaterOrEqual(122222999);
        User user = TestUser.SingleSig(rules);

        Account account = null;
        yield return SourceAccount(blockchain, user, asset, (Account _account) => account = _account);

        Assert.AreEqual(1, account.AuthDescriptor.Count);
    }

    // shouldn't be able to create too many rules
    [UnityTest]
    public IEnumerator AuthDescriptorRuleTestRun23()
    {
        yield return SetupBlockchain();
        Asset asset = null;
        yield return CreateAsset(blockchain, (Asset _asset) => asset = _asset);

        var rules = Rules.BlockHeight().GreaterThan(1).And().BlockHeight().GreaterThan(10000).And().BlockTime().GreaterOrEqual(122222999);

        for (int i = 0; i < 400; i++)
        {
            rules = rules.And().BlockHeight().GreaterOrEqual(i);
        }

        User user = TestUser.SingleSig(rules);

        Account account = null;
        yield return SourceAccount(blockchain, user, asset, (Account _account) => account = _account);

        Assert.AreEqual(null, account);
    }
}
