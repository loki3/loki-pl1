using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace loki3.core.test
{
	[TestFixture]
	class TEST_EvalList
	{
		class TestDelims : IParseLineDelimiters
		{
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
			internal TestSum() { Init(DataForPatterns.Single("a", "ValueInt"), DataForPatterns.Single("b", "ValueInt")); }

			internal override Value Eval(DelimiterNode prev, DelimiterNode next, IScope scope, INodeRequestor nodes)
			{
				Value value1 = EvalNode.Do(prev, scope, nodes);
				Value value2 = EvalNode.Do(next, scope, nodes);
				int sum = value1.AsInt + value2.AsInt;
				return new ValueInt(sum);
			}
		}

		/// <summary>Function that multiplies previous and next ints</summary>
		class TestProduct : ValueFunction
		{
			internal TestProduct() { Init(DataForPatterns.Single("a", "ValueInt"), DataForPatterns.Single("b", "ValueInt"), Precedence.High); }

			internal override Value Eval(DelimiterNode prev, DelimiterNode next, IScope scope, INodeRequestor nodes)
			{
				Value value1 = EvalNode.Do(prev, scope, nodes);
				Value value2 = EvalNode.Do(next, scope, nodes);
				int product = value1.AsInt * value2.AsInt;
				return new ValueInt(product);
			}
		}

		/// <summary>Function that doubles the previous value</summary>
		class TestDoubled : ValueFunctionPost
		{
			internal TestDoubled() { Init(DataForPatterns.Single("a", "ValueInt"), Precedence.High); }

			internal override Value Eval(DelimiterNode prev, IScope scope, INodeRequestor nodes)
			{
				Value value = EvalNode.Do(prev, scope, nodes);
				return new ValueInt(value.AsInt * 2);
			}
		}

		/// <summary>Function that multiplies the next value by a canned value</summary>
		class TestMultiplier : ValueFunctionPre
		{
			internal TestMultiplier(int f) { Init(DataForPatterns.Single("a", "ValueInt"), Precedence.High);  m_f = f; }

			internal override Value Eval(DelimiterNode next, IScope scope, INodeRequestor nodes)
			{
				Value value = EvalNode.Do(next, scope, nodes);
				return new ValueInt(value.AsInt * m_f);
			}

			private int m_f;
		}

		/// <summary>Function that creates a TestMultiplier function based on a passed in int</summary>
		class TestCreateMultiplier : ValueFunctionPre
		{
			internal TestCreateMultiplier() { Init(DataForPatterns.Single("a", "ValueInt"), Precedence.High); }

			internal override Value Eval(DelimiterNode next, IScope scope, INodeRequestor nodes)
			{
				Value value = EvalNode.Do(next, scope, nodes);
				return new TestMultiplier(value.AsInt);
			}
		}

		/// <summary>Registry of test functions</summary>
		internal class TestScope : ScopeChain
		{
			internal TestScope() : base(null)
			{
				SetValue("x", new ValueInt(6));
				SetValue("+", new TestSum());
				SetValue("*", new TestProduct());
				SetValue("doubled", new TestDoubled());
				SetValue("triple", new TestMultiplier(3));
				SetValue("create-multiplier", new TestCreateMultiplier());
			}
		}

		[Test]
		public void TestInfix()
		{
			IParseLineDelimiters pld = new TestDelims();
			IScope scope = new TestScope();

			{
				DelimiterList list = ParseLine.Do("3 + 8", pld);
				Value value = EvalList.Do(list.Nodes, scope);
				Assert.AreEqual(11, value.AsInt);
			}
			{
				DelimiterList list = ParseLine.Do("4 + 2 + 7", pld);
				Value value = EvalList.Do(list.Nodes, scope);
				Assert.AreEqual(13, value.AsInt);
			}
			{	// precedence should work as expected
				DelimiterList list = ParseLine.Do("4 + 2 * 7", pld);
				Value value = EvalList.Do(list.Nodes, scope);
				Assert.AreEqual(18, value.AsInt);
			}
			{	// precedence comes into play
				DelimiterList list = ParseLine.Do("4 * 2 + 7", pld);
				Value value = EvalList.Do(list.Nodes, scope);
				Assert.AreEqual(15, value.AsInt);
			}
			{
				DelimiterList list = ParseLine.Do("3 + x", pld);
				Value value = EvalList.Do(list.Nodes, scope);
				Assert.AreEqual(9, value.AsInt);
			}
		}

		[Test]
		public void TestPrefix()
		{
			IParseLineDelimiters pld = new TestDelims();
			IScope scope = new TestScope();

			{
				DelimiterList list = ParseLine.Do("triple 3", pld);
				Value value = EvalList.Do(list.Nodes, scope);
				Assert.AreEqual(9, value.AsInt);
			}
			{
				DelimiterList list = ParseLine.Do("triple triple 2", pld);
				Value value = EvalList.Do(list.Nodes, scope);
				Assert.AreEqual(18, value.AsInt);
			}
			{
				DelimiterList list = ParseLine.Do("triple x", pld);
				Value value = EvalList.Do(list.Nodes, scope);
				Assert.AreEqual(18, value.AsInt);
			}
		}

		[Test]
		public void TestPostfix()
		{
			IParseLineDelimiters pld = new TestDelims();
			IScope scope = new TestScope();

			{
				DelimiterList list = ParseLine.Do("4 doubled", pld);
				Value value = EvalList.Do(list.Nodes, scope);
				Assert.AreEqual(8, value.AsInt);
			}
			{	// last doubled runs first, asks for the previous value & triggers first doubled
				DelimiterList list = ParseLine.Do("4 doubled doubled", pld);
				Value value = EvalList.Do(list.Nodes, scope);
				Assert.AreEqual(16, value.AsInt);
			}
		}

		[Test]
		public void TestAllfix()
		{
			IParseLineDelimiters pld = new TestDelims();
			IScope scope = new TestScope();

			{
				DelimiterList list = ParseLine.Do("triple 2 + 3 doubled", pld);
				Value value = EvalList.Do(list.Nodes, scope);
				Assert.AreEqual(12, value.AsInt);
			}
		}

		[Test]
		public void TestNested()
		{
			IParseLineDelimiters pld = new TestDelims();
			IScope scope = new TestScope();

			{
				DelimiterList list = ParseLine.Do("2 * ( 3 + 4 )", pld);
				Value value = EvalList.Do(list.Nodes, scope);
				Assert.AreEqual(14, value.AsInt);
			}

			{
				DelimiterList list = ParseLine.Do("2 * ( 3 + ( 2 * 4 ) doubled + triple ( 2 + 3 ) )", pld);
				Value value = EvalList.Do(list.Nodes, scope);
				Assert.AreEqual(68, value.AsInt);
			}
		}

		[Test]
		public void TestFunction()
		{
			IParseLineDelimiters pld = new TestDelims();
			IScope scope = new TestScope();

			{	// 2 + 4 * 5
				DelimiterList list = ParseLine.Do("2 + create-multiplier 4  5", pld);
				Value value = EvalList.Do(list.Nodes, scope);
				Assert.AreEqual(22, value.AsInt);
			}
		}
	}
}
