using System.Collections.Generic;
using loki3.core;

namespace loki3.builtin
{
	/// <summary>
	/// Built-in functions that operate on arrays and maps
	/// </summary>
	class ArrayFunctions
	{
		/// <summary>
		/// Add built-in array functions to the scope
		/// </summary>
		internal static void Register(IScope scope)
		{
			scope.SetValue("l3.combine", new Combine());
			scope.SetValue("l3.addToArray", new AddToArray());
			scope.SetValue("l3.arrayToArray", new ArrayToArray());
			scope.SetValue("l3.foldLeft", new FoldLeft());
			scope.SetValue("l3.foldRight", new FoldRight());
		}


		/// <summary>[a1] [a2] -> [a1 a2]</summary>
		class Combine : ValueFunctionPre
		{
			internal override Value ValueCopy() { return new Combine(); }

			internal Combine()
			{
				SetDocString("Concatenates two arrays or maps.");
				List<Value> list = new List<Value>();
				list.Add(PatternData.Single("a"));
				list.Add(PatternData.Single("b"));
				ValueArray array = new ValueArray(list);
				Init(array);
			}

			internal override Value Eval(Value arg, IScope scope)
			{
				List<Value> list = arg.AsArray;
				Value v1 = list[0];
				Value v2 = list[1];
				return Utility.Combine(v1, v2);
			}
		}
		
		/// <summary>{ :array :value } -> add value to array</summary>
		class AddToArray : ValueFunctionPre
		{
			internal override Value ValueCopy() { return new AddToArray(); }

			internal AddToArray()
			{
				SetDocString("Add value to an array");

				Map map = new Map();
				map["array"] = PatternData.Single("array", ValueType.Array);
				map["value"] = PatternData.Single("value");
				ValueMap vMap = new ValueMap(map);
				Init(vMap);
			}

			internal override Value Eval(Value arg, IScope scope)
			{
				Map map = arg.AsMap;
				Value array = map["array"];
				Value value = map["value"];

				array.AsArray.Add(value);
				return array;
			}
		}

		/// <summary>{ :array :filter? :transform } -> return a filtered and transformed array</summary>
		class ArrayToArray : ValueFunctionPre
		{
			internal override Value ValueCopy() { return new ArrayToArray(); }

			internal ArrayToArray()
			{
				SetDocString("Return a new array, where elements from the original array are present if filter returns true.  The values are transformed by the given function.");

				Map map = new Map();
				map["array"] = PatternData.Single("array", ValueType.Array);
				map["filter?"] = PatternData.Single("filter?", ValueType.Function, ValueNil.Nil);
				map["transform"] = PatternData.Single("transform", ValueType.Function, ValueNil.Nil);
				ValueMap vMap = new ValueMap(map);
				Init(vMap);
			}

			internal override Value Eval(Value arg, IScope scope)
			{
				Map map = arg.AsMap;
				List<Value> array = map["array"].AsArray;
				ValueFunction filter = map["filter?"] as ValueFunction;
				ValueFunction transform = map["transform"] as ValueFunction;
				if (filter == null && transform == null)
					return map["array"];

				List<Value> newarray = new List<Value>(array.Count);
				bool bPre = ((filter != null && filter.ConsumesPrevious) || (transform != null && transform.ConsumesPrevious));
				int i = 0;
				foreach (Value val in array)
				{
					DelimiterNode prev = (bPre ? new DelimiterNodeValue(new ValueInt(i)) : null);
					DelimiterNode node = new DelimiterNodeValue(val);

					// if we should use this value...
					if (filter == null || filter.Eval(prev, node, scope, scope, null, null).AsBool)
					{	// ...transform if appropriate
						Value newval = (transform == null ? val : transform.Eval(prev, node, scope, scope, null, null));
						newarray.Add(newval);
					}
					i++;
				}
				return new ValueArray(newarray);
			}
		}

		/// <summary>{ :array :function } -> turn array into a single value by applying function to members as pairs</summary>
		class FoldLeft : ValueFunctionPre
		{
			internal override Value ValueCopy() { return new FoldLeft(); }

			internal FoldLeft()
			{
				SetDocString("Apply infix function to first and second item, then result and third item, etc.  Returns a single value.");

				Map map = new Map();
				map["array"] = PatternData.Single("array", ValueType.Array);
				map["function"] = PatternData.Single("function", ValueType.Function);
				ValueMap vMap = new ValueMap(map);
				Init(vMap);
			}

			internal override Value Eval(Value arg, IScope scope)
			{
				Map map = arg.AsMap;
				List<Value> array = map["array"].AsArray;
				ValueFunction function = map["function"] as ValueFunction;

				bool bFirst = true;
				Value last = null;
				foreach (Value val in array)
				{
					if (bFirst)
					{
						last = val;
						bFirst = false;
					}
					else
					{
						DelimiterNode node1 = new DelimiterNodeValue(last);
						DelimiterNode node2 = new DelimiterNodeValue(val);
						last = function.Eval(node1, node2, scope, scope, null, null);
					}
				}
				return last;
			}
		}

		/// <summary>{ :array :function } -> turn array into a single value by applying function to members as pairs</summary>
		class FoldRight : ValueFunctionPre
		{
			internal override Value ValueCopy() { return new FoldRight(); }

			internal FoldRight()
			{
				SetDocString("Apply infix function to last and second to last item, then result and third to last, etc.  Returns a single value.");

				Map map = new Map();
				map["array"] = PatternData.Single("array", ValueType.Array);
				map["function"] = PatternData.Single("function", ValueType.Function);
				ValueMap vMap = new ValueMap(map);
				Init(vMap);
			}

			internal override Value Eval(Value arg, IScope scope)
			{
				Map map = arg.AsMap;
				List<Value> array = map["array"].AsArray;
				ValueFunction function = map["function"] as ValueFunction;

				Value last = array[array.Count - 1];
				for (int i = array.Count - 2; i >= 0; i--)
				{
					Value val = array[i];
					DelimiterNode node1 = new DelimiterNodeValue(last);
					DelimiterNode node2 = new DelimiterNodeValue(val);
					last = function.Eval(node2, node1, scope, scope, null, null);
				}
				return last;
			}
		}
	}
}
