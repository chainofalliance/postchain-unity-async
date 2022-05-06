using Chromia.Postchain.Client;
using System.Collections;
using Newtonsoft.Json;
using System;

using Cysharp.Threading.Tasks;

namespace Chromia.Postchain.Ft3
{
    public class RateLimit
    {
        public int Points;

        [JsonProperty(PropertyName = "last_update")]
        public long LastUpdate;

        public RateLimit(int points, long last_update)
        {
            Points = points;
            LastUpdate = last_update;
        }

        public int GetRequestsLeft()
        {
            return Points;
        }

        public static UniTask<PostchainResponse<string>> ExecFreeOperation(string accountID, Blockchain blockchain)
        {
            return blockchain.TransactionBuilder()
                .Add(AccountDevOperations.FreeOp(accountID))
                .Add(AccountOperations.Nop())
                .Build(new byte[][] { })
                .PostAndWait();
        }

        public static UniTask<PostchainResponse<RateLimit>> GetByAccountRateLimit(string id, Blockchain blockchain)
        {
            return blockchain.Query<RateLimit>("ft3.get_account_rate_limit_last_update",
                new (string, object)[] { ("account_id", id) });
        }

        public static UniTask<PostchainResponse<string>> GivePoints(string accountID, int points, Blockchain blockchain)
        {
            return blockchain.TransactionBuilder()
                .Add(AccountDevOperations.GivePoints(accountID, points))
                .Add(AccountOperations.Nop())
                .Build(new byte[][] { })
                .PostAndWait();
        }

        public static UniTask<PostchainResponse<long>> GetLastTimestamp(Blockchain blockchain)
        {
            return blockchain.Query<long>("ft3.get_last_timestamp", null);
        }

        public static async UniTask<int> GetPointsAvailable(int points, int lastOperation, Blockchain blockchain)
        {
            var maxCount = blockchain.Info.RateLimitInfo.MaxPoints;
            var recoveryTime = blockchain.Info.RateLimitInfo.RecoveryTime;
            var lastTimestamp = 0L;

            var res = await GetLastTimestamp(blockchain);

            var available = 0;

            if (res.Error)
            {
                UnityEngine.Debug.LogWarning(res.ErrorMessage);
            }
            else
            {
                decimal delta = lastTimestamp - lastOperation;

                var pointsAvailable = (int)Math.Floor(delta / recoveryTime) + points;
                if (pointsAvailable > maxCount)
                    available = maxCount;

                if (pointsAvailable > 0)
                    available = pointsAvailable;
            }

            return available;
        }
    }
}