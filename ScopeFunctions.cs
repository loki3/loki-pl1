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
			scope.SetValue("l3.setScopeName", new SetScopeName());
			scope.SetValue("l3.popScope", new PopScope());
		}

		/// <summary>Get the current scope</summary>
		class GetScope : ValueFunction
		{
			internal override Value ValueCopy() { return new GetScope(); }

			internal GetScope()
			{
				SetDocString("Get a map representing the current scope.");
			}

			internal override Value Eval(DelimiterNode prev, DelimiterNode next, IScope scope, INodeRequestor nodes, ILineRequestor requestor)
			{
				return new ValueMap(scope.AsMap);
			}
		}

		/// <summary>{ :name [ :scope ] } -> assign a name to the current scope</summary>
		class SetScopeName : ValueFunctionPre
		{
			internal override Value ValueCopy() { return new SetScopeName(); }

			internal SetScopeName()
			{
				SetDocString("Assign a name to the current scope.");

				Map map = new Map();
				map["name"] = PatternData.Single("name", ValueType.String);
				map["scope"] = PatternData.Single("scope", ValueType.String, new ValueString("current"));
				ValueMap vMap = new ValueMap(map);
				Init(vMap);
			}

			internal override Value Eval(Value arg, IScope scope)
			{
				// extract parameters
				Map map = arg.AsMap;
				string name = map["name"].AsString;
				// todo: turn this into an enum, at least "current" & "parent"
				bool bParentScope = (map["scope"].AsString == "parent");

				IScope toModify = (bParentScope && scope.Parent != null) ? scope.Parent : scope;

				toModify.Name = name;
				return map["name"];
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
	}
}
