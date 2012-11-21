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
			scope.SetValue("l3.createMap", new CreateMap());
			scope.SetValue("l3.createRange", new CreateRange());
			scope.SetValue("l3.createFunction", new CreateFunction());
			scope.SetValue("l3.createDelimiter", new CreateDelimiter());
			scope.SetValue("l3.createEvalDelimiter", new CreateEvalDelimiter());
			scope.SetValue("l3.createArrayDelimiter", new CreateArrayDelimiter());
			scope.SetValue("l3.getCount", new GetCount());
			scope.SetValue("l3.getMetadata", new GetMetadata());
		}


		/// <summary>{ :key :value [:create?] [:map] [:scope] } -> value  and stores the key value pair on the specified map or scope</summary>
		class SetValue : ValueFunctionPre
		{
			internal SetValue()
			{
				SetDocString("Set the value on given map key.\nEither creates new variable on active scope or stores value on existing variable.\nCan be stored on a map, current or parent scope.");

				Map map = new Map();
				map["key"] = PatternData.Single("key");
				map["value"] = PatternData.Single("value");
				map["create?"] = PatternData.Single("create?", ValueType.Bool, ValueBool.True);
				map["map"] = PatternData.Single("map", ValueType.Map, ValueNil.Nil);
				map["scope"] = PatternData.Single("scope", ValueType.String, new ValueString("current"));
				ValueMap vMap = new ValueMap(map);
				Init(vMap);
			}

			internal override Value Eval(Value arg, IScope scope)
			{
				Map map = arg.AsMap;

				// extract parameters
				Value key = map["key"];
				Value value = map["value"];
				bool bCreate = map["create?"].AsBool;
				ValueMap valueMap = map["map"] as ValueMap;
				// todo: turn this into an enum, at least "current" & "parent"
				bool bParentScope = (map["scope"].AsString == "parent");

				// key must be a type that can be used for pattern matching
				if (key.Type != ValueType.String && key.Type != ValueType.Array && key.Type != ValueType.Map)
					throw new Loki3Exception().AddWrongType(ValueType.String, key.Type);

				// find the matches in the pattern, ignoring the leftover
				Value match, leftover;
				PatternChecker.Do(value, key, true/*bShortPat*/, out match, out leftover);

				// scope we're going to modify
				IScope toModify = null;
				if (valueMap != null)
					toModify = new ScopeChain(valueMap.AsMap);
				else
					toModify = (bParentScope && scope.Parent != null) ? scope.Parent : scope;

				// add/modify scope
				if (match != null)
					Utility.AddToScope(key, match, toModify, bCreate);
				return value;
			}
		}

		/// <summary>{ :object :key } -> value,  either map[key.AsString] or array[key.AsInt]</summary>
		class GetValue : ValueFunctionPre
		{
			internal GetValue()
			{
				SetDocString("On a map, gets the value attached to a key.\nOn an array, gets the indexed item.");

				Map map = new Map();
				// todo: better pattern definition, map|array & string|int
				// or make this into two functions
				map["object"] = PatternData.Single("object");
				map["key"] = PatternData.Single("key");
				ValueMap vMap = new ValueMap(map);
				Init(vMap);
			}

			internal override Value Eval(Value arg, IScope scope)
			{
				Map map = arg.AsMap;

				// extract parameters
				Value obj = map["object"];
				Value key = map["key"];

				ValueMap objMap = obj as ValueMap;
				if (objMap != null)
				{
					return objMap.AsMap.GetOptional(key.AsString, ValueNil.Nil);
				}
				ValueArray objArr = obj as ValueArray;
				if (objArr != null)
				{
					return objArr.AsArray[key.AsInt];
				}
				// todo: better error
				throw new Loki3Exception().AddWrongType(ValueType.Map, obj.Type);
			}
		}

		/// <summary>[k1 v1 k2 v2 ...] -> map</summary>
		class CreateMap : ValueFunctionPre
		{
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
					if (start < end)
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

		/// <summary>{ :start :end [:type] [function] } -> delimiter</summary>
		class CreateDelimiter : ValueFunctionPre
		{
			internal CreateDelimiter()
			{
				SetDocString("Create a delimiter, with start and end strings.\nOptionally specify how the contents should be interpreted and/or a function to be run on the contents.");

				Map map = new Map();
				map["start"] = PatternData.Single("start", ValueType.String);
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
				string start = map["start"].AsString;
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

				return new ValueDelimiter(start, end, type, function);
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
				ValueDelimiter value = new ValueDelimiter(start, end, DelimiterType);
				scope.SetValue(start, value);
				return value;
			}

			protected abstract DelimiterType DelimiterType { get; }
		}

		class CreateEvalDelimiter : CreateSimpleDelimiter
		{
			protected override DelimiterType DelimiterType { get { return DelimiterType.AsValue; } }
		}
		class CreateArrayDelimiter : CreateSimpleDelimiter
		{
			protected override DelimiterType DelimiterType { get { return DelimiterType.AsArray; } }
		}

		/// <summary>:value -> count of items in value</summary>
		class GetCount : ValueFunctionPre
		{
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
				else if (arg is ValueArray)
					count = arg.AsArray.Count;
				else if (arg is ValueMap)
					count = arg.AsMap.Count;
				return new ValueInt(count);
			}
		}

		/// <summary>{ [:key : string] [:value] [:writable? : bool] ] } -> map containing the metadata</summary>
		class GetMetadata : ValueFunctionPre
		{
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
				Value value = null;
				Value key = map["key"];
				if (key != ValueNil.Nil)
				{
					Token token = new Token(key.AsString);
					value = scope.GetValue(token);
					if (value == null)
						throw new Loki3Exception().AddBadToken(token);
				}
				else if (map.ContainsKey("value"))
				{
					value = map["value"];
				}

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
	}
}
