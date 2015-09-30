using System;
using System.Collections.Generic;
using loki3.core;
using NUnit.Framework;

namespace loki3.builtin.test
{
	[TestFixture]
	class TEST_Logic
	{
		static IScope CreateValueScope()
		{
			ScopeChain scope = new ScopeChain();
			Logic.Register(scope);

			scope.SetValue("[", new ValueDelimiter("]", DelimiterType.AsArray));
			scope.SetValue("`", new ValueDelimiter("`", DelimiterType.AsRaw));

			return scope;
		}

		[Test]
		public void TestEquals()
		{
			IScope scope = CreateValueScope();

			{
				Value value = TestSupport.ToValue("l3.equal? [ 1 1 ]", scope);
				Assert.True(value.AsBool);
			}
			{
				Value value = TestSupport.ToValue("l3.equal? [ 1 2 ]", scope);
				Assert.False(value.AsBool);
			}
			{
				Value value = TestSupport.ToValue("l3.equal? [ [ :a true ] [ :a true ] ]", scope);
				Assert.True(value.AsBool);
			}
			{
				Value value = TestSupport.ToValue("l3.equal? [ [ :a true ] [ :a true 3 ] ]", scope);
				Assert.False(value.AsBool);
			}
		}

		[Test]
		public void TestLogic()
		{
			IScope scope = CreateValueScope();

			// AND
			{
				Value value = TestSupport.ToValue("l3.and? [ true `true` ]", scope);
				Assert.True(value.AsBool);
			}
			{
				Value value = TestSupport.ToValue("l3.and? [ true `false` ]", scope);
				Assert.False(value.AsBool);
			}

			// OR
			{
				Value value = TestSupport.ToValue("l3.or? [ true `false` ]", scope);
				Assert.True(value.AsBool);
			}
			{
				Value value = TestSupport.ToValue("l3.or? [ false `false` ]", scope);
				Assert.False(value.AsBool);
			}

			// NOT
			{
				Value value = TestSupport.ToValue("l3.not? false", scope);
				Assert.True(value.AsBool);
			}
			{
				Value value = TestSupport.ToValue("l3.not? true", scope);
				Assert.False(value.AsBool);
			}
		}
	}
}
