using System.Collections.Generic;
using NUnit.Framework;

namespace loki3.core.test
{
	[TestFixture]
	class TEST_PartialFunction
	{
		/// <summary>[a1 a2] -> a1 - a2</summary>
		class SubtractArray : ValueFunctionPre
		{
			internal SubtractArray()
			{
				List<Value> list = new List<Value>();
				list.Add(PatternData.Single("a", ValueType.Int));
				list.Add(PatternData.Single("b", ValueType.Int));
				ValueArray array = new ValueArray(list);
				Init(array);
			}

			internal override Value Eval(Value arg, IScope scope)
			{
				List<Value> list = arg.AsArray;
				return new ValueInt(list[0].AsInt - list[1].AsInt);
			}
		}

		/// <summary>{:a :b} -> a - b</summary>
		class SubtractMap : ValueFunctionPost
		{
			internal SubtractMap()
			{
				Map map = new Map();
				map["a"] = PatternData.Single("a", ValueType.Int);
				map["b"] = PatternData.Single("b", ValueType.Int);
				ValueMap vMap = new ValueMap(map);
				Init(vMap);
			}

			internal override Value Eval(Value arg, IScope scope)
			{
				Map map = arg.AsMap;
				return new ValueInt(map["a"].AsInt - map["b"].AsInt);
			}
		}

		/// <summary>[k1 v1 k2 v2 ...] -> map</summary>
		class CreateMap : ValueFunctionPre
		{
			internal CreateMap() { Init(PatternData.ArrayEnd("a")); }

			internal override Value Eval(Value arg, IScope scope)
			{
				List<Value> list = arg.AsArray;

				Map map = new Map();
				int count = list.Count;
				for (int i = 0; i < count; i += 2)
				{
					string key = list[i].AsString;
					Value value = list[i + 1];
					map[key] = value;
				}
				return new ValueMap(map);
			}
		}

		static IScope CreateScope()
		{
			ScopeChain scope = new ScopeChain();
			scope.SetValue("subtract-array", new SubtractArray());
			scope.SetValue("subtract-map", new SubtractMap());
			scope.SetValue("create-map", new CreateMap());

			ValueDelimiter square = new ValueDelimiter("[", "]", DelimiterType.AsArray);
			scope.SetValue("[", square);
			ValueDelimiter curly = new ValueDelimiter("{", "}", DelimiterType.AsArray, new CreateMap());
			scope.SetValue("{", curly);

			return scope;
		}

		static Value ToValue(string s, IScope scope)
		{
			DelimiterList list = ParseLine.Do(s, scope);
			return EvalList.Do(list.Nodes, scope);
		}

		[Test]
		public void TestPre()
		{
			IScope scope = CreateScope();

			{	// partial function
				Value partial = ToValue("subtract-array [ 3 ]", scope);
				Assert.IsTrue(partial is ValueFunction);

				// "subtract-array [ 3 ]" gets turned into a function that takes one more value
				Value value = ToValue("subtract-array [ 3 ] [ 1 ]", scope);
				Assert.AreEqual(2, value.AsInt);

				// this is the same as passing them both at the same time
				value = ToValue("subtract-array [ 3 1 ]", scope);
				Assert.AreEqual(2, value.AsInt);
			}
		}

		[Test]
		public void TestPost()
		{
			IScope scope = CreateScope();

			{	// bind to a
				Value partial = ToValue("{ :a 3 } subtract-map", scope);
				Assert.IsTrue(partial is ValueFunction);

				// "{ :a 3 } subtract-map" gets turned into a function that takes one more value
				Value value = ToValue("{ :b 1 } { :a 3 } subtract-map", scope);
				Assert.AreEqual(2, value.AsInt);
			}

			{	// bind to b
				Value partial = ToValue("{ :b 7 } subtract-map", scope);
				Assert.IsTrue(partial is ValueFunction);

				// "{ :b 7 } subtract-map" gets turned into a function that takes one more value
				Value value = ToValue("{ :a 10 } { :b 7 } subtract-map", scope);
				Assert.AreEqual(3, value.AsInt);
			}
		}
	}
}
