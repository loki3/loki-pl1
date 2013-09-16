using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace loki3.core.test
{
	[TestFixture]
	class TEST_ParseChars
	{
		class TestParseCharsDelimiter : IParseLineDelimiters
		{
			public ValueDelimiter GetDelim(string start, out bool anyToken) { anyToken = false;  return null; }

			public Dictionary<string, ValueDelimiter> GetStringDelims()
			{
				Dictionary<string, ValueDelimiter> d = new Dictionary<string, ValueDelimiter>();
				d["'"] = m_single;
				d["\""] = m_double;
				d[".'"] = m_toEnd;
				return d;
			}

			private ValueDelimiter m_single = new ValueDelimiter("'", DelimiterType.AsString);
			private ValueDelimiter m_double = new ValueDelimiter("\"", DelimiterType.AsString);
			private ValueDelimiter m_toEnd = new ValueDelimiter(null, DelimiterType.AsString);
		}

		[Test]
		public void TestBasic()
		{
			IParseLineDelimiters d = new TestParseCharsDelimiter();
			{
				string[] strs = ParseChars.Do("one", d);
				Assert.AreEqual(1, strs.Length);
				Assert.AreEqual("one", strs[0]);
			}

			{
				string[] strs = ParseChars.Do("one two", d);
				Assert.AreEqual(2, strs.Length);
				Assert.AreEqual("one", strs[0]);
				Assert.AreEqual("two", strs[1]);
			}

			{
				string[] strs = ParseChars.Do("one two 3 four 5", d);
				Assert.AreEqual(5, strs.Length);
				Assert.AreEqual("one", strs[0]);
				Assert.AreEqual("two", strs[1]);
				Assert.AreEqual("3", strs[2]);
				Assert.AreEqual("four", strs[3]);
				Assert.AreEqual("5", strs[4]);
			}
		}

		[Test]
		public void TestStrings()
		{
			IParseLineDelimiters d = new TestParseCharsDelimiter();
			{
				string[] strs = ParseChars.Do("'one'", d);
				Assert.AreEqual(3, strs.Length);
				Assert.AreEqual("'", strs[0]);
				Assert.AreEqual("one", strs[1]);
				Assert.AreEqual("'", strs[2]);
			}

			{
				string[] strs = ParseChars.Do("' one '", d);
				Assert.AreEqual(3, strs.Length);
				Assert.AreEqual("'", strs[0]);
				Assert.AreEqual(" one ", strs[1]);
				Assert.AreEqual("'", strs[2]);
			}

			{
				string[] strs = ParseChars.Do("test ' \tone  ' \"more \" end", d);
				Assert.AreEqual(8, strs.Length);
				Assert.AreEqual("test", strs[0]);
				Assert.AreEqual("'", strs[1]);
				Assert.AreEqual(" \tone  ", strs[2]);
				Assert.AreEqual("'", strs[3]);
				Assert.AreEqual("\"", strs[4]);
				Assert.AreEqual("more ", strs[5]);
				Assert.AreEqual("\"", strs[6]);
				Assert.AreEqual("end", strs[7]);
			}
		}

		[Test]
		public void TestToEnd()
		{
			IParseLineDelimiters d = new TestParseCharsDelimiter();
			{	// this documents that multi-char string delims still need work
				// it should be 3 items where "qwert  yuiop" is the last item
				string[] strs = ParseChars.Do("asdf .' qwert  yuiop", d);
				Assert.AreEqual(4, strs.Length);
				Assert.AreEqual("asdf", strs[0]);
				Assert.AreEqual(".'", strs[1]);
				Assert.AreEqual("qwert", strs[2]);
				Assert.AreEqual("yuiop", strs[3]);
			}
		}
	}
}
