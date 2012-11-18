using loki3.core;
using NUnit.Framework;

namespace loki3.builtin.test
{
	[TestFixture]
	class TEST_TestCode
	{
		[Test]
		public void Test()
		{
			try
			{
				ScopeChain scope = new ScopeChain();
				AllBuiltins.RegisterAll(scope);
				EvalFile.Do("../../l3/bootstrap.l3", scope);
				EvalFile.Do("../../l3/unittest.l3", scope);

				// use the loki3 unittest framework to test the code
				Value v = TestSupport.ToValue("unittest [ :../../l3/test.l3 :../../l3/test_tests.l3 ]", scope);
				Assert.True(v.AsBool);
			}
			catch (Loki3Exception e)
			{
				Assert.Fail(e.ToString());
			}
		}
	}
}
