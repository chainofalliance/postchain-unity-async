
using System.Collections;
using Newtonsoft.Json;
using System;

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

        public static IEnumerator GetByAccountId(string id, Blockchain blockchain, Action<AssetBalance[]> onSuccess, Action<string> onError)
        {
            yield return blockchain.Query<AssetBalance[]>("ft3.get_asset_balances",
                new (string, object)[] { ("account_id", id) }, onSuccess, onError);
        }

        public static IEnumerator GetByAccountAndAssetId(string accountId, string assetId, Blockchain blockchain,
            Action<AssetBalance> onSuccess, Action<string> onError)
        {
            yield return blockchain.Query<AssetBalance>("ft3.get_asset_balance",
                new (string, object)[] { ("account_id", accountId), ("asset_id", assetId) }, onSuccess, onError);
        }

        public static IEnumerator GiveBalance(string accountId, string assetId, int amount, Blockchain blockchain, Action onSuccess, Action<string> onError)
        {
            yield return blockchain.TransactionBuilder()
                .Add(Operation.Op("ft3.dev_give_balance", assetId, accountId, amount))
                .Add(AccountOperations.Nop())
                .Build(new byte[][] { }, onError)
                .PostAndWait(onSuccess);
        }
    }
}