using System.Collections.Generic;
using loki3.core;
using NUnit.Framework;

namespace loki3.builtin.test
{
	[TestFixture]
	class TEST_String
	{
		static IScope CreateStringScope()
		{
			ScopeChain scope = new ScopeChain();
			String.Register(scope);

			ValueDelimiter square = new ValueDelimiter("[", "]", DelimiterType.AsArray);
			scope.SetValue("[", square);
			ValueDelimiter str = new ValueDelimiter("\"", "\"", DelimiterType.AsString);
			scope.SetValue("\"", str);

			return scope;
		}

		[Test]
		public void TestConcat()
		{
			IScope scope = CreateStringScope();
			{
				Value value = TestSupport.ToValue("l3.stringConcat [ \" one \" \" two \" ]", scope);
				Assert.AreEqual("onetwo", value.AsString);
			}
		}
	}
}
