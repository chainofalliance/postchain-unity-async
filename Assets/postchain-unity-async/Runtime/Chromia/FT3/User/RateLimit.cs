using System.Collections.Generic;
using System.Collections;
using Newtonsoft.Json;
using System;

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

        public static IEnumerator ExecFreeOperation(string accountID, Blockchain blockchain, Action onSuccess, Action<string> onError)
        {
            yield return blockchain.TransactionBuilder()
                .Add(AccountDevOperations.FreeOp(accountID))
                .Add(AccountOperations.Nop())
                .Build(new byte[][] { }, onError)
                .PostAndWait(onSuccess);
        }

        public static IEnumerator GetByAccountRateLimit(string id, Blockchain blockchain, Action<RateLimit> onSuccess, Action<string> onError)
        {
            yield return blockchain.Query<RateLimit>("ft3.get_account_rate_limit_last_update",
                new (string, object)[] { ("account_id", id) }, onSuccess, onError);
        }

        public static IEnumerator GivePoints(string accountID, int points, Blockchain blockchain, Action onSuccess, Action<string> onError)
        {
            yield return blockchain.TransactionBuilder()
                .Add(AccountDevOperations.GivePoints(accountID, points))
                .Add(AccountOperations.Nop())
                .Build(new byte[][] { }, onError)
                .PostAndWait(onSuccess);
        }

        public static IEnumerator GetLastTimestamp(Blockchain blockchain, Action<long> onSuccess, Action<string> onError)
        {
            yield return blockchain.Query<long>("ft3.get_last_timestamp", null, onSuccess, onError);
        }

        public static IEnumerator GetPointsAvailable(int points, int lastOperation, Blockchain blockchain,
            Action<int> onSuccess, Action<string> onError)
        {
            var maxCount = blockchain.Info.RateLimitInfo.MaxPoints;
            var recoveryTime = blockchain.Info.RateLimitInfo.RecoveryTime;
            var lastTimestamp = 0L;

            yield return GetLastTimestamp(blockchain, (long _lastTimeStamp) => { lastTimestamp = _lastTimeStamp; }, onError);
            decimal delta = lastTimestamp - lastOperation;

            var pointsAvailable = (int)Math.Floor(delta / recoveryTime) + points;
            if (pointsAvailable > maxCount)
            {
                onSuccess(maxCount);
            }

            if (pointsAvailable > 0)
            {
                onSuccess(pointsAvailable);
            }
            else
            {
                onSuccess(0);
            }
        }
    }
}