using UnityEngine.TestTools;
using Chromia.Postchain.Ft3;
using NUnit.Framework;
using System.Linq;

using Cysharp.Threading.Tasks;

public class AssetTest
{
    // should be successfully registered
    [UnityTest]
    public async UniTask AssetTestRun1()
    {
        var blockchain = await BlockchainUtil.GetDefaultBlockchain();

        var asset = await Asset.Register(
            TestUtil.GenerateAssetName(),
            TestUtil.GenerateId(),
            blockchain
        );

        Assert.False(asset.Error);
        Assert.NotNull(asset.Content);
    }

    // should be returned when queried by name
    [UnityTest]
    public async UniTask AssetTestRun2()
    {
        var blockchain = await BlockchainUtil.GetDefaultBlockchain();

        var assetName = TestUtil.GenerateAssetName();

        var asset = await Asset.Register(
            assetName,
            TestUtil.GenerateId(),
            blockchain
        );

        var assets = await Asset.GetByName(assetName, blockchain);

        Assert.False(asset.Error);
        Assert.False(assets.Error);

        Assert.AreEqual(1, assets.Content.Length);
        Assert.AreEqual(asset.Content.Name, assets.Content[0].Name);
    }

    // should be returned when queried by id
    [UnityTest]
    public async UniTask AssetTestRun3()
    {
        var blockchain = await BlockchainUtil.GetDefaultBlockchain();

        var assetName = TestUtil.GenerateAssetName();
        var testChainId = TestUtil.GenerateId();

        var asset = await Asset.Register(
            assetName,
            testChainId,
            blockchain
        );
        Assert.False(asset.Error);

        var expectedAsset = await Asset.GetById(asset.Content.Id, blockchain);
        Assert.False(expectedAsset.Error);

        Assert.AreEqual(assetName, expectedAsset.Content.Name);
        Assert.AreEqual(asset.Content.Id.ToUpper(), expectedAsset.Content.Id.ToUpper());
        Assert.AreEqual(testChainId.ToUpper(), expectedAsset.Content.IssuingChainRid.ToUpper());
    }

    // should return all the assets registered
    [UnityTest]
    public async UniTask AssetTestRun4()
    {
        var blockchain = await BlockchainUtil.GetDefaultBlockchain();
        var asset1 = await Asset.Register(TestUtil.GenerateAssetName(), TestUtil.GenerateId(), blockchain);
        var asset2 = await Asset.Register(TestUtil.GenerateAssetName(), TestUtil.GenerateId(), blockchain);
        var asset3 = await Asset.Register(TestUtil.GenerateAssetName(), TestUtil.GenerateId(), blockchain);
        var expectedAsset = await Asset.GetAssets(blockchain);

        Assert.False(asset1.Error);
        Assert.False(asset2.Error);
        Assert.False(asset3.Error);
        Assert.False(expectedAsset.Error);

        var assetNames = expectedAsset.Content.Select(elem => elem.Name).ToList();
        Assert.Contains(asset1.Content.Name, assetNames);
        Assert.Contains(asset2.Content.Name, assetNames);
        Assert.Contains(asset3.Content.Name, assetNames);
    }
}
