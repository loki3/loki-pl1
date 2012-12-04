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
			scope.SetValue("l3.intToChar", new IntToChar());
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

		/// <summary>:a -> convert an int to a character in a string</summary>
		class IntToChar : ValueFunctionPre
		{
			internal override Value ValueCopy() { return new IntToChar(); }

			internal IntToChar()
			{
				SetDocString("Convert an int to a character in a string.");
				Init(PatternData.Single("a", ValueType.Int));
			}

			internal override Value Eval(Value arg, IScope scope)
			{
				int a = arg.AsInt;
				if (a == 13)
					return new ValueString("\n");
				return new ValueString(System.Char.ConvertFromUtf32(a));
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

		/// <summary>{ :array :columns [:dashesAfterFirst?] [:spaces] } -> a string that's the array formatted w/ the given number of columns</summary>
		class FormatTable : ValueFunctionPre
		{
			internal override Value ValueCopy() { return new FormatTable(); }

			internal FormatTable()
			{
				SetDocString("Creates a string that's the array formatted w/ the given number of columns.");

				Map map = new Map();
				map["array"] = PatternData.Single("array", ValueType.Array);
				map["columns"] = PatternData.Single("columns", ValueType.Int);
				map["dashesAfterFirst?"] = PatternData.Single("dashesAfterFirst?", ValueType.Bool, ValueBool.False);
				map["spaces"] = PatternData.Single("spaces", ValueType.Int, new ValueInt(1));
				ValueMap vMap = new ValueMap(map);
				Init(vMap);
			}

			internal override Value Eval(Value arg, IScope scope)
			{
				// extract parameters
				Map map = arg.AsMap;
				List<Value> array = map["array"].AsArray;
				int columns = map["columns"].AsInt;
				bool dashesAfterFirst = map["dashesAfterFirst?"].AsBool;
				int spaces = map["spaces"].AsInt;

				// first figure out how wide each column should be
				List<int> widths = new List<int>();
				for (int i = 0; i < columns; i++)
					widths.Add(0);
				List<string> cache = new List<string>();
				int iColumn = 0;
				foreach (Value line in array)
				{
					string s = line.ToString();
					cache.Add(s);

					int len = s.Length;
					if (len > widths[iColumn])
						widths[iColumn] = len;

					iColumn = (iColumn+1) % columns;
				}

				// second build up entire table using calced widths
				string table = "";
				bool bFirstLine = true;
				int lineWidth = 0;
				iColumn = 0;
				foreach (string line in cache)
				{
					int len = line.Length;
					int width = widths[iColumn] + spaces;
					lineWidth += width;

					table += line;
					// don't tack on trailing spaces for last column
					if (iColumn < columns - 1)
						table += new string(' ', width - len);

					iColumn = (iColumn + 1) % columns;

					// add a row of dashes after first line if needed
					if (iColumn == 0 && bFirstLine && dashesAfterFirst)
					{
						table += "\n";
						table += new string('-', lineWidth - spaces);
						table += "\n";
						dashesAfterFirst = false;
					}
					else if (iColumn == 0)
					{
						bFirstLine = false;
						table += "\n";
						lineWidth = 0;
					}
				}

				return new ValueString(table);
			}
		}
	}
}
