using System;
using System.Collections.Generic;
using loki3.core;
using NUnit.Framework;

namespace loki3.builtin.test
{
	[TestFixture]
	class TEST_Conditional
	{
		static IScope CreateScope()
		{
			ScopeChain scope = new ScopeChain();
			Conditional.Register(scope);

			scope.SetValue("[", new ValueDelimiter("[", "]", DelimiterType.AsArray));
			scope.SetValue("'", new ValueDelimiter("'", "'", DelimiterType.AsString));

			return scope;
		}


		[Test]
		public void TestIf()
		{
			IScope scope = CreateScope();
			Values.Register(scope);

			{	// run body
				Value value = TestSupport.ToValue("l3.ifBody l3.createMap [ :do? true :body [ ' 5 ' ] ]", scope);
				Assert.AreEqual(5, value.AsInt);
			}
			{	// skip body
				Value value = TestSupport.ToValue("l3.ifBody l3.createMap [ :do? false :body [ ' 5 ' ] ]", scope);
				Assert.AreEqual(false, value.AsBool);
			}
		}
	}
}
