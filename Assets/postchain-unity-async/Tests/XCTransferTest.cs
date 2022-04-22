using System.Collections.Generic;
using System.Collections;
using UnityEngine.TestTools;
using Chromia.Postchain.Ft3;
using NUnit.Framework;

public class XCTransferTest
{
    private Blockchain blockchain;

    private IEnumerator SetupBlockchain()
    {
        yield return BlockchainUtil.GetDefaultBlockchain((Blockchain _blockchain) => { blockchain = _blockchain; });
    }

    private void DefaultErrorHandler(string error) { UnityEngine.Debug.Log(error); }
    private void EmptyCallback() { }

    // Cross-chain transfer
    [UnityTest]
    public IEnumerator XcTransferTestRun1()
    {
        yield return SetupBlockchain();
        Asset asset = null;
        yield return Asset.Register(TestUtil.GenerateAssetName(), TestUtil.GenerateId(), blockchain, (Asset _asset) => asset = _asset, DefaultErrorHandler);

        var destinationChainId = TestUtil.GenerateId();
        var destinationAccountId = TestUtil.GenerateId();
        User user = TestUser.SingleSig();

        AccountBuilder accountBuilder = AccountBuilder.CreateAccountBuilder(blockchain, user);
        accountBuilder.WithParticipants(new List<KeyPair>() { user.KeyPair });
        accountBuilder.WithBalance(asset, 100);
        accountBuilder.WithPoints(1);
        Account account = null;
        yield return accountBuilder.Build((Account _account) => account = _account);

        yield return account.XcTransfer(destinationChainId, destinationAccountId, asset.Id, 10, EmptyCallback, DefaultErrorHandler);

        AssetBalance accountBalance = null;
        yield return AssetBalance.GetByAccountAndAssetId(account.Id, asset.Id, blockchain, (AssetBalance _balance) => accountBalance = _balance, DefaultErrorHandler);

        AssetBalance chainBalance = null;
        yield return AssetBalance.GetByAccountAndAssetId(
            TestUtil.BlockchainAccountId(destinationChainId),
            asset.Id,
            blockchain,
            (AssetBalance _balance) => chainBalance = _balance,
            DefaultErrorHandler
        );

        Assert.AreEqual(90, accountBalance.Amount);
        Assert.AreEqual(10, chainBalance.Amount);
    }
}
