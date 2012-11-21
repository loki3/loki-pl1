using System.Collections.Generic;
using loki3.core;

namespace loki3.builtin
{
	/// <summary>
	/// Built-in functions that operate on arrays
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
			scope.SetValue("l3.apply", new Apply());
			scope.SetValue("l3.foldLeft", new FoldLeft());
			scope.SetValue("l3.foldRight", new FoldRight());
			scope.SetValue("l3.filter", new Filter());
		}


		/// <summary>[a1] [a2] -> [a1 a2]</summary>
		class Combine : ValueFunctionPre
		{
			internal Combine()
			{
				SetDocString("Concatenates two arrays.");
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

		/// <summary>{ :array :function } -> apply function to every element of array</summary>
		class Apply : ValueFunctionPre
		{
			internal Apply()
			{
				SetDocString("Apply function to every element of array.  Function takes a single value.  Returns the new array.");

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
				ValueFunctionPre function = map["function"] as ValueFunctionPre;

				List<Value> newarray = new List<Value>(array.Count);
				foreach (Value val in array)
				{
					Value newval = function.Eval(val, scope);
					newarray.Add(newval);
				}
				return new ValueArray(newarray);
			}
		}

		/// <summary>{ :array :function } -> turn array into a single value by applying function to members as pairs</summary>
		class FoldLeft : ValueFunctionPre
		{
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
						last = function.Eval(node1, node2, scope, null, null);
					}
				}
				return last;
			}
		}

		/// <summary>{ :array :function } -> turn array into a single value by applying function to members as pairs</summary>
		class FoldRight : ValueFunctionPre
		{
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
					last = function.Eval(node1, node2, scope, null, null);
				}
				return last;
			}
		}

		/// <summary>{ :array :function } -> apply function to every element of array</summary>
		class Filter : ValueFunctionPre
		{
			internal Filter()
			{
				SetDocString("Any members of array for which the function returns true are added to a new array.  Returns the new array.");

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
				ValueFunctionPre function = map["function"] as ValueFunctionPre;

				List<Value> newarray = new List<Value>();
				foreach (Value val in array)
				{
					Value check = function.Eval(val, scope);
					if (check.AsBool)
						newarray.Add(val);
				}
				return new ValueArray(newarray);
			}
		}
	}
}
