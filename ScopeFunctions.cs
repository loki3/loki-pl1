using System.Collections.Generic;
using loki3.core;

namespace loki3.builtin
{
	/// <summary>
	/// Built-in functions for dealing with scopes
	/// </summary>
	class ScopeFunctions
	{
		/// <summary>
		/// Add built-in Value functions to the scope
		/// </summary>
		internal static void Register(IScope scope)
		{
			scope.SetValue("l3.getScope", new GetScope());
			scope.SetValue("l3.popScope", new PopScope());
			scope.SetValue("l3.callWithScope", new CallWithScope());
		}

		/// <summary>Get the current scope</summary>
		class GetScope : ValueFunctionPre
		{
			internal override Value ValueCopy() { return new GetScope(); }

			internal GetScope()
			{
				SetDocString("Get a map representing the current scope.");

				Map map = new Map();
				Utility.AddParamsForScopeToModify(map, false/*bIncludeMap*/);
				Init(new ValueMap(map));
			}

			internal override Value Eval(Value arg, IScope scope)
			{
				Map map = arg.AsMap;
				IScope theScope = Utility.GetScopeToModify(map, scope, false/*bIncludeMap*/);
				return theScope.AsValue;
			}
		}

		/// <summary>{ :name [ :return ] } -> pop the stack back to the named scope</summary>
		class PopScope : ValueFunctionPre
		{
			internal override Value ValueCopy() { return new PopScope(); }

			internal PopScope()
			{
				SetDocString("Pop the stack back to the named scope.");

				Map map = new Map();
				map["name"] = PatternData.Single("name", ValueType.String);
				map["return"] = PatternData.Single("return", ValueNil.Nil);
				ValueMap vMap = new ValueMap(map);
				Init(vMap);
			}

			internal override Value Eval(Value arg, IScope scope)
			{
				// extract parameters
				Map map = arg.AsMap;
				string name = map["name"].AsString;
				Value returnValue = map["return"];

				throw new PopStackException(name, returnValue);
			}
		}

		/// <summary>Call a function using a given scope</summary>
		class CallWithScope : ValueFunctionPre
		{
			internal override Value ValueCopy() { return new CallWithScope(); }

			internal CallWithScope()
			{
				SetDocString("Call a function using a given scope.");

				Map map = new Map();
				map["function"] = PatternData.Single("function", ValueType.Function);
				map["map"] = PatternData.Single("map", ValueType.Map);
				map["previous"] = PatternData.Single("previous", ValueNil.Nil);
				map["next"] = PatternData.Single("next", ValueNil.Nil);
				Init(new ValueMap(map));
			}

			internal override Value Eval(Value arg, IScope scope)
			{
				Map map = arg.AsMap;
				ValueFunction function = map["function"] as ValueFunction;
				ValueMap mapOrScope = map["map"] as ValueMap;
				Value prevValue = map["previous"];
				Value nextValue = map["next"];

				// either use the scope the map represents
				// or dummy up a scope for the passed in map
				IScope where = mapOrScope.Scope;
				if (where == null)
					where = new ScopeChain(mapOrScope.AsMap);

				DelimiterNodeValue prev = (prevValue == ValueNil.Nil ? null : new DelimiterNodeValue(prevValue));
				DelimiterNodeValue next = (nextValue == ValueNil.Nil ? null : new DelimiterNodeValue(nextValue));
				Value value = function.Eval(prev, next, where, null, null);
				return value;
			}
		}
	}
}
