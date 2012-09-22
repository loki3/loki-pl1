using System;
using System.Collections.Generic;
using loki3.core;
using NUnit.Framework;

namespace loki3.builtin.test
{
	[TestFixture]
	class TEST_Math
	{
		static IStack CreateMathStack()
		{
			StateStack stack = new StateStack(null);
			Math.Register(stack);

			ValueDelimiter square = new ValueDelimiter("[", "]", DelimiterType.AsArray);
			stack.SetValue("[", square);

			return stack;
		}

		[Test]
		public void TestAdd()
		{
			IStack stack = CreateMathStack();
			{	// 3
				DelimiterList list = ParseLine.Do("l3.add [ 3 ]", stack);
				Value value = EvalList.Do(list.Nodes, stack);
				Assert.AreEqual(3, value.AsInt);
			}
			{	// 3.0
				DelimiterList list = ParseLine.Do("l3.add [ 3.0 ]", stack);
				Value value = EvalList.Do(list.Nodes, stack);
				Assert.AreEqual(3.0, value.AsFloat);
			}
			{	// 3 + 2 + 1
				DelimiterList list = ParseLine.Do("l3.add [ 3 2 1 ]", stack);
				Value value = EvalList.Do(list.Nodes, stack);
				Assert.AreEqual(6, value.AsInt);
			}
			{	// 3 + 2.5 + 1
				DelimiterList list = ParseLine.Do("l3.add [ 3 2.5 1 ]", stack);
				Value value = EvalList.Do(list.Nodes, stack);
				Assert.AreEqual(6.5, value.AsFloat);
			}

			// error reporting
			bool bException = false;
			try
			{
				DelimiterList list = ParseLine.Do("l3.add [ 3 true 1 ]", stack);
				Value value = EvalList.Do(list.Nodes, stack);
			}
			catch (WrongTypeException e)
			{
				Assert.AreEqual(loki3.core.ValueType.Float, e.Expected);
				Assert.AreEqual(loki3.core.ValueType.Bool, e.Actual);
				bException = true;
			}
			Assert.True(bException);
		}

		[Test]
		public void TestSubtract()
		{
			IStack stack = CreateMathStack();
			{	// 42 - 31
				DelimiterList list = ParseLine.Do("l3.subtract [ 42 31 ]", stack);
				Value value = EvalList.Do(list.Nodes, stack);
				Assert.AreEqual(11, value.AsInt);
			}
			{	// 42 - 31.0
				DelimiterList list = ParseLine.Do("l3.subtract [ 42 31.0 ]", stack);
				Value value = EvalList.Do(list.Nodes, stack);
				Assert.AreEqual(11.0, value.AsFloat);
			}

			// error reporting
			bool bException = false;
			try
			{
				DelimiterList list = ParseLine.Do("l3.subtract [ 3 ]", stack);
				Value value = EvalList.Do(list.Nodes, stack);
			}
			catch (WrongSizeArray e)
			{
				Assert.AreEqual(e.Expected, 2);
				Assert.AreEqual(e.Actual, 1);
				bException = true;
			}
			Assert.True(bException);
		}

		[Test]
		public void TestMultiply()
		{
			IStack stack = CreateMathStack();
			{	// 2 * 3 * 4
				DelimiterList list = ParseLine.Do("l3.multiply [ 2 3 4 ]", stack);
				Value value = EvalList.Do(list.Nodes, stack);
				Assert.AreEqual(24, value.AsInt);
			}
			{	// 1.5 2
				DelimiterList list = ParseLine.Do("l3.multiply [ 1.5 2 ]", stack);
				Value value = EvalList.Do(list.Nodes, stack);
				Assert.AreEqual(3.0, value.AsFloat);
			}
		}

		[Test]
		public void TestDivide()
		{
			IStack stack = CreateMathStack();
			{	// 6 / 2
				DelimiterList list = ParseLine.Do("l3.divide [ 6 2 ]", stack);
				Value value = EvalList.Do(list.Nodes, stack);
				Assert.AreEqual(3, value.AsInt);
			}
			{	// 1.5 / 3
				DelimiterList list = ParseLine.Do("l3.divide [ 1.5 3 ]", stack);
				Value value = EvalList.Do(list.Nodes, stack);
				Assert.AreEqual(0.5, value.AsFloat);
			}
		}

		[Test]
		public void TestSquareRoot()
		{
			IStack stack = CreateMathStack();
			{	// 
				DelimiterList list = ParseLine.Do("l3.sqrt 16", stack);
				Value value = EvalList.Do(list.Nodes, stack);
				Assert.AreEqual(4, value.AsFloat);
			}
		}
	}
}
