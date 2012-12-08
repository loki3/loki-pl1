using System.Collections.Generic;
using loki3.core;

namespace loki3.builtin
{
	/// <summary>
	/// Built-in functions that operate on arrays
	/// </summary>
	class MapFunctions
	{
		/// <summary>
		/// Add built-in array functions to the scope
		/// </summary>
		internal static void Register(IScope scope)
		{
			scope.SetValue("l3.applyMap", new ApplyMap());
			scope.SetValue("l3.filterMap", new FilterMap());
		}


		/// <summary>{ :map :function } -> apply function to every element of a map</summary>
		class ApplyMap : ValueFunctionPre
		{
			internal override Value ValueCopy() { return new ApplyMap(); }

			internal ApplyMap()
			{
				SetDocString("Apply function to every element of a map.  Functions are infix or prefix.  Returns the new map.");

				Map map = new Map();
				map["map"] = PatternData.Single("map", ValueType.Map);
				map["function"] = PatternData.Single("function", ValueType.Function);
				ValueMap vMap = new ValueMap(map);
				Init(vMap);
			}

			internal override Value Eval(Value arg, IScope scope)
			{
				Map map = arg.AsMap;
				Map inputMap = map["map"].AsMap;
				ValueFunction function = map["function"] as ValueFunction;

				Dictionary<string, Value> dict = inputMap.Raw;
				Dictionary<string, Value> newdict = new Dictionary<string, Value>();

				bool bPre = (function is ValueFunctionPre);
				foreach (string key in dict.Keys)
				{
					DelimiterNode prev = (bPre ? null : new DelimiterNodeValue(new ValueString(key)));
					DelimiterNode next = new DelimiterNodeValue(dict[key]);
					Value newval = function.Eval(prev, next, scope, null, null);
					newdict[key] = newval;
				}
				return new ValueMap(new Map(newdict));
			}
		}

		/// <summary>{ :map :function } -> { any values for which the function returns true }</summary>
		class FilterMap : ValueFunctionPre
		{
			internal override Value ValueCopy() { return new FilterMap(); }

			internal FilterMap()
			{
				SetDocString("Any values of map for which the function returns true are added to a new map.  Returns the new map.");

				Map map = new Map();
				map["map"] = PatternData.Single("map", ValueType.Map);
				map["function"] = PatternData.Single("function", ValueType.Function);
				ValueMap vMap = new ValueMap(map);
				Init(vMap);
			}

			internal override Value Eval(Value arg, IScope scope)
			{
				Map map = arg.AsMap;
				Map inputMap = map["map"].AsMap;
				ValueFunction function = map["function"] as ValueFunction;

#if true
				return null;
#else
				List<Value> newarray = new List<Value>(array.Count);
				foreach (Value val in array)
				{
					DelimiterNode node = new DelimiterNodeValue(val);
					Value check = function.Eval(null, node, scope, null, null);
					if (check.AsBool)
						newarray.Add(val);
				}
				return new ValueArray(newarray);
#endif
			}
		}
	}
}
