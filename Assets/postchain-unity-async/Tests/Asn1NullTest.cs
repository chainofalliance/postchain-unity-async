using NUnit.Framework;

using Chromia.Postchain.Client;
using Chromia.Postchain.Client.ASN1;

public class Asn1NullTest
{
    [Test]
    public void NullTest()
    {
        var val = new GTXValue();
        val.Choice = GTXValueChoice.Null;

        var decoded = GTXValue.Decode(new AsnReader(val.Encode()));
        Assert.AreEqual(val.Choice, decoded.Choice);
        Assert.AreEqual(val, decoded);

    }
}
