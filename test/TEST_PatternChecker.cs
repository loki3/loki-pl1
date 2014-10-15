using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace loki3.core.test
{
	[TestFixture]
	class TEST_PatternChecker
	{
		[Test]
		public void TestSingleTypeless()
		{
			Value pattern = PatternData.Single("a");
			Value match = null, leftover = null;

			{	// single item - match
				Value input = new ValueBool(true);
				Assert.IsTrue(PatternChecker.Do(input, pattern, false/*bShortPat*/, out match, out leftover));
				Assert.IsTrue(match.AsBool);
				Assert.AreEqual(null, leftover);
			}
		}

		[Test]
		public void TestBool()
		{
			Value pattern = PatternData.Single("a", ValueType.Bool);
			Value match = null, leftover = null;

			{	// single bool type - match
				Value input = new ValueBool(true);
				Assert.IsTrue(PatternChecker.Do(input, pattern, false/*bShortPat*/, out match, out leftover));
				Assert.IsTrue(match.AsBool);
				Assert.AreEqual(null, leftover);
			}

			{	// single int type - not a match
				Value input = new ValueInt(42);
				Assert.IsFalse(PatternChecker.Do(input, pattern, false/*bShortPat*/, out match, out leftover));
			}
		}

		[Test]
		public void TestInt()
		{
			Value pattern = PatternData.Single("a", ValueType.Int);
			Value match = null, leftover = null;

			{	// single int type - match
				Value input = new ValueInt(42);
				Assert.IsTrue(PatternChecker.Do(input, pattern, false/*bShortPat*/, out match, out leftover));
				Assert.AreEqual(42, match.AsInt);
				Assert.AreEqual(null, leftover);
			}

			{	// single float type - not a match
				Value input = new ValueFloat(3.14);
				Assert.IsFalse(PatternChecker.Do(input, pattern, false/*bShortPat*/, out match, out leftover));
			}
		}

		[Test]
		public void TestNumber()
		{
			Value pattern = PatternData.Single("a", ValueType.Number);
			Value match = null, leftover = null;

			{	// single int type - match
				Value input = new ValueInt(42);
				Assert.IsTrue(PatternChecker.Do(input, pattern, false/*bShortPat*/, out match, out leftover));
				Assert.AreEqual(42, match.AsInt);
				Assert.AreEqual(null, leftover);
			}

			{	// single float type - match
				Value input = new ValueFloat(3.5);
				Assert.IsTrue(PatternChecker.Do(input, pattern, false/*bShortPat*/, out match, out leftover));
				Assert.AreEqual(3.5, match.AsFloat);
				Assert.AreEqual(null, leftover);
			}

			{	// single string type - not a match
				Value input = new ValueString("hello");
				Assert.IsFalse(PatternChecker.Do(input, pattern, false/*bShortPat*/, out match, out leftover));
			}
		}

		[Test]
		public void TestArray()
		{
			List<Value> list = new List<Value>();
			list.Add(PatternData.Single("a", ValueType.Int));
			list.Add(PatternData.Single("b", ValueType.String));
			ValueArray pattern = new ValueArray(list);
			Value match = null, leftover = null;

			{	// single object - not a match
				Value input = new ValueInt(37);
				Assert.IsFalse(PatternChecker.Do(input, pattern, false/*bShortPat*/, out match, out leftover));
			}

			{	// array with one item - match with leftover
				List<Value> one = new List<Value>();
				one.Add(new ValueInt(5));
				ValueArray input = new ValueArray(one);

				Assert.IsTrue(PatternChecker.Do(input, pattern, false/*bShortPat*/, out match, out leftover));
				Assert.AreEqual(1, match.AsArray.Count);
				Assert.AreEqual(5, match.AsArray[0].AsInt);
				Assert.AreEqual(1, leftover.AsArray.Count);
				Assert.AreEqual("b", leftover.AsArray[0].AsString);
			}

			{	// array with two items of proper type - match
				List<Value> two = new List<Value>();
				two.Add(new ValueInt(5));
				two.Add(new ValueString("hello"));
				ValueArray input = new ValueArray(two);

				Assert.IsTrue(PatternChecker.Do(input, pattern, false/*bShortPat*/, out match, out leftover));
				Assert.AreEqual(2, match.AsArray.Count);
				Assert.AreEqual(5, match.AsArray[0].AsInt);
				Assert.AreEqual("hello", match.AsArray[1].AsString);
				Assert.AreEqual(null, leftover);
			}

			{	// array with two items of wrong type - not a match
				List<Value> two = new List<Value>();
				two.Add(new ValueInt(5));
				two.Add(new ValueFloat(2.718));
				ValueArray input = new ValueArray(two);

				Assert.IsFalse(PatternChecker.Do(input, pattern, false/*bShortPat*/, out match, out leftover));
			}

			{	// array with three items - not a match
				List<Value> three = new List<Value>();
				three.Add(new ValueInt(5));
				three.Add(new ValueString("hello"));
				three.Add(new ValueString("goodbye"));
				ValueArray input = new ValueArray(three);

				Assert.IsFalse(PatternChecker.Do(input, pattern, false/*bShortPat*/, out match, out leftover));
			}

			{	// array with three items & smaller pattern is allowed
				List<Value> three = new List<Value>();
				three.Add(new ValueInt(5));
				three.Add(new ValueString("hello"));
				three.Add(new ValueString("goodbye"));
				ValueArray input = new ValueArray(three);

				Assert.IsTrue(PatternChecker.Do(input, pattern, true/*bShortPat*/, out match, out leftover));
				Assert.AreEqual(2, match.AsArray.Count);
				Assert.AreEqual(5, match.AsArray[0].AsInt);
				Assert.AreEqual("hello", match.AsArray[1].AsString);
				Assert.AreEqual(null, leftover);
			}
		}

		[Test]
		public void TestArrayDefault()
		{
			List<Value> list = new List<Value>();
			list.Add(PatternData.Single("a", ValueType.Int));
			list.Add(PatternData.Single("b", new ValueInt(17)));
			ValueArray pattern = new ValueArray(list);
			Value match = null, leftover = null;

			{	// single object - not a match
				Value input = new ValueInt(37);
				Assert.IsFalse(PatternChecker.Do(input, pattern, false/*bShortPat*/, out match, out leftover));
			}

			{	// array with one item - match with default added
				List<Value> one = new List<Value>();
				one.Add(new ValueInt(5));
				ValueArray input = new ValueArray(one);

				Assert.IsTrue(PatternChecker.Do(input, pattern, false/*bShortPat*/, out match, out leftover));
				Assert.AreEqual(2, match.AsArray.Count);
				Assert.AreEqual(5, match.AsArray[0].AsInt);
				Assert.AreEqual(17, match.AsArray[1].AsInt);
				Assert.AreEqual(null, leftover);
			}

			{	// array with two items of proper type - match
				List<Value> two = new List<Value>();
				two.Add(new ValueInt(5));
				two.Add(new ValueString("hello"));
				ValueArray input = new ValueArray(two);

				Assert.IsTrue(PatternChecker.Do(input, pattern, false/*bShortPat*/, out match, out leftover));
				Assert.AreEqual(2, match.AsArray.Count);
				Assert.AreEqual(5, match.AsArray[0].AsInt);
				Assert.AreEqual("hello", match.AsArray[1].AsString);
				Assert.AreEqual(null, leftover);
			}
		}

		[Test]
		public void TestRestOfArray()
		{
			List<Value> list = new List<Value>();
			list.Add(PatternData.Single("a", ValueType.Int));
			list.Add(PatternData.Rest("b"));
			ValueArray pattern = new ValueArray(list);
			Value match = null, leftover = null;

			{	// array with one item - remainder is empty
				List<Value> one = new List<Value>();
				one.Add(new ValueInt(5));
				ValueArray input = new ValueArray(one);

				Assert.IsTrue(PatternChecker.Do(input, pattern, false/*bShortPat*/, out match, out leftover));
				Assert.AreEqual(2, match.AsArray.Count);
				Assert.AreEqual(5, match.AsArray[0].AsInt);
				List<Value> rest = match.AsArray[1].AsArray;
				Assert.AreEqual(0, rest.Count);
				Assert.AreEqual(null, leftover);
			}

			{	// array with two items of proper type - single remainder
				List<Value> two = new List<Value>();
				two.Add(new ValueInt(5));
				two.Add(new ValueString("hello"));
				ValueArray input = new ValueArray(two);

				Assert.IsTrue(PatternChecker.Do(input, pattern, false/*bShortPat*/, out match, out leftover));
				Assert.AreEqual(2, match.AsArray.Count);
				Assert.AreEqual(5, match.AsArray[0].AsInt);
				List<Value> rest = match.AsArray[1].AsArray;
				Assert.AreEqual(1, rest.Count);
				Assert.AreEqual("hello", rest[0].AsString);
				Assert.AreEqual(null, leftover);
			}

			{	// array with three items of proper type - two items in remainder
				List<Value> three = new List<Value>();
				three.Add(new ValueInt(5));
				three.Add(new ValueString("hello"));
				three.Add(new ValueInt(7));
				ValueArray input = new ValueArray(three);

				Assert.IsTrue(PatternChecker.Do(input, pattern, false/*bShortPat*/, out match, out leftover));
				Assert.AreEqual(2, match.AsArray.Count);
				Assert.AreEqual(5, match.AsArray[0].AsInt);
				List<Value> rest = match.AsArray[1].AsArray;
				Assert.AreEqual(2, rest.Count);
				Assert.AreEqual("hello", rest[0].AsString);
				Assert.AreEqual(7, rest[1].AsInt);
				Assert.AreEqual(null, leftover);
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
				Assert.IsFalse(PatternChecker.Do(input, pattern, false/*bShortPat*/, out match, out leftover));
			}

			{	// input is a map w/ fewer keys - match w/ leftover
				map = new Map();
				map["do?"] = new ValueBool(true);
				ValueMap input = new ValueMap(map);

				Assert.IsTrue(PatternChecker.Do(input, pattern, false/*bShortPat*/, out match, out leftover));
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

				Assert.IsTrue(PatternChecker.Do(input, pattern, false/*bShortPat*/, out match, out leftover));
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

				Assert.IsFalse(PatternChecker.Do(input, pattern, false/*bShortPat*/, out match, out leftover));
			}
		}

		[Test]
		public void TestMapDefault()
		{
			Map map = new Map();
			map["first"] = PatternData.Single("first", new ValueInt(156));	// default
			map["second"] = PatternData.Single("second");
			ValueMap pattern = new ValueMap(map);
			Value match = null, leftover = null;

			{	// input is missing key w/ default - full match
				map = new Map();
				map["second"] = new ValueInt(3);
				ValueMap input = new ValueMap(map);

				Assert.IsTrue(PatternChecker.Do(input, pattern, false/*bShortPat*/, out match, out leftover));
				Assert.AreEqual(2, match.AsMap.Count);
				Assert.AreEqual(156, match.AsMap["first"].AsInt);
				Assert.AreEqual(3, match.AsMap["second"].AsInt);
				Assert.AreEqual(null, leftover);
			}

			{	// input is missing key w/out default - match w/ leftover
				map = new Map();
				map["first"] = new ValueInt(5);
				ValueMap input = new ValueMap(map);

				Assert.IsTrue(PatternChecker.Do(input, pattern, false/*bShortPat*/, out match, out leftover));
				Assert.AreEqual(1, match.AsMap.Count);
				Assert.AreEqual(5, match.AsMap["first"].AsInt);
				Assert.AreEqual(1, leftover.AsMap.Count);
				Assert.AreEqual("second", leftover.AsMap["second"].AsString);
			}
		}

		[Test]
		public void TestRestOfMap()
		{
			Map map = new Map();
			map["a"] = PatternData.Single("a", ValueType.Int);
			map["other"] = PatternData.Rest("other");
			ValueMap pattern = new ValueMap(map);
			Value match = null, leftover = null;

			{	// input is a map w/ fewer keys - match w/ leftover
				map = new Map();
				map["a"] = new ValueInt(14);
				ValueMap input = new ValueMap(map);

				Assert.IsTrue(PatternChecker.Do(input, pattern, false/*bShortPat*/, out match, out leftover));
				Assert.AreEqual(2, match.AsMap.Count);
				Assert.AreEqual(14, match.AsMap["a"].AsInt);
				Map rest = match.AsMap["other"].AsMap;
				Assert.AreEqual(0, rest.Count);				// 'rest' is present but empty
				Assert.AreEqual(null, leftover);
			}

			{	// input contains extra keys that will get stuffed in 'other'
				map = new Map();
				map["a"] = new ValueInt(14);
				map["b"] = new ValueInt(3);
				map["c"] = new ValueInt(15);
				ValueMap input = new ValueMap(map);

				Assert.IsTrue(PatternChecker.Do(input, pattern, false/*bShortPat*/, out match, out leftover));
				Assert.AreEqual(2, match.AsMap.Count);
				Assert.AreEqual(14, match.AsMap["a"].AsInt);
				Map rest = match.AsMap["other"].AsMap;
				Assert.AreEqual(2, rest.Count);
				Assert.AreEqual(3, rest["b"].AsInt);
				Assert.AreEqual(15, rest["c"].AsInt);
				Assert.AreEqual(null, leftover);
			}
		}

		[Test]
		public void TestMapFromArrayPattern()
		{
			List<Value> list = new List<Value>();
			list.Add(PatternData.Single("a", ValueType.Int));
			list.Add(PatternData.Single("b", new ValueInt(17)));
			ValueArray pattern = new ValueArray(list);
			Value match = null, leftover = null;

			{	// get a & b
				Map map = new Map();
				map["a"] = new ValueInt(42);
				map["b"] = new ValueInt(31);
				ValueMap input = new ValueMap(map);

				Assert.IsTrue(PatternChecker.Do(input, pattern, false/*bShortPat*/, out match, out leftover));
				Assert.AreEqual(2, match.AsMap.Count);
				Assert.AreEqual(42, match.AsMap["a"].AsInt);
				Assert.AreEqual(31, match.AsMap["b"].AsInt);
				Assert.AreEqual(null, leftover);
			}

			{	// fill in default for b
				Map map = new Map();
				map["a"] = new ValueInt(42);
				ValueMap input = new ValueMap(map);

				Assert.IsTrue(PatternChecker.Do(input, pattern, false/*bShortPat*/, out match, out leftover));
				Assert.AreEqual(2, match.AsMap.Count);
				Assert.AreEqual(42, match.AsMap["a"].AsInt);
				Assert.AreEqual(17, match.AsMap["b"].AsInt);
				Assert.AreEqual(null, leftover);
			}

			{	// leftover
				Map map = new Map();
				map["b"] = new ValueInt(31);
				ValueMap input = new ValueMap(map);

				Assert.IsTrue(PatternChecker.Do(input, pattern, false/*bShortPat*/, out match, out leftover));
				Assert.AreEqual(1, match.AsMap.Count);
				Assert.AreEqual(31, match.AsMap["b"].AsInt);
				Assert.AreEqual(1, leftover.AsMap.Count);
				Assert.AreEqual("a", leftover.AsMap["a"].AsString);
			}

			{	// get a & b, ignore c because bShortPat=true
				Map map = new Map();
				map["a"] = new ValueInt(42);
				map["b"] = new ValueInt(31);
				map["c"] = new ValueInt(31);
				ValueMap input = new ValueMap(map);

				Assert.IsTrue(PatternChecker.Do(input, pattern, true/*bShortPat*/, out match, out leftover));
				Assert.AreEqual(2, match.AsMap.Count);
				Assert.AreEqual(42, match.AsMap["a"].AsInt);
				Assert.AreEqual(31, match.AsMap["b"].AsInt);
				Assert.AreEqual(null, leftover);
			}

			{	// missing key
				List<Value> list2 = new List<Value>();
				list2.Add(PatternData.Single("a", ValueType.Int));
				list2.Add(PatternData.Rest("other"));
				ValueArray pattern2 = new ValueArray(list2);

				Map map = new Map();
				map["b"] = new ValueInt(31);
				map["c"] = new ValueInt(31);
				ValueMap input = new ValueMap(map);
				Assert.IsFalse(PatternChecker.Do(input, pattern2, true/*bShortPat*/, out match, out leftover));

				// but if "a" is present, it matches
				map["a"] = new ValueInt(42);
				Assert.IsTrue(PatternChecker.Do(input, pattern2, true/*bShortPat*/, out match, out leftover));
			}
		}

		[Test]
		public void TestArrayOneOf()
		{
			// passed value must be a member of this list
			List<Value> options = new List<Value>();
			options.Add(new ValueInt(5));
			options.Add(new ValueInt(6));
			ValueArray oneOf = new ValueArray(options);

			List<Value> list = new List<Value>();
			list.Add(PatternData.Single("a", oneOf));
			ValueArray pattern = new ValueArray(list);
			Value match = null, leftover = null;

			{	// single object that's not a match
				Value input = new ValueInt(4);
				Assert.IsFalse(PatternChecker.Do(input, pattern, false/*bShortPat*/, out match, out leftover));
			}

			{	// array with one item that matches one of the options
				List<Value> one = new List<Value>();
				one.Add(new ValueInt(6));
				ValueArray input = new ValueArray(one);

				Assert.IsTrue(PatternChecker.Do(input, pattern, false/*bShortPat*/, out match, out leftover));
				Assert.AreEqual(1, match.AsArray.Count);
				Assert.AreEqual(6, match.AsArray[0].AsInt);
				Assert.AreEqual(null, leftover);
			}
		}
	}
}
