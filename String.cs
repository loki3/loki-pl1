using System.Collections.Generic;
using loki3.core;

namespace loki3.builtin
{
	/// <summary>
	/// Built-in string functions
	/// </summary>
	class String
	{
		/// <summary>
		/// Add built-in functions to the scope
		/// </summary>
		internal static void Register(IScope scope)
		{
			scope.SetValue("l3.toString", new ConvertToString());
			scope.SetValue("l3.stringConcat", new StringConcat());
			scope.SetValue("l3.formatTable", new FormatTable());
		}


		/// <summary>:a -> convert a value to a string</summary>
		class ConvertToString : ValueFunctionPre
		{
			internal override Value ValueCopy() { return new ConvertToString(); }

			internal ConvertToString()
			{
				SetDocString("Convert a value to a string.");
				Init(PatternData.Single("a"));
			}

			internal override Value Eval(Value arg, IScope scope)
			{
				return new ValueString(arg.ToString());
			}
		}

		/// <summary>{ :array [:spaces] } -> convert a series of values to strings and concatenate them</summary>
		class StringConcat : ValueFunctionPre
		{
			internal override Value ValueCopy() { return new StringConcat(); }

			internal StringConcat()
			{
				SetDocString("Concatenate two strings.");

				Map map = new Map();
				map["array"] = PatternData.Rest("array");
				map["spaces"] = PatternData.Single("spaces", ValueType.Int, new ValueInt(0));
				ValueMap vMap = new ValueMap(map);
				Init(vMap);
			}

			internal override Value Eval(Value arg, IScope scope)
			{
				Map map = arg.AsMap;
				List<Value> list = map["array"].AsArray;
				int spaces = map["spaces"].AsInt;

				string padding = (spaces == 0 ? null : new string(' ', spaces));

				string s = "";
				bool bFirst = true;
				foreach (Value val in list)
				{
					if (bFirst)
						bFirst = false;
					else if (padding != null)
						s += padding;
					s += val.ToString();
				}
				return new ValueString(s);
			}
		}

		/// <summary>{ :arrayOfArrays [:dashesAfterFirst?] [:spaces] } -> a string that's a formatted table of the arrays of arrays</summary>
		class FormatTable : ValueFunctionPre
		{
			internal override Value ValueCopy() { return new FormatTable(); }

			internal FormatTable()
			{
				SetDocString("Creates a string that's a formatted table of the arrays of arrays.");

				Map map = new Map();
				map["arrayOfArrays"] = PatternData.Single("arrayOfArrays", ValueType.Array);
				map["dashesAfterFirst?"] = PatternData.Single("dashesAfterFirst?", ValueType.Bool, ValueBool.False);
				map["spaces"] = PatternData.Single("spaces", ValueType.Int, new ValueInt(1));
				ValueMap vMap = new ValueMap(map);
				Init(vMap);
			}

			internal override Value Eval(Value arg, IScope scope)
			{
				// extract parameters
				Map map = arg.AsMap;
				List<Value> array = map["arrayOfArrays"].AsArray;
				bool dashesAfterFirst = map["dashesAfterFirst?"].AsBool;
				int spaces = map["spaces"].AsInt;

				// first figure out how wide each column should be
				List<int> widths = new List<int>();
				List<List<string>> cache = new List<List<string>>();
				foreach (Value line in array)
				{
					List<Value> lineArray = line.AsArray;
					List<string> lineCache = new List<string>();
					int iColumn = 0;
					foreach (Value val in lineArray)
					{
						string s = val.ToString();
						lineCache.Add(s);
						int len = s.Length;
						if (widths.Count < iColumn + 1)
							widths.Add(len);
						else if (len > widths[iColumn])
							widths[iColumn] = len;
						iColumn++;
					}
					cache.Add(lineCache);
				}

				// second build up entire table using calced widths
				string table = "";
				bool bFirstLine = true;
				foreach (List<string> lineCache in cache)
				{
					if (bFirstLine)
						bFirstLine = false;
					else
						table += "\n";

					int totalWidth = 0;
					int nColumns = lineCache.Count;
					int iColumn = 0;
					foreach (string s in lineCache)
					{
						int len = s.Length;
						int width = widths[iColumn] + spaces;
						totalWidth += width;

						table += s;
						// don't tack on trailing spaces for last column
						if (iColumn < nColumns - 1)
							table += new string(' ', width - len);

						iColumn++;
					}

					// add a row of dashes if needed
					if (dashesAfterFirst)
					{
						table += "\n";
						table += new string('-', totalWidth - spaces);
						dashesAfterFirst = false;
					}
				}

				return new ValueString(table);
			}
		}
	}
}
