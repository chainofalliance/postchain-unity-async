using System.Collections.Generic;
using System.Collections;
using UnityEngine.TestTools;
using Chromia.Postchain.Ft3;
using NUnit.Framework;

public class BlockchainTest
{
    private Blockchain blockchain;

    private IEnumerator SetupBlockchain()
    {
        yield return BlockchainUtil.GetDefaultBlockchain((Blockchain _blockchain) => { blockchain = _blockchain; });
    }

    private void DefaultErrorHandler(string error) { UnityEngine.Debug.Log(error); }
    private void EmptyCallback() { }

    // should provide info
    [UnityTest]
    public IEnumerator BlockchainTestRun1()
    {
        yield return SetupBlockchain();
        yield return BlockchainInfo.GetInfo(blockchain.Connection,
        (BlockchainInfo info) => { Assert.AreEqual(info.Name, "Unity FT3"); }, DefaultErrorHandler);
    }

    // should be able to register an account
    [UnityTest]
    public IEnumerator BlockchainTestRun2()
    {
        yield return SetupBlockchain();

        User user = TestUser.SingleSig();
        BlockchainSession session = blockchain.NewSession(user);

        Account account = null;
        Account foundAccount = null;
        yield return blockchain.RegisterAccount(user.AuthDescriptor, user, (Account _account) => { account = _account; }, DefaultErrorHandler);
        yield return session.GetAccountById(account.Id, (Account _account) => { foundAccount = _account; }, DefaultErrorHandler);

        Assert.AreEqual(account.Id.ToUpper(), foundAccount.Id.ToUpper());
    }

    // should return account by participant id
    [UnityTest]
    public IEnumerator BlockchainTestRun3()
    {
        yield return SetupBlockchain();
        User user = TestUser.SingleSig();

        AccountBuilder accountBuilder = AccountBuilder.CreateAccountBuilder(blockchain, user);
        accountBuilder.WithParticipants(new List<KeyPair>() { user.KeyPair });

        Account account = null;
        Account[] foundAccounts = null;
        yield return accountBuilder.Build((Account _account) => { account = _account; });

        yield return blockchain.GetAccountsByParticipantId(
            Util.ByteArrayToString(user.KeyPair.PubKey), user, (Account[] _accounts) => { foundAccounts = _accounts; }, DefaultErrorHandler
        );

        Assert.AreEqual(1, foundAccounts.Length);
        Assert.AreEqual(account.Id.ToUpper(), foundAccounts[0].Id.ToUpper());
    }

    // should return account by auth descriptor id
    [UnityTest]
    public IEnumerator BlockchainTestRun4()
    {
        yield return SetupBlockchain();
        User user = TestUser.SingleSig();

        AccountBuilder accountBuilder = AccountBuilder.CreateAccountBuilder(blockchain, user);
        accountBuilder.WithParticipants(new List<KeyPair>() { user.KeyPair });

        Account account = null;
        Account[] foundAccounts = null;
        yield return accountBuilder.Build((Account _account) => { account = _account; });

        yield return blockchain.GetAccountsByAuthDescriptorId(
            user.AuthDescriptor.ID, user, (Account[] _accounts) => { foundAccounts = _accounts; }, DefaultErrorHandler
        );

        Assert.AreEqual(1, foundAccounts.Length);
        Assert.AreEqual(account.Id.ToUpper(), foundAccounts[0].Id.ToUpper());
    }

    // should be able to link other chain
    [UnityTest]
    public IEnumerator BlockchainTestRun5()
    {
        yield return SetupBlockchain();

        var chainId1 = TestUtil.GenerateId();
        yield return blockchain.LinkChain(chainId1, EmptyCallback, DefaultErrorHandler);
        yield return blockchain.IsLinkedWithChain(chainId1, (bool isLinked) => Assert.True(isLinked), DefaultErrorHandler);
    }

    // should be able to link multiple chains
    [UnityTest]
    public IEnumerator BlockchainTestRun6()
    {
        yield return SetupBlockchain();
        var chainId1 = TestUtil.GenerateId();
        var chainId2 = TestUtil.GenerateId();

        yield return blockchain.LinkChain(chainId1, EmptyCallback, DefaultErrorHandler);
        yield return blockchain.LinkChain(chainId2, EmptyCallback, DefaultErrorHandler);

        yield return blockchain.GetLinkedChainsIds(
            (string[] linkedChains) =>
            {
                Assert.Contains(chainId1.ToUpper(), linkedChains);
                Assert.Contains(chainId2.ToUpper(), linkedChains);
            },
            DefaultErrorHandler
        );
    }

    // should return false when isLinkedWithChain is called for unknown chain id
    [UnityTest]
    public IEnumerator BlockchainTestRun7()
    {
        yield return SetupBlockchain();
        yield return blockchain.IsLinkedWithChain(TestUtil.GenerateId(),
            (bool isLinked) => Assert.False(isLinked), DefaultErrorHandler
        );
    }

    // should return asset queried by id
    [UnityTest]
    public IEnumerator BlockchainTestRun8()
    {
        yield return SetupBlockchain();
        Asset asset = null;
        yield return Asset.Register(TestUtil.GenerateAssetName(), TestUtil.GenerateId(), blockchain,
            (Asset _asset) => asset = _asset, DefaultErrorHandler
        );

        yield return blockchain.GetAssetById(asset.Id, (Asset _asset) => Assert.AreEqual(asset.Id.ToUpper(), _asset.Id.ToUpper()), DefaultErrorHandler);
    }
}
