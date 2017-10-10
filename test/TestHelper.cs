namespace loki3.test
{
    using System.IO;
    using System.Reflection;
    using loki3.core;
    using CoreEvalFile = loki3.core.EvalFile;

    internal static class TestHelper
    {
        private static readonly string s_assemblyDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

        internal static void EvalFile(string fileName, IScope scope)
        {
            CoreEvalFile.Do(
                MakeTestSourcePath(fileName),
                scope);
        }

        internal static string MakeTestSourcePath(string fileName)
        {
            return Path.Combine(s_assemblyDir, fileName);
        }
    }
}
