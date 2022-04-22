using System.Collections.Generic;
using System;

namespace Chromia.Postchain.Ft3
{
    public class TransactionBuilder
    {
        private List<Operation> _operations;
        public readonly Blockchain Blockchain;

        public TransactionBuilder(Blockchain blockchain)
        {
            Blockchain = blockchain;
            _operations = new List<Operation>();
        }

        public TransactionBuilder Add(Operation operation)
        {
            _operations.Add(operation);
            return this;
        }

        public Transaction Build(byte[][] signers, Action<string> onError)
        {
            var tx = Blockchain.Connection.NewTransaction(signers, onError);
            foreach (var op in _operations)
            {
                tx.AddOperation(op.Name, op.Args);
            }

            return new Transaction(tx);
        }

        public Transaction BuildAndSign(User user, Action<string> onError)
        {
            return this.Build(user.AuthDescriptor.Signers.ToArray(), onError).Sign(user.KeyPair);
        }
    }
}