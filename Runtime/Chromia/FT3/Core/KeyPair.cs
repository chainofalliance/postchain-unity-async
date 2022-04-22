namespace Chromia.Postchain.Ft3
{
    public class KeyPair
    {
        public readonly byte[] PubKey;
        public readonly byte[] PrivKey;

        public KeyPair(byte[] privateKey, byte[] pubKey)
        {
            PubKey = pubKey;
            PrivKey = privateKey;
        }

        public KeyPair(string privateKey = null)
        {
            if (privateKey != null)
            {
                this.PrivKey = Util.HexStringToBuffer(privateKey);
                this.PubKey = Client.PostchainUtil.VerifyKeyPair(privateKey);
            }
            else
            {
                var keyPair = Client.PostchainUtil.MakeKeyPair();
                this.PubKey = keyPair["pubKey"];
                this.PrivKey = keyPair["privKey"];
            }
        }
    }
}