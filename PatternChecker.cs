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

			switch (input.Type)
			{
				case ValueType.Nil:
					return DoSingle(input as ValueNil, ValueType.Nil, pattern as ValueString, out match);

				case ValueType.Bool:
					return DoSingle(input as ValueBool, ValueType.Bool, pattern as ValueString, out match);
				case ValueType.Int:
					return DoSingle(input as ValueInt, ValueType.Int, pattern as ValueString, out match);
				case ValueType.Float:
					return DoSingle(input as ValueFloat, ValueType.Float, pattern as ValueString, out match);
				case ValueType.String:
					return DoSingle(input as ValueString, ValueType.String, pattern as ValueString, out match);

				case ValueType.Array:
					return Do(input as ValueArray, pattern, bShortPat, out match, out leftover);
				case ValueType.Map:
					if (Do(input as ValueMap, pattern as ValueMap, bShortPat, out match, out leftover))
						return true;
					if (Do(input as ValueMap, pattern as ValueArray, bShortPat, out match, out leftover))
						return true;
					return DoSingle(input as ValueMap, ValueType.Map, pattern as ValueString, out match);

				case ValueType.Function:
					return DoSingle(input as ValueFunction, ValueType.Function, pattern as ValueString, out match);
				case ValueType.Delimiter:
					return DoSingle(input as ValueDelimiter, ValueType.Delimiter, pattern as ValueString, out match);
				case ValueType.Raw:
					return DoSingle(input as ValueRaw, ValueType.Raw, pattern as ValueString, out match);
			}

			return false;
		}

		/// <summary>Match against a non-aggregate type</summary>
		private static bool DoSingle<T>(T input, ValueType target, ValueString pattern, out Value match) where T : Value
		{
			match = null;
			if (pattern == null)
				return false;	// this only matches against a single item
			PatternData data = new PatternData(pattern);
			ValueType type = data.ValueType;
			if (type != ValueType.Nil && type != target)
			{
				if (type != ValueType.Number || (target != ValueType.Int && target != ValueType.Float))
					return false;	// they asked for a different type
			}
			match = input;
			return true;
		}

		/// <summary>Match against an array</summary>
		private static bool Do(ValueArray input, Value pattern, bool bShortPat, out Value match, out Value leftover)
		{
			match = null;
			leftover = null;

			// check if we asked for the entire array
			ValueString patternString = pattern as ValueString;
			if (patternString != null)
			{
				PatternData data = new PatternData(patternString);
				match = input;
				return true;
			}

			ValueArray patternArray = pattern as ValueArray;
			if (patternArray == null)
				return false;	// this only matches against another array

			List<Value> inarray = input.AsArray;
			List<Value> patarray = patternArray.AsArray;
			int incount = inarray.Count;
			int patcount = patarray.Count;
			if (!bShortPat && patcount < incount)
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
				if (leftoverlist.Count > 0)
					leftover = new ValueArray(leftoverlist);
			}

			// matches + defaults
			match = new ValueArray(matchlist);

			return true;
		}

		/// <summary>Match against a map</summary>
		private static bool Do(ValueMap input, ValueMap pattern, bool bShortPat, out Value match, out Value leftover)
		{
			match = null;
			leftover = null;
			if (pattern == null)
				return false;	// this only matches against another map

			Map inmap = input.AsMap;
			Map patmap = pattern.AsMap;
			int incount = inmap.Count;
			int patcount = patmap.Count;
			if (!bShortPat && patcount < incount)
				return false;	// pattern has fewer elements than input - not a match

			// matches
			Map matchmap = new Map();
			if (inmap.Raw != null)
				foreach (string key in inmap.Raw.Keys)
				{
					Value submatch, subleftover;
					if (Do(inmap[key], patmap[key], bShortPat, out submatch, out subleftover))
						matchmap[key] = submatch;
					else if (!bShortPat)
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
					else if (!inmap.ContainsKey(key))
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
		private static bool Do(ValueMap input, ValueArray pattern, bool bShortPat, out Value match, out Value leftover)
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

			// matches
			Map matchmap = new Map();
			Map leftovermap = new Map();
			foreach (Value key in patarray)
			{
				Value submatch, subleftover;
				string keyString = key.AsString;
				if (inmap.ContainsKey(keyString) &&
					Do(inmap[keyString], key, bShortPat, out submatch, out subleftover))
				{
					matchmap[keyString] = submatch;
				}
				else
				{	// doesn't have key or it's a mismatch
					PatternData data = new PatternData(key as ValueString);
					if (data.Default != null)
						matchmap[keyString] = data.Default;
					else
						leftovermap[keyString] = key;
				}
			}

			if (matchmap.Count == 0)
				return false;

			match = new ValueMap(matchmap);
			if (leftovermap.Count > 0)
				leftover = new ValueMap(leftovermap);

			return true;
		}
	}
}
