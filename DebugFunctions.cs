using System.Collections.Generic;
using loki3.core;

namespace loki3.builtin
{
	/// <summary>
	/// Built-in functions that help with debugging
	/// </summary>
	class DebugFunctions
	{
		/// <summary>
		/// Add built-in debug functions to the scope
		/// </summary>
		internal static void Register(IScope scope)
		{
			scope.SetValue("l3.debug.break", new Break());
			scope.SetValue("l3.throw", new Throw());
		}


		/// <summary>debugger breakpoint</summary>
		class Break : ValueFunction
		{
			internal override Value ValueCopy() { return new Break(); }

			internal Break()
			{
				SetDocString("Break in the debugger for the run time.");
				Init(null, null);
			}

			internal override Value Eval(DelimiterNode prev, DelimiterNode next, IScope scope, INodeRequestor nodes, ILineRequestor requestor)
			{
				System.Diagnostics.Debugger.Break();
				return ValueNil.Nil;
			}
		}

		/// <summary>throw an exception</summary>
		class Throw : ValueFunctionPre
		{
			internal override Value ValueCopy() { return new Throw(); }

			internal Throw()
			{
				SetDocString("Throw an exception.  Exceptions can be caught by a scope named l3.catch, with the exception stuffed into l3.exception.");
				Init(PatternData.Single("map", ValueType.Map));
			}

			internal override Value Eval(Value arg, IScope scope)
			{
				throw new Loki3Exception(arg.AsMap);
			}
		}
	}
}
