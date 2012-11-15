using System;
using System.Collections.Generic;
using NUnit.Framework;
using loki3.builtin.test;

namespace loki3.core.test
{
	[TestFixture]
	class TEST_CreateFunction
	{
		/// <summary>Function that adds previous and next ints</summary>
		class TestSum : ValueFunction
		{
			internal TestSum() { Init(PatternData.Single("a", ValueType.Int), PatternData.Single("b", ValueType.Int)); }

			internal override Value Eval(DelimiterNode prev, DelimiterNode next, IScope scope, INodeRequestor nodes, ILineRequestor requestor)
			{
				Value value1 = EvalNode.Do(prev, scope, nodes, requestor);
				Value value2 = EvalNode.Do(next, scope, nodes, requestor);
				int sum = value1.AsInt + value2.AsInt;
				return new ValueInt(sum);
			}
		}

		[Test]
		public void TestSimple()
		{
			ScopeChain scope = new ScopeChain();
			scope.SetValue("+", new TestSum());

			{	// create postfix function
				List<string> body = new List<string>();
				body.Add("x + 5");
				CreateFunction.Do(scope, "5+", null, new ValueString("x"), body);
			}
			{
				DelimiterList list = ParseLine.Do("5+ 3", scope);
				Value value = EvalList.Do(list.Nodes, scope);
				Assert.AreEqual(8, value.AsInt);
			}

			{	// create prefix function
				List<string> body = new List<string>();
				body.Add("5 + x");
				CreateFunction.Do(scope, "+5", new ValueString("x"), null, body);
			}
			{
				DelimiterList list = ParseLine.Do("4 +5", scope);
				Value value = EvalList.Do(list.Nodes, scope);
				Assert.AreEqual(9, value.AsInt);
			}

			{	// test both
				DelimiterList list = ParseLine.Do("5+ 2 +5", scope);
				Value value = EvalList.Do(list.Nodes, scope);
				Assert.AreEqual(12, value.AsInt);
			}
		}

		[Test]
		public void TestArrayParams()
		{
			ScopeChain scope = new ScopeChain();
			scope.SetValue("+", new TestSum());
			ValueDelimiter square = new ValueDelimiter("[", "]", DelimiterType.AsArray);
			scope.SetValue("[", square);

			{	// create postfix function that takes [ a b ]
				List<Value> list = new List<Value>();
				list.Add(PatternData.Single("a", ValueType.Int));
				list.Add(PatternData.Single("b", ValueType.Int));
				ValueArray array = new ValueArray(list);

				List<string> body = new List<string>();
				body.Add("a + b");

				CreateFunction.Do(scope, "add", null, array, body);
			}
			{	// try out the function
				DelimiterList list = ParseLine.Do("add [ 2 5 ]", scope);
				Value value = EvalList.Do(list.Nodes, scope);
				Assert.AreEqual(7, value.AsInt);
			}
		}

		[Test]
		public void TestMapParams()
		{
			ScopeChain scope = new ScopeChain();
			scope.SetValue("+", new TestSum());
			ValueDelimiter square = new ValueDelimiter("[", "]", DelimiterType.AsArray);
			scope.SetValue("[", square);
			ValueDelimiter curly = new ValueDelimiter("{", "}", DelimiterType.AsArray, new CreateMap());
			scope.SetValue("{", curly);

			{	// create postfix function that takes { :a :b }
				Map map = new Map();
				map["a"] = PatternData.Single("a", ValueType.Int);
				map["b"] = PatternData.Single("b", ValueType.Int);
				ValueMap vMap = new ValueMap(map);

				List<string> body = new List<string>();
				body.Add("a + b");

				CreateFunction.Do(scope, "add", null, vMap, body);
			}
			{	// try out the function
				DelimiterList list = ParseLine.Do("add { :a 2 :b 5 }", scope);
				Value value = EvalList.Do(list.Nodes, scope);
				Assert.AreEqual(7, value.AsInt);
			}

			{	// try it as a partial
				DelimiterList list = ParseLine.Do("add { :a 2 } { :b 5 }", scope);
				Value value = EvalList.Do(list.Nodes, scope);
				Assert.AreEqual(7, value.AsInt);
			}
		}
	}
}
