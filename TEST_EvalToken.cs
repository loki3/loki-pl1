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

			internal override Value Eval(DelimiterNode prev, DelimiterNode next, IFunctionRequestor functions)
			{
				Value value1 = EvalNode.Do(prev, functions, null);
				Value value2 = EvalNode.Do(next, functions, null);
				int sum = value1.AsInt + value2.AsInt;
				return new ValueInt(sum);
			}
		}

		/// <summary>Function that adds 1 to previous</summary>
		class TestPrevious1 : ValueFunction
		{
			internal TestPrevious1() : base(true, false) { }

			internal override Value Eval(DelimiterNode prev, DelimiterNode next, IFunctionRequestor functions)
			{
				Value value = EvalNode.Do(prev, functions, null);
				int sum = value.AsInt + 1;
				return new ValueInt(sum);
			}
		}

		/// <summary>Function that adds 1 to next</summary>
		class TestNext1 : ValueFunction
		{
			internal TestNext1() : base(false, true) { }

			internal override Value Eval(DelimiterNode prev, DelimiterNode next, IFunctionRequestor functions)
			{
				Value value = EvalNode.Do(next, functions, null);
				int sum = 1 + value.AsInt;
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
		class TestValueRequestor : INodeRequestor
		{
			public DelimiterNode GetPrevious() { return new DelimiterNodeToken(new Token("3")); }
			public DelimiterNode GetNext() { return new DelimiterNodeToken(new Token("7")); }
		}

		static DelimiterNode ToNode(string s)
		{
			return new DelimiterNodeToken(new Token(s));
		}


		[Test]
		public void TestBuiltin()
		{
			IFunctionRequestor functions = new TestFunctionRequestor();
			INodeRequestor values = new TestValueRequestor();

			Value result = EvalNode.Do(ToNode("1234"), functions, values);
			Assert.AreEqual(1234, result.AsInt);
		}

		[Test]
		public void TestFunctions()
		{
			IFunctionRequestor functions = new TestFunctionRequestor();
			INodeRequestor values = new TestValueRequestor();

			// infix
			Value result = EvalNode.Do(ToNode("sum"), functions, values);
			Assert.AreEqual(10, result.AsInt);

			// only uses previous token
			result = EvalNode.Do(ToNode("prev1"), functions, values);
			Assert.AreEqual(4, result.AsInt);

			// only uses next token
			result = EvalNode.Do(ToNode("next1"), functions, values);
			Assert.AreEqual(8, result.AsInt);
		}

		[Test]
		public void TestFailure()
		{
			IFunctionRequestor functions = new TestFunctionRequestor();
			INodeRequestor values = new TestValueRequestor();

			bool bCatch = false;
			try
			{
				Value result = EvalNode.Do(ToNode("qwer"), functions, values);
			}
			catch (UnrecognizedTokenException)
			{
				bCatch = true;
			}
			Assert.True(bCatch);
		}
	}
}
