using System.Collections.Generic;
using loki3.core;

namespace loki3.builtin
{
	/// <summary>
	/// Built-in functions for dealing with modules,
	/// i.e. files/packages that contain code
	/// </summary>
	class Module
	{
		/// <summary>
		/// Add built-in module functions to the scope
		/// </summary>
		internal static void Register(IScope scope)
		{
			scope.SetValue("l3.loadModule", new LoadModule());
		}


		/// <summary>{ :file [:scope] } -> load the given module, optionally on a scope</summary>
		class LoadModule : ValueFunctionPre
		{
			internal LoadModule()
			{
				SetDocString("Load a module into the current scope.  Returns true if the file was found.");

				Map map = new Map();
				map["file"] = PatternData.Single("file", ValueType.String);
				map["scope"] = PatternData.Single("scope", ValueType.String, new ValueString("current"));
				ValueMap vMap = new ValueMap(map);
				Init(vMap);
			}

			internal override Value Eval(Value arg, IScope scope)
			{
				Map map = arg.AsMap;
				
				string file = map["file"].AsString;
				if (!System.IO.File.Exists(file))
					return ValueBool.False;

				// todo: turn this into an enum, at least "current" & "parent"
				bool bParentScope = (map["scope"].AsString == "parent");
				IScope runOn = (bParentScope && scope.Parent != null) ? scope.Parent : scope;
				EvalFile.Do(file, runOn);
				return ValueBool.True;
			}
		}
	}
}
