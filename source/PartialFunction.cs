using System.Collections.Generic;

namespace loki3.core
{
	internal class PartialFunctionPre : ValueFunctionPre
	{
		internal override Value ValueCopy() { return new PartialFunctionPre(m_nested, m_passed); }

		/// <summary>
		/// Create a function that waits for the missing parameters passed to
		/// another function, then bundles them all up and calls the first function
		/// </summary>
		/// <param name="nested">function to call once we get remaining parameters</param>
		/// <param name="passed">arguments that have already been passed</param>
		/// <param name="needed">parameters we still need</param>
		internal PartialFunctionPre(ValueFunctionPre nested, Value passed, Value needed)
		{
			m_nested = nested;
			m_passed = passed;
			// our pattern is the remaining parameters; use same precedence as nested func
			Init(needed, nested.Order);
		}

		internal PartialFunctionPre(ValueFunctionPre nested, Value passed)
		{
			m_nested = nested;
			m_passed = passed;
		}

		internal override Value Eval(Value arg, IScope scope)
		{
			Value full = Utility.Combine(m_passed, arg);
			return (full == null ? null : m_nested.Eval(full, scope));
		}

		private ValueFunctionPre m_nested;
		private Value m_passed;
	}

	internal class PartialFunctionPost : ValueFunctionPost
	{
		internal override Value ValueCopy() { return new PartialFunctionPost(m_nested, m_passed); }

		/// <summary>
		/// Create a function that waits for the missing parameters passed to
		/// another function, then bundles them all up and calls the first function
		/// </summary>
		/// <param name="nested">function to call once we get remaining parameters</param>
		/// <param name="passed">arguments that have already been passed</param>
		/// <param name="needed">parameters we still need</param>
		internal PartialFunctionPost(ValueFunctionPost nested, Value passed, Value needed)
		{
			m_nested = nested;
			m_passed = passed;
			// our pattern is the remaining parameters; use same precedence as nested func
			Init(needed, nested.Order);
		}

		internal PartialFunctionPost(ValueFunctionPost nested, Value passed)
		{
			m_nested = nested;
			m_passed = passed;
		}

		internal override Value Eval(Value arg, IScope scope)
		{
			Value full = Utility.Combine(m_passed, arg);
			return (full == null ? null : m_nested.Eval(full, scope));
		}

		private ValueFunctionPost m_nested;
		private Value m_passed;
	}

	/// <summary>
	/// An infix function that still needs either its pre or post parameter
	/// </summary>
	internal class PartialFunctionIn : ValueFunction
	{
		internal override Value ValueCopy() { return new PartialFunctionIn(m_nested, m_prev, m_next); }

		/// <summary>
		/// Create a function that waits for the missing pre or post parameter,
		/// then bundles them all up and calls the first function
		/// </summary>
		/// <param name="nested">function to call once we get remaining parameters</param>
		/// <param name="passed">arguments that have already been passed</param>
		internal PartialFunctionIn(ValueFunction nested, DelimiterNode prev, DelimiterNode next)
		{
			m_nested = nested;
			m_prev = prev;
			m_next = next;

			// our pattern is the missing parameter; use same precedence as nested func
			Map meta = nested.Metadata;
			if (m_prev != null)
				Init(null, meta[keyNextPattern], nested.Order);
			else
				Init(meta[keyPreviousPattern], null, nested.Order);
		}

		internal override Value Eval(DelimiterNode prev, DelimiterNode next, IScope paramScope, IScope bodyScope, INodeRequestor nodes, ILineRequestor requestor)
		{
			if (m_prev != null)
				return m_nested.Eval(m_prev, next, paramScope, bodyScope, nodes, requestor);
			return m_nested.Eval(prev, m_next, paramScope, bodyScope, nodes, requestor);
		}

		private ValueFunction m_nested;
		private DelimiterNode m_prev;
		private DelimiterNode m_next;
	}
}
