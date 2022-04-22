using System.Collections.Generic;
using System.Collections;
using UnityEngine.TestTools;
using Chromia.Postchain.Ft3;
using NUnit.Framework;
using System.Linq;

public class AssetTest
{
    private Blockchain blockchain;

    private IEnumerator SetupBlockchain()
    {
        yield return BlockchainUtil.GetDefaultBlockchain((Blockchain _blockchain) => { blockchain = _blockchain; });
    }

    private void DefaultErrorHandler(string error) { UnityEngine.Debug.Log(error); }
    private void EmptyCallback() { }

    // should be successfully registered
    [UnityTest]
    public IEnumerator AssetTestRun1()
    {
        yield return SetupBlockchain();

        Asset asset = null;
        yield return Asset.Register(
            TestUtil.GenerateAssetName(),
            TestUtil.GenerateId(),
            blockchain,
            (Asset _asset) => asset = _asset,
            DefaultErrorHandler
        );

        Assert.NotNull(asset);
    }

    // should be returned when queried by name
    [UnityTest]
    public IEnumerator AssetTestRun2()
    {
        yield return SetupBlockchain();

        var assetName = TestUtil.GenerateAssetName();

        Asset asset = null;
        yield return Asset.Register(
            assetName,
            TestUtil.GenerateId(),
            blockchain,
            (Asset _asset) => asset = _asset,
            DefaultErrorHandler
        );

        Asset[] assets = null;
        yield return Asset.GetByName(assetName, blockchain, (Asset[] _assets) => assets = _assets, DefaultErrorHandler);

        Assert.AreEqual(1, assets.Length);
        Assert.AreEqual(asset.Name, assets[0].Name);
    }

    // should be returned when queried by id
    [UnityTest]
    public IEnumerator AssetTestRun3()
    {
        yield return SetupBlockchain();

        var assetName = TestUtil.GenerateAssetName();
        var testChainId = TestUtil.GenerateId();

        Asset asset = null;
        yield return Asset.Register(
            assetName,
            testChainId,
            blockchain,
            (Asset _asset) => asset = _asset,
            DefaultErrorHandler
        );

        Asset expectedAsset = null;
        yield return Asset.GetById(asset.Id, blockchain, (Asset _asset) => expectedAsset = _asset, DefaultErrorHandler);

        Assert.AreEqual(assetName, expectedAsset.Name);
        Assert.AreEqual(asset.Id.ToUpper(), expectedAsset.Id.ToUpper());
        Assert.AreEqual(testChainId.ToUpper(), expectedAsset.IssuingChainRid.ToUpper());
    }

    // should return all the assets registered
    [UnityTest]
    public IEnumerator AssetTestRun4()
    {
        yield return SetupBlockchain();
        Asset asset1 = null;
        yield return Asset.Register(TestUtil.GenerateAssetName(), TestUtil.GenerateId(), blockchain, (Asset _asset) => asset1 = _asset, DefaultErrorHandler);
        Asset asset2 = null;
        yield return Asset.Register(TestUtil.GenerateAssetName(), TestUtil.GenerateId(), blockchain, (Asset _asset) => asset2 = _asset, DefaultErrorHandler);
        Asset asset3 = null;
        yield return Asset.Register(TestUtil.GenerateAssetName(), TestUtil.GenerateId(), blockchain, (Asset _asset) => asset3 = _asset, DefaultErrorHandler);

        Asset[] expectedAsset = null;

        yield return Asset.GetAssets(blockchain, (Asset[] _assets) => expectedAsset = _assets, DefaultErrorHandler);
        var assetNames = expectedAsset.Select(elem => elem.Name).ToList();
        Assert.Contains(asset1.Name, assetNames);
        Assert.Contains(asset2.Name, assetNames);
        Assert.Contains(asset3.Name, assetNames);
    }
}
