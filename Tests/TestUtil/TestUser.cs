using Chromia.Postchain.Ft3;
using System.Collections.Generic;
using System.Linq;

public class TestUser
{
    public static User SingleSig(IAuthdescriptorRule rule = null)
    {
        KeyPair keyPair = new KeyPair();
        SingleSignatureAuthDescriptor singleSigAuthDescriptor = new SingleSignatureAuthDescriptor(
            keyPair.PubKey,
            new List<FlagsType>() { FlagsType.Account, FlagsType.Transfer }.ToArray(),
            rule
        );
        return new User(keyPair, singleSigAuthDescriptor);
    }

    public static User MultiSig(int requiredSignatures, int numberOfParticipants, IAuthdescriptorRule rule = null)
    {
        List<KeyPair> keyPairs = new List<KeyPair>();
        for (int i = 0; i < numberOfParticipants; i++) { keyPairs.Add(new KeyPair()); }

        MultiSignatureAuthDescriptor multiSignatureAuthDescriptor = new MultiSignatureAuthDescriptor(
            keyPairs.Select(elem => elem.PubKey).ToList(),
            requiredSignatures,
            new List<FlagsType>() { FlagsType.Account, FlagsType.Transfer }.ToArray(),
            rule
        );

        return new User(keyPairs[0], multiSignatureAuthDescriptor);
    }
}
