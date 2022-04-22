namespace Chromia.Postchain.Ft3
{
    public class XferInput : GtvSerializable
    {
        public readonly string AccountId;
        public readonly string AuthDescriptorId;
        public readonly string AssetId;
        public readonly long Amount;

        public XferInput(string accountId, string authId, string assetId, long amount)
        {
            AccountId = accountId;
            AuthDescriptorId = authId;
            AssetId = assetId;
            Amount = amount;
        }

        public static XferInput Create(string accountId, string authId, string assetId, long amount)
        {
            return new XferInput(accountId, authId, assetId, amount);
        }

        public object[] ToGTV()
        {
            return new object[]
            {
                this.AccountId,
                this.AssetId,
                this.AuthDescriptorId,
                this.Amount,
                // Not sure if neccessary
                new object[] {}
            };
        }
    }

    public class XferOutput : GtvSerializable
    {
        public readonly string AccountId;
        public readonly string AssetId;
        public readonly long Amount;

        public XferOutput(string accountId, string assetId, long amount)
        {
            AccountId = accountId;
            Amount = amount;
            AssetId = assetId;
        }

        public static XferOutput Create(string accountId, string assetId, long amount)
        {
            return new XferOutput(accountId, assetId, amount);
        }

        public object[] ToGTV()
        {
            return new object[]
            {
                this.AccountId,
                this.AssetId,
                this.Amount,
                // Not sure if neccessary
                new object[] {}
            };
        }
    }

    public class SimpleTransfer : GtvSerializable
    {
        public readonly string SourceAccountId;
        public readonly string DestinationAccountId;
        public readonly string AuthDescriptorId;
        public readonly string AssetId;
        public readonly long Amount;

        public SimpleTransfer(string sourceAccountId, string destinationAccountId,
            string assetId, string authDescriptorId, long amount)
        {
            SourceAccountId = sourceAccountId;
            DestinationAccountId = destinationAccountId;
            AssetId = assetId;
            AuthDescriptorId = authDescriptorId;
            Amount = amount;
        }

        public static SimpleTransfer Create(string sourceAccountId, string destinationAccountId,
            string assetId, string authDescriptorId, long amount)
        {
            return new SimpleTransfer(sourceAccountId, destinationAccountId, assetId, authDescriptorId, amount);
        }

        public object[] ToGTV()
        {
            return new object[]
            {
                "simple_transfer",
                XferInput.Create(SourceAccountId, AuthDescriptorId, AssetId, Amount),
                XferOutput.Create(DestinationAccountId, AssetId, Amount)
            };
        }
    }

    public class Transfer : GtvSerializable
    {
        public readonly XferInput[] Inputs;
        public readonly XferOutput[] Outputs;

        public Transfer(XferInput[] inputs, XferOutput[] outputs)
        {
            Inputs = inputs;
            Outputs = outputs;
        }

        public object[] ToGTV()
        {
            return new object[]
            {
                "transfer",
                Inputs,
                Outputs
            };
        }
    }
}