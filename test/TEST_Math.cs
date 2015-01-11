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
			ScopeChain scope = new ScopeChain();
			Math.Register(scope);

			ValueDelimiter square = new ValueDelimiter("]", DelimiterType.AsArray);
			scope.SetValue("[", square);

			return scope;
		}

		[Test]
		public void TestAdd()
		{
			IScope scope = CreateMathScope();
			{
				Value value = TestSupport.ToValue("l3.add [ 3 4 ]", scope);
				Assert.AreEqual(7, value.AsInt);
			}
			{
				Value value = TestSupport.ToValue("l3.add [ 3.5 4 ]", scope);
				Assert.AreEqual(7.5, value.AsFloat);
			}

			{	// 3
				Value value = TestSupport.ToValue("l3.addArray [ 3 ]", scope);
				Assert.AreEqual(3, value.AsInt);
			}
			{	// 3.0
				Value value = TestSupport.ToValue("l3.addArray [ 3.0 ]", scope);
				Assert.AreEqual(3.0, value.AsFloat);
			}
			{	// 3 + 2 + 1
				Value value = TestSupport.ToValue("l3.addArray [ 3 2 1 ]", scope);
				Assert.AreEqual(6, value.AsInt);
			}
			{	// 3 + 2.5 + 1
				Value value = TestSupport.ToValue("l3.addArray [ 3 2.5 1 ]", scope);
				Assert.AreEqual(6.5, value.AsFloat);
			}

			// error reporting
			bool bException = false;
			try
			{
				TestSupport.ToValue("l3.addArray [ 3 true 1 ]", scope);
			}
			catch (Loki3Exception e)
			{
				Assert.AreEqual("float", e.ExpectedType);
				Assert.AreEqual("bool", e.ActualType);
				bException = true;
			}
			Assert.True(bException);
		}

		[Test]
		public void TestSubtract()
		{
			IScope scope = CreateMathScope();
			{	// 42 - 31
				Value value = TestSupport.ToValue("l3.subtract [ 42 31 ]", scope);
				Assert.AreEqual(11, value.AsInt);
			}
			{	// 42 - 31.0
				Value value = TestSupport.ToValue("l3.subtract [ 42 31.0 ]", scope);
				Assert.AreEqual(11.0, value.AsFloat);
			}

			// error reporting
			bool bException = false;
			try
			{
				TestSupport.ToValue("l3.subtract [ true ]", scope);
			}
			catch (Loki3Exception)
			{
				bException = true;
			}
			Assert.True(bException);
		}

		[Test]
		public void TestMultiply()
		{
			IScope scope = CreateMathScope();
			{	// 2 * 3
				Value value = TestSupport.ToValue("l3.multiply [ 2 3 ]", scope);
				Assert.AreEqual(6, value.AsInt);
			}
			{	// 2 * 3.5
				Value value = TestSupport.ToValue("l3.multiply [ 2 3.5 ]", scope);
				Assert.AreEqual(7, value.AsFloat);
			}

			{	// 2 * 3 * 4
				Value value = TestSupport.ToValue("l3.multiplyArray [ 2 3 4 ]", scope);
				Assert.AreEqual(24, value.AsInt);
			}
			{	// 1.5 2
				Value value = TestSupport.ToValue("l3.multiplyArray [ 1.5 2 ]", scope);
				Assert.AreEqual(3.0, value.AsFloat);
			}
		}

		[Test]
		public void TestDivide()
		{
			IScope scope = CreateMathScope();
			{	// 6 / 2
				Value value = TestSupport.ToValue("l3.divide [ 6 2 ]", scope);
				Assert.AreEqual(3, value.AsInt);
			}
			{	// 1.5 / 3
				Value value = TestSupport.ToValue("l3.divide [ 1.5 3 ]", scope);
				Assert.AreEqual(0.5, value.AsFloat);
			}
		}

		[Test]
		public void TestSquareRoot()
		{
			IScope scope = CreateMathScope();
			{
				Value value = TestSupport.ToValue("l3.sqrt 16", scope);
				Assert.AreEqual(4, value.AsFloat);
			}
		}

		[Test]
		public void TestFloor()
		{
			IScope scope = CreateMathScope();
			{
				Value value = TestSupport.ToValue("l3.floor 2.25", scope);
				Assert.AreEqual(2, value.AsInt);
			}
		}

		[Test]
		public void TestCeling()
		{
			IScope scope = CreateMathScope();
			{
				Value value = TestSupport.ToValue("l3.ceiling 2.25", scope);
				Assert.AreEqual(3, value.AsInt);
			}
		}
	}
}
