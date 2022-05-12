using System;
using System.Collections;
using NUnit.Framework;

using Chromia.Postchain.Client;
using Chromia.Postchain.Client.ASN1;

public class Asn1ByteArrayTest
{
    [Test]
    public void ByteArrayTest()
    {
        var val = new GTXValue();
        val.Choice = GTXValueChoice.ByteArray;
        val.ByteArray = new byte[] { 0xaf, 0xfe, 0xca, 0xfe };

        var decoded = GTXValue.Decode(new AsnReader(val.Encode()));
        Assert.AreEqual(val.Choice, decoded.Choice);
        Assert.AreEqual(val.ByteArray, decoded.ByteArray);
        Assert.AreEqual(val, decoded);
    }

    [Test]
    public void EmptyByteArrayTest()
    {
        var val = new GTXValue();
        val.Choice = GTXValueChoice.ByteArray;
        val.ByteArray = new byte[] { };

        var decoded = GTXValue.Decode(new AsnReader(val.Encode()));
        Assert.AreEqual(val.Choice, decoded.Choice);
        Assert.AreEqual(val.ByteArray, decoded.ByteArray);
        Assert.AreEqual(val, decoded);
    }

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
        val.Integer = Int32.MaxValue;

        var decoded = GTXValue.Decode(new AsnReader(val.Encode()));
        Assert.AreEqual(val.Choice, decoded.Choice);
        Assert.AreEqual(val.Integer, decoded.Integer);
        Assert.AreEqual(val, decoded);
    }
}
