using Chromia.Postchain.Client;
using System.Collections.Generic;
using System.Linq;
using System;

namespace Chromia.Postchain.Ft3
{
    public class MultiSignatureAuthDescriptor : AuthDescriptor
    {
        public List<byte[]> PubKeys;
        public Flags Flags;
        public int SignatureRequired;
        public readonly IAuthdescriptorRule AuthRule;

        public MultiSignatureAuthDescriptor(List<byte[]> pubkeys, int signatureRequired, FlagsType[] flags, IAuthdescriptorRule rule = null)
        {
            if (signatureRequired > pubkeys.Count)
            {
                throw new Exception("Number of required signatures have to be less or equal to number of pubkeys");
            }

            this.PubKeys = pubkeys;
            this.SignatureRequired = signatureRequired;
            this.Flags = new Flags(flags.ToList());
            this.AuthRule = rule;
        }

        public List<byte[]> Signers
        {
            get => this.PubKeys;
        }

        public string ID
        {
            get => Util.ByteArrayToString(this.Hash());
        }

        public IAuthdescriptorRule Rule
        {
            get => this.AuthRule;
        }

        public object[] ToGTV()
        {
            var hexPubs = new List<string>();
            foreach (var pubkey in this.PubKeys)
            {
                hexPubs.Add(Util.ByteArrayToString(pubkey));
            }

            var gtv = new object[] {
                Util.AuthTypeToString(AuthType.MultiSig),
                hexPubs.ToArray(),
                new object[]
                {
                    this.Flags.ToGTV(),
                    this.SignatureRequired,
                    hexPubs.ToArray()
                },
                this.AuthRule?.ToGTV()
            };

            return gtv;
        }

        public byte[] Hash()
        {
            var hexPubs = new List<string>();
            foreach (var pubkey in this.PubKeys)
            {
                hexPubs.Add(Util.ByteArrayToString(pubkey));
            }

            var gtv = new object[] {
                Util.AuthTypeToString(AuthType.MultiSig),
                this.PubKeys.ToArray(),
                new object[]
                {
                    this.Flags.ToGTV(),
                    this.SignatureRequired,
                    hexPubs.ToArray()
                },
                this.AuthRule?.ToGTV()
            };

            return PostchainUtil.HashGTV(gtv);
        }
    }
}