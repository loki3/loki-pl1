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

		static Value ToValue(string s, IScope scope)
		{
			DelimiterList list = ParseLine.Do(s, scope);
			return EvalList.Do(list.Nodes, scope);
		}

		[Test]
		public void TestConcat()
		{
			IScope scope = CreateStringScope();
			{
				Value value = ToValue("l3.stringConcat [ \" one \" \" two \" ]", scope);
				Assert.AreEqual("onetwo", value.AsString);
			}
		}
	}
}
