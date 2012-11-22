using System.Collections.Generic;
using NUnit.Framework;

namespace loki3.core.test
{
	[TestFixture]
	class TEST_EvalLines
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

		/// <summary>adds up everything in the body</summary>
		class AddUpBody : ValueFunctionPre
		{
			internal override Value ValueCopy() { return new AddUpBody(); }

			internal AddUpBody()
			{
				Init(PatternData.Body());
			}

			internal override Value Eval(Value arg, IScope scope)
			{
				List<DelimiterList> body = ValueFunction.GetBody(arg);
				LineConsumer consumer = new LineConsumer(body);
				int total = 0;
				while (consumer.HasCurrent())
				{
					Value value = EvalLines.DoOne(consumer, scope);
					total += value.AsInt;
				}
				return new ValueInt(total);
			}
		}

		static IScope CreateScope()
		{
			ScopeChain scope = new ScopeChain();
			scope.SetValue("subtract", new SubtractArray());
			scope.SetValue("add", new AddUpBody());

			ValueDelimiter square = new ValueDelimiter("[", "]", DelimiterType.AsArray);
			scope.SetValue("[", square);

			return scope;
		}

		[Test]
		public void TestSingleLine()
		{
			{	// simple int
				string[] strings = { "5" };
				LineConsumer lines = new LineConsumer(strings);
				IScope scope = CreateScope();
				Value value = EvalLines.Do(lines, scope);
				Assert.AreEqual(5, value.AsInt);
			}

			{	// function
				string[] strings = { "subtract [ 7 4 ]" };
				LineConsumer lines = new LineConsumer(strings);
				IScope scope = CreateScope();
				Value value = EvalLines.Do(lines, scope);
				Assert.AreEqual(3, value.AsInt);
			}

#if false
			{	// functions
				string[] strings = { "subtract [ 7 subtract [ 15 11 ] ]" };
				LineConsumer lines = new LineConsumer(strings);
				IScope scope = CreateScope();
				Value value = EvalLines.Do(lines, scope);
				Assert.AreEqual(3, value.AsInt);
			}
#endif
		}


		[Test]
		public void TestBody()
		{
			{
				string[] strings = { "add", " 5", " 4" };
				LineConsumer lines = new LineConsumer(strings);
				IScope scope = CreateScope();
				Value value = EvalLines.Do(lines, scope);
				Assert.AreEqual(9, value.AsInt);
			}
		}

		[Test]
		public void TestNestedBodies()
		{
			{	// 5 + (3 + 4) + 7
				string[] strings = { "add", " 5", " add", "  3", "  4", " 7" };
				LineConsumer lines = new LineConsumer(strings);
				IScope scope = CreateScope();
				Value value = EvalLines.Do(lines, scope);
				Assert.AreEqual(19, value.AsInt);
			}
		}
	}
}
