using System;
using System.Collections.Generic;

namespace loki3
{
	/// <summary>
	/// Create a new function
	/// </summary>
	internal class CreateFunction
	{

		/// <summary>
		/// Handles a user defined function
		/// </summary>
		internal class UserFunction : ValueFunction
		{
			internal UserFunction(Value pattern1, Value pattern2, List<string> rawLines, Precedence precedence)
				: base(pattern1 != null, pattern2 != null, precedence)
			{
				m_pattern1 = pattern1;
				m_pattern2 = pattern2;
				// these restrictions will go away
				if (pattern1 != null)
					System.Diagnostics.Debug.Assert(pattern1.Type == ValueType.String);
				if (pattern2 != null)
					System.Diagnostics.Debug.Assert(pattern2.Type == ValueType.String);
				m_rawLines = rawLines;
			}

			internal override Value Eval(DelimiterNode prev, DelimiterNode next, IStack parent, INodeRequestor nodes)
			{
				// lazily parse
				EnsureParsed(parent);

				// create a new scope and add passed in arguments
				StateStack stack = new StateStack(parent);
				if (m_pattern1 != null)
				{
					Value value1 = EvalNode.Do(prev, stack, nodes);
					stack.SetValue(m_pattern1.AsString, value1);
				}
				if (m_pattern2 != null)
				{
					Value value2 = EvalNode.Do(next, stack, nodes);
					stack.SetValue(m_pattern2.AsString, value2);
				}

				// eval each line using current scope
				Value retval = null;
				foreach (DelimiterList line in m_parsedLines)
					retval = EvalList.Do(line.Nodes, stack);
				return (retval == null ? new ValueNil() : retval);
			}

			/// <summary>If we haven't already parsed our lines, do so now</summary>
			private void EnsureParsed(IParseLineDelimiters delims)
			{
				if (m_rawLines == null)
					return;
				m_parsedLines = new List<DelimiterList>(m_rawLines.Count);
				foreach (string s in m_rawLines)
				{
					DelimiterList line = ParseLine.Do(s, delims);
					m_parsedLines.Add(line);
				}
				m_rawLines = null;
			}

			private Value m_pattern1;
			private Value m_pattern2;
			private List<string> m_rawLines;
			private List<DelimiterList> m_parsedLines = null;
		}

		/// <summary>
		/// Create a new function, prefix, postfix or infix
		/// </summary>
		/// <param name="pattern1">null, variable name, or pattern to match for previous token</param>
		/// <param name="pattern2">null, variable name, or pattern to match for next token</param>
		/// <param name="rawLines">lines to parse and run when function is invoked</param>
		/// <param name="precedence">the order in which function should be evaled</param>
		internal static void Do(Value pattern1, Value pattern2, List<string> rawLines, Precedence precedence)
		{
			new UserFunction(pattern1, pattern2, rawLines, precedence);
		}

		/// <summary>
		/// Add a new function (prefix, postfix or infix), to the stack
		/// </summary>
		/// <param name="stack">stack to add function definition to</param>
		/// <param name="name">name of the new function</param>
		/// <param name="pattern1">null, variable name, or pattern to match for previous token</param>
		/// <param name="pattern2">null, variable name, or pattern to match for next token</param>
		/// <param name="rawLines">lines to parse and run when function is invoked</param>
		/// <param name="precedence">the order in which function should be evaled</param>
		internal static void Do(IStack stack, string name,
			Value pattern1, Value pattern2, List<string> rawLines, Precedence precedence)
		{
			ValueFunction func = new UserFunction(pattern1, pattern2, rawLines, precedence);
			stack.SetValue(name, func);
		}
	}
}