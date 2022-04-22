using System.Collections.Generic;
using System.Collections;
using UnityEngine.TestTools;
using Chromia.Postchain.Ft3;
using NUnit.Framework;
using System;
using System.Linq;

public class RateLimitTest
{
    const int REQUEST_MAX_COUNT = 10;
    const int RECOVERY_TIME = 5000;
    const int POINTS_AT_ACCOUNT_CREATION = 1;
    private Blockchain blockchain;

    private IEnumerator SetupBlockchain()
    {
        yield return BlockchainUtil.GetDefaultBlockchain((Blockchain _blockchain) => { blockchain = _blockchain; });
    }

    private void DefaultErrorHandler(string error) { UnityEngine.Debug.Log(error); }
    private void EmptyCallback() { }

    // Should have a limit of 10 requests per minute
    [UnityTest]
    public IEnumerator RateLimitTestRun1()
    {
        yield return SetupBlockchain();
        BlockchainInfo info = null;
        yield return BlockchainInfo.GetInfo(blockchain.Connection,
            (BlockchainInfo _info) => info = _info, DefaultErrorHandler
        );

        Assert.AreEqual(REQUEST_MAX_COUNT, info.RateLimitInfo.MaxPoints);
        Assert.AreEqual(RECOVERY_TIME, info.RateLimitInfo.RecoveryTime);
    }

    // should show 10 at request count
    [UnityTest]
    public IEnumerator RateLimitTestRun2()
    {
        yield return SetupBlockchain();
        User user = TestUser.SingleSig();

        AccountBuilder builder = AccountBuilder.CreateAccountBuilder(blockchain, user);
        builder.WithParticipants(new List<KeyPair>() { user.KeyPair });
        Account account = null;
        yield return builder.Build((Account _account) => account = _account);

        yield return account.Sync(EmptyCallback, DefaultErrorHandler);
        Assert.AreEqual(1, account.RateLimit.Points);
    }

    // waits 20 seconds and gets 4 points
    [UnityTest]
    public IEnumerator RateLimitTestRun3()
    {
        yield return SetupBlockchain();
        User user = TestUser.SingleSig();

        AccountBuilder builder = AccountBuilder.CreateAccountBuilder(blockchain, user);
        builder.WithParticipants(new List<KeyPair>() { user.KeyPair });
        Account account = null;
        yield return builder.Build((Account _account) => account = _account);

        yield return new UnityEngine.WaitForSeconds(20);

        yield return RateLimit.ExecFreeOperation(account.GetID(), blockchain, EmptyCallback, DefaultErrorHandler); // used to make one block
        yield return RateLimit.ExecFreeOperation(account.GetID(), blockchain, EmptyCallback, DefaultErrorHandler); // used to calculate the last block's timestamp (previous block).
        // check the balance
        yield return account.Sync(EmptyCallback, DefaultErrorHandler);
        Assert.AreEqual(4 + POINTS_AT_ACCOUNT_CREATION, account.RateLimit.Points); // 20 seconds / 5s recovery time
    }

    // can make 4 operations
    [UnityTest]
    public IEnumerator RateLimitTestRun4()
    {
        yield return SetupBlockchain();
        User user = TestUser.SingleSig();

        AccountBuilder builder = AccountBuilder.CreateAccountBuilder(blockchain, user);
        builder.WithParticipants(new List<KeyPair>() { user.KeyPair });
        builder.WithPoints(4);
        Account account = null;
        yield return builder.Build((Account _account) => account = _account);
        Assert.NotNull(account);

        bool successful = false;
        yield return MakeRequests(account, 4 + POINTS_AT_ACCOUNT_CREATION, () => successful = true);
        Assert.True(successful);

        yield return account.Sync(EmptyCallback, DefaultErrorHandler);
        Assert.AreEqual(0, account.RateLimit.Points);
    }

    // can't make another operation because she has 0 points
    [UnityTest]
    public IEnumerator RateLimitTestRun5()
    {
        yield return SetupBlockchain();
        User user = TestUser.SingleSig();

        AccountBuilder builder = AccountBuilder.CreateAccountBuilder(blockchain, user);
        builder.WithParticipants(new List<KeyPair>() { user.KeyPair });
        builder.WithPoints(4);
        Account account = null;
        yield return builder.Build((Account _account) => account = _account);

        bool successful = false;
        yield return MakeRequests(account, 4 + POINTS_AT_ACCOUNT_CREATION, () => successful = true);
        Assert.True(successful);

        yield return account.Sync(EmptyCallback, DefaultErrorHandler);

        successful = false;
        yield return MakeRequests(account, 8, () => successful = true);
        Assert.False(successful);
    }


    public IEnumerator MakeRequests(Account account, int requests, Action onSuccess)
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

        var tx = txBuilder.Build(signers.ToArray(), DefaultErrorHandler);
        tx.Sign(account.Session.User.KeyPair);

        foreach (var user in users)
        {
            tx.Sign(user.KeyPair);
        }

        yield return tx.PostAndWait(onSuccess);
    }
}
