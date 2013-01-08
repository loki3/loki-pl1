using System;

namespace loki3.core
{
	/// <summary>
	/// Base exception class for all parsing and evaluation errors.
	/// Details are stored in a map, allowing each level of eval
	/// to tack on additional context as error bubbles up.
	/// </summary>
	internal class Loki3Exception : Exception
	{
		internal Loki3Exception() { m_map = new Map(); }
		internal Loki3Exception(Map map) { m_map = map; }

		/// <summary>A start delimiter was found without an end delimiter</summary>
		internal Loki3Exception AddMissingEndDelimiter(ValueDelimiter delim)
		{
			m_map[keyMissingEndDelimiter] = delim;
			return this;
		}

		/// <summary>A function required a body but a body wasn't provided</summary>
		internal Loki3Exception AddMissingBody(ValueFunction function)
		{
			m_map[keyMissingBody] = function;
			return this;
		}

		/// <summary>A map that was required to have a key</summary>
		internal Loki3Exception AddMissingKey(Value key)
		{
			m_map[keyMissingKey] = key;
			return this;
		}

		/// <summary>A function was expecting a previous or next value</summary>
		internal Loki3Exception AddMissingValue(bool bPrevious)
		{
			m_map[keyMissingValue] = new ValueString(bPrevious ? "previous" : "next");
			return this;
		}

		/// <summary>Could not parse token</summary>
		internal Loki3Exception AddBadToken(Token token)
		{
			m_map[keyBadToken] = new ValueString(token.Value);
			return this;
		}

		/// <summary>Couldn't eval line in a body</summary>
		internal Loki3Exception AddBadLine(Value v)
		{
			m_map[keyBadLine] = v;
			return this;
		}

		/// <summary>Value wasn't of requested type</summary>
		internal Loki3Exception AddWrongType(ValueType expected, ValueType actual)
		{
			m_map[keyExpectedType] = new ValueString(ValueClasses.ClassOf(expected));
			m_map[keyActualType] = new ValueString(ValueClasses.ClassOf(actual));
			return this;
		}

		/// <summary>The passed-in value didn't match the expected pattern</summary>
		internal Loki3Exception AddWrongPattern(Value expected, Value actual)
		{
			m_map[keyExpectedPattern] = expected;
			m_map[keyActualPattern] = actual;
			return this;
		}

		/// <summary>Function overload is of a different "fix" type</summary>
		internal Loki3Exception AddWrongFix(string expected, string actual)
		{
			m_map[keyExpectedFix] = new ValueString(expected);
			m_map[keyActualFix] = new ValueString(actual);
			return this;
		}


		/// <summary>Tack on the function where the error occurred</summary>
		internal Loki3Exception AddFunction(string function)
		{
			m_map[keyFunction] = new ValueString(function);
			return this;
		}

		/// <summary>Tack on the line number where the error occurred</summary>
		internal Loki3Exception AddLineNumber(int line)
		{
			if (!m_map.ContainsKey(keyLineNumber))
				m_map[keyLineNumber] = new ValueInt(line);
			return this;
		}

		/// <summary>Tack on the line where the error occurred</summary>
		internal Loki3Exception AddLine(string line)
		{
			m_map[keyLineContents] = new ValueString(line);
			return this;
		}

		/// <summary>Tack on the file where the error occurred</summary>
		internal Loki3Exception AddFileName(string file)
		{
			m_map[keyFileName] = new ValueString(file);
			return this;
		}

		/// <summary>Tack on the file where the error occurred</summary>
		internal Loki3Exception AddScope(IScope scope)
		{
			m_map[keyScope] = scope.AsValue;
			return this;
		}


		/// <summary>Get collection of all errors</summary>
		internal Map Errors { get { return m_map; } }

		internal string ExpectedType { get { return m_map.GetOptionalT<string>(keyExpectedType, ""); } }
		internal string ActualType { get { return m_map.GetOptionalT<string>(keyActualType, ""); } }

		public override string ToString()
		{
			return m_map.ToString();
		}

		#region Keys
		/// <summary>Delimiter that was missing ending</summary>
		internal static string keyMissingEndDelimiter = "l3.error.missingEndDelimiter";
		/// <summary>Function that needed a body</summary>
		internal static string keyMissingBody = "l3.error.missingBody";
		/// <summary>Map that was required to have a key</summary>
		internal static string keyMissingKey = "l3.error.missingKey";
		/// <summary>Function needed a previous or next value</summary>
		internal static string keyMissingValue = "l3.error.missingValue";
		/// <summary>Token that couldn't be parsed</summary>
		internal static string keyBadToken = "l3.error.badToken";
		/// <summary>Bad line in a body</summary>
		internal static string keyBadLine = "l3.error.badLine";
		/// <summary>Expected type</summary>
		internal static string keyExpectedType = "l3.error.expectedType";
		/// <summary>Actual type</summary>
		internal static string keyActualType = "l3.error.actualType";
		/// <summary>Expected pattern</summary>
		internal static string keyExpectedPattern = "l3.error.expectedPattern";
		/// <summary>Actual pattern</summary>
		internal static string keyActualPattern = "l3.error.actualPattern";
		/// <summary>Overloaded function was expected to be in/post/prefix</summary>
		internal static string keyExpectedFix = "l3.error.expectedFix";
		/// <summary>Overloaded function was actually in/post/prefix</summary>
		internal static string keyActualFix = "l3.error.actualFix";

		/// <summary>Function where error occurred</summary>
		internal static string keyFunction = "l3.error.function";
		/// <summary>Line number where error occurred</summary>
		internal static string keyLineNumber = "l3.error.lineNumber";
		/// <summary>Line where error occurred</summary>
		internal static string keyLineContents = "l3.error.lineContents";
		/// <summary>File where error occurred</summary>
		internal static string keyFileName = "l3.error.fileName";
		/// <summary>Current stack when error occurred</summary>
		internal static string keyScope = "l3.error.scope";
		#endregion

		/// <summary>Named scope that can catch exceptions</summary>
		internal static string catchScopeName = "l3.catch";
		/// <summary>Key where current exception is stored</summary>
		internal static string exceptionKey = "l3.exception";

		private Map m_map;
	}


	/// <summary>
	/// Used to pop the stack back to a named scope
	/// </summary>
	internal class PopStackException : Exception
	{
		internal PopStackException(string name, Value returnValue)
		{
			m_name = name;
			m_return = returnValue;
		}

		/// <summary>Name of scope to pop back to</summary>
		internal string ScopeName { get { return m_name; } }
		/// <summary>Return value of function at named scope</summary>
		internal Value Return { get { return m_return; } }

		private string m_name;
		private Value m_return;
	}
}
