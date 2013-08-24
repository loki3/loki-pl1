using System.Collections.Generic;

namespace loki3.core
{
	/// <summary>
	/// Store multiple functions in order from most to least specific overload.
	/// When trying to eval, look for first full match.  If none, look for
	/// first partial match.
	/// </summary>
	internal class ValueFunctionOverload : ValueFunction
	{
		internal ValueFunctionOverload()
		{
			WritableMetadata[keyIsOverload] = new ValueBool(true);
		}
		internal ValueFunctionOverload(ValueFunction function)
		{
			WritableMetadata[keyIsOverload] = new ValueBool(true);
			Add(function);
		}

		/// <summary>Add a new overload to the list</summary>
		internal void Add(ValueFunction function)
		{
			if (m_functions.AsArray.Count == 0)
			{	// first function decides if it's pre/in/postfix
				m_bConsumesPrevious = function.ConsumesPrevious;
				m_bConsumesNext = function.ConsumesNext;
				m_bRequiresBody = function.RequiresBody();
				// copy all metadata except parameters from single function to overload
				foreach (string key in function.Metadata.Raw.Keys)
					if (key != ValueFunction.keyPreviousPattern && key != ValueFunction.keyNextPattern)
						WritableMetadata[key] = function.Metadata[key];
				AddFunction(function);
			}
			else
			{	// other functions must match fix
				if (m_bConsumesPrevious != function.ConsumesPrevious
					|| m_bConsumesNext != function.ConsumesNext)
				{
					string expected = GetFix(m_bConsumesPrevious, m_bConsumesNext);
					string actual = GetFix(function.ConsumesPrevious, function.ConsumesNext);
					throw new Loki3Exception().AddWrongFix(expected, actual);
				}
				// other function must have same body requirement
				if (m_bRequiresBody != function.RequiresBody())
					throw new Loki3Exception().AddMissingBody(function);
				// copy all metadata from overload to new function
				foreach (string key in Metadata.Raw.Keys)
					function.WritableMetadata[key] = Metadata[key];
				AddFunction(function);
			}
		}

		/// <summary>Examine overloads to figure out which function to eval</summary>
		internal override Value Eval(DelimiterNode prev, DelimiterNode next, IScope paramScope, IScope bodyScope, INodeRequestor nodes, ILineRequestor requestor)
		{
			// if we've got a single function, simply use it
			List<Value> functions = m_functions.AsArray;
			if (m_functions.Count == 1)
			{
				ValueFunction single = functions[0] as ValueFunction;
				return single.Eval(prev, next, paramScope, bodyScope, nodes, requestor);
			}

			// if they didn't pass any parameters, then we eval as ourselves
			// todo: if this is infix & one param is passed, bind it to create a partial
			// (note: we could also try to create partials based on the contents of prev or next)
			if ((m_bConsumesPrevious && prev == null) || (m_bConsumesNext && next == null))
				return this;

			// todo: avoid evaling twice (or is it cached behind the scenes?)
			// and pattern matching twice for the successfull function
			Value value1 = (m_bConsumesPrevious ? EvalNode.Do(prev, paramScope, nodes, requestor) : null);
			Value value2 = (m_bConsumesNext ? EvalNode.Do(next, paramScope, nodes, requestor) : null);

			// eval the first function that's a full match,
			// else eval the first function that was a match with leftover,
			// else fail
			ValueFunction best = null;
			foreach (Value value in functions)
			{
				ValueFunction function = value as ValueFunction;
				Value match1 = null, match2 = null;
				Value leftover1 = null, leftover2 = null;

				// check parameters
				if (function.ConsumesPrevious)
					if (!PatternChecker.Do(value1, function.Metadata[ValueFunction.keyPreviousPattern], false/*bShortPat*/, out match1, out leftover1))
						continue;
				if (function.ConsumesNext)
					if (!PatternChecker.Do(value2, function.Metadata[ValueFunction.keyNextPattern], false/*bShortPat*/, out match2, out leftover2))
						continue;

				// if no leftover, we found our function
				if (leftover1 == null && leftover2 == null)
				{
					best = function;
					break;
				}
				// if this is the first function w/ leftover, we'll eval it if we don't find a later match
				if (best == null)
					best = function;
			}

			if (best == null)
				// todo: NoMatches
				throw new Loki3Exception();
			// eval the best match we found
			return best.Eval(prev, next, paramScope, bodyScope, nodes, requestor);
		}

		#region	ValueFunction
		internal override bool ConsumesPrevious { get { return m_bConsumesPrevious; } }
		internal override bool ConsumesNext { get { return m_bConsumesNext; } }
		internal override bool RequiresBody() { return m_bRequiresBody; }
		#endregion

		#region Keys
		internal static string keyIsOverload = "l3.func.overload?";
		internal static string keyOverloadLevel = "l3.func.overloadLevel";
		#endregion


		#region Value
		internal override bool Equals(Value v)
		{
			ValueFunction other = v as ValueFunction;
			return (other == null ? false : this == other);
		}

		internal override Value ValueCopy()
		{
			ValueFunctionOverload copy = new ValueFunctionOverload();
			List<Value> array = new List<Value>();
			foreach (Value f in m_functions.AsArray)
				array.Add(f);
			copy.m_functions = new ValueArray(array);
			copy.m_bConsumesPrevious = m_bConsumesPrevious;
			copy.m_bConsumesNext = m_bConsumesNext;
			return copy;
		}

		internal override int Count { get { return m_functions.Count; } }
		#endregion

		internal ValueFunction GetFunction(int i) { return m_functions.AsArray[i] as ValueFunction; }


		/// <summary>Figure out how specific overload is and add to sorted list</summary>
		private void AddFunction(ValueFunction function)
		{
			// calc the overload level of this function based on the params it expects,
			// store the level on the function
			int countParams = 0;
			int level = 0;
			if (function.ConsumesPrevious)
			{
				Value pattern = function.Metadata[ValueFunction.keyPreviousPattern];
				countParams += pattern.Count;
				level += CalcLevel(pattern);
			}
			if (function.ConsumesNext)
			{
				Value pattern = function.Metadata[ValueFunction.keyNextPattern];
				countParams += pattern.Count;
				level += CalcLevel(pattern);
			}
			level += PointsPerParam * countParams * (countParams - 1) / 2;
			function.WritableMetadata[keyOverloadLevel] = new ValueInt(level);

			// list is sorted from largest to smallest overload levels
			List<Value> list = m_functions.AsArray;
			int count = list.Count;
			bool bAdded = false;
			// todo: make this a binary search
			for (int i = 0; i < count; i++)
			{
				Value currentV = list[i];
				int currentLevel = currentV.Metadata[keyOverloadLevel].AsInt;
				if (level > currentLevel)
				{
					list.Insert(i, function);
					bAdded = true;
					break;
				}
			}
			if (!bAdded)
				list.Add(function);
		}

		/// <summary>
		/// Figure out where this overload fits relative to other overloads
		/// based on the number of parameters it takes and how type-specific
		/// they are.  Higher points mean overload should take precedence.
		/// </summary>
		/// <param name="pattern">pattern to evaluate</param>
		private int CalcLevel(Value pattern)
		{
			// n params always gets more points than n-1,
			// bonus points for type-specific params
			int level = 0;
			if (pattern is ValueArray)
			{
				List<Value> array = pattern.AsArray;
				foreach (Value value in array)
					level += CalcPoints(value);
			}
			else if (pattern is ValueMap)
			{
				Map map = pattern.AsMap;
				foreach (Value value in map.Raw.Values)
					level += CalcPoints(value);
			}
			else
			{	// matching against a single value
				level += CalcPoints(pattern);
			}
			return level;
		}

		/// <summary>
		/// Score a parameter based on type annotations, etc.
		/// </summary>
		private int CalcPoints(Value value)
		{
			ValueString pattern = value as ValueString;
			if (pattern == null)
				return 0;
			PatternData data = new PatternData(pattern);
			string theType = data.Type;
			if (theType == ValueClasses.ClassOf(ValueType.Nil))
				return 0;
			else if (theType == ValueClasses.ClassOf(ValueType.Number))
				return 1;
			return 2;
		}

		private string GetFix(bool pre, bool post)
		{
			if (pre)
				return (post ? "infix" : "postfix");
			return (post ? "prefix" : "nofix");
		}

		/// <summary>The range of points a param can have</summary>
		static private int PointsPerParam = 3;

		private ValueArray m_functions = new ValueArray(new List<Value>());
		private bool m_bConsumesPrevious = false;
		private bool m_bConsumesNext = false;
		private bool m_bRequiresBody = false;
	}
}
