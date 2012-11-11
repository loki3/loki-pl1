using loki3.core;
using NUnit.Framework;

namespace loki3.builtin.test
{
	[TestFixture]
	class TEST_TestCode
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
				ScopeChain scope = new ScopeChain();
				AllBuiltins.RegisterAll(scope);
				EvalFile.Do("../../l3/bootstrap.l3", scope);
				EvalFile.Do("../../l3/test.l3", scope);

				{	// different ways of doing fizzbuzz
					string str20 = "[ 1 2 \"fizz\" 4 \"buzz\" \"fizz\" 7 8 \"fizz\" \"buzz\" 11 \"fizz\" 13 14 \"fizzbuzz\" 16 17 \"fizz\" 19 \"buzz\" ]";

					Value a = scope.GetValue(new Token("fizzBuzz1"));
					Assert.AreEqual(str20, a.ToString());

					Value b = scope.GetValue(new Token("fizzBuzz2"));
					Assert.AreEqual(str20, b.ToString());
				}
			}
			catch (Loki3Exception e)
			{
				Assert.Fail(e.ToString());
			}
		}
	}
}
