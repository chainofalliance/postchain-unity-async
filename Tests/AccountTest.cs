using System.Collections.Generic;
using System.Collections;
using UnityEngine.TestTools;
using Chromia.Postchain.Ft3;
using NUnit.Framework;
using System;

public class AccountTest
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

    // Correctly creates keypair
    [UnityTest]
    public IEnumerator AccountTest1()
    {
        var keyPair = Chromia.Postchain.Client.PostchainUtil.MakeKeyPair();
        var user = new KeyPair(Util.ByteArrayToString(keyPair["privKey"]));

        Assert.AreEqual(user.PrivKey, keyPair["privKey"]);
        Assert.AreEqual(user.PubKey, keyPair["pubKey"]);
        yield return null;
    }

    // Register account on blockchain
    [UnityTest]
    public IEnumerator AccountTest2()
    {
        yield return SetupBlockchain();
        User user = TestUser.SingleSig();
        Account account = null;
        yield return blockchain.RegisterAccount(user.AuthDescriptor, user, (Account _account) => account = _account, DefaultErrorHandler);

        Assert.NotNull(account);
    }

    // can add new auth descriptor if has account edit rights
    [UnityTest]
    public IEnumerator AccountTest3()
    {
        yield return SetupBlockchain();
        User user = TestUser.SingleSig();

        AccountBuilder accountBuilder = AccountBuilder.CreateAccountBuilder(blockchain, user);
        accountBuilder.WithParticipants(new List<KeyPair>() { user.KeyPair });
        accountBuilder.WithPoints(1);

        Account account = null;
        yield return accountBuilder.Build((Account _account) => { account = _account; }, DefaultErrorHandler);
        Assert.NotNull(account);

        yield return account.AddAuthDescriptor(
            new SingleSignatureAuthDescriptor(
                    user.KeyPair.PubKey,
                    new List<FlagsType>() { FlagsType.Transfer }.ToArray()
            ),
            EmptyCallback, DefaultErrorHandler
        );

        Assert.AreEqual(2, account.AuthDescriptor.Count);
    }

    // cannot add new auth descriptor if account doesn't have account edit rights
    [UnityTest]
    public IEnumerator AccountTest4()
    {
        yield return SetupBlockchain();
        User user = TestUser.SingleSig();
        Account account = null;
        yield return Account.Register(
            new SingleSignatureAuthDescriptor(
                user.KeyPair.PubKey,
                new List<FlagsType>() { FlagsType.Transfer }.ToArray()
            ),
            blockchain.NewSession(user), (Account _account) => account = _account, DefaultErrorHandler
        );
        Assert.NotNull(account);

        yield return account.AddAuthDescriptor(
            new SingleSignatureAuthDescriptor(
                user.KeyPair.PubKey,
                new List<FlagsType>() { FlagsType.Transfer }.ToArray()
            ), EmptyCallback, DefaultErrorHandler
        );

        Assert.AreEqual(1, account.AuthDescriptor.Count);
    }

    // should create new multisig account
    [UnityTest]
    public IEnumerator AccountTest5()
    {
        yield return SetupBlockchain();
        User user1 = TestUser.SingleSig();
        User user2 = TestUser.SingleSig();

        AuthDescriptor multiSig = new MultiSignatureAuthDescriptor(
               new List<byte[]>(){
                    user1.KeyPair.PubKey, user2.KeyPair.PubKey
               },
               2,
               new List<FlagsType>() { FlagsType.Account, FlagsType.Transfer }.ToArray()
        );

        var tx = blockchain.Connection.NewTransaction(multiSig.Signers.ToArray(), (string error) => { });
        var op = AccountDevOperations.Register(multiSig);
        tx.AddOperation(op.Name, op.Args);
        tx.Sign(user1.KeyPair.PrivKey, user1.KeyPair.PubKey);
        tx.Sign(user2.KeyPair.PrivKey, user2.KeyPair.PubKey);

        bool successfully = false;
        yield return tx.PostAndWait(() => successfully = true);
        Assert.True(successfully);
    }

    // should update account if 2 signatures provided
    [UnityTest]
    public IEnumerator AccountTest6()
    {
        yield return SetupBlockchain();
        User user1 = TestUser.SingleSig();
        User user2 = TestUser.SingleSig();

        AuthDescriptor multisig = new MultiSignatureAuthDescriptor(
            new List<byte[]>(){
                user1.KeyPair.PubKey, user2.KeyPair.PubKey
            },
            2,
            new List<FlagsType>() { FlagsType.Account, FlagsType.Transfer }.ToArray()
        );
        yield return blockchain.TransactionBuilder()
            .Add(AccountDevOperations.Register(multisig))
            .Build(multisig.Signers.ToArray(), DefaultErrorHandler)
            .Sign(user1.KeyPair)
            .Sign(user2.KeyPair)
            .PostAndWait(EmptyCallback)
        ;

        Account account = null;
        yield return Account.GetById(multisig.ID, blockchain.NewSession(user1), (Account _account) => account = _account, DefaultErrorHandler);
        Assert.NotNull(account);

        AuthDescriptor authDescriptor = new SingleSignatureAuthDescriptor(
                user1.KeyPair.PubKey,
                new List<FlagsType>() { FlagsType.Transfer }.ToArray()
        );

        yield return blockchain.TransactionBuilder()
            .Add(AccountOperations.AddAuthDescriptor(account.Id, account.AuthDescriptor[0].ID, authDescriptor))
            .Build(account.AuthDescriptor[0].Signers.ToArray(), DefaultErrorHandler)
            .Sign(user1.KeyPair)
            .Sign(user2.KeyPair)
            .PostAndWait(EmptyCallback)
        ;

        yield return account.Sync(EmptyCallback, DefaultErrorHandler);

        Assert.AreEqual(2, account.AuthDescriptor.Count);
    }

    // should fail if only one signature provided
    [UnityTest]
    public IEnumerator AccountTest7()
    {
        yield return SetupBlockchain();
        User user1 = TestUser.SingleSig();
        User user2 = TestUser.SingleSig();

        AuthDescriptor multiSig = new MultiSignatureAuthDescriptor(
               new List<byte[]>(){
                    user1.KeyPair.PubKey, user2.KeyPair.PubKey
               },
               2,
               new List<FlagsType>() { FlagsType.Account, FlagsType.Transfer }.ToArray()
        );

        var tx = blockchain.Connection.NewTransaction(multiSig.Signers.ToArray(), (string error) => { });
        var op = AccountDevOperations.Register(multiSig);
        tx.AddOperation(op.Name, op.Args);
        tx.Sign(user1.KeyPair.PrivKey, user1.KeyPair.PubKey);
        tx.Sign(user2.KeyPair.PrivKey, user2.KeyPair.PubKey);

        bool successfully = false;
        yield return tx.PostAndWait(() => successfully = true);
        Assert.True(successfully);

        Account account = null;
        yield return blockchain.NewSession(user1).GetAccountById(multiSig.ID, (Account _account) => account = _account, DefaultErrorHandler);
        Assert.NotNull(account);

        successfully = false;
        yield return account.AddAuthDescriptor(
            new SingleSignatureAuthDescriptor(
                user1.KeyPair.PubKey,
                new List<FlagsType>() { FlagsType.Transfer }.ToArray()
            ), () => successfully = true, DefaultErrorHandler
        );

        Assert.False(successfully);
        Assert.AreEqual(1, account.AuthDescriptor.Count);
    }

    // should be returned when queried by participant id
    [UnityTest]
    public IEnumerator AccountTest8()
    {
        yield return SetupBlockchain();
        User user = TestUser.SingleSig();
        AccountBuilder accountBuilder = AccountBuilder.CreateAccountBuilder(blockchain, user);
        accountBuilder.WithParticipants(new List<KeyPair>() { user.KeyPair });

        Account account = null;
        yield return accountBuilder.Build((Account _account) => { account = _account; }, DefaultErrorHandler);

        Account[] accounts = null;
        yield return Account.GetByParticipantId(
            Util.ByteArrayToString(user.KeyPair.PubKey),
            blockchain.NewSession(user),
            (Account[] _accounts) => { accounts = _accounts; },
            DefaultErrorHandler
        );
        Assert.AreEqual(1, accounts.Length);
        Assert.AreEqual(Util.ByteArrayToString(user.KeyPair.PubKey), Util.ByteArrayToString(accounts[0].AuthDescriptor[0].Signers[0]));
    }

    // should return two accounts when account is participant of two accounts
    [UnityTest]
    public IEnumerator AccountTest9()
    {
        yield return SetupBlockchain();
        User user1 = TestUser.SingleSig();
        User user2 = TestUser.SingleSig();

        AccountBuilder accountBuilder = AccountBuilder.CreateAccountBuilder(blockchain, user1);
        accountBuilder.WithParticipants(new List<KeyPair>() { user1.KeyPair });
        Account account = null;
        yield return accountBuilder.Build((Account _account) => { account = _account; }, DefaultErrorHandler);

        AccountBuilder accountBuilder2 = AccountBuilder.CreateAccountBuilder(blockchain, user2);
        accountBuilder2.WithParticipants(new List<KeyPair>() { user2.KeyPair });
        accountBuilder2.WithPoints(1);
        Account account2 = null;
        yield return accountBuilder2.Build((Account _account) => { account2 = _account; }, DefaultErrorHandler);

        yield return AddAuthDescriptorTo(account2, user2, user1, EmptyCallback);

        Account[] accounts = null;
        yield return Account.GetByParticipantId(
            Util.ByteArrayToString(user1.KeyPair.PubKey),
            blockchain.NewSession(user1),
            (Account[] _accounts) => { accounts = _accounts; },
            DefaultErrorHandler
        );

        Assert.AreEqual(2, accounts.Length);
    }

    // should return account by id
    [UnityTest]
    public IEnumerator AccountTest10()
    {
        yield return SetupBlockchain();
        User user = TestUser.SingleSig();

        AccountBuilder accountBuilder = AccountBuilder.CreateAccountBuilder(blockchain, user);
        Account account = null;
        yield return accountBuilder.Build((Account _account) => { account = _account; }, DefaultErrorHandler);

        yield return Account.GetById(account.Id, blockchain.NewSession(user),
        (Account _account) => Assert.AreEqual(account.Id.ToUpper(), _account.Id.ToUpper()), DefaultErrorHandler);
    }

    // should have only one auth descriptor after calling deleteAllAuthDescriptorsExclude
    [UnityTest]
    public IEnumerator AccountTest11()
    {
        yield return SetupBlockchain();
        User user1 = TestUser.SingleSig();
        User user2 = TestUser.SingleSig();
        User user3 = TestUser.SingleSig();

        AccountBuilder accountBuilder = AccountBuilder.CreateAccountBuilder(blockchain, user1);
        accountBuilder.WithParticipants(new List<KeyPair>() { user1.KeyPair });
        accountBuilder.WithPoints(4);

        Account account = null;
        yield return accountBuilder.Build((Account _account) => { account = _account; }, DefaultErrorHandler);

        yield return AddAuthDescriptorTo(account, user1, user2, EmptyCallback);
        yield return AddAuthDescriptorTo(account, user1, user3, EmptyCallback);

        yield return account.DeleteAllAuthDescriptorsExclude(user1.AuthDescriptor, EmptyCallback, DefaultErrorHandler);
        yield return blockchain.NewSession(user1).GetAccountById(account.Id,
            (Account _account) => account = _account, DefaultErrorHandler
        );
        Assert.AreEqual(1, account.AuthDescriptor.Count);
    }

    // should be able to register account by directly calling \'register_account\' operation
    [UnityTest]
    public IEnumerator AccountTest12()
    {
        yield return SetupBlockchain();

        User user = TestUser.SingleSig();

        yield return blockchain.Call(AccountOperations.Op("ft3.dev_register_account",
            new object[] { user.AuthDescriptor.ToGTV() })
        , user, EmptyCallback, DefaultErrorHandler);

        BlockchainSession session = blockchain.NewSession(user);

        Account account = null;
        yield return session.GetAccountById(user.AuthDescriptor.ID, (Account _account) => account = _account, DefaultErrorHandler);
        Assert.NotNull(account);
    }

    // should be possible for auth descriptor to delete itself without admin flag
    [UnityTest]
    public IEnumerator AccountTest13()
    {
        yield return SetupBlockchain();
        User user1 = TestUser.SingleSig();

        AccountBuilder accountBuilder = AccountBuilder.CreateAccountBuilder(blockchain, user1);
        accountBuilder.WithParticipants(new List<KeyPair>() { user1.KeyPair });
        accountBuilder.WithPoints(4);

        Account account = null;
        yield return accountBuilder.Build((Account _account) => { account = _account; });

        KeyPair keyPair = new KeyPair();
        User user2 = new User(keyPair,
            new SingleSignatureAuthDescriptor(
                keyPair.PubKey,
                new FlagsType[] { FlagsType.Transfer }
            )
        );

        yield return AddAuthDescriptorTo(account, user1, user2, EmptyCallback);

        Account account2 = null;
        yield return blockchain.NewSession(user2).GetAccountById(account.Id, (Account _account) => account2 = _account, DefaultErrorHandler);
        bool successfully = false;
        yield return account2.DeleteAuthDescriptor(user2.AuthDescriptor, () => successfully = true, DefaultErrorHandler);

        Assert.True(successfully);
        yield return account2.Sync(EmptyCallback, DefaultErrorHandler);
        Assert.AreEqual(1, account2.AuthDescriptor.Count);

    }

    // shouldn't be possible for auth descriptor to delete other auth descriptor without admin flag
    [UnityTest]
    public IEnumerator AccountTest15()
    {
        yield return SetupBlockchain();
        User user1 = TestUser.SingleSig();

        Account account = null;
        yield return AccountBuilder.CreateAccountBuilder(blockchain, user1)
            .WithParticipants(new KeyPair[] { user1.KeyPair })
            .WithPoints(4)
            .Build((Account _account) => account = _account);

        KeyPair keyPair2 = new KeyPair();
        User user2 = new User(keyPair2,
            new SingleSignatureAuthDescriptor(
                keyPair2.PubKey,
                new FlagsType[] { FlagsType.Transfer }
            )
        );

        KeyPair keyPair3 = new KeyPair();
        User user3 = new User(keyPair3,
            new SingleSignatureAuthDescriptor(
                keyPair3.PubKey,
                new FlagsType[] { FlagsType.Transfer }
            )
        );

        yield return AddAuthDescriptorTo(account, user1, user2, EmptyCallback);
        yield return AddAuthDescriptorTo(account, user1, user3, EmptyCallback);

        Account account2 = null;
        yield return blockchain.NewSession(user3).GetAccountById(account.Id, (Account _account) => account2 = _account, DefaultErrorHandler);

        bool successfully = false;
        yield return account2.DeleteAuthDescriptor(user2.AuthDescriptor, () => successfully = true, DefaultErrorHandler);
        Assert.False(successfully);
    }
}
