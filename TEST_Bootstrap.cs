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
					Value a = ToValue(":a = 3", scope);
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
					":complex = func [ ->x ->y ]",
					"	{ :real x :imaginary y }",
					":complex @order 1",
					// 5 i -> 5i
					":i = /( ->y postfix",
					"	{ :real 0 :imaginary y }",
					":i @order 1",
					// addition
					// todo: adjust when we have function overloading & default values
					":+c = /( { :real ->x1 :imaginary ->y1 } infix { :real ->x2 :imaginary ->y2 }",
					"	{ :real ( x1 + x2 ) :imaginary ( y1 + y2 ) }",
					":+i = /( ->x1 infix { :real ->x2 :imaginary ->y2 }",
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
					":result = 0",
					// todo: improve once there's better syntax for creating/updating vars
					"if flag1?",
					"	l3.setValue { :key :result :value 1 :create? false }",
					"elsif flag2?",
					"	l3.setValue { :key :result :value 2 :create? false }",
					"else",
					"	l3.setValue { :key :result :value 3 :create? false }",
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
	}
}
