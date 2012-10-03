using System.Collections.Generic;
using NUnit.Framework;

namespace loki3.core.test
{
	[TestFixture]
	class TEST_EvalLines
	{
		/// <summary>provide interface for EvalLines consuming an array of strings</summary>
		internal class HandOffLines : ILineRequestor
		{
			internal HandOffLines(string[] lines) { m_lines = lines; }

			#region ILineRequestor Members
			public string GetNextLine()
			{
				return (m_current >= m_lines.Length ? null : m_lines[m_current++]);
			}


			#endregion

			private string[] m_lines;
			private int m_current = 0;
		}

		/// <summary>[a1 a2] -> a1 - a2</summary>
		class SubtractArray : ValueFunctionPre
		{
			internal SubtractArray()
			{
				List<Value> list = new List<Value>();
				list.Add(PatternData.Single("a", ValueType.Int));
				list.Add(PatternData.Single("b", ValueType.Int));
				ValueArray array = new ValueArray(list);
				Init(array);
			}

			internal override Value Eval(Value arg, IScope scope)
			{
				List<Value> list = arg.AsArray;
				return new ValueInt(list[0].AsInt - list[1].AsInt);
			}
		}

		/// <summary>adds up everything in the body</summary>
		class AddBody : ValueFunctionPre
		{
			internal AddBody()
			{
				Init(PatternData.Body());
			}

			internal override Value Eval(Value arg, IScope scope)
			{
				List<Value> body = ValueFunction.GetBody(arg);
				int total = 0;
				foreach (Value line in body)
				{
					DelimiterList list = ParseLine.Do(line.AsString, scope);
					Value value = EvalList.Do(list.Nodes, scope);
					total += value.AsInt;
				}
				return new ValueInt(total);
			}
		}

		static IScope CreateScope()
		{
			ScopeChain scope = new ScopeChain(null);
			scope.SetValue("subtract", new SubtractArray());
			scope.SetValue("add", new AddBody());

			ValueDelimiter square = new ValueDelimiter("[", "]", DelimiterType.AsArray);
			scope.SetValue("[", square);

			return scope;
		}

		[Test]
		public void TestSingleLine()
		{
			{	// simple int
				string[] strings = { "5" };
				HandOffLines lines = new HandOffLines(strings);
				IScope scope = CreateScope();
				Value value = EvalLines.Do(lines, scope);
				Assert.AreEqual(5, value.AsInt);
			}

			{	// function
				string[] strings = { "subtract [ 7 4 ]" };
				HandOffLines lines = new HandOffLines(strings);
				IScope scope = CreateScope();
				Value value = EvalLines.Do(lines, scope);
				Assert.AreEqual(3, value.AsInt);
			}

#if false
			{	// functions
				string[] strings = { "subtract [ 7 subtract [ 15 11 ] ]" };
				HandOffLines lines = new HandOffLines(strings);
				IScope scope = CreateScope();
				Value value = EvalLines.Do(lines, scope);
				Assert.AreEqual(3, value.AsInt);
			}
#endif
		}


		[Test]
		public void TestBody()
		{
			{
				string[] strings = { "add", " 5", " 4" };
				HandOffLines lines = new HandOffLines(strings);
				IScope scope = CreateScope();
				Value value = EvalLines.Do(lines, scope);
				Assert.AreEqual(9, value.AsInt);
			}
		}
	}
}
