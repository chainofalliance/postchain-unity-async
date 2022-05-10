using NUnit.Framework;

using Chromia.Postchain.Client;
using Chromia.Postchain.Client.ASN1;

public class Asn1StringTest
{
    [Test]
    public void StringTest()
    {
        var val = new GTXValue();
        val.Choice = GTXValueChoice.String;
        val.String = "test";

        var decoded = GTXValue.Decode(new AsnReader(val.Encode()));
        Assert.AreEqual(val.Choice, decoded.Choice);
        Assert.AreEqual(val.String, decoded.String);
        Assert.AreEqual(val, decoded);

    }

    [Test]
    public void EmptyStringTest()
    {
        var val = new GTXValue();
        val.Choice = GTXValueChoice.String;
        val.String = "";

        var decoded = GTXValue.Decode(new AsnReader(val.Encode()));
        Assert.AreEqual(val.Choice, decoded.Choice);
        Assert.AreEqual(val.String, decoded.String);
        Assert.AreEqual(val, decoded);

    }

    [Test]
    public void LongStringTest()
    {
        var val = new GTXValue();
        val.Choice = GTXValueChoice.String;
        val.String = new string('x', 2048);

        var decoded = GTXValue.Decode(new AsnReader(val.Encode()));
        Assert.AreEqual(val.Choice, decoded.Choice);
        Assert.AreEqual(val.String, decoded.String);
        Assert.AreEqual(val, decoded);

    }

    [Test]
    public void UTF8StringTest()
    {
        var strings = new string[]{
            "Swedish: Åå Ää Öö",
            "Danish/Norway: Ææ Øø Åå",
            "German/Finish: Ää Öö Üü",
            "Greek lower: αβγδϵζηθικλμνξοπρστυϕχψω",
            "Greek upper: ΑΒΓΔΕΖΗΘΙΚΛΜΝΞΟΠΡΣΤΥΦΧΨΩ",
            "Russian: АаБбВвГгДдЕеЁёЖжЗзИиЙйКкЛлМмНнОоПпСсТтУуФфХхЦцЧчШшЩщЪъЫыЬьЭэЮюЯя"
        };

        foreach (var str in strings)
        {
            var val = new GTXValue();
            val.Choice = GTXValueChoice.String;
            val.String = str;

            var decoded = GTXValue.Decode(new AsnReader(val.Encode()));
            Assert.AreEqual(val.Choice, decoded.Choice);
            Assert.AreEqual(val.String, decoded.String);
            Assert.AreEqual(val, decoded);
        }

    }
}
