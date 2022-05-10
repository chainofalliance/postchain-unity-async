using System;
using NUnit.Framework;

using Chromia.Postchain.Client;
using Chromia.Postchain.Client.ASN1;

public class Asn1IntegerTest
{
    [Test]
    public void IntegerTest()
    {
        var val = new GTXValue();
        val.Choice = GTXValueChoice.Integer;
        val.Integer = 1337;

        var decoded = GTXValue.Decode(new AsnReader(val.Encode()));
        Assert.AreEqual(val.Choice, decoded.Choice);
        Assert.AreEqual(val.Integer, decoded.Integer);
        Assert.AreEqual(val, decoded);

    }

    [Test]
    public void NegativeIntegerTest()
    {
        var val = new GTXValue();
        val.Choice = GTXValueChoice.Integer;
        val.Integer = -1337;

        var decoded = GTXValue.Decode(new AsnReader(val.Encode()));
        Assert.AreEqual(val.Choice, decoded.Choice);
        Assert.AreEqual(val.Integer, decoded.Integer);
        Assert.AreEqual(val, decoded);

    }

    [Test]
    public void ZeroIntegerTest()
    {
        var val = new GTXValue();
        val.Choice = GTXValueChoice.Integer;
        val.Integer = 0;

        var decoded = GTXValue.Decode(new AsnReader(val.Encode()));
        Assert.AreEqual(val.Choice, decoded.Choice);
        Assert.AreEqual(val.Integer, decoded.Integer);
        Assert.AreEqual(val, decoded);

    }

    [Test]
    public void MaxIntegerTest()
    {
        var val = new GTXValue();
        val.Choice = GTXValueChoice.Integer;
        val.Integer = Int32.MaxValue;

        var decoded = GTXValue.Decode(new AsnReader(val.Encode()));
        Assert.AreEqual(val.Choice, decoded.Choice);
        Assert.AreEqual(val.Integer, decoded.Integer);
        Assert.AreEqual(val, decoded);

    }

    [Test]
    public void MinIntegerTest()
    {
        var val = new GTXValue();
        val.Choice = GTXValueChoice.Integer;
        val.Integer = Int32.MinValue;

        var decoded = GTXValue.Decode(new AsnReader(val.Encode()));
        Assert.AreEqual(val.Choice, decoded.Choice);
        Assert.AreEqual(val.Integer, decoded.Integer);
        Assert.AreEqual(val, decoded);

    }
}
