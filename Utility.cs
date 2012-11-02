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
				SetOnScope(scope, pattern.AsString, match, bCreate);
			}
			else if (pattern is ValueMap && match is ValueMap)
			{	// add matched dictionary to current scope
				Map patmap = pattern.AsMap;
				Map matmap = match.AsMap;
				foreach (string key in matmap.Raw.Keys)
					SetOnScope(scope, patmap[key].AsString, matmap[key], bCreate);
			}
			else if (pattern is ValueArray && match is ValueArray)
			{	// add matched array values to current scope
				List<Value> patarray = pattern.AsArray;
				List<Value> matcharray = match.AsArray;
				int count = System.Math.Min(patarray.Count, matcharray.Count);
				for (int i = 0; i < count; i++)
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

		/// <summary>
		/// If bCreate, always set value on current scope,
		/// else, change value on scope value exists on or throw exception if it doesn't exist
		/// </summary>
		internal static void SetOnScope(IScope scope, string key, Value value, bool bCreate)
		{
			if (bCreate)
			{
				scope.SetValue(key, value);
			}
			else
			{
				IScope where = scope.Exists(key);
				if (where == null)
					throw new Loki3Exception().AddBadToken(new Token(key));
				where.SetValue(key, value);
			}
		}

		/// <summary>Add two arrays or maps into one</summary>
		internal static Value Combine(Value a, Value b)
		{
			if (a is ValueArray)
				return ValueArray.Combine(a as ValueArray, b as ValueArray);
			else if (a is ValueMap)
				return ValueMap.Combine(a as ValueMap, b as ValueMap);
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
	}
}
