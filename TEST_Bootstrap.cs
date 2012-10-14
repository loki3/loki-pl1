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

#if false
			{
				DelimiterList list = ParseLine.Do("+ [ 5 7 ]", scope, null);
				Value value = EvalList.Do(list.Nodes, scope);
				Assert.AreEqual(12, value.AsInt);
			}

			try
			{
				DelimiterList list = ParseLine.Do("5 + 7", scope, null);
				Value value = EvalList.Do(list.Nodes, scope);
				Assert.AreEqual(12, value.AsInt);
			}
			catch (Loki3Exception e)
			{
				Assert.Fail(e.ToString());
			}
#endif
		}
	}
}
