using System.Collections.Generic;
using Chromia.Postchain.Client;
using Chromia.Postchain.Ft3;
using NUnit.Framework;
using System;

using Cysharp.Threading.Tasks;

public class RateLimitTest
{
    const int REQUEST_MAX_COUNT = 10;
    const int RECOVERY_TIME = 5000;
    const int POINTS_AT_ACCOUNT_CREATION = 1;

    // Should have a limit of 10 requests per minute
    [Test]
    public async void RateLimitTestRun1()
    {
        var blockchain = await BlockchainUtil.GetDefaultBlockchain();
        var info = await BlockchainInfo.GetInfo(blockchain.Connection);

        Assert.AreEqual(REQUEST_MAX_COUNT, info.Content.RateLimitInfo.MaxPoints);
        Assert.AreEqual(RECOVERY_TIME, info.Content.RateLimitInfo.RecoveryTime);
    }

    // should show 10 at request count
    [Test]
    public async void RateLimitTestRun2()
    {
        var blockchain = await BlockchainUtil.GetDefaultBlockchain();
        User user = TestUser.SingleSig();

        AccountBuilder builder = AccountBuilder.CreateAccountBuilder(blockchain, user);
        builder.WithParticipants(new List<KeyPair>() { user.KeyPair });
        var account = await builder.Build();

        await account.Content.Sync();
        Assert.AreEqual(1, account.Content.RateLimit.Points);
    }

    // waits 20 seconds and gets 4 points
    [Test]
    public async void RateLimitTestRun3()
    {
        var blockchain = await BlockchainUtil.GetDefaultBlockchain();
        User user = TestUser.SingleSig();

        AccountBuilder builder = AccountBuilder.CreateAccountBuilder(blockchain, user);
        builder.WithParticipants(new List<KeyPair>() { user.KeyPair });
        var account = await builder.Build();

        await UniTask.Delay(TimeSpan.FromSeconds(20), ignoreTimeScale: false);

        await RateLimit.ExecFreeOperation(account.Content.GetID(), blockchain); // used to make one block
        await RateLimit.ExecFreeOperation(account.Content.GetID(), blockchain); // used to calculate the last block's timestamp (previous block).
        // check the balance
        await account.Content.Sync();
        Assert.AreEqual(4 + POINTS_AT_ACCOUNT_CREATION, account.Content.RateLimit.Points); // 20 seconds / 5s recovery time
    }

    // can make 4 operations
    [Test]
    public async void RateLimitTestRun4()
    {
        var blockchain = await BlockchainUtil.GetDefaultBlockchain();
        User user = TestUser.SingleSig();

        AccountBuilder builder = AccountBuilder.CreateAccountBuilder(blockchain, user);
        builder.WithParticipants(new List<KeyPair>() { user.KeyPair });
        builder.WithPoints(4);
        var account = await builder.Build();

        Assert.IsFalse(account.Error);
        Assert.NotNull(account.Content);

        await MakeRequests(account.Content, 4 + POINTS_AT_ACCOUNT_CREATION, blockchain);

        await account.Content.Sync();
        Assert.AreEqual(0, account.Content.RateLimit.Points);
    }

    // can't make another operation because she has 0 points
    [Test]
    public async void RateLimitTestRun5()
    {
        var blockchain = await BlockchainUtil.GetDefaultBlockchain();
        User user = TestUser.SingleSig();

        AccountBuilder builder = AccountBuilder.CreateAccountBuilder(blockchain, user);
        builder.WithParticipants(new List<KeyPair>() { user.KeyPair });
        builder.WithPoints(4);
        var account = await builder.Build();

        var res = await MakeRequests(account.Content, 4 + POINTS_AT_ACCOUNT_CREATION, blockchain);
        Assert.False(res.Error);

        await account.Content.Sync();

        res = await MakeRequests(account.Content, 8, blockchain);
        Assert.True(res.Error);
    }


    public UniTask<PostchainResponse<string>> MakeRequests(Account account, int requests, Blockchain blockchain)
    {
        var txBuilder = blockchain.TransactionBuilder();
        var signers = new List<byte[]>();
        var users = new List<User>();
        signers.AddRange(account.Session.User.AuthDescriptor.Signers);

        for (int i = 0; i < requests; i++)
        {
            var user = TestUser.SingleSig();
            signers.AddRange(user.AuthDescriptor.Signers);
            users.Add(user);

            txBuilder.Add(AccountOperations.AddAuthDescriptor(account.Id, account.Session.User.AuthDescriptor.ID, user.AuthDescriptor));
        }

        var tx = txBuilder.Build(signers.ToArray());
        tx.Sign(account.Session.User.KeyPair);

        foreach (var user in users)
        {
            tx.Sign(user.KeyPair);
        }

        return tx.PostAndWait();
    }
}
