#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;

namespace SeweralIdeas.Config.Tests
{
    public class StreamConfigStorageTests
    {
        private static Dictionary<string, string> Parse(string text)
        {
            var values = new Dictionary<string, string>();
            StreamConfigStorage.ParseText(new StringReader(text), values);
            return values;
        }

        private static string Write(string key, string? value, bool quote)
        {
            var writer = new StringWriter();
            StreamConfigStorage.WriteEntry(writer, key, value, quote);
            return writer.ToString();
        }

        [Test]
        public void Unquoted_IgnoresUnescapedSpaces_AndTakesTheEscapedOnes()
        {
            var values = Parse("name=Hello\\_World\n  other  =  a b\\_c  ");
            Assert.AreEqual("Hello World", values["name"]);
            Assert.AreEqual("ab c", values["other"]);
        }

        [Test]
        public void Quoted_IsTakenAsItIs()
        {
            var values = Parse("a = \"  two  spaces = and an equals \"\nb=\"x\"   \n");
            Assert.AreEqual("  two  spaces = and an equals ", values["a"]);
            Assert.AreEqual("x", values["b"]);
        }

        [Test]
        public void Quoted_Escapes()
        {
            var values = Parse("a = \"say \\\"hi\\\"\\nnext\\\\ C:\\dir\"");
            Assert.AreEqual("say \"hi\"\nnext\\ C:\\dir", values["a"]);
        }

        [Test]
        public void Quoted_CanGoOverSeveralLines()
        {
            var values = Parse("a = \"one\r\ntwo\"\r\nb = 1\r\n");
            Assert.AreEqual("one\ntwo", values["a"]);
            Assert.AreEqual("1", values["b"]);
        }

        [Test]
        public void Quoted_Empty()
        {
            Assert.AreEqual(string.Empty, Parse("a = \"\"")["a"]);
        }

        [Test]
        public void SecondEquals_IsPartOfTheValue()
        {
            Assert.AreEqual("b=c", Parse("a = b=c")["a"]);
        }

        [Test]
        public void QuoteInsideAnUnquotedValue_IsJustAQuote()
        {
            Assert.AreEqual("ab\"c", Parse("a = ab\"c")["a"]);
        }

        [Test]
        public void DamagedData_Throws()
        {
            Assert.Throws<Exception>(() => Parse("a = \"never closed"));
            Assert.Throws<Exception>(() => Parse("a = \"closed\" and more"));
            Assert.Throws<Exception>(() => Parse("a = bad\\xescape"));
        }

        [TestCase("")]
        [TestCase("plain")]
        [TestCase("  spaces  and = equals ")]
        [TestCase("quote \" and backslash \\ and C:\\dir\\new")]
        [TestCase("line\nbreak\r\nand\rmore")]
        public void WrittenString_ReadsBackTheSame(string value)
        {
            Assert.AreEqual(value, Parse(Write("key", value, true))["key"]);
        }

        [Test]
        public void WrittenUnquoted_ReadsBackTheSame()
        {
            Assert.AreEqual("1.5", Parse(Write("key", "1.5", false))["key"]);
            Assert.AreEqual("a b=c", Parse(Write("key with space", "a b=c", false))["key with space"]);
        }

        [Test]
        public void WrittenString_IsInQuotes_WithoutEscapedSpaces()
        {
            Assert.AreEqual("name = \"Ground, Air\"", Write("name", "Ground, Air", true));
        }
    }
}
