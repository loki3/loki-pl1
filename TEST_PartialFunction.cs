using System.Collections.Generic;
using NUnit.Framework;
using loki3.builtin.test;

namespace loki3.core.test
{
	[TestFixture]
	class TEST_PartialFunction
	{
		/// <summary>[a1 a2] -> a1 - a2</summary>
		class SubtractArray : ValueFunctionPre
		{
			internal override Value ValueCopy() { return new SubtractArray(); }

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
			internal override Value ValueCopy() { return new SubtractMap(); }

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

		static IScope CreateScope()
		{
			ScopeChain scope = new ScopeChain();
			scope.SetValue("subtract-array", new SubtractArray());
			scope.SetValue("subtract-map", new SubtractMap());
			scope.SetValue("create-map", new CreateMap());

			ValueDelimiter square = new ValueDelimiter("]", DelimiterType.AsArray);
			scope.SetValue("[", square);
			ValueDelimiter curly = new ValueDelimiter("}", DelimiterType.AsArray, new CreateMap());
			scope.SetValue("{", curly);

			return scope;
		}

		[Test]
		public void TestPre()
		{
			IScope scope = CreateScope();

			{	// partial function
				Value partial = TestSupport.ToValue("subtract-array [ 3 ]", scope);
				Assert.IsTrue(partial is ValueFunction);

				// "subtract-array [ 3 ]" gets turned into a function that takes one more value
				Value value = TestSupport.ToValue("subtract-array [ 3 ] [ 1 ]", scope);
				Assert.AreEqual(2, value.AsInt);

				// this is the same as passing them both at the same time
				value = TestSupport.ToValue("subtract-array [ 3 1 ]", scope);
				Assert.AreEqual(2, value.AsInt);
			}
		}

		[Test]
		public void TestPost()
		{
			IScope scope = CreateScope();

			{	// bind to a
				Value partial = TestSupport.ToValue("{ :a 3 } subtract-map", scope);
				Assert.IsTrue(partial is ValueFunction);

				// "{ :a 3 } subtract-map" gets turned into a function that takes one more value
				Value value = TestSupport.ToValue("{ :b 1 } { :a 3 } subtract-map", scope);
				Assert.AreEqual(2, value.AsInt);
			}

			{	// bind to b
				Value partial = TestSupport.ToValue("{ :b 7 } subtract-map", scope);
				Assert.IsTrue(partial is ValueFunction);

				// "{ :b 7 } subtract-map" gets turned into a function that takes one more value
				Value value = TestSupport.ToValue("{ :a 10 } { :b 7 } subtract-map", scope);
				Assert.AreEqual(3, value.AsInt);
			}
		}
	}
}
