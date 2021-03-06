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
			scope.SetValue("l3.getMapKeys", new GetMapKeys());
			scope.SetValue("l3.getMapValues", new GetMapValues());
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
				map["value?"] = PatternData.Single("value?", ValueType.Bool, ValueBool.True);
				map["filter?"] = PatternData.Single("filter?", ValueType.Function, ValueNil.Nil);
				map["transform"] = PatternData.Single("transform", ValueType.Function, ValueNil.Nil);
				ValueMap vMap = new ValueMap(map);
				Init(vMap);
			}

			internal override Value Eval(Value arg, IScope scope)
			{
				Map map = arg.AsMap;
				Map inputMap = map["map"].AsMap;
				bool bUseValue = map["value?"].AsBool;
				ValueFunction filter = map["filter?"] as ValueFunction;
				ValueFunction transform = map["transform"] as ValueFunction;
				if (filter == null && transform == null)
					return map["map"];

				Dictionary<string, Value> dict = inputMap.Raw;
				Dictionary<string, Value> newdict = new Dictionary<string, Value>();
				if (dict == null)
					return new ValueMap(new Map(newdict));

				bool bPre = ((filter != null && filter.ConsumesPrevious) || (transform != null && transform.ConsumesPrevious));
				foreach (string key in dict.Keys)
				{
					DelimiterNode prev = (bPre ? new DelimiterNodeValue(new ValueString(key)) : null);
					DelimiterNode next = new DelimiterNodeValue(dict[key]);

					// if we should use this value...
					if (filter == null || filter.Eval(prev, next, scope, scope, null, null).AsBool)
					{	// ...transform if appropriate
						if (bUseValue)
						{
							Value newval = (transform == null ? dict[key] : transform.Eval(prev, next, scope, scope, null, null));
							newdict[key] = newval;
						}
						else
						{
							string newkey = (transform == null ? key : transform.Eval(prev, next, scope, scope, null, null).AsString);
							newdict[newkey] = dict[key];
						}
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
				SetDocString("Return a new array, where keys or values from the original map are present if filter returns true.  The keys or values are transformed by the given function.");

				Map map = new Map();
				map["map"] = PatternData.Single("map", ValueType.Map);
				map["value?"] = PatternData.Single("value?", ValueType.Bool, ValueBool.True);
				map["filter?"] = PatternData.Single("filter?", ValueType.Function, ValueNil.Nil);
				map["transform"] = PatternData.Single("transform", ValueType.Function, ValueNil.Nil);
				ValueMap vMap = new ValueMap(map);
				Init(vMap);
			}

			internal override Value Eval(Value arg, IScope scope)
			{
				Map map = arg.AsMap;
				Map inputMap = map["map"].AsMap;
				bool bKeepValue = map["value?"].AsBool;
				ValueFunction filter = map["filter?"] as ValueFunction;
				ValueFunction transform = map["transform"] as ValueFunction;
				if (filter == null && transform == null)
					return map["map"];

				Dictionary<string, Value> dict = inputMap.Raw;
				List<Value> newarray = new List<Value>();

				if (dict != null)
				{
					bool bPre = (filter is ValueFunctionPre || transform is ValueFunctionPre);
					foreach (string key in dict.Keys)
					{
						DelimiterNode prev = (bPre ? null : new DelimiterNodeValue(new ValueString(key)));
						DelimiterNode toFilter = new DelimiterNodeValue(dict[key]);

						// if we should use this value...
						if (filter == null || filter.Eval(prev, toFilter, scope, scope, null, null).AsBool)
						{	// get value or key & possibly transform
							Value newval = (bKeepValue ? dict[key] : new ValueString(key));
							if (transform != null)
							{
								DelimiterNode toTransform = new DelimiterNodeValue(newval);
								newval = transform.Eval(prev, toTransform, scope, scope, null, null);
							}
							newarray.Add(newval);
						}
					}
				}
				return new ValueArray(newarray);
			}
		}

		/// <summary>get an array of map keys</summary>
		class GetMapKeys : ValueFunctionPre
		{
			internal override Value ValueCopy() { return new GetMapKeys(); }

			internal GetMapKeys()
			{
				SetDocString("Return an array of all the map's keys.");
				Init(PatternData.Single("map", ValueType.Map));
			}

			internal override Value Eval(Value arg, IScope scope)
			{
				Map map = arg.AsMap;
				List<Value> array = new List<Value>();
				Dictionary<string, Value>.KeyCollection keys = map.Raw.Keys;
				foreach (string key in keys)
					array.Add(new ValueString(key));
				return new ValueArray(array);
			}
		}

		/// <summary>get an array of map values</summary>
		class GetMapValues : ValueFunctionPre
		{
			internal override Value ValueCopy() { return new GetMapValues(); }

			internal GetMapValues()
			{
				SetDocString("Return an array of all the map's values.");
				Init(PatternData.Single("map", ValueType.Map));
			}

			internal override Value Eval(Value arg, IScope scope)
			{
				Map map = arg.AsMap;
				List<Value> array = new List<Value>();
				Dictionary<string, Value>.KeyCollection keys = map.Raw.Keys;
				foreach (string key in keys)
					array.Add(map[key]);
				return new ValueArray(array);
			}
		}
	}
}
