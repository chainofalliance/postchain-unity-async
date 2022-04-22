using System;
using Newtonsoft.Json;
using System.Collections;
using Chromia.Postchain.Client;

namespace Chromia.Postchain.Ft3
{
    public class BlockchainInfo
    {
        public string Name;
        public string Website;
        public string Description;
        public RateLimitInfo RateLimitInfo;

        public BlockchainInfo(string name, string website, string description, RateLimitInfo rateLimitInfo)
        {
            this.Name = name;
            this.Website = website;
            this.Description = description;
            this.RateLimitInfo = rateLimitInfo;
        }

        [JsonConstructor]
        public BlockchainInfo(string name, string website, string description,
            int rate_limit_active, int rate_limit_max_points, int rate_limit_recovery_time, int rate_limit_points_at_account_creation)
        {
            this.Name = name;
            this.Website = website;
            this.Description = description;
            this.RateLimitInfo = new RateLimitInfo(
                        rate_limit_active == 1,
                        rate_limit_max_points,
                        rate_limit_recovery_time,
                        rate_limit_points_at_account_creation
                    );
        }

        public static IEnumerator GetInfo(BlockchainClient connection, Action<BlockchainInfo> onSuccess, Action<string> onError)
        {
            yield return connection.Query<BlockchainInfo>("ft3.get_blockchain_info", null, onSuccess, onError);
        }
    }
}