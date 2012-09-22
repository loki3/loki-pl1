using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace loki3
{
	[TestFixture]
	class TEST_EvalList
	{
		class TestDelims : IParseLineDelimiters
		{
			public ValueDelimiter GetDelim(char start)
			{
				return null;
			}

			public ValueDelimiter GetDelim(string start)
			{
				if (start == "(")
					return ValueDelimiter.Basic;
				return null;
			}
		}

		/// <summary>Function that adds previous and next ints</summary>
		class TestSum : ValueFunction
		{
			internal TestSum() : base(true/*bConsumesPrevious*/, true/*bConsumesNext*/, Precedence.Medium) { }

			internal override Value Eval(DelimiterNode prev, DelimiterNode next, IFunctionRequestor functions, INodeRequestor nodes)
			{
				Value value1 = EvalNode.Do(prev, functions, nodes);
				Value value2 = EvalNode.Do(next, functions, nodes);
				int sum = value1.AsInt + value2.AsInt;
				return new ValueInt(sum);
			}
		}

		/// <summary>Function that multiplies previous and next ints</summary>
		class TestProduct : ValueFunction
		{
			internal TestProduct() : base(true/*bConsumesPrevious*/, true/*bConsumesNext*/, Precedence.High) { }

			internal override Value Eval(DelimiterNode prev, DelimiterNode next, IFunctionRequestor functions, INodeRequestor nodes)
			{
				Value value1 = EvalNode.Do(prev, functions, nodes);
				Value value2 = EvalNode.Do(next, functions, nodes);
				int product = value1.AsInt * value2.AsInt;
				return new ValueInt(product);
			}
		}

		/// <summary>Function that doubles the previous value</summary>
		class TestDoubled : ValueFunction
		{
			internal TestDoubled() : base(true/*bConsumesPrevious*/, false/*bConsumesNext*/, Precedence.High) { }

			internal override Value Eval(DelimiterNode prev, DelimiterNode next, IFunctionRequestor functions, INodeRequestor nodes)
			{
				Value value = EvalNode.Do(prev, functions, nodes);
				return new ValueInt(value.AsInt * 2);
			}
		}

		/// <summary>Function that multiplies the next value by a canned value</summary>
		class TestMultiplier : ValueFunction
		{
			internal TestMultiplier(int f) : base(false/*bConsumesPrevious*/, true/*bConsumesNext*/, Precedence.High) { m_f = f; }

			internal override Value Eval(DelimiterNode prev, DelimiterNode next, IFunctionRequestor functions, INodeRequestor nodes)
			{
				Value value = EvalNode.Do(next, functions, nodes);
				return new ValueInt(value.AsInt * m_f);
			}

			private int m_f;
		}

		/// <summary>Function that creates a TestMultiplier function based on a passed in int</summary>
		class TestCreateMultiplier : ValueFunction
		{
			internal TestCreateMultiplier() : base(false/*bConsumesPrevious*/, true/*bConsumesNext*/, Precedence.High) { }

			internal override Value Eval(DelimiterNode prev, DelimiterNode next, IFunctionRequestor functions, INodeRequestor nodes)
			{
				Value value = EvalNode.Do(next, functions, nodes);
				return new TestMultiplier(value.AsInt);
			}
		}

		/// <summary>Registry of test functions</summary>
		internal class TestFunctions : IFunctionRequestor
		{
			internal TestFunctions()
			{
				m_funcs["+"] = new TestSum();
				m_funcs["*"] = new TestProduct();
				m_funcs["doubled"] = new TestDoubled();
				m_funcs["triple"] = new TestMultiplier(3);
				m_funcs["create-multiplier"] = new TestCreateMultiplier();
			}

			#region IFunctionRequestor Members
			public ValueFunction Get(Token token)
			{
				ValueFunction func;
				if (m_funcs.TryGetValue(token.Value, out func))
					return func;
				return null;
			}
			#endregion

			private Dictionary<string, ValueFunction> m_funcs = new Dictionary<string, ValueFunction>();
		}

		[Test]
		public void TestInfix()
		{
			IParseLineDelimiters pld = new TestDelims();
			IFunctionRequestor functions = new TestFunctions();

			{
				DelimiterList list = ParseLine.Do("3 + 8", pld);
				Value value = EvalList.Do(list.Nodes, functions);
				Assert.AreEqual(11, value.AsInt);
			}
			{
				DelimiterList list = ParseLine.Do("4 + 2 + 7", pld);
				Value value = EvalList.Do(list.Nodes, functions);
				Assert.AreEqual(13, value.AsInt);
			}
			{
				DelimiterList list = ParseLine.Do("4 + 2 * 7", pld);
				Value value = EvalList.Do(list.Nodes, functions);
				Assert.AreEqual(18, value.AsInt);
			}
			{	// precedence comes into play
				DelimiterList list = ParseLine.Do("4 * 2 + 7", pld);
				Value value = EvalList.Do(list.Nodes, functions);
				Assert.AreEqual(15, value.AsInt);
			}
		}

		[Test]
		public void TestPrefix()
		{
			IParseLineDelimiters pld = new TestDelims();
			IFunctionRequestor functions = new TestFunctions();

			{
				DelimiterList list = ParseLine.Do("triple 3", pld);
				Value value = EvalList.Do(list.Nodes, functions);
				Assert.AreEqual(9, value.AsInt);
			}
			{
				DelimiterList list = ParseLine.Do("triple triple 2", pld);
				Value value = EvalList.Do(list.Nodes, functions);
				Assert.AreEqual(18, value.AsInt);
			}
		}

		[Test]
		public void TestPostfix()
		{
			IParseLineDelimiters pld = new TestDelims();
			IFunctionRequestor functions = new TestFunctions();

			{
				DelimiterList list = ParseLine.Do("4 doubled", pld);
				Value value = EvalList.Do(list.Nodes, functions);
				Assert.AreEqual(8, value.AsInt);
			}
			{	// last doubled runs first, asks for the previous value & triggers first doubled
				DelimiterList list = ParseLine.Do("4 doubled doubled", pld);
				Value value = EvalList.Do(list.Nodes, functions);
				Assert.AreEqual(16, value.AsInt);
			}
		}

		[Test]
		public void TestAllfix()
		{
			IParseLineDelimiters pld = new TestDelims();
			IFunctionRequestor functions = new TestFunctions();

			{
				DelimiterList list = ParseLine.Do("triple 2 + 3 doubled", pld);
				Value value = EvalList.Do(list.Nodes, functions);
				Assert.AreEqual(12, value.AsInt);
			}
		}

		[Test]
		public void TestNested()
		{
			IParseLineDelimiters pld = new TestDelims();
			IFunctionRequestor functions = new TestFunctions();

			{
				DelimiterList list = ParseLine.Do("2 * ( 3 + 4 )", pld);
				Value value = EvalList.Do(list.Nodes, functions);
				Assert.AreEqual(14, value.AsInt);
			}

			{
				DelimiterList list = ParseLine.Do("2 * ( 3 + ( 2 * 4 ) doubled + triple ( 2 + 3 ) )", pld);
				Value value = EvalList.Do(list.Nodes, functions);
				Assert.AreEqual(68, value.AsInt);
			}
		}

		[Test]
		public void TestFunction()
		{
			IParseLineDelimiters pld = new TestDelims();
			IFunctionRequestor functions = new TestFunctions();

			{	// 2 + 4 * 5
				DelimiterList list = ParseLine.Do("2 + create-multiplier 4  5", pld);
				Value value = EvalList.Do(list.Nodes, functions);
				Assert.AreEqual(22, value.AsInt);
			}
		}
	}
}
