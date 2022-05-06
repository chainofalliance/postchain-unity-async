using Chromia.Postchain.Client;
using Newtonsoft.Json;

using Cysharp.Threading.Tasks;

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

        public async static UniTask<PostchainResponse<Asset>> Register(string name, string chainId, Blockchain blockchain)
        {
            var res = await blockchain.TransactionBuilder()
                .Add(Operation.Op("ft3.dev_register_asset", name, chainId))
                .Build(new byte[][] { })
                .PostAndWait();

            if (res.Error)
                return PostchainResponse<Asset>.ErrorResponse(res.ErrorMessage);
            else
                return PostchainResponse<Asset>.SuccessResponse(new Asset(name, chainId));
        }

        public static UniTask<PostchainResponse<Asset[]>> GetByName(string name, Blockchain blockchain)
        {
            return blockchain.Query<Asset[]>("ft3.get_asset_by_name", new (string, object)[] { ("name", name) });
        }

        public static UniTask<PostchainResponse<Asset>> GetById(string id, Blockchain blockchain)
        {
            return blockchain.Query<Asset>("ft3.get_asset_by_id", new (string, object)[] { ("asset_id", id) });
        }

        public static UniTask<PostchainResponse<Asset[]>> GetAssets(Blockchain blockchain)
        {
            return blockchain.Query<Asset[]>("ft3.get_all_assets", null);
        }
    }
}