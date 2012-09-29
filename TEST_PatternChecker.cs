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
			ValueBool input = new ValueBool(true);
			Value match = null, leftover = null;

			{	// single unspecified type - match
				Value pattern = PatternData.Single("a");
				Assert.IsTrue(PatternChecker.Do(input, pattern, out match, out leftover));
				Assert.IsTrue(match.AsBool);
				Assert.AreEqual(null, leftover);
			}

			{	// single bool type - match
				Value pattern = PatternData.Single("a", ValueType.Bool);
				Assert.IsTrue(PatternChecker.Do(input, pattern, out match, out leftover));
				Assert.IsTrue(match.AsBool);
				Assert.AreEqual(null, leftover);
			}

			{	// single int type - not a match
				Value pattern = PatternData.Single("a", ValueType.Int);
				Assert.IsFalse(PatternChecker.Do(input, pattern, out match, out leftover));
			}
		}

		[Test]
		public void TestInt()
		{
			ValueInt input = new ValueInt(42);
			Value match = null, leftover = null;

			{	// single unspecified type - match
				Value pattern = PatternData.Single("a");
				Assert.IsTrue(PatternChecker.Do(input, pattern, out match, out leftover));
				Assert.AreEqual(42, match.AsInt);
				Assert.AreEqual(null, leftover);
			}

			{	// single int type - match
				Value pattern = PatternData.Single("a", ValueType.Int);
				Assert.IsTrue(PatternChecker.Do(input, pattern, out match, out leftover));
				Assert.AreEqual(42, match.AsInt);
				Assert.AreEqual(null, leftover);
			}

			{	// single float type - not a match
				Value pattern = PatternData.Single("a", ValueType.Float);
				Assert.IsFalse(PatternChecker.Do(input, pattern, out match, out leftover));
			}
		}

		[Test]
		public void TestArray()
		{
			List<Value> list = new List<Value>();
			list.Add(new ValueInt(5));
			list.Add(new ValueString("hello"));
			ValueArray input = new ValueArray(list);
			Value match = null, leftover = null;

			{	// single object - not a match
				Value pattern = PatternData.Single("a");
				Assert.IsFalse(PatternChecker.Do(input, pattern, out match, out leftover));
			}

			{	// array with one item - match with leftover
				List<Value> one = new List<Value>();
				one.Add(PatternData.Single("asdf"));
				ValueArray pattern = new ValueArray(one);

				Assert.IsTrue(PatternChecker.Do(input, pattern, out match, out leftover));
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

				Assert.IsTrue(PatternChecker.Do(input, pattern, out match, out leftover));
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

				Assert.IsFalse(PatternChecker.Do(input, pattern, out match, out leftover));
			}
		}

		[Test]
		public void TestMap()
		{
			Map map = new Map();
			map["do?"] = PatternData.Single("do?", ValueType.Bool);
			map["body"] = PatternData.Body();
			ValueMap pattern = new ValueMap(map);
			Value match = null, leftover = null;

			{	// single object - not a match
				Value input = PatternData.Single("a");
				Assert.IsFalse(PatternChecker.Do(input, pattern, out match, out leftover));
			}

			{	// input is a map w/ fewer keys - match w/ leftover
				map = new Map();
				map["do?"] = new ValueBool(true);
				ValueMap input = new ValueMap(map);

				Assert.IsTrue(PatternChecker.Do(input, pattern, out match, out leftover));
				Assert.AreEqual(1, match.AsMap.Count);
				Assert.AreEqual(true, match.AsMap["do?"].AsBool);
				Assert.AreEqual(1, leftover.AsMap.Count);
				Assert.AreEqual("l3.body", leftover.AsMap["body"].AsString);
			}

			{	// map w/ same keys - match
				map = new Map();
				map["do?"] = new ValueBool(true);
				map["body"] = PatternData.Body();
				ValueMap input = new ValueMap(map);

				Assert.IsTrue(PatternChecker.Do(input, pattern, out match, out leftover));
				Assert.AreEqual(2, match.AsMap.Count);
				Assert.AreEqual(true, match.AsMap["do?"].AsBool);
				Assert.AreEqual("l3.body", match.AsMap["body"].AsString);
				Assert.AreEqual(null, leftover);
			}

			{	// map w/ extra keys - not a match
				map = new Map();
				map["do?"] = new ValueBool(true);
				map["body"] = PatternData.Body();
				map["something"] = new ValueInt(42);
				ValueMap input = new ValueMap(map);

				Assert.IsFalse(PatternChecker.Do(input, pattern, out match, out leftover));
			}
		}
	}
}
