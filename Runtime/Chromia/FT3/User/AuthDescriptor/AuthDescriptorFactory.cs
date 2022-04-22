using System.Collections.Generic;
using Chromia.Postchain.Client;
using Chromia.Postchain.Client.ASN1;

namespace Chromia.Postchain.Ft3
{
    public class AuthDescriptorFactory
    {
        public AuthDescriptor Create(AuthType type, byte[] args)
        {
            switch (type)
            {
                case AuthType.SingleSig:
                    return this.CreateSingleSig(args);
                case AuthType.MultiSig:
                    return this.CreateMultiSig(args);
            }
            return null;
        }

        private SingleSignatureAuthDescriptor CreateSingleSig(byte[] args)
        {
            var gtxTransaction = new AsnReader(args);
            var gtxValue = GTXValue.Decode(gtxTransaction);
            var flags = new List<FlagsType>();

            foreach (var flag in gtxValue.Array[0].Array)
            {
                flags.Add(Util.StringToFlagType((string)flag.String));
            }

            return new SingleSignatureAuthDescriptor(
                Util.HexStringToBuffer((string)gtxValue.Array[1].String),
                flags.ToArray()
            );
        }

        private MultiSignatureAuthDescriptor CreateMultiSig(byte[] args)
        {
            var gtxTransaction = new AsnReader(args);
            var gtxValue = GTXValue.Decode(gtxTransaction);

            var flags = new List<FlagsType>();
            var pubKeys = new List<byte[]>();
            var signatureRequired = (int)gtxValue.Array[1].Integer;

            foreach (var flag in gtxValue.Array[0].Array)
            {
                flags.Add(Util.StringToFlagType((string)flag.String));
            }

            foreach (var pubKey in gtxValue.Array[2].Array)
            {
                pubKeys.Add(Util.HexStringToBuffer((string)pubKey.String));
            }

            return new MultiSignatureAuthDescriptor(
                pubKeys, signatureRequired, flags.ToArray()
            );
        }

        public struct AuthDescriptorQuery
        {
            public string id;
            public string type;
            public string args;
        }
    }
}