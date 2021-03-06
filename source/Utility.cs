using System.Collections.Generic;

namespace loki3.core
{
	internal class Utility
	{
		/// <summary>Add the values from the matched pattern to the scope</summary>
		/// <param name="bCreate">should key be created in current scope if it doesn't exist?</param>
		/// <param name="bOverload">if value is a function, determines whether it's added to override list on key</param>
		/// <param name="bInitOnly">if true and key already exists, don't change value</param>
		/// <returns>true if it set values</returns>
		internal static bool AddToScope(Value pattern, Value match, IScope scope, bool bCreate, bool bOverload, bool bInitOnly)
		{
			if (pattern is ValueString)
			{
				return SetOnScope(scope, pattern.AsString, match, bCreate, bOverload, bInitOnly);
			}
			else if (pattern is ValueMap && match is ValueMap)
			{	// add matched dictionary to current scope
				Map patmap = pattern.AsMap;
				Map matmap = match.AsMap;
				foreach (string key in matmap.Raw.Keys)
					if (!SetSubOnScope(scope, patmap[key], matmap[key], bCreate, bOverload, bInitOnly))
						return false;
				return matmap.Raw.Keys.Count > 0;
			}
			else if (pattern is ValueArray && match is ValueArray)
			{	// add matched array values to current scope
				List<Value> patarray = pattern.AsArray;
				List<Value> matcharray = match.AsArray;
				int count = System.Math.Min(patarray.Count, matcharray.Count);
				for (int i = 0; i < count; i++)
					if (!SetSubOnScope(scope, patarray[i], matcharray[i], bCreate, bOverload, bInitOnly))
						return false;
				return count > 0;
			}
			else if (pattern is ValueArray && match is ValueMap)
			{
				Map matmap = match.AsMap;
				foreach (string key in matmap.Raw.Keys)
					if (!SetOnScope(scope, key, matmap[key], bCreate, bOverload, bInitOnly))
						return false;
				return matmap.Raw.Keys.Count > 0;
			}
			return false;
		}
		/// <summary>Add the values from the matched pattern to the scope</summary>
		internal static bool AddToScope(Value pattern, Value match, IScope scope)
		{
			return AddToScope(pattern, match, scope, true/*bCreate*/, false/*bOverload*/, false/*bInitOnly*/);
		}

		/// <summary>Add values from one scope onto another</summary>
		internal static void AddToScope(IScope from, IScope to)
		{
			Dictionary<string, Value> dict = from.AsMap.Raw;
			if (dict != null)
				foreach (string key in dict.Keys)
					to.SetValue(key, dict[key]);
		}

		/// <summary>
		/// Either set a key directly or recurse into a collection
		/// </summary>
		private static bool SetSubOnScope(IScope scope, Value subPattern, Value value, bool bCreate, bool bOverload, bool bInitOnly)
		{
			if (subPattern is ValueString)
			{	// key set directly
				if (!SetOnScope(scope, subPattern.AsString, value, bCreate, bOverload, bInitOnly))
					return false;
			}
			else if (subPattern is ValueArray || subPattern is ValueMap)
			{	// recurse into collection
				if (!AddToScope(subPattern, value, scope, bCreate, bOverload, bInitOnly))
					return false;
			}
			return true;
		}

		/// <summary>
		/// If bCreate, always set value on current scope,
		/// else, change value on scope value exists on or throw exception if it doesn't exist
		/// </summary>
		/// <returns>true if it actually set a value</returns>
		internal static bool SetOnScope(IScope scope, string key, Value value, bool bCreate, bool bOverload, bool bInitOnly)
		{
			// figure out the scope to modify
			IScope where;
			if (bCreate)
			{
				where = scope;
			}
			else
			{
				where = scope.Exists(key);
				if (where == null)
					throw new Loki3Exception().AddBadToken(new Token(key));
			}

			// if we're setting a function, it may be an overload
			if (bOverload && value.Type == ValueType.Function)
			{
				// first, if there's nothing there, see if key exists on an ancestor scope
				bool bFound = where.AsMap.ContainsKey(key);
				if (!bFound)
				{
					IScope ancestor = scope.Exists(key);
					if (ancestor != null)
					{
						Value existing = ancestor.AsMap[key];
						if (existing.Type == ValueType.Function)
						{	// copy function(s) to this scope so we can overload it
							// and yet this definition only exists in this scope
							Value copiedToThisScope = existing.Copy();
							where.SetValue(key, copiedToThisScope);
							bFound = true;
						}
					}
				}

				if (bFound)
				{
					Value existing = where.AsMap[key];
					ValueFunctionOverload overload = null;
					if (existing.Type == ValueType.Function)
					{	// change function value into an overload value
						overload = existing as ValueFunctionOverload;
						if (overload == null)
						{
							overload = new ValueFunctionOverload(existing as ValueFunction);
							overload.Add(value as ValueFunction);
							where.SetValue(key, overload);
						}
						else
						{
							overload.Add(value as ValueFunction);
						}
						return true;
					}
				}
			}

			if (bInitOnly && where.AsMap.ContainsKey(key))
				return false;
			where.SetValue(key, value);
			return true;
		}

		/// <summary>If :key is present, lookup value, else if :value is present, return value.</summary>
		internal static Value GetFromKeyOrValue(Map map, IScope scope)
		{
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
			if (value == null)
				// todo: better error
				throw new Loki3Exception().AddMissingKey(new ValueString("key or value required"));
			return value;
		}

		/// <summary>
		/// Get the scope to modify based on values in 'map'.
		///		:level = the number of parents up the chain to modify
		///		:map   = if present, the map to modify
		/// </summary>
		internal static IScope GetScopeToModify(Map map, IScope scope, bool bIncludeMap)
		{
			if (!map.ContainsKey("map") && !map.ContainsKey("level"))
				return scope;

			ValueMap valueMap = (bIncludeMap ? map["map"] as ValueMap : null);
			int level = (map.ContainsKey("level") ? map["level"].AsInt : 0);

			// scope we're going to modify
			IScope toModify = scope;
			if (valueMap != null)
				toModify = new ScopeChain(valueMap.AsMap);
			else
				for (int i = 0; i < level && toModify.Parent != null; i++)
					toModify = toModify.Parent;
			if (toModify == null)
				toModify = scope;
			return toModify;
		}
		internal static void AddParamsForScopeToModify(Map map, bool bIncludeMap)
		{
			if (bIncludeMap)
				map["map"] = PatternData.Single("map", ValueType.Map, ValueNil.Nil);
			map["level"] = PatternData.Single("level", ValueType.Int, new ValueInt(0));
		}

		/// <summary>Add two arrays or maps into one</summary>
		internal static Value Combine(Value a, Value b)
		{
			if (a is ValueArray)
				return ValueArray.Combine(a as ValueArray, b);
			else if (a.IsNil & b is ValueArray)
				return b;
			else if (a is ValueMap && b is ValueMap)
				return ValueMap.Combine(a as ValueMap, b as ValueMap);
			else if (a.IsNil && b is ValueMap)
				return b;
			return null;
		}

		/// <summary>Count the number of whitespace chars at the start of the line</summary>
		internal static int CountIndent(string s)
		{
			int count = 0;
			foreach (char c in s)
				if (System.Char.IsWhiteSpace(c))
					count++;
				else
					break;
			return count;
		}

		/// <summary>Return the list of indented lines, if any</summary>
		internal static ILineRequestor GetSubLineRequestor(List<Value> valueLines, int start, out int end)
		{
			end = start;
			Value v = valueLines[start];
			if (!(v is ValueString))
				return null;	// only strings have indents
			int count = valueLines.Count;
			if (start == count - 1)
				return null;	// no more lines

			string sThis = v.AsString;
			string sNext = valueLines[start+1].AsString;
			int indent = Utility.CountIndent(sThis);
			int nextindent = (start < count - 1 ? Utility.CountIndent(sNext) : indent);
			if (nextindent <= indent)
				return null;	// next line isn't indented

			List<string> subStrings = new List<string>();
			subStrings.Add(sNext);
			end++;
			for (int i = start + 2; i < count; i++)
			{
				string s = valueLines[i].AsString;
				nextindent = Utility.CountIndent(s);
				if (nextindent <= indent)
				{
					end = i - 1;
					break;	// ran out of indented lines
				}
				subStrings.Add(s);
			}
			return new LineConsumer(subStrings, true/*isSubset*/);
		}

		internal static ILineRequestor GetSubLineRequestor(List<DelimiterList> lines, int start, out int end)
		{
			end = start;
			int count = lines.Count;
			if (start == count - 1)
				return null;	// no more lines

			DelimiterList dThis = lines[start];
			DelimiterList dNext = lines[start + 1];
			int indent = dThis.Indent;
			int nextindent = (start < count - 1 ? dNext.Indent : indent);
			if (nextindent <= indent)
				return null;	// next line isn't indented

			List<DelimiterList> subLines = new List<DelimiterList>();
			subLines.Add(dNext);
			end++;
			for (int i = start + 2; i < count; i++)
			{
				DelimiterList s = lines[i];
				nextindent = s.Indent;
				if (nextindent <= indent)
				{
					end = i - 1;
					break;	// ran out of indented lines
				}
				subLines.Add(s);
			}
			return new LineConsumer(subLines, true/*isSubset*/);
		}

		/// <summary>
		/// Evals a string, array of strings, or array of raw value");
		/// </summary>
		/// <param name="scope">if value doesn't have an attacked scope, use the passed in scope</param>
		internal static Value EvalValue(Value value, IScope scope)
		{
			switch (value.Type)
			{
				case ValueType.Array:
				case ValueType.RawList:
					return EvalBody.Do(value, scope);
				case ValueType.String:
					DelimiterList dList = ParseLine.Do(value.AsString, scope);
					IScope listScope = (dList.Scope != null ? dList.Scope : scope);
					List<DelimiterList> list = new List<DelimiterList>();
					list.Add(dList);
					ValueLine line = new ValueLine(list, listScope);
					return EvalBody.Do(line, listScope);
				case ValueType.Raw:
					ValueRaw rawValue = (value as ValueRaw);
					DelimiterList dList2 = rawValue.GetValue();
					List<DelimiterList> list2 = new List<DelimiterList>();
					list2.Add(dList2);
					IScope rawScope = (rawValue.Scope != null ? rawValue.Scope : scope);
					ValueLine line2 = new ValueLine(list2, rawScope);
					return EvalBody.Do(line2, rawScope);
				case ValueType.Function:
					ValueFunction function = value as ValueFunction;
					return function.Eval(null, null, scope, scope, null, null);
				default:
					return value;
			}
		}
	}
}
