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

			scope.SetValue("[", new ValueDelimiter("]", DelimiterType.AsArray));
			scope.SetValue("'", new ValueDelimiter("'", DelimiterType.AsString));

			return scope;
		}


		[Test]
		public void TestIf()
		{
			IScope scope = CreateScope();
			Values.Register(scope);

			{	// run body
				Value value = TestSupport.ToValue("l3.ifBody l3.createMap [ :do? true :body [ ' 5 ' ] ]", scope);
                var array = value.AsArray;
				Assert.AreEqual(5, array[0].AsInt);
				Assert.AreEqual(true, array[1].AsBool);
			}
			{	// skip body
				Value value = TestSupport.ToValue("l3.ifBody l3.createMap [ :do? false :body [ ' 5 ' ] ]", scope);
                var array = value.AsArray;
                Assert.IsFalse(array[0].AsBool);
                Assert.IsFalse(array[1].AsBool);
            }
        }
	}
}
