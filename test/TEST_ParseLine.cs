using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace loki3.core.test
{
	[TestFixture]
	class TEST_ParseLine
	{
		class TestParseLineDelimiter : IParseLineDelimiters
		{
			public ValueDelimiter GetDelim(string start, out bool anyToken)
			{
				anyToken = true;
				if (start == "(")
					return ValueDelimiter.Basic;
				if (start == "[")
					return m_square;
				if (start == "<[")
					return m_fancy;
				if (start == "{{")
					return m_double;
				if (start == "/*")
					return m_comment;
				anyToken = false;
				return null;
			}

			private ValueDelimiter m_square = new ValueDelimiter("]");
			private ValueDelimiter m_fancy = new ValueDelimiter("]>");
			private ValueDelimiter m_double = new ValueDelimiter("}}", DelimiterType.AsString);
			private ValueDelimiter m_comment = new ValueDelimiter("*/", DelimiterType.AsComment);
		}

		[Test]
		public void NoDelimiters()
		{
			{	// empty string
				DelimiterList list = ParseLine.Do("", null);
				Assert.AreEqual(ValueDelimiter.Line, list.Delimiter);
				Assert.AreEqual(0, list.Nodes.Count);
			}

			{	// one item
				DelimiterList list = ParseLine.Do("asdf", null);
				Assert.AreEqual(ValueDelimiter.Line, list.Delimiter);
				Assert.AreEqual(1, list.Nodes.Count);
				Assert.IsTrue(list.Nodes[0] is DelimiterNodeToken);
				DelimiterNodeToken node = list.Nodes[0] as DelimiterNodeToken;
				Assert.AreEqual("asdf", node.Token.Value);
			}

			{	// multiple
				DelimiterList list = ParseLine.Do("asdf  qwert\t yuiop", null);
				Assert.AreEqual(ValueDelimiter.Line, list.Delimiter);
				Assert.AreEqual(3, list.Nodes.Count);
				Assert.AreEqual("asdf", list.Nodes[0].Token.Value);
				Assert.AreEqual("qwert", list.Nodes[1].Token.Value);
				Assert.AreEqual("yuiop", list.Nodes[2].Token.Value);
			}
		}

		[Test]
		public void SeparateDelimiters()
		{
			TestParseLineDelimiter delims = new TestParseLineDelimiter();

			{	// delimiters are separate tokens
				DelimiterList list = ParseLine.Do("asdf ( qwert yuiop ) ghjkl", delims);
				Assert.AreEqual(ValueDelimiter.Line, list.Delimiter);
				Assert.AreEqual(3, list.Nodes.Count);
				Assert.AreEqual("asdf", list.Nodes[0].Token.Value);
				Assert.AreEqual("ghjkl", list.Nodes[2].Token.Value);

				DelimiterList sublist = list.Nodes[1].List;
				Assert.AreEqual(")", sublist.Delimiter.End);
				Assert.AreEqual(2, sublist.Nodes.Count);
				Assert.AreEqual("qwert", sublist.Nodes[0].Token.Value);
				Assert.AreEqual("yuiop", sublist.Nodes[1].Token.Value);
			}

			{	// nested different delimiters
				DelimiterList list = ParseLine.Do("a ( b <[ c ]> d ) e", delims);
				Assert.AreEqual(ValueDelimiter.Line, list.Delimiter);
				Assert.AreEqual(3, list.Nodes.Count);
				Assert.AreEqual("a", list.Nodes[0].Token.Value);
				Assert.AreEqual("e", list.Nodes[2].Token.Value);

				DelimiterList sublist = list.Nodes[1].List;
				Assert.AreEqual(")", sublist.Delimiter.End);
				Assert.AreEqual(3, sublist.Nodes.Count);
				Assert.AreEqual("b", sublist.Nodes[0].Token.Value);
				Assert.AreEqual("d", sublist.Nodes[2].Token.Value);

				DelimiterList subsublist = sublist.Nodes[1].List;
				Assert.AreEqual("]>", subsublist.Delimiter.End);
				Assert.AreEqual(1, subsublist.Nodes.Count);
				Assert.AreEqual("c", subsublist.Nodes[0].Token.Value);
			}

			{	// nested same delimiters
				DelimiterList list = ParseLine.Do("a ( b ( c ) d ) e", delims);
				Assert.AreEqual(ValueDelimiter.Line, list.Delimiter);
				Assert.AreEqual(3, list.Nodes.Count);
				Assert.AreEqual("a", list.Nodes[0].Token.Value);
				Assert.AreEqual("e", list.Nodes[2].Token.Value);

				DelimiterList sublist = list.Nodes[1].List;
				Assert.AreEqual(")", sublist.Delimiter.End);
				Assert.AreEqual(3, sublist.Nodes.Count);
				Assert.AreEqual("b", sublist.Nodes[0].Token.Value);
				Assert.AreEqual("d", sublist.Nodes[2].Token.Value);

				DelimiterList subsublist = sublist.Nodes[1].List;
				Assert.AreEqual(")", subsublist.Delimiter.End);
				Assert.AreEqual(1, subsublist.Nodes.Count);
				Assert.AreEqual("c", subsublist.Nodes[0].Token.Value);
			}
		}

		[Test]
		public void DontTokenize()
		{
			TestParseLineDelimiter delims = new TestParseLineDelimiter();

			{	// everything inside {{ }} gets passed through as-is
				DelimiterList list = ParseLine.Do("asdf {{ qwert ( yuiop }} ghjkl", delims);
				Assert.AreEqual(ValueDelimiter.Line, list.Delimiter);
				Assert.AreEqual(3, list.Nodes.Count);
				Assert.AreEqual("asdf", list.Nodes[0].Token.Value);
				Assert.AreEqual("ghjkl", list.Nodes[2].Token.Value);

				DelimiterList sublist = list.Nodes[1].List;
				Assert.AreEqual("}}", sublist.Delimiter.End);
				Assert.AreEqual(1, sublist.Nodes.Count);
				Assert.AreEqual("qwert ( yuiop", sublist.Nodes[0].Token.Value);
			}

			{	// everything inside /* */ is a comment and is ignored
				DelimiterList list = ParseLine.Do("asdf /* qwert ( yuiop */ ghjkl", delims);
				Assert.AreEqual(ValueDelimiter.Line, list.Delimiter);
				Assert.AreEqual(2, list.Nodes.Count);
				Assert.AreEqual("asdf", list.Nodes[0].Token.Value);
				Assert.AreEqual("ghjkl", list.Nodes[1].Token.Value);
			}
		}

		[Test]
		public void GlommedOn()
		{
			TestParseLineDelimiter delims = new TestParseLineDelimiter();

			{	// front delimiter is part of its first token
				DelimiterList list = ParseLine.Do("asdf (qwert yuiop ) ghjkl", delims);
				Assert.AreEqual(ValueDelimiter.Line, list.Delimiter);
				Assert.AreEqual(3, list.Nodes.Count);
				Assert.AreEqual("asdf", list.Nodes[0].Token.Value);
				Assert.AreEqual("ghjkl", list.Nodes[2].Token.Value);

				DelimiterList sublist = list.Nodes[1].List;
				Assert.AreEqual(")", sublist.Delimiter.End);
				Assert.AreEqual(2, sublist.Nodes.Count);
				Assert.AreEqual("qwert", sublist.Nodes[0].Token.Value);
				Assert.AreEqual("yuiop", sublist.Nodes[1].Token.Value);
			}

			{	// front delimiter is part of its first token
				DelimiterList list = ParseLine.Do(":a v= \"\"", delims);
				Assert.AreEqual(ValueDelimiter.Line, list.Delimiter);
				Assert.AreEqual(3, list.Nodes.Count);
				Assert.AreEqual(":a", list.Nodes[0].Token.Value);
				Assert.AreEqual("v=", list.Nodes[1].Token.Value);
				Assert.AreEqual("\"\"", list.Nodes[2].Token.Value);
			}
		}
	}
}
