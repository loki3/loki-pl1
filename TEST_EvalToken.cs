using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace loki3
{
	[TestFixture]
	class TEST_EvalToken
	{
		/// <summary>Function that adds previous and next ints</summary>
		class TestSum : ValueFunction
		{
			internal TestSum() : base(true, true) {}

			internal override Value Eval(Value prev, Value next)
			{
				int sum = prev.AsInt + next.AsInt;
				return new ValueInt(sum);
			}
		}

		/// <summary>Function that adds 1 to previous</summary>
		class TestPrevious1 : ValueFunction
		{
			internal TestPrevious1() : base(true, false) { }

			internal override Value Eval(Value prev, Value next)
			{
				int sum = prev.AsInt + 1;
				return new ValueInt(sum);
			}
		}

		/// <summary>Function that adds 1 to next</summary>
		class TestNext1 : ValueFunction
		{
			internal TestNext1() : base(false, true) { }

			internal override Value Eval(Value prev, Value next)
			{
				int sum = 1 + next.AsInt;
				return new ValueInt(sum);
			}
		}

		/// <summary>Simple function registry</summary>
		class TestFunctionRequestor : IFunctionRequestor
		{
			public ValueFunction Get(Token token)
			{
				if (token.Value == "sum")
					return new TestSum();
				if (token.Value == "prev1")
					return new TestPrevious1();
				if (token.Value == "next1")
					return new TestNext1();
				return null;
			}
		}

		/// <summary>Returns previous and next values as ints</summary>
		class TestValueRequestor : IValueRequestor
		{
			public Value GetPrevious() { return new ValueInt(3); }
			public Value GetNext() { return new ValueInt(7); }
		}

		[Test]
		public void Test()
		{
			IFunctionRequestor functions = new TestFunctionRequestor();
			IValueRequestor values = new TestValueRequestor();

			// infix
			Value result = EvalToken.Do(new Token("sum"), functions, values);
			Assert.AreEqual(10, result.AsInt);

			// only uses previous token
			result = EvalToken.Do(new Token("prev1"), functions, values);
			Assert.AreEqual(4, result.AsInt);

			// only uses next token
			result = EvalToken.Do(new Token("next1"), functions, values);
			Assert.AreEqual(8, result.AsInt);
		}
	}
}
