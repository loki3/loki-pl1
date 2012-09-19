using System;
using NUnit.Framework;

namespace loki3
{
	[TestFixture]
	class TEST_EvalBuiltin
	{
		[Test]
		public void TestNil()
		{
			Value value = EvalBuiltin.Do(new Token("nil"));
			Assert.True(value is ValueNil);
		}

		[Test]
		public void TestBool()
		{
			Value value1 = EvalBuiltin.Do(new Token("true"));
			Assert.True(value1.AsBool);

			Value value2 = EvalBuiltin.Do(new Token("false"));
			Assert.False(value2.AsBool);
		}

		[Test]
		public void TestInt()
		{
			Value value1 = EvalBuiltin.Do(new Token("42"));
			Assert.AreEqual(42, value1.AsInt);

			Value value2 = EvalBuiltin.Do(new Token("-3"));
			Assert.AreEqual(-3, value2.AsInt);
		}

		[Test]
		public void TestFloat()
		{
			Value value1 = EvalBuiltin.Do(new Token("42.25"));
			Assert.AreEqual(42.25, value1.AsFloat);

			Value value2 = EvalBuiltin.Do(new Token("-3.5e10"));
			Assert.AreEqual(-3.5e10, value2.AsFloat);
		}

		[Test]
		public void TestGarbage()
		{
			bool bCatch = false;
			try
			{
				EvalBuiltin.Do(new Token("!@#$"));
			}
			catch (Exception)
			{
				bCatch = true;
			}
			Assert.True(bCatch);
		}
	}
}
