namespace loki3.test
{
    using System;
    using System.IO;
    using System.Reflection;
    using loki3.core;
    using CoreEvalFile = loki3.core.EvalFile;

    internal static class TestHelper
    {
        private static readonly string s_assemblyDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

        internal static void EvalFile(string fileName, IScope scope)
        {
            SetTestPath();
            CoreEvalFile.Do(fileName, scope);
        }

        /// <summary>
        /// Point the current directory to the location that allows loki3
        /// test files to be found under 'l3'
        /// </summary>
        internal static void SetTestPath()
        {
            // hardcoded assumption that tests are being run from bin\Debug
            var path = s_assemblyDir.Substring(0, s_assemblyDir.Length - "bin\\Debug\\".Length);
            Directory.SetCurrentDirectory(path);
        }
    }
}
