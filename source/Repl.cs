using System;
using System.Collections.Generic;

namespace loki3.core
{
	class Repl
	{
		/// <summary>
		/// Start a read-eval-print loop at the console
		/// </summary>
		/// <param name="scope">scope for parse-eval</param>
		static internal void Do(IScope scope, string prompt)
		{
			prompt += " ";
			string s = "";
			do
			{
				Console.Write(prompt);

				// keep reading lines as long as they end with \
				List<string> lines = new List<string>();
				bool bMore = false;
				do
				{
					s = Console.ReadLine();
					bMore = (s.Length > 2 && s[s.Length - 1] == '\\' && s[s.Length - 2] == ' ');
					if (bMore)
						s = s.Substring(0, s.Length - 2);
					lines.Add(s);
				} while (bMore);
				LineConsumer consumer = new LineConsumer(lines);

				// eval the line(s)
				try
				{
					Value v = EvalLines.Do(consumer, scope);
					Console.WriteLine(v.ToString());
				}
				catch (Loki3Exception error)
				{
					// if we're at the root scope, remove it to avoid infinite
					// recursion in Map.ToString
					if (error.Errors.ContainsKey("l3.error.scope"))
						if (error.Errors["l3.error.scope"].AsMap == scope.AsMap)
							error.Errors.Raw.Remove("l3.error.scope");

					if (scope.Exists("prettify") != null)
					{
						scope.SetValue("lastError", new ValueMap(error.Errors));
						Value v = loki3.builtin.test.TestSupport.ToValue("prettify lastError", scope);
						Console.WriteLine("LOKI3 ERROR:\n" + v.AsString);
						if (error.Errors.ContainsKey(Loki3Exception.keyScope))
						{
							scope.SetValue("lastScope", error.Errors[Loki3Exception.keyScope]);
							v = loki3.builtin.test.TestSupport.ToValue("dumpStack lastScope", scope);
							Console.WriteLine("STACK:\n" + v.AsString);
						}
					}
					else
					{
						Console.WriteLine(error.ToString());
					}
				}
				catch (Exception error)
				{
					Console.WriteLine("INTERNAL ERROR: " + error.ToString());
				}
			} while (s != "");
		}
	}
}
