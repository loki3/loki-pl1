using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace loki3.core.test
{
	[TestFixture]
	class TEST_EvalNode
	{
		/// <summary>Function that adds previous and next ints</summary>
		class TestSum : ValueFunction
		{
			internal override Value ValueCopy() { return new TestSum(); }

			internal TestSum() { Init(PatternData.Single("a", ValueType.Int), PatternData.Single("b", ValueType.Int)); }

			internal override Value Eval(DelimiterNode prev, DelimiterNode next, IScope paramScope, IScope bodyScope, INodeRequestor nodes, ILineRequestor requestor)
			{
				Value value1 = EvalNode.Do(prev, paramScope, nodes, requestor);
				Value value2 = EvalNode.Do(next, paramScope, nodes, requestor);
				int sum = value1.AsInt + value2.AsInt;
				return new ValueInt(sum);
			}
		}

		/// <summary>Function that adds 1 to previous</summary>
		class TestPrevious1 : ValueFunctionPost
		{
			internal override Value ValueCopy() { return new TestPrevious1(); }

			internal TestPrevious1() { Init(PatternData.Single("a", ValueType.Int)); }

			internal override Value Eval(Value arg, IScope scope)
			{
				int sum = arg.AsInt + 1;
				return new ValueInt(sum);
			}
		}

		/// <summary>Function that adds 1 to next</summary>
		class TestNext1 : ValueFunctionPre
		{
			internal override Value ValueCopy() { return new TestNext1(); }

			internal TestNext1() { Init(PatternData.Single("a", ValueType.Int)); }

			internal override Value Eval(Value arg, IScope scope)
			{
				int sum = 1 + arg.AsInt;
				return new ValueInt(sum);
			}
		}

		/// <summary>Function that makes next ALL CAPS</summary>
		class TestCaps : ValueFunctionPre
		{
			internal override Value ValueCopy() { return new TestCaps(); }

			internal TestCaps() { Init(PatternData.Single("a", ValueType.String)); }

			internal override Value Eval(Value arg, IScope scope)
			{
				string caps = arg.AsString.ToUpper();
				return new ValueString(caps);
			}
		}

		/// <summary>Simple function registry</summary>
		class TestScope : IScope
		{
			public Value GetValue(Token token)
			{
				if (token.Value == "sum")
					return new TestSum();
				if (token.Value == "prev1")
					return new TestPrevious1();
				if (token.Value == "next1")
					return new TestNext1();
				if (token.Value == "caps")
					return new TestCaps();
				if (token.Value == "x")
					return new ValueFloat(3.5);
				return null;
			}
			public void SetValue(string token, Value value) { }
			public string Name { get { return ""; } }
			public string Category { get { return ""; } }
			public ValueDelimiter GetDelim(string start, out bool anyToken) { anyToken = false; return null; }
			public Dictionary<string, ValueDelimiter> GetStringDelims() { return null; }
			public IScope Exists(string token) { return null; }
			public IScope Parent { get { return null; } }
			public Value AsValue { get { return null; } }
			public Map AsMap { get { return null; } }
			public string FunctionName { set { } }
			public void Exit() {}
			public void AddOnExit(ValueFunction function, Value prevValue, Value nextValue) {}
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
			IScope scope = new TestScope();
			INodeRequestor values = new TestValueIntRequestor();

			Value result = EvalNode.Do(ToNode("1234"), scope, values, null);
			Assert.AreEqual(1234, result.AsInt);
		}

		[Test]
		public void TestVariable()
		{
			IScope scope = new TestScope();
			INodeRequestor values = new TestValueIntRequestor();

			Value result = EvalNode.Do(ToNode("x"), scope, values, null);
			Assert.AreEqual(3.5, result.AsFloat);
		}

		[Test]
		public void TestFunctions()
		{
			IScope scope = new TestScope();
			INodeRequestor values = new TestValueIntRequestor();

			// infix
			Value result = EvalNode.Do(ToNode("sum"), scope, values, null);
			Assert.AreEqual(10, result.AsInt);

			// only uses previous token
			result = EvalNode.Do(ToNode("prev1"), scope, values, null);
			Assert.AreEqual(4, result.AsInt);

			// only uses next token
			result = EvalNode.Do(ToNode("next1"), scope, values, null);
			Assert.AreEqual(8, result.AsInt);
		}

		[Test]
		public void TestFailure()
		{
			IScope scope = new TestScope();
			INodeRequestor values = new TestValueIntRequestor();

			bool bCatch = false;
			try
			{
				Value result = EvalNode.Do(ToNode("qwer"), scope, values, null);
			}
			catch (Loki3Exception e)
			{
				Assert.True(e.Errors.ContainsKey(Loki3Exception.keyBadToken));
				Assert.AreEqual("qwer", e.Errors[Loki3Exception.keyBadToken].AsString);
				bCatch = true;
			}
			Assert.True(bCatch);
		}

		[Test]
		public void TestDelimitedString()
		{
			ValueDelimiter delim = new ValueDelimiter("'", DelimiterType.AsString);
			List<DelimiterNode> nodes = new List<DelimiterNode>();
			string str = "this is a test";
			nodes.Add(ToNode(str));
			DelimiterList list = new DelimiterList(delim, nodes, 0, "'", str, null);
			DelimiterNodeList nodelist = new DelimiterNodeList(list);

			IScope scope = new TestScope();
			INodeRequestor values = new TestValueNodeRequestor(nodelist);

			{	// delimited string
				Value result = EvalNode.Do(nodelist, scope, values, null);
				Assert.AreEqual("this is a test", result.AsString);
			}

			{	// function that takes a delimited string
				Value result = EvalNode.Do(ToNode("caps"), scope, values, null);
				Assert.AreEqual("THIS IS A TEST", result.AsString);
			}
		}

		[Test]
		public void TestArray()
		{
			ValueDelimiter delim = new ValueDelimiter("]", DelimiterType.AsArray);
			List<DelimiterNode> nodes = new List<DelimiterNode>();
			nodes.Add(ToNode("3"));
			nodes.Add(ToNode("7"));
			nodes.Add(ToNode("3"));
			DelimiterList list = new DelimiterList(delim, nodes, 0, "[", "3 7 3", null);
			DelimiterNodeList nodelist = new DelimiterNodeList(list);

			IScope scope = new TestScope();
			INodeRequestor values = new TestValueNodeRequestor(nodelist);

			{	// [3 7 3]
				Value result = EvalNode.Do(nodelist, scope, values, null);
				Assert.AreEqual(result.Type, ValueType.Array);
				List<Value> array = result.AsArray;
				Assert.AreEqual(3, array.Count);
				Assert.AreEqual(3, array[0].AsInt);
				Assert.AreEqual(7, array[1].AsInt);
				Assert.AreEqual(3, array[2].AsInt);
			}
		}

		/// <summary>Simple function that adds an array of ints</summary>
		class AddFunction : ValueFunctionPre
		{
			internal override Value ValueCopy() { return new AddFunction(); }

			internal AddFunction() { Init(PatternData.Rest("a", ValueType.Int)); }

			internal override Value Eval(Value arg, IScope scope)
			{
				List<Value> list = arg.AsArray;

				int sum = 0;
				foreach (Value val in list)
					sum += val.AsInt;

				return new ValueInt(sum);
			}
		}

		[Test]
		public void TestDelimiterFunction()
		{
			ValueFunction function = new AddFunction();
			ValueDelimiter delim = new ValueDelimiter(">", DelimiterType.AsArray, function);
			List<DelimiterNode> nodes = new List<DelimiterNode>();
			nodes.Add(ToNode("3"));
			nodes.Add(ToNode("6"));
			nodes.Add(ToNode("9"));
			DelimiterList list = new DelimiterList(delim, nodes, 0, "<", "3 6 9", null);
			DelimiterNodeList nodelist = new DelimiterNodeList(list);

			IScope scope = new TestScope();
			INodeRequestor values = new TestValueNodeRequestor(nodelist);

			Value result = EvalNode.Do(nodelist, scope, values, null);
			Assert.AreEqual(18, result.AsInt);
		}
	}
}
