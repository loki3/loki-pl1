using System;
using System.Collections.Generic;

namespace loki3.core
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
			{
				Init(pattern1, pattern2, precedence);

				m_pattern1 = pattern1;
				m_pattern2 = pattern2;
				// these restrictions will go away
				if (pattern1 != null && !(pattern1 is ValueNil))
					System.Diagnostics.Debug.Assert(pattern1.Type == ValueType.String);
				if (pattern2 != null && !(pattern2 is ValueNil))
					System.Diagnostics.Debug.Assert(pattern2.Type == ValueType.String);
				m_rawLines = rawLines;
			}

			internal override Value Eval(DelimiterNode prev, DelimiterNode next, IScope parent, INodeRequestor nodes)
			{
				// lazily parse
				EnsureParsed(parent);

				// create a new scope and add passed in arguments
				ScopeChain scope = new ScopeChain(parent);
				if (m_pattern1 != null && !m_pattern1.IsNil)
				{
					Value value1 = EvalNode.Do(prev, scope, nodes);
					scope.SetValue(m_pattern1.AsString, value1);
				}
				if (m_pattern2 != null && !m_pattern2.IsNil)
				{
					Value value2 = EvalNode.Do(next, scope, nodes);
					scope.SetValue(m_pattern2.AsString, value2);
				}

				// eval each line using current scope
				return EvalBody.Do(m_parsedLines, scope);
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
		internal static ValueFunction Do(Value pattern1, Value pattern2, List<string> rawLines, Precedence precedence)
		{
			return new UserFunction(pattern1, pattern2, rawLines, precedence);
		}
		internal static ValueFunction Do(Value pattern1, Value pattern2, List<string> rawLines)
		{
			return new UserFunction(pattern1, pattern2, rawLines, Precedence.Medium);
		}

		/// <summary>
		/// Add a new function (prefix, postfix or infix), to the scope
		/// </summary>
		/// <param name="scope">scope to add function definition to</param>
		/// <param name="name">name of the new function</param>
		/// <param name="pattern1">null, variable name, or pattern to match for previous token</param>
		/// <param name="pattern2">null, variable name, or pattern to match for next token</param>
		/// <param name="rawLines">lines to parse and run when function is invoked</param>
		/// <param name="precedence">the order in which function should be evaled</param>
		internal static void Do(IScope scope, string name,
			Value pattern1, Value pattern2, List<string> rawLines, Precedence precedence)
		{
			ValueFunction func = new UserFunction(pattern1, pattern2, rawLines, precedence);
			scope.SetValue(name, func);
		}
		internal static void Do(IScope scope, string name,
			Value pattern1, Value pattern2, List<string> rawLines)
		{
			Do(scope, name, pattern1, pattern2, rawLines, Precedence.Medium);
		}
	}
}
