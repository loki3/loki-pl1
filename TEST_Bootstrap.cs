using loki3.core;
using NUnit.Framework;

namespace loki3.builtin.test
{
	[TestFixture]
	class TEST_Bootstrap
	{
		[Test]
		public void Test()
		{
			ScopeChain scope = new ScopeChain(null);
			AllBuiltins.RegisterAll(scope);
			EvalFile.Do("../../l3/bootstrap.l3", scope);
		}
	}
}
