using System.Collections.Generic;
using UnityEngine.TestTools;
using Chromia.Postchain.Client;
using Chromia.Postchain.Ft3;
using NUnit.Framework;

using Cysharp.Threading.Tasks;

public class AccountTest
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

    // Correctly creates keypair
    [UnityTest]
    public void AccountTest1()
    {
        var keyPair = PostchainUtil.MakeKeyPair();
        var user = new KeyPair(Util.ByteArrayToString(keyPair["privKey"]));

        Assert.AreEqual(user.PrivKey, keyPair["privKey"]);
        Assert.AreEqual(user.PubKey, keyPair["pubKey"]);
    }

    // Register account on blockchain
    [UnityTest]
    public async UniTask AccountTest2()
    {
        var blockchain = await BlockchainUtil.GetDefaultBlockchain();
        User user = TestUser.SingleSig();
        var res = await blockchain.RegisterAccount(user.AuthDescriptor, user);

        Assert.IsFalse(res.Error);
        Assert.NotNull(res.Content);
    }

    // can add new auth descriptor if has account edit rights
    [UnityTest]
    public async UniTask AccountTest3()
    {
        var blockchain = await BlockchainUtil.GetDefaultBlockchain();
        User user = TestUser.SingleSig();

        AccountBuilder accountBuilder = AccountBuilder.CreateAccountBuilder(blockchain, user);
        accountBuilder.WithParticipants(new List<KeyPair>() { user.KeyPair });
        accountBuilder.WithPoints(1);

        var res = await accountBuilder.Build();

        Assert.IsFalse(res.Error);
        Assert.NotNull(res.Content);

        await res.Content.AddAuthDescriptor(
            new SingleSignatureAuthDescriptor(
                    user.KeyPair.PubKey,
                    new List<FlagsType>() { FlagsType.Transfer }.ToArray()
            )
        );

        Assert.AreEqual(2, res.Content.AuthDescriptor.Count);
    }

    // cannot add new auth descriptor if account doesn't have account edit rights
    [UnityTest]
    public async UniTask AccountTest4()
    {
        var blockchain = await BlockchainUtil.GetDefaultBlockchain();
        User user = TestUser.SingleSig();
        var res = await Account.Register(
            new SingleSignatureAuthDescriptor(
                user.KeyPair.PubKey,
                new List<FlagsType>() { FlagsType.Transfer }.ToArray()
            ),
            blockchain.NewSession(user)
        );

        Assert.IsFalse(res.Error);
        Assert.NotNull(res.Content);

        await res.Content.AddAuthDescriptor(
            new SingleSignatureAuthDescriptor(
                user.KeyPair.PubKey,
                new List<FlagsType>() { FlagsType.Transfer }.ToArray()
            )
        );

        Assert.AreEqual(1, res.Content.AuthDescriptor.Count);
    }

    // should create new multisig account
    [UnityTest]
    public async UniTask AccountTest5()
    {
        var blockchain = await BlockchainUtil.GetDefaultBlockchain();
        User user1 = TestUser.SingleSig();
        User user2 = TestUser.SingleSig();

        AuthDescriptor multiSig = new MultiSignatureAuthDescriptor(
               new List<byte[]>(){
                    user1.KeyPair.PubKey, user2.KeyPair.PubKey
               },
               2,
               new List<FlagsType>() { FlagsType.Account, FlagsType.Transfer }.ToArray()
        );

        var tx = blockchain.Connection.NewTransaction(multiSig.Signers.ToArray());
        var op = AccountDevOperations.Register(multiSig);
        tx.AddOperation(op.Name, op.Args);
        tx.Sign(user1.KeyPair.PrivKey, user1.KeyPair.PubKey);
        tx.Sign(user2.KeyPair.PrivKey, user2.KeyPair.PubKey);

        var res = await tx.PostAndWait();
        Assert.False(res.Error);
    }

    // should update account if 2 signatures provided
    [UnityTest]
    public async UniTask AccountTest6()
    {
        var blockchain = await BlockchainUtil.GetDefaultBlockchain();
        User user1 = TestUser.SingleSig();
        User user2 = TestUser.SingleSig();

        AuthDescriptor multisig = new MultiSignatureAuthDescriptor(
            new List<byte[]>(){
                user1.KeyPair.PubKey, user2.KeyPair.PubKey
            },
            2,
            new List<FlagsType>() { FlagsType.Account, FlagsType.Transfer }.ToArray()
        );
        await blockchain.TransactionBuilder()
            .Add(AccountDevOperations.Register(multisig))
            .Build(multisig.Signers.ToArray())
            .Sign(user1.KeyPair)
            .Sign(user2.KeyPair)
            .PostAndWait()
        ;

        var res = await Account.GetById(multisig.ID, blockchain.NewSession(user1));
        Assert.IsFalse(res.Error);

        var account = res.Content;
        Assert.NotNull(account);

        AuthDescriptor authDescriptor = new SingleSignatureAuthDescriptor(
                user1.KeyPair.PubKey,
                new List<FlagsType>() { FlagsType.Transfer }.ToArray()
        );

        var addAuthRes = await blockchain.TransactionBuilder()
            .Add(AccountOperations.AddAuthDescriptor(account.Id, account.AuthDescriptor[0].ID, authDescriptor))
            .Build(account.AuthDescriptor[0].Signers.ToArray())
            .Sign(user1.KeyPair)
            .Sign(user2.KeyPair)
            .PostAndWait()
        ;

        await account.Sync();

        Assert.AreEqual(2, account.AuthDescriptor.Count);
    }

    // should fail if only one signature provided
    [UnityTest]
    public async UniTask AccountTest7()
    {
        var blockchain = await BlockchainUtil.GetDefaultBlockchain();
        User user1 = TestUser.SingleSig();
        User user2 = TestUser.SingleSig();

        AuthDescriptor multiSig = new MultiSignatureAuthDescriptor(
               new List<byte[]>(){
                    user1.KeyPair.PubKey, user2.KeyPair.PubKey
               },
               2,
               new List<FlagsType>() { FlagsType.Account, FlagsType.Transfer }.ToArray()
        );

        var tx = blockchain.Connection.NewTransaction(multiSig.Signers.ToArray());
        var op = AccountDevOperations.Register(multiSig);
        tx.AddOperation(op.Name, op.Args);
        tx.Sign(user1.KeyPair.PrivKey, user1.KeyPair.PubKey);
        tx.Sign(user2.KeyPair.PrivKey, user2.KeyPair.PubKey);

        var res = await tx.PostAndWait();
        Assert.False(res.Error);

        var resAccount = await blockchain.NewSession(user1).GetAccountById(multiSig.ID);
        Assert.False(resAccount.Error);
        Assert.NotNull(resAccount.Content);

        var resAddAuth = await resAccount.Content.AddAuthDescriptor(
            new SingleSignatureAuthDescriptor(
                user1.KeyPair.PubKey,
                new List<FlagsType>() { FlagsType.Transfer }.ToArray()
            )
        );

        Assert.False(resAddAuth.Error);
        Assert.AreEqual(1, resAccount.Content.AuthDescriptor.Count);
    }

    // should be returned when queried by participant id
    [UnityTest]
    public async UniTask AccountTest8()
    {
        var blockchain = await BlockchainUtil.GetDefaultBlockchain();
        User user = TestUser.SingleSig();
        AccountBuilder accountBuilder = AccountBuilder.CreateAccountBuilder(blockchain, user);
        accountBuilder.WithParticipants(new List<KeyPair>() { user.KeyPair });

        await accountBuilder.Build();

        var res = await Account.GetByParticipantId(
            Util.ByteArrayToString(user.KeyPair.PubKey),
            blockchain.NewSession(user)
        );

        Assert.False(res.Error);
        Account[] accounts = res.Content;
        Assert.AreEqual(1, accounts.Length);
        Assert.AreEqual(Util.ByteArrayToString(user.KeyPair.PubKey), Util.ByteArrayToString(accounts[0].AuthDescriptor[0].Signers[0]));
    }

    // should return two accounts when account is participant of two accounts
    [UnityTest]
    public async UniTask AccountTest9()
    {
        var blockchain = await BlockchainUtil.GetDefaultBlockchain();
        User user1 = TestUser.SingleSig();
        User user2 = TestUser.SingleSig();

        AccountBuilder accountBuilder = AccountBuilder.CreateAccountBuilder(blockchain, user1);
        accountBuilder.WithParticipants(new List<KeyPair>() { user1.KeyPair });
        await accountBuilder.Build();

        AccountBuilder accountBuilder2 = AccountBuilder.CreateAccountBuilder(blockchain, user2);
        accountBuilder2.WithParticipants(new List<KeyPair>() { user2.KeyPair });
        accountBuilder2.WithPoints(1);
        var res = await accountBuilder2.Build();
        var account2 = res.Content;

        await AddAuthDescriptorTo(account2, user2, user1, blockchain);

        var qRes = await Account.GetByParticipantId(
            Util.ByteArrayToString(user1.KeyPair.PubKey),
            blockchain.NewSession(user1)
        );

        Assert.False(qRes.Error);
        Assert.AreEqual(2, qRes.Content.Length);
    }

    // should return account by id
    [UnityTest]
    public async UniTask AccountTest10()
    {
        var blockchain = await BlockchainUtil.GetDefaultBlockchain();
        User user = TestUser.SingleSig();

        AccountBuilder accountBuilder = AccountBuilder.CreateAccountBuilder(blockchain, user);
        var res = await accountBuilder.Build();
        var account = res.Content;

        var qRes = await Account.GetById(account.Id, blockchain.NewSession(user));

        Assert.AreEqual(account.Id.ToUpper(), qRes.Content.Id.ToUpper());
    }

    // should have only one auth descriptor after calling deleteAllAuthDescriptorsExclude
    [UnityTest]
    public async UniTask AccountTest11()
    {
        var blockchain = await BlockchainUtil.GetDefaultBlockchain();
        User user1 = TestUser.SingleSig();
        User user2 = TestUser.SingleSig();
        User user3 = TestUser.SingleSig();

        AccountBuilder accountBuilder = AccountBuilder.CreateAccountBuilder(blockchain, user1);
        accountBuilder.WithParticipants(new List<KeyPair>() { user1.KeyPair });
        accountBuilder.WithPoints(4);

        var res = await accountBuilder.Build();
        Account account = res.Content;

        await AddAuthDescriptorTo(account, user1, user2, blockchain);
        await AddAuthDescriptorTo(account, user1, user3, blockchain);

        await account.DeleteAllAuthDescriptorsExclude(user1.AuthDescriptor);
        Assert.AreEqual(1, account.AuthDescriptor.Count);
    }

    // should be able to register account by directly calling \'register_account\' operation
    [UnityTest]
    public async UniTask AccountTest12()
    {
        var blockchain = await BlockchainUtil.GetDefaultBlockchain();

        User user = TestUser.SingleSig();

        await blockchain.Call(AccountOperations.Op("ft3.dev_register_account",
            new object[] { user.AuthDescriptor.ToGTV() }), user);

        BlockchainSession session = blockchain.NewSession(user);

        var res = await session.GetAccountById(user.AuthDescriptor.ID);
        Assert.NotNull(res.Content);
    }

    // should be possible for auth descriptor to delete itself without admin flag
    [UnityTest]
    public async UniTask AccountTest13()
    {
        var blockchain = await BlockchainUtil.GetDefaultBlockchain();
        User user1 = TestUser.SingleSig();

        AccountBuilder accountBuilder = AccountBuilder.CreateAccountBuilder(blockchain, user1);
        accountBuilder.WithParticipants(new List<KeyPair>() { user1.KeyPair });
        accountBuilder.WithPoints(4);

        var res = await accountBuilder.Build();
        var account = res.Content;

        KeyPair keyPair = new KeyPair();
        User user2 = new User(keyPair,
            new SingleSignatureAuthDescriptor(
                keyPair.PubKey,
                new FlagsType[] { FlagsType.Transfer }
            )
        );

        await AddAuthDescriptorTo(account, user1, user2, blockchain);

        res = await blockchain.NewSession(user2).GetAccountById(account.Id);
        Account account2 = res.Content;

        var delRes = await account2.DeleteAuthDescriptor(user2.AuthDescriptor);

        Assert.False(delRes.Error);
        await account2.Sync();
        Assert.AreEqual(1, account2.AuthDescriptor.Count);

    }

    // shouldn't be possible for auth descriptor to delete other auth descriptor without admin flag
    [UnityTest]
    public async UniTask AccountTest15()
    {
        var blockchain = await BlockchainUtil.GetDefaultBlockchain();
        User user1 = TestUser.SingleSig();

        var res = await AccountBuilder.CreateAccountBuilder(blockchain, user1)
            .WithParticipants(new KeyPair[] { user1.KeyPair })
            .WithPoints(4)
            .Build();

        var account = res.Content;

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

        await AddAuthDescriptorTo(account, user1, user2, blockchain);
        await AddAuthDescriptorTo(account, user1, user3, blockchain);

        var res2 = await blockchain.NewSession(user3).GetAccountById(account.Id);

        var qRes = await res2.Content.DeleteAuthDescriptor(user2.AuthDescriptor);
        Assert.False(qRes.Error);
    }
}
