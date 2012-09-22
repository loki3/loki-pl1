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

		static Value ToValue(string s, IStack stack)
		{
			DelimiterList list = ParseLine.Do(s, stack);
			return EvalList.Do(list.Nodes, stack);
		}

		[Test]
		public void TestAdd()
		{
			IStack stack = CreateMathStack();
			{	// 3
				Value value = ToValue("l3.add [ 3 ]", stack);
				Assert.AreEqual(3, value.AsInt);
			}
			{	// 3.0
				Value value = ToValue("l3.add [ 3.0 ]", stack);
				Assert.AreEqual(3.0, value.AsFloat);
			}
			{	// 3 + 2 + 1
				Value value = ToValue("l3.add [ 3 2 1 ]", stack);
				Assert.AreEqual(6, value.AsInt);
			}
			{	// 3 + 2.5 + 1
				Value value = ToValue("l3.add [ 3 2.5 1 ]", stack);
				Assert.AreEqual(6.5, value.AsFloat);
			}

			// error reporting
			bool bException = false;
			try
			{
				ToValue("l3.add [ 3 true 1 ]", stack);
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
				Value value = ToValue("l3.subtract [ 42 31 ]", stack);
				Assert.AreEqual(11, value.AsInt);
			}
			{	// 42 - 31.0
				Value value = ToValue("l3.subtract [ 42 31.0 ]", stack);
				Assert.AreEqual(11.0, value.AsFloat);
			}

			// error reporting
			bool bException = false;
			try
			{
				ToValue("l3.subtract [ 3 ]", stack);
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
				Value value = ToValue("l3.multiply [ 2 3 4 ]", stack);
				Assert.AreEqual(24, value.AsInt);
			}
			{	// 1.5 2
				Value value = ToValue("l3.multiply [ 1.5 2 ]", stack);
				Assert.AreEqual(3.0, value.AsFloat);
			}
		}

		[Test]
		public void TestDivide()
		{
			IStack stack = CreateMathStack();
			{	// 6 / 2
				Value value = ToValue("l3.divide [ 6 2 ]", stack);
				Assert.AreEqual(3, value.AsInt);
			}
			{	// 1.5 / 3
				Value value = ToValue("l3.divide [ 1.5 3 ]", stack);
				Assert.AreEqual(0.5, value.AsFloat);
			}
		}

		[Test]
		public void TestSquareRoot()
		{
			IStack stack = CreateMathStack();
			{
				Value value = ToValue("l3.sqrt 16", stack);
				Assert.AreEqual(4, value.AsFloat);
			}
		}
	}
}
