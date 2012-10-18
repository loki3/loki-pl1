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
			/// <summary>
			/// Create a new user defined function
			/// </summary>
			/// <param name="pattern1">pattern for parameters that come before function name</param>
			/// <param name="pattern2">pattern for parameters that come after function name</param>
			/// <param name="rawLines">body of function that will be parsed if needed</param>
			/// <param name="precedence">order that function should be evaled relative to other functions</param>
			internal UserFunction(Value pattern1, Value pattern2, List<Value> rawLines, Order precedence)
			{
				Init(pattern1, pattern2, precedence);

				m_usePrevious = (pattern1 != null && !pattern1.IsNil);
				m_useNext = (pattern2 != null && !pattern2.IsNil);
				m_pattern1 = pattern1;
				m_pattern2 = pattern2;
				m_rawLines = rawLines;

				m_passed = null;
				m_fullPattern = null;
			}

			/// <summary>
			/// Create a partial function based on another user defined function
			/// </summary>
			/// <param name="func">function to build off of</param>
			/// <param name="passed">values that were passed to function already</param>
			/// <param name="pattern1">values that still need to be passed if it uses previous</param>
			/// <param name="pattern2">values that still need to be passed if it uses next</param>
			internal UserFunction(UserFunction func, Value passed, Value pattern1, Value pattern2)
			{
				m_usePrevious = (pattern1 != null && !pattern1.IsNil);
				m_useNext = (pattern2 != null && !pattern2.IsNil);
				m_pattern1 = pattern1;
				m_pattern2 = pattern2;
				m_rawLines = func.m_rawLines;
				m_parsedLines = func.m_parsedLines;
				Init(pattern1, pattern2, func.Precedence);

				m_passed = passed;
				m_fullPattern = (m_usePrevious ? func.Metadata[keyPreviousPattern] : func.Metadata[keyNextPattern]);
			}

			internal override Value Eval(DelimiterNode prev, DelimiterNode next, IScope parent, INodeRequestor nodes, ILineRequestor requestor)
			{
				// create a new scope and add passed in arguments
				ScopeChain scope = new ScopeChain(parent);
				// note: second copy is so that evaling previous doesn't impact scope used to eval next
				ScopeChain scope2 = new ScopeChain(parent);
				if (m_fullPattern != null && m_passed != null)
				{
					Utility.AddToScope(m_fullPattern, m_passed, scope);
					Utility.AddToScope(m_fullPattern, m_passed, scope2);
				}

				if (m_usePrevious)
				{
					Value value1 = EvalNode.Do(prev, scope, nodes, requestor);
					Value match, leftover;
					if (!PatternChecker.Do(value1, Metadata[keyPreviousPattern], out match, out leftover))
						throw new Loki3Exception().AddWrongPattern(Metadata[keyPreviousPattern], value1);

					if (leftover != null)
					{
						if (m_useNext)
							// currently can't do partials for infix
							throw new Loki3Exception().AddWrongPattern(Metadata[keyPreviousPattern], value1);
						// create a partial function that starts w/ match & still needs leftover
						return new UserFunction(this, match, leftover, null);
					}

					Utility.AddToScope(m_pattern1, match, scope);
				}
				if (m_useNext)
				{
					Value value2 = EvalNode.Do(next, scope2, nodes, requestor);
					Value match, leftover;
					if (!PatternChecker.Do(value2, Metadata[keyNextPattern], out match, out leftover))
						throw new Loki3Exception().AddWrongPattern(Metadata[keyNextPattern], value2);

					// if we created a function that needs a body, add it if present
					ValueFunction matchFunc = match as ValueFunction;
					if (matchFunc != null && matchFunc.RequiresBody() && requestor != null)
						match = EvalList.DoAddBody(matchFunc, scope, requestor);

					if (leftover != null)
					{
						if (m_usePrevious)
							// currently can't do partials for infix
							throw new Loki3Exception().AddWrongPattern(Metadata[keyNextPattern], value2);
						// create a partial function that starts w/ match & still needs leftover
						return new UserFunction(this, match, null, leftover);
					}

					Utility.AddToScope(m_pattern2, match, scope);
				}

				// tack on body if requested
				if (Metadata.GetOptionalT<bool>("body?", false))
				{
					List<Value> body = EvalList.DoGetBody(scope2, requestor);
					scope.SetValue("body", new ValueArray(body));
				}

				// lazily parse
				EnsureParsed(parent);

				// eval each line using current scope
				return EvalBody.Do(m_parsedLines, scope);
			}

			/// <summary>If we haven't already parsed our lines, do so now</summary>
			private void EnsureParsed(IParseLineDelimiters delims)
			{
				if (m_rawLines == null)
					return;
				m_parsedLines = new List<DelimiterList>(m_rawLines.Count);
				foreach (Value v in m_rawLines)
				{
					DelimiterList line = null;
					if (v is ValueString)
						line = ParseLine.Do(v.AsString, delims);
					else if (v is ValueRaw)
						line = (v as ValueRaw).GetValue();

					if (line != null)
						m_parsedLines.Add(line);
				}
				m_rawLines = null;
			}

			private bool m_usePrevious;
			private bool m_useNext;
			private Value m_pattern1;
			private Value m_pattern2;
			private List<Value> m_rawLines;
			private List<DelimiterList> m_parsedLines = null;

			private Value m_passed;
			private Value m_fullPattern;
		}

		/// <summary>
		/// Create a new function, prefix, postfix or infix
		/// </summary>
		/// <param name="pattern1">null, variable name, or pattern to match for previous token</param>
		/// <param name="pattern2">null, variable name, or pattern to match for next token</param>
		/// <param name="rawLines">lines to parse (if string, not ValueRaw) and run when function is invoked</param>
		/// <param name="precedence">the order in which function should be evaled</param>
		internal static ValueFunction Do(Value pattern1, Value pattern2, List<Value> rawLines, Order precedence)
		{
			return new UserFunction(pattern1, pattern2, rawLines, precedence);
		}
		internal static ValueFunction Do(Value pattern1, Value pattern2, List<Value> rawLines)
		{
			return new UserFunction(pattern1, pattern2, rawLines, Order.Medium);
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
			Value pattern1, Value pattern2, List<Value> rawLines, Order precedence)
		{
			ValueFunction func = new UserFunction(pattern1, pattern2, rawLines, precedence);
			scope.SetValue(name, func);
		}
		internal static void Do(IScope scope, string name,
			Value pattern1, Value pattern2, List<Value> rawLines)
		{
			Do(scope, name, pattern1, pattern2, rawLines, Order.Medium);
		}

		internal static void Do(IScope scope, string name,
			Value pattern1, Value pattern2, List<string> rawLines)
		{
			List<Value> lines = new List<Value>();
			foreach (string s in rawLines)
				lines.Add(new ValueString(s));
			Do(scope, name, pattern1, pattern2, lines, Order.Medium);
		}
	}
}
