using System.Collections.Generic;

namespace loki3.core
{
	internal class Utility
	{
		/// <summary>Add the values from the matched pattern to the scope</summary>
		internal static void AddToScope(Value pattern, Value match, IScope scope)
		{
			if (pattern is ValueString)
			{
				scope.SetValue(pattern.AsString, match);
			}
			else if (pattern is ValueArray)
			{	// add matched array values to current scope
				List<Value> patarray = pattern.AsArray;
				List<Value> matcharray = match.AsArray;
				for (int i = 0; i < patarray.Count; i++)
					scope.SetValue(patarray[i].AsString, matcharray[i]);
			}
			else if (pattern is ValueMap)
			{	// add matched dictionary to current scope
				foreach (string key in match.AsMap.Raw.Keys)
					scope.SetValue(key, match.AsMap[key]);
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
	}
}
