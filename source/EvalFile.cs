using System.IO;
using System.Collections.Generic;

namespace loki3.core
{
	internal class EvalFile
	{
		/// <summary>Read and eval all the lines in a file</summary>
		internal static void Do(string file, IScope scope)
		{
			StreamReader stream = null;
			try
			{
				List<string> lines = new List<string>();
				stream = new StreamReader(file);
				while (!stream.EndOfStream)
					lines.Add(stream.ReadLine());

				LineConsumer consumer = new LineConsumer(lines);
				EvalLines.Do(consumer, scope);
				stream.Close();
			}
			catch (Loki3Exception e)
			{
				e.AddFileName(file);
				if (stream != null)
					stream.Close();
				throw e;
			}
			catch (System.Exception e)
			{
				if (stream != null)
					stream.Close();
				throw e;
			}
		}
	}
}
