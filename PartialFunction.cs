using System.Collections.Generic;

namespace loki3.core
{
	internal class PartialFunctionPre : ValueFunctionPre
	{
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
			Init(needed, nested.Precedence);
		}

		internal override Value Eval(Value arg, IScope scope)
		{
			if (arg is ValueArray)
			{
				ValueArray full = ValueArray.Combine(m_passed as ValueArray, arg as ValueArray);
				return m_nested.Eval(full, scope);
			}
			else if (arg is ValueMap)
			{
				ValueMap full = ValueMap.Combine(m_passed as ValueMap, arg as ValueMap);
				return m_nested.Eval(full, scope);
			}
			return null;
		}

		private ValueFunctionPre m_nested;
		private Value m_passed;
	}

	internal class PartialFunctionPost : ValueFunctionPost
	{
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
			Init(needed, nested.Precedence);
		}

		internal override Value Eval(Value arg, IScope scope)
		{
			if (arg is ValueArray)
			{
				ValueArray full = ValueArray.Combine(m_passed as ValueArray, arg as ValueArray);
				return m_nested.Eval(full, scope);
			}
			else if (arg is ValueMap)
			{
				ValueMap full = ValueMap.Combine(m_passed as ValueMap, arg as ValueMap);
				return m_nested.Eval(full, scope);
			}
			return null;
		}

		private ValueFunctionPost m_nested;
		private Value m_passed;
	}
}
