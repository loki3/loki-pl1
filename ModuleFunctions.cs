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


		/// <summary>{ :file [:scope] [:force?] } -> load the given module, optionally on a scope</summary>
		class LoadModule : ValueFunctionPre
		{
			internal override Value ValueCopy() { return new LoadModule(); }

			internal LoadModule()
			{
				SetDocString("Load a module into the current scope.  Returns true if the file was found.");

				Map map = new Map();
				map["file"] = PatternData.Single("file", ValueType.String);
				map["scope"] = PatternData.Single("scope", ValueType.String, new ValueString("current"));
				map["force?"] = PatternData.Single("force?", ValueType.Bool, ValueBool.False);
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

				// check if we should load or reload
				bool bForce = map["force?"].AsBool;
				Map loadedList = GetModuleList(runOn);
				if (!bForce && loadedList.ContainsKey(file))
					return ValueBool.False;	// module is already loaded

				EvalFile.Do(file, runOn);
				loadedList[file] = ValueBool.True;
				return ValueBool.True;
			}

			// get/create the scope metadata listing all the loaded modules
			private Map GetModuleList(IScope scope)
			{
				Map metadata = scope.AsValue.WritableMetadata;
				if (metadata.ContainsKey(ScopeChain.keyModules))
					return metadata[ScopeChain.keyModules].AsMap;
				Map loadedList = new Map();
				metadata[ScopeChain.keyModules] = new ValueMap(loadedList);
				return loadedList;
			}
		}
	}
}
