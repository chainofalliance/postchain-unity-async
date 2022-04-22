using System;
using System.Collections;
using NUnit.Framework;
using UnityEngine.TestTools;

using Chromia.Postchain.Client;
using Chromia.Postchain.Client.ASN1;

public class Asn1IntegerTest
{
    [UnityTest]
    public IEnumerator IntegerTest()
    {
        var val = new GTXValue();
        val.Choice = GTXValueChoice.Integer;
        val.Integer = 1337;

        var decoded = GTXValue.Decode(new AsnReader(val.Encode()));
        Assert.AreEqual(val.Choice, decoded.Choice);
        Assert.AreEqual(val.Integer, decoded.Integer);
        Assert.AreEqual(val, decoded);
        yield return null;
    }
    
    [UnityTest]
    public IEnumerator NegativeIntegerTest()
    {
        var val = new GTXValue();
        val.Choice = GTXValueChoice.Integer;
        val.Integer = -1337;

        var decoded = GTXValue.Decode(new AsnReader(val.Encode()));
        Assert.AreEqual(val.Choice, decoded.Choice);
        Assert.AreEqual(val.Integer, decoded.Integer);
        Assert.AreEqual(val, decoded);
        yield return null;
    }
    
    [UnityTest]
    public IEnumerator ZeroIntegerTest()
    {
        var val = new GTXValue();
        val.Choice = GTXValueChoice.Integer;
        val.Integer = 0;

        var decoded = GTXValue.Decode(new AsnReader(val.Encode()));
        Assert.AreEqual(val.Choice, decoded.Choice);
        Assert.AreEqual(val.Integer, decoded.Integer);
        Assert.AreEqual(val, decoded);
        yield return null;
    }
    
    [UnityTest]
    public IEnumerator MaxIntegerTest()
    {
        var val = new GTXValue();
        val.Choice = GTXValueChoice.Integer;
        val.Integer = Int32.MaxValue;

        var decoded = GTXValue.Decode(new AsnReader(val.Encode()));
        Assert.AreEqual(val.Choice, decoded.Choice);
        Assert.AreEqual(val.Integer, decoded.Integer);
        Assert.AreEqual(val, decoded);
        yield return null;
    }
    
    [UnityTest]
    public IEnumerator MinIntegerTest()
    {
        var val = new GTXValue();
        val.Choice = GTXValueChoice.Integer;
        val.Integer = Int32.MinValue;

        var decoded = GTXValue.Decode(new AsnReader(val.Encode()));
        Assert.AreEqual(val.Choice, decoded.Choice);
        Assert.AreEqual(val.Integer, decoded.Integer);
        Assert.AreEqual(val, decoded);
        yield return null;
    }
}
