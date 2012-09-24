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
			ScopeChain scope = new ScopeChain(null);
			Conditional.Register(scope);

			scope.SetValue("[", new ValueDelimiter("[", "]", DelimiterType.AsArray));
			scope.SetValue("'", new ValueDelimiter("'", "'", DelimiterType.AsString));

			return scope;
		}

		static Value ToValue(string s, IScope scope)
		{
			DelimiterList list = ParseLine.Do(s, scope);
			return EvalList.Do(list.Nodes, scope);
		}


		[Test]
		public void TestIf()
		{
			IScope scope = CreateScope();
			Values.Register(scope);

			{	// run body
				Value value = ToValue("l3.if l3.createMap [ :do? true :body [ ' 5 ' ] ]", scope);
				Assert.AreEqual(5, value.AsInt);
			}
			{	// skip body
				Value value = ToValue("l3.if l3.createMap [ :do? false :body [ ' 5 ' ] ]", scope);
				Assert.AreEqual(false, value.AsBool);
			}
		}
	}
}
