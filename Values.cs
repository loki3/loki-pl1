using System.Collections.Generic;
using loki3.core;

namespace loki3.builtin
{
	/// <summary>
	/// Built-in functions for dealing with Value objects
	/// </summary>
	class Values
	{
		/// <summary>
		/// Add built-in Value functions to the scope
		/// </summary>
		internal static void Register(IScope scope)
		{
			scope.SetValue("l3.setValue", new SetValue());
			scope.SetValue("l3.getValue", new GetValue());
			scope.SetValue("l3.copy", new Copy());
			scope.SetValue("l3.createMap", new CreateMap());
			scope.SetValue("l3.createRange", new CreateRange());
			scope.SetValue("l3.createFunction", new CreateFunction());
			scope.SetValue("l3.createDelimiter", new CreateDelimiter());
			scope.SetValue("l3.createEvalDelimiter", new CreateEvalDelimiter());
			scope.SetValue("l3.createArrayDelimiter", new CreateArrayDelimiter());
			scope.SetValue("l3.getCount", new GetCount());
			scope.SetValue("l3.getMetadata", new GetMetadata());
			scope.SetValue("l3.getType", new GetTypeFunction());
			scope.SetValue("l3.getFunctionBody", new GetFunctionBody());
			scope.SetValue("l3.eval", new EvalValue());
			scope.SetValue("l3.bindFunction", new BindFunction());
		}


		/// <summary>{ :key :value [:create?] [:map] [:scope] [:overload?] [:onlyInit?] [:returnSuccess?] } -> value  and stores the key value pair on the specified map or scope</summary>
		class SetValue : ValueFunctionPre
		{
			internal override Value ValueCopy() { return new SetValue(); }

			internal SetValue()
			{
				SetDocString("Set the value on given map key.\nEither creates new variable on active scope or stores value on existing variable.\nCan be stored on a map, current or parent scope.");

				Map map = new Map();
				map["key"] = PatternData.Single("key");
				map["value"] = PatternData.Single("value");
				map["create?"] = PatternData.Single("create?", ValueType.Bool, ValueBool.True);
				// used if value is a function - overload? then add it to functions on this key, else make it the only function for this key
				map["overload?"] = PatternData.Single("overload?", ValueType.Bool, ValueBool.False);
				// if initOnly? && (key already exists), then don't change the value
				map["initOnly?"] = PatternData.Single("initOnly?", ValueType.Bool, ValueBool.False);
				// if returnSuccess?, return value tells whether assignment succeeded, else returns value
				map["returnSuccess?"] = PatternData.Single("returnSuccess?", ValueType.Bool, ValueBool.False);
				Utility.AddParamsForScopeToModify(map, true/*bIncludeMap*/);
				Init(new ValueMap(map));
			}

			internal override Value Eval(Value arg, IScope scope)
			{
				Map map = arg.AsMap;
				bool bInitOnly = map["initOnly?"].AsBool;
				bool bReturnSuccess = map["returnSuccess?"].AsBool;

				PatternAssign assign = new PatternAssign(map, scope);
				Value value = map["value"];
				if (bReturnSuccess)
				{	// if we want to know about success, we ignore exceptions,
					// since they're probably not interested
					try
					{
						bool bSuccess = assign.Assign(value, bInitOnly);
						return bSuccess ? ValueBool.True : ValueBool.False;
					}
					catch (Loki3Exception)
					{
						return ValueBool.False;
					}
				}
				else
				{
					if (assign.Assign(value, bInitOnly))
						return value;
					return ValueNil.Nil;
				}
			}
		}

		/// <summary>{ :object :key [:default] } -> value,  either map[key.AsString] or array[key.AsInt]</summary>
		class GetValue : ValueFunctionPre
		{
			internal override Value ValueCopy() { return new GetValue(); }

			internal GetValue()
			{
				SetDocString("On a map, gets the value attached to a key.\nOn an array, gets the indexed item.");

				Map map = new Map();
				// todo: better pattern definition, map|array & string|int
				// or make this into two functions
				map["object"] = PatternData.Single("object", ValueNil.Nil);
				map["key"] = PatternData.Single("key");
				map["default"] = PatternData.Single("default", ValueNil.Nil);
				ValueMap vMap = new ValueMap(map);
				Init(vMap);
			}

			internal override Value Eval(Value arg, IScope scope)
			{
				Map map = arg.AsMap;

				// extract parameters
				Value obj = map["object"];
				Value key = map["key"];

				if (obj.IsNil)
					return scope.GetValue(new Token(key.AsString));

				ValueMap objMap = obj as ValueMap;
				if (objMap != null)
				{
					string keystr = key.AsString;
					Value result = null;
					if (!objMap.AsMap.TryGetValue(keystr, out result))
						// if key isn't present, return map's default value
						return GetDefault(obj, key, map);
					return result;
				}

				ValueArray objArr = obj as ValueArray;
				if (objArr != null)
				{
					List<Value> array = objArr.AsArray;
					int i = key.AsInt;
					if (i < 0 || i >= array.Count)
						// if index is out of bounds, return array's default value
						return GetDefault(obj, key, map);
					return objArr.AsArray[i];
				}

				ValueFunctionOverload overload = obj as ValueFunctionOverload;
				if (overload != null)
				{
					int i = key.AsInt;
					if (i < 0 || i >= overload.Count)
						// if index is out of bounds, return default value
						return GetDefault(obj, key, map);
					return overload.GetFunction(i);
				}
				ValueFunction function = obj as ValueFunction;
				if (function != null)
				{
					int i = key.AsInt;
					if (i != 0)
						return ValueNil.Nil;
					return function;
				}

				// todo: better error
				throw new Loki3Exception().AddWrongType(ValueType.Map, obj.Type);
			}

			private Value GetDefault(Value value, Value key, Map findDefault)
			{
				Map meta = value.Metadata;
				Value result = null;
				if (meta == null || !meta.TryGetValue(PatternData.keyDefault, out result))
				{	// object doesn't have a default, use default passed as a parameter
					return findDefault["default"];
				}
				return result;
			}
		}

		/// <summary>{ [ :key ] [ :value ] } -> new value</summary>
		class Copy : ValueFunctionPre
		{
			internal override Value ValueCopy() { return new Copy(); }

			internal Copy()
			{
				SetDocString("Create a copy of a value.");

				Map map = new Map();
				map["key"] = PatternData.Single("key", ValueType.String, ValueNil.Nil);
				map["value"] = PatternData.Single("value", ValueNil.Nil);
				Init(new ValueMap(map));
			}

			internal override Value Eval(Value arg, IScope scope)
			{
				Map map = arg.AsMap;
				Value value = map["value"];
				if (value.IsNil)
				{
					Value key = map["key"];
					if (!key.IsNil)
						value = scope.GetValue(new Token(key.AsString));
				}

				if (value != null)
					return value.Copy();
				return ValueNil.Nil;
			}
		}

		/// <summary>[k1 v1 k2 v2 ...] -> map</summary>
		class CreateMap : ValueFunctionPre
		{
			internal override Value ValueCopy() { return new CreateMap(); }

			internal CreateMap()
			{
				SetDocString("Create a map from an array, where the elements alternate between the key and the associated value.");

				Init(PatternData.ArrayEnd("a"));
			}

			internal override Value Eval(Value arg, IScope scope)
			{
				List<Value> list = arg.AsArray;

				Map map = new Map();
				int count = list.Count;
				for (int i = 0; i < count; i += 2)
				{
					string key = list[i].AsString;
					Value value = list[i+1];
					map[key] = value;
				}
				return new ValueMap(map);
			}
		}

		/// <summary>{ [->start : number] ->end : number [->step : number] } -> array</summary>
		class CreateRange : ValueFunctionPre
		{
			internal override Value ValueCopy() { return new CreateRange(); }

			internal CreateRange()
			{
				SetDocString("Create an array using a given range");

				Map map = new Map();
				map["start"] = PatternData.Single("start", ValueType.Number, new ValueInt(1));
				map["end"] = PatternData.Single("end", ValueType.Number);
				map["step"] = PatternData.Single("step", ValueType.Number, new ValueInt(1));
				ValueMap vMap = new ValueMap(map);
				Init(vMap);
			}

			internal override Value Eval(Value arg, IScope scope)
			{
				Map map = arg.AsMap;

				Value vStart = map["start"];
				Value vEnd = map["end"];
				Value vStep = map["step"];

				List<Value> list = new List<Value>();
				if (vStart is ValueInt && vEnd is ValueInt && vStep is ValueInt)
				{	// all params are ints - output is ints
					int start = vStart.AsInt;
					int end = vEnd.AsInt;
					int step = vStep.AsInt;
					if ((start < end && step < 0) || (start > end && step > 0))
						step = -step;

					if (start == end)
						list.Add(new ValueInt(start));
					else if (start < end)
						for (int i = start; i <= end; i += step)
							list.Add(new ValueInt(i));
					else
						for (int i = start; i >= end; i += step)
							list.Add(new ValueInt(i));
				}
				else
				{	// if anything is a float, output is floats
					double start = vStart.AsForcedFloat;
					double end = vEnd.AsForcedFloat;
					double step = vStep.AsForcedFloat;
					end += 1e-10;	// a cheap but non-robust way to deal w/ roundoff
					if ((start < end && step < 0) || (start > end && step > 0))
						step = -step;
					if (start < end)
						for (double i = start; i < end; i += step)
							list.Add(new ValueFloat(i));
					else
						for (double i = start; i > end; i += step)
							list.Add(new ValueFloat(i));
				}

				return new ValueArray(list);
			}
		}

		/// <summary>{ [:pre] [:post] :body [:order] } -> function</summary>
		class CreateFunction : ValueFunctionPre
		{
			internal override Value ValueCopy() { return new CreateFunction(); }

			internal CreateFunction()
			{
				SetDocString("Create a function.\nSpecify the previous and/or next parameter patterns and the body.\nCan optionally specify the evaluation precedence relative to other functions.");

				Map map = new Map();
				map["pre"] = PatternData.Single("pre", new ValueNil());
				map["post"] = PatternData.Single("post", new ValueNil());
				map["order"] = PatternData.Single("order", ValueType.Int, new ValueInt((int)Order.Medium));
				map["body"] = PatternData.Body();
				ValueMap vMap = new ValueMap(map);
				Init(vMap);
			}

			internal override Value Eval(Value arg, IScope scope)
			{
				Map map = arg.AsMap;

				// extract optional & required parameters
				Value pre = map["pre"];
				Value post = map["post"];
				Value body = (map.ContainsKey("body") ? map["body"] : map[ValueFunction.keyBody]);
				Order order = (Order)(map["order"].AsInt);

				if (body is ValueArray)
					return loki3.core.CreateFunction.Do(pre, post, body.AsArray, order);
				return loki3.core.CreateFunction.Do(pre, post, body.AsLine, order);
			}
		}

		/// <summary>{ :end [:type] [function] } -> delimiter</summary>
		class CreateDelimiter : ValueFunctionPre
		{
			internal override Value ValueCopy() { return new CreateDelimiter(); }

			internal CreateDelimiter()
			{
				SetDocString("Create a delimiter, with start and end strings.\nOptionally specify how the contents should be interpreted and/or a function to be run on the contents.");

				Map map = new Map();
				map["end"] = PatternData.Single("end", ValueType.String, new ValueString(""));
				map["type"] = PatternData.Single("type", new ValueNil());
				map["function"] = PatternData.Single("function", ValueType.Function, new ValueNil());
				ValueMap vMap = new ValueMap(map);
				Init(vMap);
			}

			internal override Value Eval(Value arg, IScope scope)
			{
				Map map = arg.AsMap;

				// extract optional & required parameters
				string end = map["end"].AsString;
				Value typeval = map.GetOptional("type", null);
				Value funcval = map.GetOptional("function", null);
				ValueFunction function = (funcval == null ? null : funcval as ValueFunction);

				DelimiterType type = DelimiterType.AsValue;
				if (typeval != null)
				{
					string typestr = typeval.AsString;
					if (typestr == "asArray")
						type = DelimiterType.AsArray;
					else if (typestr == "asString")
						type = DelimiterType.AsString;
					else if (typestr == "asComment")
						type = DelimiterType.AsComment;
					else if (typestr == "asRaw")
						type = DelimiterType.AsRaw;
				}

				return new ValueDelimiter(end, type, function);
			}
		}


		/// <summary>
		/// string -> delimiter
		/// Special bootstrapping method that attaches a delimiter to current scope,
		/// splitting single parameter to get the actual delimeters
		/// </summary>
		abstract class CreateSimpleDelimiter : ValueFunctionPre
		{
			internal CreateSimpleDelimiter()
			{
				SetDocString("Useful bootstrapping function for attaching a delimiter to the current scope.");

				Value delims = PatternData.Single("delims", ValueType.String);
				Init(delims);
			}

			internal override Value Eval(Value arg, IScope scope)
			{
				// extract the delimiter strings
				string delims = arg.AsString;
				string start = delims.Substring(0, delims.Length / 2);
				string end = delims.Substring(delims.Length / 2, delims.Length - delims.Length / 2);

				// create the delimiter & store it on the current scope
				ValueDelimiter value = new ValueDelimiter(end, DelimiterType);
				scope.SetValue(start, value);
				return value;
			}

			protected abstract DelimiterType DelimiterType { get; }
		}

		class CreateEvalDelimiter : CreateSimpleDelimiter
		{
			internal override Value ValueCopy() { return new CreateEvalDelimiter(); }
			protected override DelimiterType DelimiterType { get { return DelimiterType.AsValue; } }
		}
		class CreateArrayDelimiter : CreateSimpleDelimiter
		{
			internal override Value ValueCopy() { return new CreateArrayDelimiter(); }
			protected override DelimiterType DelimiterType { get { return DelimiterType.AsArray; } }
		}

		/// <summary>:value -> count of items in value</summary>
		class GetCount : ValueFunctionPre
		{
			internal override Value ValueCopy() { return new GetCount(); }

			internal GetCount()
			{
				SetDocString("Get the number of items in a value");
				Value item = PatternData.Single("value");
				Init(item);
			}

			internal override Value Eval(Value arg, IScope scope)
			{
				int count = 1;
				if (arg is ValueString)
					count = arg.AsString.Length;
				else
					count = arg.Count;
				return new ValueInt(count);
			}
		}

		/// <summary>{ [:key : string] [:value] [:writable? : bool] ] } -> map containing the metadata</summary>
		class GetMetadata : ValueFunctionPre
		{
			internal override Value ValueCopy() { return new GetMetadata(); }

			internal GetMetadata()
			{
				SetDocString("Get metadata attached to a value.\nEither pass key to do a lookup or a specific value.\nIf writable?, make sure metadata map exists before returning.");

				Map map = new Map();
				map["key"] = PatternData.Single("key", ValueType.String, ValueNil.Nil);
				map["value"] = PatternData.Single("value", ValueNil.Nil);
				map["writable?"] = PatternData.Single("writable?", ValueType.Bool, ValueBool.False);
				ValueMap vMap = new ValueMap(map);
				Init(vMap);
			}

			internal override Value Eval(Value arg, IScope scope)
			{
				Map map = arg.AsMap;

				// either lookup up value w/ specified key or just get specified value
				Value value = Utility.GetFromKeyOrValue(map, scope);

				// return associated metadata
				if (value == null)
					return ValueNil.Nil;
				bool writable = map["writable?"].AsBool;
				Map meta = writable ? value.WritableMetadata : value.Metadata;
				if (meta == null)
					return ValueNil.Nil;
				return new ValueMap(meta);
			}
		}

		/// <summary>{ [:key] [:value] [:builtin?] } -> the type, optionally just looking at built-in type</summary>
		class GetTypeFunction : ValueFunctionPre
		{
			internal override Value ValueCopy() { return new GetTypeFunction(); }

			internal GetTypeFunction()
			{
				SetDocString("Get a value's type");
				Map map = new Map();
				map["key"] = PatternData.Single("key", ValueType.String, ValueNil.Nil);
				map["value"] = PatternData.Single("value", ValueNil.Nil);
				map["builtin?"] = PatternData.Single("builtin?", ValueType.Bool, ValueBool.False);
				ValueMap vMap = new ValueMap(map);
				Init(vMap);
			}

			internal override Value Eval(Value arg, IScope scope)
			{
				// extract optional & required parameters
				Map map = arg.AsMap;
				Value value = Utility.GetFromKeyOrValue(map, scope);
				bool builtin = map["builtin?"].AsBool;

				string type = (builtin ? ValueClasses.ClassOf(value.Type) : value.MetaType);
				return new ValueString(type);
			}
		}

		/// <summary>{ [:key] [:function] } -> the body of the function</summary>
		class GetFunctionBody : ValueFunctionPre
		{
			internal override Value ValueCopy() { return new GetFunctionBody(); }

			internal GetFunctionBody()
			{
				SetDocString("Get the body of a function.");

				Map map = new Map();
				map["key"] = PatternData.Single("key", ValueType.String, ValueNil.Nil);
				map["function"] = PatternData.Single("function", ValueType.Function, ValueNil.Nil);
				Init(new ValueMap(map));
			}

			internal override Value Eval(Value arg, IScope scope)
			{
				Map map = arg.AsMap;

				// get parameter
				Value value = map["function"];
				if (value.IsNil)
				{
					Value valueKey = map["key"];
					if (!valueKey.IsNil)
					{
						string key = valueKey.AsString;
						IScope container = scope.Exists(key);
						if (container == null)
							throw new Loki3Exception().AddMissingKey(map["key"]);
						value = container.GetValue(new Token(key));
					}
				}
				if (value.IsNil)
					throw new Loki3Exception().AddMissingKey(new ValueString("function"));

				// fetch body of function
				ValueFunction function = value as ValueFunction;
				if (function == null)
					throw new Loki3Exception().AddWrongType(ValueType.Function, value.Type);
				List<DelimiterList> body = function.GetBody(scope);
				if (body == null)
					return ValueNil.Nil;

				// make an array
				List<Value> array = new List<Value>();
				foreach (DelimiterList line in body)
					array.Add(new ValueRaw(line));
				return new ValueArray(array);
			}
		}

		/// <summary>value -> evaluates the value</summary>
		class EvalValue : ValueFunctionPre
		{
			internal override Value ValueCopy() { return new EvalValue(); }

			internal EvalValue()
			{
				SetDocString("Evals a string, array of strings, or array of raw value");
				Value value = PatternData.Single("value");
				Init(value);
			}

			internal override Value Eval(Value arg, IScope scope)
			{
				return Utility.EvalValue(arg, scope);
			}
		}

		/// <summary>{ :function [:map] } -> a function that will use a map for its scope</summary>
		class BindFunction : ValueFunctionPre
		{
			internal override Value ValueCopy() { return new BindFunction(); }

			internal BindFunction()
			{
				SetDocString("Bind a map or scope to a function so it's always used when the function is called.");

				Map map = new Map();
				map["function"] = PatternData.Single("function", ValueType.Function);
				map["map"] = PatternData.Single("map", ValueType.Map, ValueNil.Nil);
				Init(new ValueMap(map));
			}

			internal override Value Eval(Value arg, IScope scope)
			{
				Map map = arg.AsMap;

				// get parameter
				ValueFunction function = map["function"] as ValueFunction;
				IScope thescope;
				if (map["map"].IsNil)
				{	// nothing specified, use current scope
					thescope = scope;
				}
				else
				{	// either use map or scope passed in
					ValueMap themap = map["map"] as ValueMap;
					thescope = (themap.Scope != null ? themap.Scope : new ScopeChain(themap.AsMap));
				}

				return new BoundFunction(function, thescope);
			}
		}
	}
}
