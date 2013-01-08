using System;
using System.Collections.Generic;

namespace loki3.core
{
	internal class PatternChecker
	{
		/// <summary>
		/// Checks the contents of a value against a pattern,
		/// filling in 'match' with the results,
		/// and 'leftover' with the portions that weren't in the pattern.
		/// Returns false if there was no match at all
		/// </summary>
		/// <param name="input">input value to match against</param>
		/// <param name="pattern">pattern to apply against input value</param>
		/// <param name="bShortPat">true if pattern is allowed to be smaller than input</param>
		/// <param name="match">portions that match</param>
		/// <param name="leftover">portions that don't match</param>
		internal static bool Do(Value input, Value pattern, bool bShortPat, out Value match, out Value leftover)
		{
			match = null;
			leftover = null;
			if (pattern.IsNil)
				return true;	// trivial match, ignore input

			switch (input.Type)
			{
				case ValueType.Nil:
					return DoSingle(input as ValueNil, ValueType.Nil, pattern, out match);

				case ValueType.Bool:
					return DoSingle(input as ValueBool, ValueType.Bool, pattern, out match);
				case ValueType.Int:
					return DoSingle(input as ValueInt, ValueType.Int, pattern, out match);
				case ValueType.Float:
					return DoSingle(input as ValueFloat, ValueType.Float, pattern, out match);
				case ValueType.String:
					return DoSingle(input as ValueString, ValueType.String, pattern, out match);

				case ValueType.Array:
					return MatchAgainstArray(input as ValueArray, pattern, bShortPat, out match, out leftover);
				case ValueType.Map:
					if (MatchAgainstMap(input as ValueMap, pattern as ValueMap, bShortPat, out match, out leftover))
						return true;
					if (MatchMapAgainstKeys(input as ValueMap, pattern as ValueArray, bShortPat, out match, out leftover))
						return true;
					return DoSingle(input as ValueMap, ValueType.Map, pattern, out match);

				case ValueType.Function:
					return DoSingle(input as ValueFunction, ValueType.Function, pattern, out match);
				case ValueType.Delimiter:
					return DoSingle(input as ValueDelimiter, ValueType.Delimiter, pattern, out match);
				case ValueType.Raw:
					return DoSingle(input as ValueRaw, ValueType.Raw, pattern, out match);
				case ValueType.RawList:
					return DoSingle(input as ValueLine, ValueType.RawList, pattern, out match);
			}

			return false;
		}

		/// <summary>Match against a non-aggregate type</summary>
		private static bool DoSingle<T>(T input, ValueType target, Value patternValue, out Value match) where T : Value
		{
			match = null;
			ValueString pattern = patternValue as ValueString;
			if (pattern == null)
			{
				T exact = patternValue as T;
				// if this is the same type, we need an exact match (e.g. [ :a 2 ] = [ :blah 2 ])
				// else fail because this only matches against a single item
				return input.Equals(exact);
			}
			PatternData data = new PatternData(pattern);

			// check type
			string patternType = data.Type;
			string targetType = ValueClasses.ClassOf(target);
			if (patternType != ValueClasses.ClassOf(ValueType.Nil) && patternType != targetType)
			{
				if (patternType != ValueClasses.ClassOf(ValueType.Number) || (target != ValueType.Int && target != ValueType.Float))
				{
					if (input.MetaType != patternType)
						return false;	// they asked for a different type
				}
			}

			// check hasKeys
			Value hasKeys = data.HasKeys;
			if (hasKeys != null)
			{
				// if input is required to have certain keys, then it's required to be a map
				Map inputMap = input.AsMap;
				if (hasKeys is ValueArray)
				{
					foreach (Value keyValue in hasKeys.AsArray)
						if (!inputMap.ContainsKey(keyValue.AsString))
							return false;	// a required key was missing
				}
				else if (hasKeys is ValueMap)
				{
					foreach (string key in hasKeys.AsMap.Raw.Keys)
						if (!inputMap.ContainsKey(key))
							return false;	// a required key was missing
				}
				else if (hasKeys is ValueString)
				{
					if (!inputMap.ContainsKey(hasKeys.AsString))
						return false;	// a required key was missing
				}
			}

			match = input;
			return true;
		}

		/// <summary>Match against an array</summary>
		private static bool MatchAgainstArray(ValueArray input, Value pattern, bool bShortPat, out Value match, out Value leftover)
		{
			match = null;
			leftover = null;

			// check if we asked for the entire array
			ValueString patternString = pattern as ValueString;
			if (patternString != null)
			{
				PatternData data = new PatternData(patternString);
				if (data.Type != ValueClasses.ClassOf(ValueType.String))
				{	// match as long as pattern doesn't say it wants type:string
					match = input;
					return true;
				}
			}

			ValueArray patternArray = pattern as ValueArray;
			if (patternArray == null)
				return false;	// this only matches against another array

			List<Value> inarray = input.AsArray;
			List<Value> patarray = patternArray.AsArray;
			int incount = inarray.Count;
			int patcount = patarray.Count;
			// check if an item in pattern wants rest of array
			Value lastPat = patarray[patcount - 1];
			bool wantsRest = (lastPat.Metadata != null && lastPat.Metadata.ContainsKey(PatternData.keyRest));
			if (wantsRest)
				--patcount;	// don't match against the last pattern item
			if (!bShortPat && patcount < incount && !wantsRest)
				return false;	// pattern has fewer elements than input - not a match

			// matches
			List<Value> matchlist = new List<Value>();
			int count = System.Math.Min(incount, patcount);
			for (int i = 0; i < count; i++)
			{
				Value submatch, subleftover;
				if (!Do(inarray[i], patarray[i], bShortPat, out submatch, out subleftover))
					return false;
				matchlist.Add(submatch);
			}

			// leftover
			if (patcount > incount)
			{
				List<Value> leftoverlist = new List<Value>();
				for (int i = incount; i < patcount; i++)
				{
					PatternData data = new PatternData(patarray[i] as ValueString);
					if (data.Default != null)
						matchlist.Add(data.Default);
					else
						leftoverlist.Add(patarray[i]);
				}
				if (leftoverlist.Count > 0 && !wantsRest)
					leftover = new ValueArray(leftoverlist);
			}
			// rest
			else if (wantsRest)
			{
				List<Value> restList = new List<Value>();
				for (int i = patcount; i < incount; i++)
					restList.Add(inarray[i]);
				matchlist.Add(new ValueArray(restList));
			}

			// matches + defaults
			match = new ValueArray(matchlist);

			return true;
		}

		/// <summary>Match against a map</summary>
		private static bool MatchAgainstMap(ValueMap input, ValueMap pattern, bool bShortPat, out Value match, out Value leftover)
		{
			match = null;
			leftover = null;
			if (pattern == null)
				return false;	// this only matches against another map

			Map inmap = input.AsMap;
			Map patmap = pattern.AsMap;
			int incount = inmap.Count;
			int patcount = patmap.Count;
			string restKey = FindRestKey(patmap);	// a key that wants the rest of the map
			if (!bShortPat && patcount < incount && restKey == null)
				return false;	// pattern has fewer elements than input - not a match

			Map matchmap = new Map();
			// if an item in pattern wants rest of map, create housing for it
			Map restMap = null;
			if (restKey != null)
			{
				restMap = new Map();
				matchmap[restKey] = new ValueMap(restMap);
			}

			// matches
			if (inmap.Raw != null)
				foreach (string key in inmap.Raw.Keys)
				{
					Value submatch, subleftover;
					if (patmap.ContainsKey(key) && Do(inmap[key], patmap[key], bShortPat, out submatch, out subleftover))
						matchmap[key] = submatch;
					else if (restMap != null)
						restMap[key] = inmap[key];
					else
						return false;	// input has key that pattern doesn't
				}

			// leftover
			if (patcount > incount)
			{
				Map leftovermap = new Map();
				foreach (string key in patmap.Raw.Keys)
				{
					PatternData data = new PatternData(patmap[key] as ValueString);
					if (!matchmap.ContainsKey(key) && data.Default != null)
						matchmap[key] = data.Default;
					else if (key != restKey && !inmap.ContainsKey(key))
						leftovermap[key] = patmap[key];
				}
				if (leftovermap.Count > 0)
					leftover = new ValueMap(leftovermap);
			}

			// matches + defaults
			match = new ValueMap(matchmap);

			return true;
		}


		/// <summary>Match a map against an array of keys</summary>
		private static bool MatchMapAgainstKeys(ValueMap input, ValueArray pattern, bool bShortPat, out Value match, out Value leftover)
		{
			match = null;
			leftover = null;
			if (pattern == null)
				return false;

			Map inmap = input.AsMap;
			List<Value> patarray = pattern.AsArray;
			int incount = inmap.Count;
			int patcount = patarray.Count;
			if (!bShortPat && patcount < incount)
				return false;	// pattern has fewer elements than input - not a match

			Map matchmap = new Map();
			// if an item in pattern wants rest of map, create housing for it
			Map restMap = null;
			Value lastPat = pattern.AsArray[patcount - 1];
			if (lastPat.Metadata != null && lastPat.Metadata.ContainsKey(PatternData.keyRest))
			{
				string restKey = lastPat.AsString;
				if (restKey != null)
				{
					restMap = new Map();
					matchmap[restKey] = new ValueMap(restMap);
				}
			}

			// matches
			Map leftovermap = new Map();
			int mismatchCount = 0;
			foreach (Value key in patarray)
			{
				Value submatch, subleftover;
				string keyString = key.AsString;
				if (inmap.ContainsKey(keyString) &&
					Do(inmap[keyString], key, bShortPat, out submatch, out subleftover))
				{
					matchmap[keyString] = submatch;
				}
				else if (restMap == null)
				{	// doesn't have key or it's a mismatch
					PatternData data = new PatternData(key as ValueString);
					if (data.Default != null)
						matchmap[keyString] = data.Default;
					else
						leftovermap[keyString] = key;
				}
				else
				{
					mismatchCount++;
				}
			}

			if (matchmap.Count == 0)
				return false;

			match = new ValueMap(matchmap);
			if (restMap != null && incount > patcount)
			{	// copy across remainder of input
				foreach (string inkey in inmap.Raw.Keys)
					if (!matchmap.ContainsKey(inkey))
						restMap[inkey] = inmap[inkey];
				mismatchCount--;
			}
			if (leftovermap.Count > 0)
				leftover = new ValueMap(leftovermap);

			return mismatchCount == 0;
		}

		/// <summary>
		/// If anything in map should get the rest of the map, return it
		/// </summary>
		private static string FindRestKey(Map map)
		{
			foreach (string key in map.Raw.Keys)
			{
				Value value = map[key];
				Map meta = value.Metadata;
				if (meta != null && meta.ContainsKey(PatternData.keyRest))
					return key;
			}
			return null;
		}
	}
}
