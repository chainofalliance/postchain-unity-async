using Chromia.Postchain.Client;
using System.Collections;
using Newtonsoft.Json;
using System;

namespace Chromia.Postchain.Ft3
{
    public class Asset
    {
        public string Id;
        public string Name;

        [JsonProperty(PropertyName = "issuing_chain_rid")]
        public string IssuingChainRid;

        public Asset(string name, string chainId)
        {
            this.Name = name;
            this.IssuingChainRid = chainId;
            this.Id = HashId();
        }

        [JsonConstructor]
        public Asset(string id, string name, string issuing_chain_rid)
        {
            this.Id = id;
            this.Name = name;
            this.IssuingChainRid = issuing_chain_rid;
        }

        private string HashId()
        {
            var body = new object[] {
                this.Name,
                Util.HexStringToBuffer(this.IssuingChainRid)
            };

            var hash = PostchainUtil.HashGTV(body);
            return Util.ByteArrayToString(hash);
        }

        public static IEnumerator Register(string name, string chainId, Blockchain blockchain, Action<Asset> onSuccess, Action<string> onError)
        {
            yield return blockchain.TransactionBuilder()
                .Add(Operation.Op("ft3.dev_register_asset", name, chainId))
                .Build(new byte[][] { }, onError)
                .PostAndWait(() => onSuccess(new Asset(name, chainId)));
        }

        public static IEnumerator GetByName(string name, Blockchain blockchain, Action<Asset[]> onSuccess, Action<string> onError)
        {
            yield return blockchain.Query<Asset[]>("ft3.get_asset_by_name",
                new (string, object)[] { ("name", name) }, onSuccess, onError);
        }

        public static IEnumerator GetById(string id, Blockchain blockchain, Action<Asset> onSuccess, Action<string> onError)
        {
            yield return blockchain.Query<Asset>("ft3.get_asset_by_id",
                new (string, object)[] { ("asset_id", id) }, onSuccess, onError);
        }

        public static IEnumerator GetAssets(Blockchain blockchain, Action<Asset[]> onSuccess, Action<string> onError)
        {
            yield return blockchain.Query<Asset[]>("ft3.get_all_assets", null, onSuccess, onError);
        }
    }
}