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

			ValueDelimiter square = new ValueDelimiter("]", DelimiterType.AsArray);
			scope.SetValue("[", square);
			ValueDelimiter str = new ValueDelimiter("'", DelimiterType.AsString);
			scope.SetValue("'", str);
			ValueDelimiter curly = new ValueDelimiter("}", DelimiterType.AsArray, new CreateMap());
			scope.SetValue("{", curly);

			return scope;
		}

		[Test]
		public void TestConcat()
		{
			IScope scope = CreateStringScope();
			{
				Value value = TestSupport.ToValue("l3.stringConcat { :array [ 'one' ' two' ] }", scope);
				Assert.AreEqual("one two", value.AsString);
			}
			{
				Value value = TestSupport.ToValue("l3.stringConcat { :array [ 2 'two' ] :spaces 2 }", scope);
				Assert.AreEqual("2  two", value.AsString);
			}
			{
				Value value = TestSupport.ToValue("l3.stringConcat { :array [ 2 'two' ] :separator ', ' }", scope);
				Assert.AreEqual("2, two", value.AsString);
			}
		}

		[Test]
		public void TestFormatTable()
		{
			IScope scope = CreateStringScope();
			{
				Value value = TestSupport.ToValue("l3.formatTable { :array [ 1 23 1234 4 ] :columns 2 }", scope);
				Assert.AreEqual("1    23\n1234 4\n", value.AsString);
			}
			{
				Value value = TestSupport.ToValue("l3.formatTable { :array [ 1 23 1234 4 ] :columns 2 :dashesAfterFirst? true :spaces 2 }", scope);
				Assert.AreEqual("1     23\n--------\n1234  4\n", value.AsString);
			}
		}

		[Test]
		public void TestFormatTable2()
		{
			IScope scope = CreateStringScope();
			{
				Value value = TestSupport.ToValue("l3.formatTable2 { :arrayOfArrays [ [ 1 23 ] [ 1234 4 ] ] }", scope);
				Assert.AreEqual("1    23\n1234 4\n", value.AsString);
			}
			{
				Value value = TestSupport.ToValue("l3.formatTable2 { :arrayOfArrays [ [ 1 23 ] [ 1234 4 ] ] :dashesAfterFirst? true :spaces 2 }", scope);
				Assert.AreEqual("1     23\n--------\n1234  4\n", value.AsString);
			}
		}

		[Test]
		public void TestStringToArray()
		{
			IScope scope = CreateStringScope();
			{
				Value value = TestSupport.ToValue("l3.stringToArray :abc", scope);
				List<Value> array = value.AsArray;
				Assert.AreEqual(3, array.Count);
				Assert.AreEqual("a", array[0].AsString);
				Assert.AreEqual("b", array[1].AsString);
				Assert.AreEqual("c", array[2].AsString);
			}
		}
	}
}
