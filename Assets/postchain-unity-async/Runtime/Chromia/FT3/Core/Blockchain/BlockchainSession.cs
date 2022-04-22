using System.Collections;
using System;

namespace Chromia.Postchain.Ft3
{
    public class BlockchainSession
    {
        public readonly User User;
        public readonly Blockchain Blockchain;

        public BlockchainSession(User user, Blockchain blockchain)
        {
            this.User = user;
            this.Blockchain = blockchain;
        }

        public IEnumerator GetAccountById(string id, Action<Account> onSuccess, Action<string> onError)
        {
            yield return Account.GetById(id, this, onSuccess, onError);
        }

        public IEnumerator GetAccountsByParticipantId(string id, Action<Account[]> onSuccess, Action<string> onError)
        {
            yield return Account.GetByParticipantId(id, this, onSuccess, onError);
        }

        public IEnumerator GetAccountsByAuthDescriptorId(string id, Action<Account[]> onSuccess, Action<string> onError)
        {
            yield return Account.GetByAuthDescriptorId(id, this, onSuccess, onError);
        }

        public IEnumerator Query<T>(string queryName, (string name, object content)[] queryObject, Action<T> onSuccess, Action<string> onError)
        {
            yield return this.Blockchain.Query<T>(queryName, queryObject, onSuccess, onError);
        }

        public IEnumerator Call(Operation operation, Action onSuccess, Action<string> onError)
        {
            yield return this.Blockchain.Call(operation, this.User, onSuccess, onError);
        }
    }
}