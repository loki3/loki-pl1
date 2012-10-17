using loki3.core;
using NUnit.Framework;

namespace loki3.builtin.test
{
	[TestFixture]
	class TEST_Bootstrap
	{
		static Value ToValue(string s, IScope scope)
		{
			DelimiterList list = ParseLine.Do(s, scope);
			return EvalList.Do(list.Nodes, scope);
		}

		[Test]
		public void Test()
		{
			try
			{
				ScopeChain scope = new ScopeChain(null);
				AllBuiltins.RegisterAll(scope);
				EvalFile.Do("../../l3/bootstrap.l3", scope);

				{	// test setting a value and adding metadata to it
					Value a = ToValue(":a = 3", scope);
					ToValue(":a @ [ :tag :hello ]", scope);
					Assert.AreEqual("hello", a.Metadata["tag"].AsString);
				}

				{	// test modifying the doc string on a function
					Value a = ToValue(":= @doc :testing", scope);
					Assert.AreEqual("testing", scope.AsMap["="].Metadata["l3.value.doc"].AsString);
				}

				{	// test infix addition
					Value value = ToValue("5 + 7", scope);
					Assert.AreEqual(12, value.AsInt);
				}

				{	// test function definition
					Value value = ToValue("sqrt 4", scope);
					Assert.AreEqual(2, value.AsFloat);
				}
			}
			catch (Loki3Exception e)
			{
				Assert.Fail(e.ToString());
			}
		}
	}
}
