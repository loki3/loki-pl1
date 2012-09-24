using System;
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
			internal override Value Eval(DelimiterNode next, IScope scope, INodeRequestor nodes)
			{
				Value post = EvalNode.Do(next, scope, nodes);
				List<Value> list = post.AsArray;
				if (list.Count != 2)
					throw new WrongSizeArray(2, list.Count);

				string key = list[0].AsString;
				Value value = list[1];
				scope.SetValue(key, value);
				return value;
			}
		}

		/// <summary>[k1 v1 k2 v2 ...] -> map</summary>
		class CreateMap : ValueFunctionPre
		{
			internal override Value Eval(DelimiterNode next, IScope scope, INodeRequestor nodes)
			{
				Value post = EvalNode.Do(next, scope, nodes);
				List<Value> list = post.AsArray;

				Dictionary<string, Value> map = new Dictionary<string, Value>();
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
			internal override Value Eval(DelimiterNode next, IScope scope, INodeRequestor nodes)
			{
				Value value = EvalNode.Do(next, scope, nodes);
				ValueMap map = value.AsMap;

				// extract optional & required parameters
				Value pre = map.GetOptional("pre", null);
				Value post = map.GetOptional("post", null);
				List<Value> valueBody = map["body"].AsArray;
				Value temp = map.GetOptional("order", null);
				Precedence order = (temp == null ? Precedence.Medium : (Precedence)temp.AsInt);

				if (pre == null && post == null)
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
			internal override Value Eval(DelimiterNode next, IScope scope, INodeRequestor nodes)
			{
				Value value = EvalNode.Do(next, scope, nodes);
				ValueMap map = value.AsMap;

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
