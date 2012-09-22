using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace loki3.core.test
{
	[TestFixture]
	class TEST_CreateFunction
	{
		/// <summary>Function that adds previous and next ints</summary>
		class TestSum : ValueFunctionIn
		{
			internal override Value Eval(DelimiterNode prev, DelimiterNode next, IStack stack, INodeRequestor nodes)
			{
				Value value1 = EvalNode.Do(prev, stack, nodes);
				Value value2 = EvalNode.Do(next, stack, nodes);
				int sum = value1.AsInt + value2.AsInt;
				return new ValueInt(sum);
			}
		}

		[Test]
		public void Test()
		{
			StateStack stack = new StateStack(null);
			stack.SetValue("+", new TestSum());

			{	// create postfix function
				List<string> body = new List<string>();
				body.Add("x + 5");
				CreateFunction.Do(stack, "5+", null, new ValueString("x"), body);
			}
			{
				DelimiterList list = ParseLine.Do("5+ 3", stack);
				Value value = EvalList.Do(list.Nodes, stack);
				Assert.AreEqual(8, value.AsInt);
			}

			{	// create prefix function
				List<string> body = new List<string>();
				body.Add("5 + x");
				CreateFunction.Do(stack, "+5", new ValueString("x"), null, body);
			}
			{
				DelimiterList list = ParseLine.Do("4 +5", stack);
				Value value = EvalList.Do(list.Nodes, stack);
				Assert.AreEqual(9, value.AsInt);
			}

			{	// test both
				DelimiterList list = ParseLine.Do("5+ 2 +5", stack);
				Value value = EvalList.Do(list.Nodes, stack);
				Assert.AreEqual(12, value.AsInt);
			}
		}
	}
}
