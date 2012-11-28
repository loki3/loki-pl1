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

				{	// test setting a value and adding metadata to it
					Value a = TestSupport.ToValue(":a <- 3", scope);
					TestSupport.ToValue(":a @ [ :tag :hello ]", scope);
					Assert.AreEqual("hello", a.Metadata["tag"].AsString);
				}

				{	// test modifying the doc string on a function
					Value a = TestSupport.ToValue(":= @doc :testing", scope);
					Assert.AreEqual("testing", scope.AsMap["="].Metadata["l3.value.doc"].AsString);
				}

				{	// test infix addition
					Value value = TestSupport.ToValue("5 + 7", scope);
					Assert.AreEqual(12, value.AsInt);
				}

				{	// a function that needs params but doesn't get them is just a function
					Value value = TestSupport.ToValue("+", scope);
					Assert.AreEqual(ValueType.Function, value.Type);
				}

				{	// test function definition
					Value value = TestSupport.ToValue("sqrt 4", scope);
					Assert.AreEqual(2, value.AsFloat);
				}

				{	// test comparison
					Value a = TestSupport.ToValue("a", scope);
					Assert.AreNotEqual(4, a.AsInt);
					Value value = TestSupport.ToValue("4 =? a", scope);
					Assert.IsFalse(value.AsBool);
				}

				{	// test value-to-array comparison
					Value value = TestSupport.ToValue("4 =any? [ 3 4 5 ]", scope);
					Assert.IsTrue(value.AsBool);
					value = TestSupport.ToValue("4 =any? [ 3 7 5 ]", scope);
					Assert.IsFalse(value.AsBool);
				}

				{	// test eval order
					Value value = TestSupport.ToValue("2 * 2 + 3", scope);
					Assert.AreEqual(7, value.AsInt);
					value = TestSupport.ToValue("2 + 2 * 3", scope);
					Assert.AreEqual(8, value.AsInt);
				}

				{	// array concat
					Value value = TestSupport.ToValue("[ 1 2 ] += 3", scope);
					System.Collections.Generic.List<Value> values = value.AsArray;
					Assert.AreEqual(3, values.Count);
					Assert.AreEqual(1, values[0].AsInt);
					Assert.AreEqual(2, values[1].AsInt);
					Assert.AreEqual(3, values[2].AsInt);
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
					":complex <- func [ ->x ->y ]",
					"	{ :real x :imaginary y }",
					":complex @order 1",
					// 5 i -> 5i
					":i <- /( ->y postfix",
					"	{ :real 0 :imaginary y }",
					":i @order 1",
					// addition
					// todo: adjust when we have function overloading & default values
					":+c <- /( { :real ->x1 :imaginary ->y1 } infix { :real ->x2 :imaginary ->y2 }",
					"	{ :real ( x1 + x2 ) :imaginary ( y1 + y2 ) }",
					":+i <- /( ->x1 infix { :real ->x2 :imaginary ->y2 }",
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
		public void TestIf()
		{
			try
			{
				IScope scope = GetBootstrapScope();

				string[] lines = {
					":result <- 0",
					"if flag1?",
					"	:result = 1",
					"elsif flag2?",
					"	:result = 2",
					"else",
					"	:result = 3",
				};

				{	// if block is run
					scope.SetValue("flag1?", ValueBool.True);
					scope.SetValue("flag2?", ValueBool.True);
					LineConsumer requestor = new LineConsumer(lines);
					EvalLines.Do(requestor, scope);
					Assert.AreEqual(1, scope.GetValue(new Token("result")).AsInt);
				}

				{	// elsif block is run
					scope.SetValue("flag1?", ValueBool.False);
					scope.SetValue("flag2?", ValueBool.True);
					LineConsumer requestor = new LineConsumer(lines);
					EvalLines.Do(requestor, scope);
					Assert.AreEqual(2, scope.GetValue(new Token("result")).AsInt);
				}

				{	// else block is run
					scope.SetValue("flag1?", ValueBool.False);
					scope.SetValue("flag2?", ValueBool.False);
					LineConsumer requestor = new LineConsumer(lines);
					EvalLines.Do(requestor, scope);
					Assert.AreEqual(3, scope.GetValue(new Token("result")).AsInt);
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
					":result <- 0",
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
					":addUp <- func [ ( ->a @@type :int ) ( ->b @@default 5 ) ]",
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
					Value value = TestSupport.ToValue("{ :a :a :remainder ( :remainder @@rest ) } <- { :a 4 :b 5 :c 6 }", nested);
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
					Value value = TestSupport.ToValue("[ ->a ( ->remainder @@rest ) ] <- { :a 4 :b 5 :c 6 }", nested);
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
					Value value = TestSupport.ToValue("[ ->a ( ->remainder @@rest ) ] <- [ 7 8 9 ]", nested);
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
						":total <- 0",
						":i <- 0",
						"l3.loop /{ :check /` i !=? 5",
						"	:i = i + 1",
						"	:total = total + i",
					};
					LineConsumer requestor = new LineConsumer(lines);
					Value result = EvalLines.Do(requestor, scope);
					Assert.AreEqual(15, result.AsInt);
				}

				{	// while
					string[] lines = {
						":total <- 0",
						":i <- 0",
						"while /` i !=? 5",
						"	:i = i + 1",
						"	:total = total + i",
					};
					LineConsumer requestor = new LineConsumer(lines);
					Value result = EvalLines.Do(requestor, scope);
					Assert.AreEqual(15, result.AsInt);
				}

				{	// for
					string[] lines = {
						":total <- 0",
						// todo: should be able to declare i inside for
						":i <- 0",
						"for /[ ` :i = 0 ` ` i !=? 5 ` ` :i = i + 1 `",
						"	:total = total + i",
					};
					LineConsumer requestor = new LineConsumer(lines);
					Value result = EvalLines.Do(requestor, scope);
					Assert.AreEqual(10, result.AsInt);
				}

				{	// break
					string[] lines = {
						":total = 0",
						":i = 0",
						"while /` i !=? 8",
						"	:i = i + 1",
						"	if i =? 4",
						"		break",
						"	:total = total + i",
						"total",
					};
					LineConsumer requestor = new LineConsumer(lines);
					Value result = EvalLines.Do(requestor, scope);
					Assert.AreEqual(6, result.AsInt);
				}
			}
			catch (Loki3Exception e)
			{
				Assert.Fail(e.ToString());
			}
		}

		[Test]
		public void TestForEach()
		{
			try
			{
				IScope scope = GetBootstrapScope();

				{	// loop thru all items in an array
					string[] lines = {
						":total <- 0",
						":i forEach ( 1 .. 5 )",
						"	:total = total + i",
					};
					LineConsumer requestor = new LineConsumer(lines);
					Value result = EvalLines.Do(requestor, scope);
					Assert.AreEqual(15, result.AsInt);
				}

				{	// loop thru all items in a map
					string[] lines = {
						":total <- 0",
						":i forEach { :a 2 :b 4 }",
						"	:total = total + i . 1",
					};
					LineConsumer requestor = new LineConsumer(lines);
					Value result = EvalLines.Do(requestor, scope);
					Assert.AreEqual(6, result.AsInt);
				}

				{	// create a partial function to use for a later loop
					string[] lines = {
						":1to5 <- /( :i forEach /( 5 .. 1",	// partial that needs a body
						":total <- 0",
						"1to5",
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

		[Test]
		public void TestFunctions()
		{
			try
			{
				IScope scope = GetBootstrapScope();

				{
					string[] lines = {
						":double <- (( :x ` x * 2 ` ))",
						"double 21",
					};
					LineConsumer requestor = new LineConsumer(lines);
					Value result = EvalLines.Do(requestor, scope);
					Assert.AreEqual(42, result.AsInt);
				}

				{
					string[] lines = {
						":a <- [ 1 2 3 ] apply /(( :x /` x * 2",
						"a =>str"
					};
					LineConsumer requestor = new LineConsumer(lines);
					Value result = EvalLines.Do(requestor, scope);
					Assert.AreEqual("[ 2 4 6 ]", result.AsString);
				}
			}
			catch (Loki3Exception e)
			{
				Assert.Fail(e.ToString());
			}
		}

		[Test]
		public void TestEnum()
		{
			try
			{
				IScope scope = GetBootstrapScope();

				{
					string[] lines = {
						":mine enum [ :first :second ]",
					};
					LineConsumer requestor = new LineConsumer(lines);
					Value result = EvalLines.Do(requestor, scope);
					Assert.True(result.AsMap != null);
					Map mineMap = scope.GetValue(new Token("mine")).AsMap;
					Assert.AreEqual(0, mineMap["mine.first"].AsInt);
					Assert.AreEqual(1, mineMap["mine.second"].AsInt);
					Assert.True(mineMap["mine.first"].Metadata != null);
				}
			}
			catch (Loki3Exception e)
			{
				Assert.Fail(e.ToString());
			}
		}

		[Test]
		public void TestBodies()
		{
			try
			{
				IScope scope = GetBootstrapScope();

				{
					string[] lines = {
						":myarray <- makeArray",
						"	:blah",
						"	func ->a",
						"		a + 2",
						"	[ 1 2 ]",
					};
					LineConsumer requestor = new LineConsumer(lines);
					Value result = EvalLines.Do(requestor, scope);
					Assert.True(result.AsArray != null);
					List<Value> array = scope.GetValue(new Token("myarray")).AsArray;
					Assert.AreEqual(3, array.Count);
					Assert.AreEqual("blah", array[0].AsString);
					Assert.IsTrue(array[1] as ValueFunction != null);
					Assert.AreEqual(2, array[2].AsArray.Count);
				}

				{
					string[] lines = {
						":mymap <- makeMap",
						"	:key1 5",
						"	:func /( func ->a",
						"		a + 2",
						"	:key2 (| ## + 2 |)",
					};
					LineConsumer requestor = new LineConsumer(lines);
					Value result = EvalLines.Do(requestor, scope);
					Assert.True(result.AsMap != null);
					Map mineMap = scope.GetValue(new Token("mymap")).AsMap;
					Assert.AreEqual(5, mineMap["key1"].AsInt);
					Assert.IsTrue(mineMap["func"] as ValueFunction != null);
					Assert.IsTrue(mineMap["key2"] as ValueFunction != null);
				}
			}
			catch (Loki3Exception e)
			{
				Assert.Fail(e.ToString());
			}
		}
	}
}
