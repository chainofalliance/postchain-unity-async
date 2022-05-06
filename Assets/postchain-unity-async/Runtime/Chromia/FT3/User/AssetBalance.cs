
using Chromia.Postchain.Client;
using Newtonsoft.Json;

using Cysharp.Threading.Tasks;

namespace Chromia.Postchain.Ft3
{
    public class AssetBalance
    {
        public long Amount;
        public Asset Asset;

        public AssetBalance(long amount, Asset asset)
        {
            this.Amount = amount;
            this.Asset = asset;
        }

        [JsonConstructor]
        public AssetBalance(string id, string name, long amount, string chain_id)
        {
            this.Amount = amount;
            this.Asset = new Asset(name, chain_id);
        }

        public static UniTask<PostchainResponse<AssetBalance[]>> GetByAccountId(string id, Blockchain blockchain)
        {
            return blockchain.Query<AssetBalance[]>("ft3.get_asset_balances", new (string, object)[] { ("account_id", id) });
        }

        public static UniTask<PostchainResponse<AssetBalance>> GetByAccountAndAssetId(string accountId, string assetId, Blockchain blockchain)
        {
            return blockchain.Query<AssetBalance>("ft3.get_asset_balance", new (string, object)[] { ("account_id", accountId), ("asset_id", assetId) });
        }

        public static UniTask<PostchainResponse<string>> GiveBalance(string accountId, string assetId, int amount, Blockchain blockchain)
        {
            return blockchain.TransactionBuilder()
                .Add(Operation.Op("ft3.dev_give_balance", assetId, accountId, amount))
                .Add(AccountOperations.Nop())
                .Build(new byte[][] { })
                .PostAndWait();
        }
    }
}