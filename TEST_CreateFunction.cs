using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace loki3.core.test
{
	[TestFixture]
	class TEST_CreateFunction
	{
		/// <summary>Function that adds previous and next ints</summary>
		class TestSum : ValueFunction
		{
			internal TestSum() { Init(PatternData.Single("a", ValueType.Int), PatternData.Single("b", ValueType.Int)); }

			internal override Value Eval(DelimiterNode prev, DelimiterNode next, IScope scope, INodeRequestor nodes)
			{
				Value value1 = EvalNode.Do(prev, scope, nodes);
				Value value2 = EvalNode.Do(next, scope, nodes);
				int sum = value1.AsInt + value2.AsInt;
				return new ValueInt(sum);
			}
		}

		[Test]
		public void Test()
		{
			ScopeChain scope = new ScopeChain(null);
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
	}
}
