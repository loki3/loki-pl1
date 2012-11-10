using loki3.core;
using NUnit.Framework;

namespace loki3.builtin.test
{
	[TestFixture]
	class TEST_Bootstrap
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

				{	// test setting a value and adding metadata to it
					Value a = ToValue(":a <- 3", scope);
					ToValue(":a @ [ :tag :hello ]", scope);
					Assert.AreEqual("hello", a.Metadata["tag"].AsString);
				}

				{	// test modifying the doc string on a function
					Value a = ToValue(":= @doc :testing", scope);
					Assert.AreEqual("testing", scope.AsMap["="].Metadata["l3.value.doc"].AsString);
				}

				{	// test infix addition
					Value value = ToValue("5 + 7", scope);
					Assert.AreEqual(12, value.AsInt);
				}

				{	// test function definition
					Value value = ToValue("sqrt 4", scope);
					Assert.AreEqual(2, value.AsFloat);
				}

				{	// test comparison
					Value a = ToValue("a", scope);
					Assert.AreNotEqual(4, a.AsInt);
					Value value = ToValue("4 =? a", scope);
					Assert.IsFalse(value.AsBool);
				}

				{	// test eval order
					Value value = ToValue("2 * 2 + 3", scope);
					Assert.AreEqual(7, value.AsInt);
					value = ToValue("2 + 2 * 3", scope);
					Assert.AreEqual(8, value.AsInt);
				}
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
				ScopeChain scope = new ScopeChain();
				AllBuiltins.RegisterAll(scope);
				EvalFile.Do("../../l3/bootstrap.l3", scope);

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
					Value value = ToValue("complex [ 1 2 ]", scope);
					Assert.AreEqual("{ :real 1 , :imaginary 2 }", value.ToString());
				}

				{
					Value value = ToValue("4 i", scope);
					Assert.AreEqual("{ :real 0 , :imaginary 4 }", value.ToString());
				}

				{
					Value value = ToValue("1 +i 3 i", scope);
					Assert.AreEqual("{ :real 1 , :imaginary 3 }", value.ToString());
				}

				{
					Value value = ToValue("( 1 +i 3 i ) +c complex [ 4 6 ]", scope);
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
				ScopeChain scope = new ScopeChain();
				AllBuiltins.RegisterAll(scope);
				EvalFile.Do("../../l3/bootstrap.l3", scope);

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
				ScopeChain scope = new ScopeChain();
				AllBuiltins.RegisterAll(scope);
				EvalFile.Do("../../l3/bootstrap.l3", scope);

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
				ScopeChain scope = new ScopeChain();
				AllBuiltins.RegisterAll(scope);
				EvalFile.Do("../../l3/bootstrap.l3", scope);

				string[] lines = {
					":addUp <- func [ ( ->a :type 2 ) ( ->b :default 5 ) ]",
					"	a + b",
				};
				LineConsumer requestor = new LineConsumer(lines);
				EvalLines.Do(requestor, scope);

				{	// pass expected parameters
					Value value = ToValue("addUp [ 1 2 ]", scope);
					Assert.AreEqual(3, value.AsInt);
				}

				{	// leave off 2nd, use default
					Value value = ToValue("addUp [ 1 ]", scope);
					Assert.AreEqual(6, value.AsInt);
				}

				// it throws if we pass wrong type
				bool bThrew = false;
				try
				{
					ToValue("addUp [ true 1 ]", scope);
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
				ScopeChain scope = new ScopeChain();
				AllBuiltins.RegisterAll(scope);
				EvalFile.Do("../../l3/bootstrap.l3", scope);

				{
					ScopeChain nested = new ScopeChain(scope);
					Value value = ToValue("{ :a :a :remainder ( :remainder :rest ) } <- { :a 4 :b 5 :c 6 }", nested);
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
					Value value = ToValue("[ ->a ( ->remainder :rest ) ] <- { :a 4 :b 5 :c 6 }", nested);
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
					Value value = ToValue("[ ->a ( ->remainder :rest ) ] <- [ 7 8 9 ]", nested);
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
				ScopeChain scope = new ScopeChain();
				AllBuiltins.RegisterAll(scope);
				EvalFile.Do("../../l3/bootstrap.l3", scope);

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

#if false
				{	// break
					string[] lines = {
						":total = 0",
						":i = 0",
						"while /` i !=? 8",
						"	:i = i + 1",
						"	if i =? 4",
						"		break",
						"	:total = total + i",
					};
					LineConsumer requestor = new LineConsumer(lines);
					Value result = EvalLines.Do(requestor, scope);
					Assert.AreEqual(10, result.AsInt);
				}
#endif
			}
			catch (Loki3Exception e)
			{
				Assert.Fail(e.ToString());
			}
		}
	}
}
