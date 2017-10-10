using loki3.core;
using loki3.test;
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
				TestHelper.SetTestPath();

				ScopeChain scope = new ScopeChain();
				AllBuiltins.RegisterAll(scope);
				TestHelper.EvalFile("l3/bootstrap.l3", scope);
				TestHelper.EvalFile("l3/unittest.l3", scope);

				// use the loki3 unittest framework to test the code
				{
					Value v = TestSupport.ToValue("unittest [ :l3/help.l3 :l3/help_tests.l3 ]", scope);
					Assert.True(v.AsBool);
				}
				{
					Value v = TestSupport.ToValue("unittest [ :l3/test.l3 :l3/test_tests.l3 ]", scope);
					Assert.True(v.AsBool);
				}
				{
					Value v = TestSupport.ToValue("runTestFile :l3/pattern_tests.l3", scope);
					Assert.True(v.AsBool);
				}
			}
			catch (Loki3Exception e)
			{
				Assert.Fail(e.ToString());
			}
		}
	}
}
