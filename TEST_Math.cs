using System;
using System.Collections.Generic;
using loki3.core;
using NUnit.Framework;

namespace loki3.builtin.test
{
	[TestFixture]
	class TEST_Math
	{
		static IScope CreateMathScope()
		{
			ScopeChain scope = new ScopeChain(null);
			Math.Register(scope);

			ValueDelimiter square = new ValueDelimiter("[", "]", DelimiterType.AsArray);
			scope.SetValue("[", square);

			return scope;
		}

		static Value ToValue(string s, IScope scope)
		{
			DelimiterList list = ParseLine.Do(s, scope);
			return EvalList.Do(list.Nodes, scope);
		}

		[Test]
		public void TestAdd()
		{
			IScope scope = CreateMathScope();
			{	// 3
				Value value = ToValue("l3.add [ 3 ]", scope);
				Assert.AreEqual(3, value.AsInt);
			}
			{	// 3.0
				Value value = ToValue("l3.add [ 3.0 ]", scope);
				Assert.AreEqual(3.0, value.AsFloat);
			}
			{	// 3 + 2 + 1
				Value value = ToValue("l3.add [ 3 2 1 ]", scope);
				Assert.AreEqual(6, value.AsInt);
			}
			{	// 3 + 2.5 + 1
				Value value = ToValue("l3.add [ 3 2.5 1 ]", scope);
				Assert.AreEqual(6.5, value.AsFloat);
			}

			// error reporting
			bool bException = false;
			try
			{
				ToValue("l3.add [ 3 true 1 ]", scope);
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
			IScope scope = CreateMathScope();
			{	// 42 - 31
				Value value = ToValue("l3.subtract [ 42 31 ]", scope);
				Assert.AreEqual(11, value.AsInt);
			}
			{	// 42 - 31.0
				Value value = ToValue("l3.subtract [ 42 31.0 ]", scope);
				Assert.AreEqual(11.0, value.AsFloat);
			}

			// error reporting
			bool bException = false;
			try
			{
				ToValue("l3.subtract [ 3 ]", scope);
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
			IScope scope = CreateMathScope();
			{	// 2 * 3 * 4
				Value value = ToValue("l3.multiply [ 2 3 4 ]", scope);
				Assert.AreEqual(24, value.AsInt);
			}
			{	// 1.5 2
				Value value = ToValue("l3.multiply [ 1.5 2 ]", scope);
				Assert.AreEqual(3.0, value.AsFloat);
			}
		}

		[Test]
		public void TestDivide()
		{
			IScope scope = CreateMathScope();
			{	// 6 / 2
				Value value = ToValue("l3.divide [ 6 2 ]", scope);
				Assert.AreEqual(3, value.AsInt);
			}
			{	// 1.5 / 3
				Value value = ToValue("l3.divide [ 1.5 3 ]", scope);
				Assert.AreEqual(0.5, value.AsFloat);
			}
		}

		[Test]
		public void TestSquareRoot()
		{
			IScope scope = CreateMathScope();
			{
				Value value = ToValue("l3.sqrt 16", scope);
				Assert.AreEqual(4, value.AsFloat);
			}
		}
	}
}