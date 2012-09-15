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
			public char GetEndDelim(char start)
			{
				return Char.MinValue;
			}

			public string GetEndDelim(string start)
			{
				return null;
			}
		}

		[Test]
		public void NoDelimiters()
		{
			{	// empty string
				DelimiterTree tree = ParseLine.Do("", null);
				Assert.AreEqual(Delimiter.Basic, tree.Delimiter);
				Assert.AreEqual(0, tree.Nodes.Count);
			}

			{	// one item
				DelimiterTree tree = ParseLine.Do("asdf", null);
				Assert.AreEqual(Delimiter.Basic, tree.Delimiter);
				Assert.AreEqual(1, tree.Nodes.Count);
				Assert.IsTrue(tree.Nodes[0] is DelimiterNodeToken);
				DelimiterNodeToken node = tree.Nodes[0] as DelimiterNodeToken;
				Assert.AreEqual("asdf", node.Token.Value);
			}

			{	// multiple
				DelimiterTree tree = ParseLine.Do("asdf  qwert\t yuiop", null);
				Assert.AreEqual(Delimiter.Basic, tree.Delimiter);
				Assert.AreEqual(3, tree.Nodes.Count);
				Assert.AreEqual("asdf", tree.Nodes[0].Value);
				Assert.AreEqual("qwert", tree.Nodes[1].Value);
				Assert.AreEqual("yuiop", tree.Nodes[2].Value);
			}
		}
	}
}
