using System.Collections.Generic;
using loki3.core;
using NUnit.Framework;

namespace loki3.builtin.test
{
	[TestFixture]
	class TEST_Module
	{
		static IScope CreateModuleScope()
		{
			ScopeChain scope = new ScopeChain();
			Module.Register(scope);
			Values.Register(scope);

			scope.SetValue("'", new ValueDelimiter("'", DelimiterType.AsString));
			scope.SetValue("{", new ValueDelimiter("}", DelimiterType.AsArray, new CreateMap()));

			return scope;
		}

		[Test]
		public void TestEquals()
		{
			IScope scope = CreateModuleScope();

			{
				Value value = TestSupport.ToValue("l3.loadModule { :file ' doesnt exist ' }", scope);
				Assert.False(value.AsBool);
			}

			{
				int pre = scope.AsMap.Raw.Keys.Count;
				Value value = TestSupport.ToValue("l3.loadModule { :file ' ../../l3/module.l3 ' }", scope);
				Assert.True(value.AsBool);
				int post = scope.AsMap.Raw.Keys.Count;
				Assert.AreEqual(pre + 1, post);	// module has a single new var in it
			}
		}
	}
}
