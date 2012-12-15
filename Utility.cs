using System.Collections.Generic;

namespace loki3.core
{
	internal class Utility
	{
		/// <summary>Add the values from the matched pattern to the scope</summary>
		internal static void AddToScope(Value pattern, Value match, IScope scope, bool bCreate)
		{
			if (pattern is ValueString)
			{
				if (!pattern.IsNil)
					SetOnScope(scope, pattern.AsString, match, bCreate);
			}
			else if (pattern is ValueMap && match is ValueMap)
			{	// add matched dictionary to current scope
				Map patmap = pattern.AsMap;
				Map matmap = match.AsMap;
				foreach (string key in matmap.Raw.Keys)
					if (!patmap[key].IsNil)
						SetOnScope(scope, patmap[key].AsString, matmap[key], bCreate);
			}
			else if (pattern is ValueArray && match is ValueArray)
			{	// add matched array values to current scope
				List<Value> patarray = pattern.AsArray;
				List<Value> matcharray = match.AsArray;
				int count = System.Math.Min(patarray.Count, matcharray.Count);
				for (int i = 0; i < count; i++)
					if (!patarray[i].IsNil)
						SetOnScope(scope, patarray[i].AsString, matcharray[i], bCreate);
			}
			else if (pattern is ValueArray && match is ValueMap)
			{
				Map matmap = match.AsMap;
				foreach (string key in matmap.Raw.Keys)
					SetOnScope(scope, key, matmap[key], bCreate);
			}
		}
		/// <summary>Add the values from the matched pattern to the scope</summary>
		internal static void AddToScope(Value pattern, Value match, IScope scope)
		{
			AddToScope(pattern, match, scope, true);
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
		/// If bCreate, always set value on current scope,
		/// else, change value on scope value exists on or throw exception if it doesn't exist
		/// </summary>
		internal static void SetOnScope(IScope scope, string key, Value value, bool bCreate)
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

			where.SetValue(key, value);
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
		///		:scope = current | parent | grandparent
		///		:map   = if present, the map to modify
		/// </summary>
		internal static IScope GetScopeToModify(Map map, IScope scope, bool bIncludeMap)
		{
			if (!map.ContainsKey("map") && !map.ContainsKey("scope"))
				return scope;

			ValueMap valueMap = (bIncludeMap ? map["map"] as ValueMap : null);
			// todo: turn this into an enum, at least "current" & "parent" & "grandparent"
			bool bParentScope = (map["scope"].AsString == "parent");
			bool bGrandparentScope = (map["scope"].AsString == "grandparent");

			// scope we're going to modify
			IScope toModify = scope;
			if (valueMap != null)
				toModify = new ScopeChain(valueMap.AsMap);
			else if (bParentScope)
				toModify = scope.Parent;
			else if (bGrandparentScope && scope.Parent != null)
				toModify = scope.Parent.Parent;
			if (toModify == null)
				toModify = scope;
			return toModify;
		}
		internal static void AddParamsForScopeToModify(Map map, bool bIncludeMap)
		{
			if (bIncludeMap)
				map["map"] = PatternData.Single("map", ValueType.Map, ValueNil.Nil);
			map["scope"] = PatternData.Single("scope", ValueType.String, new ValueString("current"));
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
	}
}
