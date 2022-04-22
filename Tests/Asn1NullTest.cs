using System;
using System.Collections;
using NUnit.Framework;
using UnityEngine.TestTools;

using Chromia.Postchain.Client;
using Chromia.Postchain.Client.ASN1;

public class Asn1NullTest
{
    [UnityTest]
    public IEnumerator NullTest()
    {
        var val = new GTXValue();
        val.Choice = GTXValueChoice.Null;

        var decoded = GTXValue.Decode(new AsnReader(val.Encode()));
        Assert.AreEqual(val.Choice, decoded.Choice);
        Assert.AreEqual(val, decoded);
        yield return null;
    }
}
