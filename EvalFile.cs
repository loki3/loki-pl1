using System.IO;
using System.Collections.Generic;

namespace loki3.core
{
	internal class EvalFile
	{
		/// <summary>Read and eval all the lines in a file</summary>
		internal static void Do(string file, IScope scope)
		{
			try
			{
				List<string> lines = new List<string>();
				StreamReader stream = new StreamReader(file);
				while (!stream.EndOfStream)
					lines.Add(stream.ReadLine());

				LineConsumer consumer = new LineConsumer(lines);
				EvalLines.Do(consumer, scope);
			}
			catch (Loki3Exception e)
			{
				e.AddFileName(file);
				throw e;
			}
		}
	}
}
