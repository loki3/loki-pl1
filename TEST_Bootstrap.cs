using System.Collections.Generic;
using loki3.core;
using NUnit.Framework;

namespace loki3.builtin.test
{
	[TestFixture]
	class TEST_Bootstrap
	{
		/// <summary>Cache the scope w/ all built-in functions and bootstrap functions</summary>
		static private IScope GetBootstrapScope()
		{
			if (m_scope == null)
			{
				m_scope = new ScopeChain();
				AllBuiltins.RegisterAll(m_scope);
				EvalFile.Do("../../l3/bootstrap.l3", m_scope);
			}
			return m_scope;
		}
		static private IScope m_scope = null;

		[Test]
		public void Test()
		{
			try
			{
				IScope scope = GetBootstrapScope();

				{	// a function that needs params but doesn't get them is just a function
					Value value = TestSupport.ToValue("enum", scope);
					Assert.AreEqual(ValueType.Function, value.Type);
				}

				{	// test eval order
					Value value = TestSupport.ToValue("2 * 2 + 3", scope);
					Assert.AreEqual(7, value.AsInt);
					value = TestSupport.ToValue("2 + 2 * 3", scope);
					Assert.AreEqual(8, value.AsInt);
				}
			}
			catch (Loki3Exception e)
			{
				Assert.Fail(e.ToString());
			}
		}

		[Test]
		public void RunUnittests()
		{
			try
			{
				IScope scope = GetBootstrapScope();
				scope = new ScopeChain(scope);
				EvalFile.Do("../../l3/unittest.l3", scope);
				EvalFile.Do("../../l3/help.l3", scope);

				// make sure all functions have @doc
				Value a = TestSupport.ToValue("checkDocs currentScope", scope);
				if (!a.AsArray[0].AsBool)	// make it obvious which functions need @doc
					Assert.AreEqual("[ ]", a.AsArray[1].AsArray);

				// currently this runs checkDocs as well
				Value v = TestSupport.ToValue("unittest [ :../../l3/bootstrap.l3 :../../l3/bootstrap_tests.l3 ]", scope);
				// if this fails, there may be output that describes which unittest & which assert
				Assert.True(v.AsBool);
			}
			catch (Loki3Exception e)
			{
				Assert.Fail(e.ToString());
			}
		}

		[Test]
		public void TestComplex()
		{
			try
			{
				IScope scope = GetBootstrapScope();

				// define complex math
				string[] lines = {
					// complex [ 1 2 ] -> 1+2i
					":complex v= func [ ->x ->y ]",
					"	{ :real x :imaginary y }",
					":complex @order 1",
					// 5 i -> 5i
					":i v= .( ->y postfix",
					"	{ :real 0 :imaginary y }",
					":i @order 1",
					// addition
					// todo: adjust when we have function overloading & default values
					":+c v= .( { :real ->x1 :imaginary ->y1 } infix { :real ->x2 :imaginary ->y2 }",
					"	{ :real ( x1 + x2 ) :imaginary ( y1 + y2 ) }",
					":+i v= .( ->x1 infix { :real ->x2 :imaginary ->y2 }",
					"	{ :real ( x1 + x2 ) :imaginary y2 }",
				};
				LineConsumer requestor = new LineConsumer(lines);
				EvalLines.Do(requestor, scope);

				{
					Value value = TestSupport.ToValue("complex [ 1 2 ]", scope);
					Assert.AreEqual("{ :real 1 , :imaginary 2 }", value.ToString());
				}

				{
					Value value = TestSupport.ToValue("4 i", scope);
					Assert.AreEqual("{ :real 0 , :imaginary 4 }", value.ToString());
				}

				{
					Value value = TestSupport.ToValue("1 +i 3 i", scope);
					Assert.AreEqual("{ :real 1 , :imaginary 3 }", value.ToString());
				}

				{
					Value value = TestSupport.ToValue("( 1 +i 3 i ) +c complex [ 4 6 ]", scope);
					Assert.AreEqual("{ :real 5 , :imaginary 9 }", value.ToString());
				}
			}
			catch (Loki3Exception e)
			{
				Assert.Fail(e.ToString());
			}
		}

		[Test]
		public void TestNested()
		{
			try
			{
				IScope scope = GetBootstrapScope();

				string[] lines = {
					":result v= 0",
					"if flag1?",
					"	if flag2?",
					"		:result = 1",
					"	else",
					"		:result = 2",
					"else",
					"	if flag3?",
					"		:result = 3",
					"	else",
					"		:result = 4",
				};

				{	// nested if block is run
					scope.SetValue("flag1?", ValueBool.True);
					scope.SetValue("flag2?", ValueBool.True);
					LineConsumer requestor = new LineConsumer(lines);
					EvalLines.Do(requestor, scope);
					Assert.AreEqual(1, scope.GetValue(new Token("result")).AsInt);
				}

				{	// nested else is run
					scope.SetValue("flag1?", ValueBool.True);
					scope.SetValue("flag2?", ValueBool.False);
					LineConsumer requestor = new LineConsumer(lines);
					EvalLines.Do(requestor, scope);
					Assert.AreEqual(2, scope.GetValue(new Token("result")).AsInt);
				}

				{	// top level else block is run, nested if
					scope.SetValue("flag1?", ValueBool.False);
					scope.SetValue("flag3?", ValueBool.True);
					LineConsumer requestor = new LineConsumer(lines);
					EvalLines.Do(requestor, scope);
					Assert.AreEqual(3, scope.GetValue(new Token("result")).AsInt);
				}

				{	// top level else block is run, nested else
					scope.SetValue("flag1?", ValueBool.False);
					scope.SetValue("flag3?", ValueBool.False);
					LineConsumer requestor = new LineConsumer(lines);
					EvalLines.Do(requestor, scope);
					Assert.AreEqual(4, scope.GetValue(new Token("result")).AsInt);
				}
			}
			catch (Loki3Exception e)
			{
				Assert.Fail(e.ToString());
			}
		}


		[Test]
		public void TestParamMetadata()
		{
			try
			{
				IScope scope = GetBootstrapScope();

				string[] lines = {
					":addUp v= func [ ( ->a @@type :int ) ( ->b @@default 5 ) ]",
					"	a + b",
				};
				LineConsumer requestor = new LineConsumer(lines);
				EvalLines.Do(requestor, scope);

				{	// pass expected parameters
					Value value = TestSupport.ToValue("addUp [ 1 2 ]", scope);
					Assert.AreEqual(3, value.AsInt);
				}

				{	// leave off 2nd, use default
					Value value = TestSupport.ToValue("addUp [ 1 ]", scope);
					Assert.AreEqual(6, value.AsInt);
				}

				// it throws if we pass wrong type
				bool bThrew = false;
				try
				{
					TestSupport.ToValue("addUp [ true 1 ]", scope);
				}
				catch (Loki3Exception)
				{
					bThrew = true;
				}
				Assert.IsTrue(bThrew);
			}
			catch (Loki3Exception e)
			{
				Assert.Fail(e.ToString());
			}
		}

		[Test]
		public void TestParamMetadata2()
		{
			try
			{
				IScope scope = GetBootstrapScope();

				{
					ScopeChain nested = new ScopeChain(scope);
					Value value = TestSupport.ToValue("{ :a :a :remainder ( :remainder @@rest ) } v= { :a 4 :b 5 :c 6 }", nested);
					// value should be { :a 4 :b 5 :c 6 }
					Assert.AreEqual(3, value.AsMap.Count);
					// nested should now contain "a" and "remainder"
					Assert.AreEqual(4, nested.GetValue(new Token("a")).AsInt);
					Map rest = nested.GetValue(new Token("remainder")).AsMap;
					Assert.AreEqual(2, rest.Count);
					Assert.AreEqual(5, rest["b"].AsInt);
					Assert.AreEqual(6, rest["c"].AsInt);
				}

				{
					ScopeChain nested = new ScopeChain(scope);
					Value value = TestSupport.ToValue("[ ->a ( ->remainder @@rest ) ] v= { :a 4 :b 5 :c 6 }", nested);
					// value should be { :a 4 :b 5 :c 6 }
					Assert.AreEqual(3, value.AsMap.Count);
					// nested should now contain "a" and "remainder"
					Assert.AreEqual(4, nested.GetValue(new Token("a")).AsInt);
					Map rest = nested.GetValue(new Token("remainder")).AsMap;
					Assert.AreEqual(2, rest.Count);
					Assert.AreEqual(5, rest["b"].AsInt);
					Assert.AreEqual(6, rest["c"].AsInt);
				}

				{
					ScopeChain nested = new ScopeChain(scope);
					Value value = TestSupport.ToValue("[ ->a ( ->remainder @@rest ) ] v= [ 7 8 9 ]", nested);
					// value should be [ 7 8 9 ]
					Assert.AreEqual(3, value.AsArray.Count);
					// nested should now contain "a" and "remainder"
					Assert.AreEqual(7, nested.GetValue(new Token("a")).AsInt);
					System.Collections.Generic.List<Value> rest = nested.GetValue(new Token("remainder")).AsArray;
					Assert.AreEqual(2, rest.Count);
					Assert.AreEqual(8, rest[0].AsInt);
					Assert.AreEqual(9, rest[1].AsInt);
				}
			}
			catch (Loki3Exception e)
			{
				Assert.Fail(e.ToString());
			}
		}


		[Test]
		public void TestLoop()
		{
			try
			{
				IScope scope = GetBootstrapScope();

				{	// l3.loop
					string[] lines = {
						":total v= 0",
						":i v= 0",
						"l3.loop .{ :check .` i !=? 5",
						"	:i = i + 1",
						"	:total = total + i",
					};
					LineConsumer requestor = new LineConsumer(lines);
					Value result = EvalLines.Do(requestor, scope);
					Assert.AreEqual(15, result.AsInt);
				}
			}
			catch (Loki3Exception e)
			{
				Assert.Fail(e.ToString());
			}
		}
	}
}
