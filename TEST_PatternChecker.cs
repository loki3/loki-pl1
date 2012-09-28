using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace loki3.core.test
{
	[TestFixture]
	class TEST_PatternChecker
	{
		[Test]
		public void TestBool()
		{
			ValueBool b = new ValueBool(true);
			Value match = null, leftover = null;

			{	// single unspecified type - match
				Value pattern = PatternData.Single("a");
				Assert.IsTrue(PatternChecker.Do(b, pattern, out match, out leftover));
				Assert.IsTrue(match.AsBool);
				Assert.AreEqual(null, leftover);
			}

			{	// single bool type - match
				Value pattern = PatternData.Single("a", "ValueBool");
				Assert.IsTrue(PatternChecker.Do(b, pattern, out match, out leftover));
				Assert.IsTrue(match.AsBool);
				Assert.AreEqual(null, leftover);
			}

			{	// single int type - not a match
				Value pattern = PatternData.Single("a", "ValueInt");
				Assert.IsFalse(PatternChecker.Do(b, pattern, out match, out leftover));
			}
		}

		[Test]
		public void TestInt()
		{
			ValueInt i = new ValueInt(42);
			Value match = null, leftover = null;

			{	// single unspecified type - match
				Value pattern = PatternData.Single("a");
				Assert.IsTrue(PatternChecker.Do(i, pattern, out match, out leftover));
				Assert.AreEqual(42, match.AsInt);
				Assert.AreEqual(null, leftover);
			}

			{	// single int type - match
				Value pattern = PatternData.Single("a", "ValueInt");
				Assert.IsTrue(PatternChecker.Do(i, pattern, out match, out leftover));
				Assert.AreEqual(42, match.AsInt);
				Assert.AreEqual(null, leftover);
			}

			{	// single float type - not a match
				Value pattern = PatternData.Single("a", "ValueFloat");
				Assert.IsFalse(PatternChecker.Do(i, pattern, out match, out leftover));
			}
		}

		[Test]
		public void TestArray()
		{
			List<Value> list = new List<Value>();
			list.Add(new ValueInt(5));
			list.Add(new ValueString("hello"));
			ValueArray array = new ValueArray(list);
			Value match = null, leftover = null;

			{	// single object - not a match
				Value pattern = PatternData.Single("a");
				Assert.IsFalse(PatternChecker.Do(array, pattern, out match, out leftover));
			}

			{	// array with one item - match with leftover
				List<Value> one = new List<Value>();
				one.Add(PatternData.Single("asdf"));
				ValueArray pattern = new ValueArray(one);

				Assert.IsTrue(PatternChecker.Do(array, pattern, out match, out leftover));
				Assert.AreEqual(1, match.AsArray.Count);
				Assert.AreEqual(5, match.AsArray[0].AsInt);
				Assert.AreEqual(1, leftover.AsArray.Count);
				Assert.AreEqual("hello", leftover.AsArray[0].AsString);
			}

			{	// array with two items - match
				List<Value> two = new List<Value>();
				two.Add(PatternData.Single("asdf"));
				two.Add(PatternData.Single("qwert"));
				ValueArray pattern = new ValueArray(two);

				Assert.IsTrue(PatternChecker.Do(array, pattern, out match, out leftover));
				Assert.AreEqual(2, match.AsArray.Count);
				Assert.AreEqual(5, match.AsArray[0].AsInt);
				Assert.AreEqual("hello", match.AsArray[1].AsString);
				Assert.AreEqual(null, leftover);
			}

			{	// array with three items - not a match
				List<Value> three = new List<Value>();
				three.Add(PatternData.Single("asdf"));
				three.Add(PatternData.Single("qwert"));
				three.Add(PatternData.Single("yuiop"));
				ValueArray pattern = new ValueArray(three);

				Assert.IsFalse(PatternChecker.Do(array, pattern, out match, out leftover));
			}
		}

		[Test]
		public void TestMap()
		{
		}
	}
}
