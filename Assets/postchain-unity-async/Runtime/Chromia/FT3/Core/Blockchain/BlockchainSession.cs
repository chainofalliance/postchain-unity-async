using Chromia.Postchain.Client;
using Cysharp.Threading.Tasks;

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

        public UniTask<PostchainResponse<Account>> GetAccountById(string id)
        {
            return Account.GetById(id, this);
        }

        public UniTask<PostchainResponse<Account[]>> GetAccountsByParticipantId(string id)
        {
            return Account.GetByParticipantId(id, this);
        }

        public UniTask<PostchainResponse<Account[]>> GetAccountsByAuthDescriptorId(string id)
        {
            return Account.GetByAuthDescriptorId(id, this);
        }

        public UniTask<PostchainResponse<T>> Query<T>(string queryName, (string name, object content)[] queryObject)
        {
            return this.Blockchain.Query<T>(queryName, queryObject);
        }

        public UniTask<PostchainResponse<string>> Call(Operation operation)
        {
            return this.Blockchain.Call(operation, this.User);
        }
    }
}