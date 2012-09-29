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
			scope.SetValue("l3.createMap", new CreateMap());
			scope.SetValue("l3.createFunction", new CreateFunction());
			scope.SetValue("l3.createDelimiter", new CreateDelimiter());
		}


		/// <summary>[key value] -> value  and stores the key value pair on the current scope</summary>
		class SetValue : ValueFunctionPre
		{
			internal SetValue()
			{
				List<Value> list = new List<Value>();
				list.Add(PatternData.Single("key", ValueType.String));
				list.Add(PatternData.Single("value"));
				ValueArray array = new ValueArray(list);
				Init(array);
			}

			protected override Value Eval(Value arg, IScope scope)
			{
				List<Value> list = arg.AsArray;
				string key = list[0].AsString;
				Value value = list[1];
				scope.SetValue(key, value);
				return value;
			}
		}

		/// <summary>[k1 v1 k2 v2 ...] -> map</summary>
		class CreateMap : ValueFunctionPre
		{
			internal CreateMap() { Init(PatternData.ArrayEnd("a")); }

			protected override Value Eval(Value arg, IScope scope)
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

		/// <summary>{ [:pre] [:post] :body [:order] } -> function</summary>
		class CreateFunction : ValueFunctionPre
		{
			internal CreateFunction()
			{
				Map map = new Map();
				map["pre"] = PatternData.Single("pre", new ValueNil());
				map["post"] = PatternData.Single("post", new ValueNil());
				map["order"] = PatternData.Single("order", ValueType.Int, new ValueInt((int)Precedence.Medium));
				map["body"] = PatternData.Body();
				ValueMap vMap = new ValueMap(map);
				Init(vMap);
			}

			protected override Value Eval(Value arg, IScope scope)
			{
				Map map = arg.AsMap;

				// extract optional & required parameters
				Value pre = map["pre"];
				Value post = map["post"];
				List<Value> valueBody = map["body"].AsArray;
				Precedence order = (Precedence)(map["order"].AsInt);

				if (pre.IsNil && post.IsNil)
					throw new MissingParameter("pre");	// TODO: richer message

				// need a list of strings, not values
				List<string> body = new List<string>(valueBody.Count);
				foreach (Value v in valueBody)
					body.Add(v.AsString);

				return loki3.core.CreateFunction.Do(pre, post, body, order);
			}
		}

		/// <summary>{ :start :end [:type] [function] } -> delimiter</summary>
		class CreateDelimiter : ValueFunctionPre
		{
			internal CreateDelimiter()
			{
				Map map = new Map();
				map["start"] = PatternData.Single("start", ValueType.String);
				map["end"] = PatternData.Single("end", ValueType.String);
				map["type"] = PatternData.Single("type", new ValueNil());
				map["function"] = PatternData.Single("function", ValueType.Function, new ValueNil());
				ValueMap vMap = new ValueMap(map);
				Init(vMap);
			}

			protected override Value Eval(Value arg, IScope scope)
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
				}

				return new ValueDelimiter(start, end, type, function);
			}
		}
	}
}
