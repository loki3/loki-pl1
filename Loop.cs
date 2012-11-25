using System.Collections.Generic;
using loki3.core;

namespace loki3.builtin
{
	/// <summary>
	/// Built-in loop functions
	/// </summary>
	class Loop
	{
		/// <summary>
		/// Add built-in If functions to the scope
		/// </summary>
		internal static void Register(IScope scope)
		{
			scope.SetValue("l3.loop", new BasicLoop());
			scope.SetValue("l3.forEach", new ForEach());
		}


		/// <summary>{ :check :body [:change] [:checkFirst?] } -> keep running body until !check</summary>
		class BasicLoop : ValueFunctionPre
		{
			internal override Value ValueCopy() { return new BasicLoop(); }

			internal BasicLoop()
			{
				SetDocString("Repeat body until a condition is no longer true. Optionally run some code at the end of each loop.  Return value is last value of body.");

				Map map = new Map();
				map["check"] = PatternData.Single("check", ValueType.Raw);
				map["change"] = PatternData.Single("change", ValueType.Raw, ValueNil.Nil);
				map["checkFirst?"] = PatternData.Single("checkFirst?", ValueType.Bool, ValueBool.True);
				map["body"] = PatternData.Body();
				ValueMap vMap = new ValueMap(map);
				Init(vMap);
			}

			internal override Value Eval(Value arg, IScope scope)
			{
				Map map = arg.AsMap;

				List<DelimiterNode> check = (map["check"] as ValueRaw).GetValue().Nodes;
				Value changeVal = map["change"];
				List<DelimiterNode> change = changeVal is ValueNil ? null : (changeVal as ValueRaw).GetValue().Nodes;
				bool checkFirst = map["checkFirst?"].AsBool;
				// todo: consolidate these bodies, one from an explicit param & one from the following lines
				Value valueBody = map.ContainsKey("body") ? map["body"] : map["l3.func.body"];

				bool isFirst = true;
				Value result = ValueBool.False;
				while (true)
				{
					// check if we should stop loop
					if (!isFirst || checkFirst)
					{
						Value retval = EvalList.Do(check, scope);
						if (!retval.AsBool)
							return result;	// last value of body
					}
					else
					{
						isFirst = false;
					}

					// eval body
					result = EvalBody.Do(valueBody, scope);

					// if per-loop code was passed, run it
					if (change != null)
						EvalList.Do(change, scope);
				}
			}
		}

		/// <summary>{ :key :collection [:delim] :body } -> run body for every item in collection</summary>
		class ForEach : ValueFunctionPre
		{
			internal override Value ValueCopy() { return new ForEach(); }

			internal ForEach()
			{
				SetDocString("Repeat body once for every item in collection.  Return value is last value of body.");

				Map map = new Map();
				map["key"] = PatternData.Single("key");
				map["collection"] = PatternData.Single("collection");
				map["delim"] = PatternData.Single("delim", ValueType.String, new ValueString(""));
				map["body"] = PatternData.Body();
				ValueMap vMap = new ValueMap(map);
				Init(vMap);
			}

			internal override Value Eval(Value arg, IScope scope)
			{
				Map map = arg.AsMap;

				// todo: generalize key for pattern matching
				string var = map["key"].AsString;
				Value collection = map["collection"];
				// todo: consolidate these bodies, one from an explicit param & one from the following lines
				Value valueBody = map.ContainsKey("body") ? map["body"] : map["l3.func.body"];

				// todo: abstract iteration to avoid these ifs
				Value result = ValueNil.Nil;
				if (collection is ValueString)
				{
					string s = collection.AsString;
					foreach (char c in s)
					{
						scope.SetValue(var, new ValueString(c.ToString()));
						result = EvalBody.Do(valueBody, scope);
					}
				}
				else if (collection is ValueArray)
				{
					List<Value> list = collection.AsArray;
					foreach (Value v in list)
					{
						scope.SetValue(var, v);
						result = EvalBody.Do(valueBody, scope);
					}
				}
				else if (collection is ValueMap)
				{
					Dictionary<string, Value> dict = collection.AsMap.Raw;
					foreach (string key in dict.Keys)
					{
						List<Value> list = new List<Value>();
						list.Add(new ValueString(key));
						list.Add(dict[key]);
						
						scope.SetValue(var, new ValueArray(list));
						result = EvalBody.Do(valueBody, scope);
					}
				}
				else if (collection is ValueLine)
				{
					List<DelimiterList> list = collection.AsLine;

					// if delimiter is specified, wrap each line w/ it
					ValueDelimiter delim = scope.GetValue(new Token(map["delim"].AsString)) as ValueDelimiter;
					if (delim != null)
					{
						List<DelimiterList> delimList = new List<DelimiterList>();
						int indent = (list.Count > 0 ? list[0].Indent : 0);
						foreach (DelimiterList line in list)
						{
							DelimiterList newLine = line;
							if (line.Indent == indent)
							{
								List<DelimiterNode> nodes = new List<DelimiterNode>();
								nodes.Add(new DelimiterNodeList(new DelimiterList(delim, line.Nodes, line.Indent)));
								newLine = new DelimiterList(delim, nodes, indent);
							}
							delimList.Add(newLine);
						}
						list = delimList;
					}

					// for each line, eval it then eval the body
					ILineRequestor lines = new LineConsumer(list);
					while (lines.HasCurrent())
					{
						Value value = EvalLines.DoOne(lines, scope);
						scope.SetValue(var, value);
						result = EvalBody.Do(valueBody, scope);
					}
				}
				return result;
			}
		}

	}
}
