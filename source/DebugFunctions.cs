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
			scope.SetValue("l3.repl", new Repl());
			scope.SetValue("l3.debug.break", new Break());
			scope.SetValue("l3.debug.getTicks", new GetTicks());
			scope.SetValue("l3.throw", new Throw());
		}


		/// <summary>run a REPL</summary>
		class Repl : ValueFunctionPre
		{
			internal override Value ValueCopy() { return new Repl(); }

			internal Repl()
			{
				SetDocString("Start a new read-eval-print loop.");
				Map map = new Map();
				map["map"] = PatternData.Single("map", ValueType.Map, ValueNil.Nil);
				map["prompt"] = PatternData.Single("prompt", ValueType.String, new ValueString("loki3>"));
				Init(new ValueMap(map));
			}

			internal override Value Eval(Value arg, IScope scope)
			{
				Map map = arg.AsMap;
				Value obj = map["map"];
				string prompt = map["prompt"].AsString;

				IScope scopeToUse = scope;
				if (obj != ValueNil.Nil)
				{
					ValueMap vMap = obj as ValueMap;
					if (vMap.Scope != null)
						scopeToUse = vMap.Scope;
					else
						scopeToUse = new ScopeChain(vMap.AsMap);
				}

				loki3.core.Repl.Do(scopeToUse, prompt);
				return ValueNil.Nil;
			}
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

			internal override Value Eval(DelimiterNode prev, DelimiterNode next, IScope paramScope, IScope bodyScope, INodeRequestor nodes, ILineRequestor requestor)
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

		/// <summary>get current tick count</summary>
		class GetTicks : ValueFunction
		{
			internal override Value ValueCopy() { return new GetTicks(); }

			internal GetTicks()
			{
				SetDocString("Get the current tick count.");
				Init(null, null);
			}

			internal override Value Eval(DelimiterNode prev, DelimiterNode next, IScope paramScope, IScope bodyScope, INodeRequestor nodes, ILineRequestor requestor)
			{
				return new ValueInt(System.Environment.TickCount);
			}
		}
	}
}
