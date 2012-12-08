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
			scope.SetValue("l3.mapToMap", new MapToMap());
			scope.SetValue("l3.mapToArray", new MapToArray());
		}


		/// <summary>{ :map :filter? :transform } -> return a filtered and transformed map</summary>
		class MapToMap : ValueFunctionPre
		{
			internal override Value ValueCopy() { return new MapToMap(); }

			internal MapToMap()
			{
				SetDocString("Return a new map, where elements from the original map are present if filter returns true.  The values are transformed by the given function.");

				Map map = new Map();
				map["map"] = PatternData.Single("map", ValueType.Map);
				map["filter?"] = PatternData.Single("filter?", ValueType.Function, ValueNil.Nil);
				map["transform"] = PatternData.Single("transform", ValueType.Function, ValueNil.Nil);
				ValueMap vMap = new ValueMap(map);
				Init(vMap);
			}

			internal override Value Eval(Value arg, IScope scope)
			{
				Map map = arg.AsMap;
				Map inputMap = map["map"].AsMap;
				ValueFunction filter = map["filter?"] as ValueFunction;
				ValueFunction transform = map["transform"] as ValueFunction;
				if (filter == null && transform == null)
					return map["map"];

				Dictionary<string, Value> dict = inputMap.Raw;
				Dictionary<string, Value> newdict = new Dictionary<string, Value>();

				bool bPre = (filter is ValueFunctionPre || transform is ValueFunctionPre);
				foreach (string key in dict.Keys)
				{
					DelimiterNode prev = (bPre ? null : new DelimiterNodeValue(new ValueString(key)));
					DelimiterNode next = new DelimiterNodeValue(dict[key]);

					// if we should use this value...
					if (filter == null || filter.Eval(prev, next, scope, null, null).AsBool)
					{	// ...transform if appropriate
						Value newval = (transform == null ? dict[key] : transform.Eval(prev, next, scope, null, null));
						newdict[key] = newval;
					}
				}
				return new ValueMap(new Map(newdict));
			}
		}

		/// <summary>{ :map :filter? :transform } -> return a filtered and transformed array of the map's values</summary>
		class MapToArray : ValueFunctionPre
		{
			internal override Value ValueCopy() { return new MapToArray(); }

			internal MapToArray()
			{
				SetDocString("Return a new array, where values from the original map are present if filter returns true.  The values are transformed by the given function.");

				Map map = new Map();
				map["map"] = PatternData.Single("map", ValueType.Map);
				map["filter?"] = PatternData.Single("filter?", ValueType.Function, ValueNil.Nil);
				map["transform"] = PatternData.Single("transform", ValueType.Function, ValueNil.Nil);
				ValueMap vMap = new ValueMap(map);
				Init(vMap);
			}

			internal override Value Eval(Value arg, IScope scope)
			{
				Map map = arg.AsMap;
				Map inputMap = map["map"].AsMap;
				ValueFunction filter = map["filter?"] as ValueFunction;
				ValueFunction transform = map["transform"] as ValueFunction;
				if (filter == null && transform == null)
					return map["map"];

				Dictionary<string, Value> dict = inputMap.Raw;
				List<Value> newarray = new List<Value>();

				bool bPre = (filter is ValueFunctionPre || transform is ValueFunctionPre);
				foreach (string key in dict.Keys)
				{
					DelimiterNode prev = (bPre ? null : new DelimiterNodeValue(new ValueString(key)));
					DelimiterNode next = new DelimiterNodeValue(dict[key]);

					// if we should use this value...
					if (filter == null || filter.Eval(prev, next, scope, null, null).AsBool)
					{	// ...transform if appropriate
						Value newval = (transform == null ? dict[key] : transform.Eval(prev, next, scope, null, null));
						newarray.Add(newval);
					}
				}
				return new ValueArray(newarray);
			}
		}
	}
}
