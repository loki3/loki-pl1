using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace loki3
{
	[TestFixture]
	class TEST_EvalNode
	{
		/// <summary>Function that adds previous and next ints</summary>
		class TestSum : ValueFunction
		{
			internal TestSum() : base(true/*bConsumesPrevious*/, true/*bConsumesNext*/) {}

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
			internal TestPrevious1() : base(true/*bConsumesPrevious*/, false/*bConsumesNext*/) { }

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
			internal TestNext1() : base(false/*bConsumesPrevious*/, true/*bConsumesNext*/) { }

			internal override Value Eval(DelimiterNode prev, DelimiterNode next, IFunctionRequestor functions)
			{
				Value value = EvalNode.Do(next, functions, null);
				int sum = 1 + value.AsInt;
				return new ValueInt(sum);
			}
		}

		/// <summary>Function that makes next ALL CAPS</summary>
		class TestCaps : ValueFunction
		{
			internal TestCaps() : base(false/*bConsumesPrevious*/, true/*bConsumesNext*/) { }

			internal override Value Eval(DelimiterNode prev, DelimiterNode next, IFunctionRequestor functions)
			{
				Value value = EvalNode.Do(next, functions, null);
				string caps = value.AsString.ToUpper();
				return new ValueString(caps);
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
				if (token.Value == "caps")
					return new TestCaps();
				return null;
			}
		}

		/// <summary>Returns previous and next values as ints</summary>
		class TestValueIntRequestor : INodeRequestor
		{
			public DelimiterNode GetPrevious() { return new DelimiterNodeToken(new Token("3")); }
			public DelimiterNode GetNext() { return new DelimiterNodeToken(new Token("7")); }
		}

		/// <summary>Returns given next node</summary>
		class TestValueNodeRequestor : INodeRequestor
		{
			internal TestValueNodeRequestor(DelimiterNode next) { m_next = next; }
			public DelimiterNode GetPrevious() { return null; }
			public DelimiterNode GetNext() { return m_next; }
			private DelimiterNode m_next;
		}

		// convenience method
		static DelimiterNode ToNode(string s)
		{
			return new DelimiterNodeToken(new Token(s));
		}


		[Test]
		public void TestBuiltin()
		{
			IFunctionRequestor functions = new TestFunctionRequestor();
			INodeRequestor values = new TestValueIntRequestor();

			Value result = EvalNode.Do(ToNode("1234"), functions, values);
			Assert.AreEqual(1234, result.AsInt);
		}

		[Test]
		public void TestFunctions()
		{
			IFunctionRequestor functions = new TestFunctionRequestor();
			INodeRequestor values = new TestValueIntRequestor();

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
			INodeRequestor values = new TestValueIntRequestor();

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

		[Test]
		public void TestDelimitedString()
		{
			ValueDelimiter delim = new ValueDelimiter("'", "'", DelimiterType.AsString);
			List<DelimiterNode> nodes = new List<DelimiterNode>();
			nodes.Add(ToNode("this is a test"));
			DelimiterTree tree = new DelimiterTree(delim, nodes);
			DelimiterNodeTree nodetree = new DelimiterNodeTree(tree);

			IFunctionRequestor functions = new TestFunctionRequestor();
			INodeRequestor values = new TestValueNodeRequestor(nodetree);

			{	// delimited string
				Value result = EvalNode.Do(nodetree, functions, values);
				Assert.AreEqual("this is a test", result.AsString);
			}

			{	// function that takes a delimited string
				Value result = EvalNode.Do(ToNode("caps"), functions, values);
				Assert.AreEqual("THIS IS A TEST", result.AsString);
			}
		}

		[Test]
		public void TestArray()
		{
			ValueDelimiter delim = new ValueDelimiter("[", "]", DelimiterType.AsArray);
			List<DelimiterNode> nodes = new List<DelimiterNode>();
			nodes.Add(ToNode("3"));
			nodes.Add(ToNode("7"));
			nodes.Add(ToNode("3"));
			DelimiterTree tree = new DelimiterTree(delim, nodes);
			DelimiterNodeTree nodetree = new DelimiterNodeTree(tree);

			IFunctionRequestor functions = new TestFunctionRequestor();
			INodeRequestor values = new TestValueNodeRequestor(nodetree);

			{	// [3 7 3]
				Value result = EvalNode.Do(nodetree, functions, values);
				Assert.AreEqual(result.Type, ValueType.Array);
				List<Value> array = result.AsArray;
				Assert.AreEqual(3, array.Count);
				Assert.AreEqual(3, array[0].AsInt);
				Assert.AreEqual(7, array[1].AsInt);
				Assert.AreEqual(3, array[2].AsInt);
			}
		}
	}
}
