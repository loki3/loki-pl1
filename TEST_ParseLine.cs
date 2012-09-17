using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace loki3
{
	[TestFixture]
	class TEST_ParseLine
	{
		class TestParseLineDelimiter : IParseLineDelimiters
		{
			public Delimiter GetDelim(char start)
			{
				switch (start)
				{
					case '(': return Delimiter.Basic;
					case '[': return m_square;
				}
				return null;
			}

			public Delimiter GetDelim(string start)
			{
				if (start.Length == 1)
					return GetDelim(start[0]);
				if (start == "{{")
					return m_double;
				if (start == "<[")
					return m_fancy;
				return null;
			}

			private Delimiter m_square = new Delimiter("[", "]");
			private Delimiter m_double = new Delimiter("{{", "}}");
			private Delimiter m_fancy = new Delimiter("<[", "]>");
		}

		[Test]
		public void NoDelimiters()
		{
			{	// empty string
				DelimiterTree tree = ParseLine.Do("", null);
				Assert.AreEqual(Delimiter.Line, tree.Delimiter);
				Assert.AreEqual(0, tree.Nodes.Count);
			}

			{	// one item
				DelimiterTree tree = ParseLine.Do("asdf", null);
				Assert.AreEqual(Delimiter.Line, tree.Delimiter);
				Assert.AreEqual(1, tree.Nodes.Count);
				Assert.IsTrue(tree.Nodes[0] is DelimiterNodeToken);
				DelimiterNodeToken node = tree.Nodes[0] as DelimiterNodeToken;
				Assert.AreEqual("asdf", node.Token.Value);
			}

			{	// multiple
				DelimiterTree tree = ParseLine.Do("asdf  qwert\t yuiop", null);
				Assert.AreEqual(Delimiter.Line, tree.Delimiter);
				Assert.AreEqual(3, tree.Nodes.Count);
				Assert.AreEqual("asdf", tree.Nodes[0].Token.Value);
				Assert.AreEqual("qwert", tree.Nodes[1].Token.Value);
				Assert.AreEqual("yuiop", tree.Nodes[2].Token.Value);
			}
		}

		[Test]
		public void SeparateDelimiters()
		{
			TestParseLineDelimiter delims = new TestParseLineDelimiter();

			{	// delimiters are separate tokens
				DelimiterTree tree = ParseLine.Do("asdf ( qwert yuiop ) ghjkl", delims);
				Assert.AreEqual(Delimiter.Line, tree.Delimiter);
				Assert.AreEqual(3, tree.Nodes.Count);
				Assert.AreEqual("asdf", tree.Nodes[0].Token.Value);
				Assert.AreEqual("ghjkl", tree.Nodes[2].Token.Value);

				DelimiterTree subtree = tree.Nodes[1].Tree;
				Assert.AreEqual("(", subtree.Delimiter.Start);
				Assert.AreEqual(2, subtree.Nodes.Count);
				Assert.AreEqual("qwert", subtree.Nodes[0].Token.Value);
				Assert.AreEqual("yuiop", subtree.Nodes[1].Token.Value);
			}

			{	// nested different delimiters
				DelimiterTree tree = ParseLine.Do("a ( b <[ c ]> d ) e", delims);
				Assert.AreEqual(Delimiter.Line, tree.Delimiter);
				Assert.AreEqual(3, tree.Nodes.Count);
				Assert.AreEqual("a", tree.Nodes[0].Token.Value);
				Assert.AreEqual("e", tree.Nodes[2].Token.Value);

				DelimiterTree subtree = tree.Nodes[1].Tree;
				Assert.AreEqual("(", subtree.Delimiter.Start);
				Assert.AreEqual(3, subtree.Nodes.Count);
				Assert.AreEqual("b", subtree.Nodes[0].Token.Value);
				Assert.AreEqual("d", subtree.Nodes[2].Token.Value);

				DelimiterTree subsubtree = subtree.Nodes[1].Tree;
				Assert.AreEqual("<[", subsubtree.Delimiter.Start);
				Assert.AreEqual(1, subsubtree.Nodes.Count);
				Assert.AreEqual("c", subsubtree.Nodes[0].Token.Value);
			}

			{	// nested same delimiters
				DelimiterTree tree = ParseLine.Do("a ( b ( c ) d ) e", delims);
				Assert.AreEqual(Delimiter.Line, tree.Delimiter);
				Assert.AreEqual(3, tree.Nodes.Count);
				Assert.AreEqual("a", tree.Nodes[0].Token.Value);
				Assert.AreEqual("e", tree.Nodes[2].Token.Value);

				DelimiterTree subtree = tree.Nodes[1].Tree;
				Assert.AreEqual("(", subtree.Delimiter.Start);
				Assert.AreEqual(3, subtree.Nodes.Count);
				Assert.AreEqual("b", subtree.Nodes[0].Token.Value);
				Assert.AreEqual("d", subtree.Nodes[2].Token.Value);

				DelimiterTree subsubtree = subtree.Nodes[1].Tree;
				Assert.AreEqual("(", subsubtree.Delimiter.Start);
				Assert.AreEqual(1, subsubtree.Nodes.Count);
				Assert.AreEqual("c", subsubtree.Nodes[0].Token.Value);
			}
		}
	}
}