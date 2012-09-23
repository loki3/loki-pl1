using System;
using System.Collections.Generic;
using loki3.core;
using NUnit.Framework;

namespace loki3.builtin.test
{
	[TestFixture]
	class TEST_Values
	{
		static IStack CreateValueStack()
		{
			StateStack stack = new StateStack(null);
			Values.Register(stack);
			Math.Register(stack);

			ValueDelimiter paren = new ValueDelimiter("(", ")", DelimiterType.AsValue);
			stack.SetValue("(", paren);
			ValueDelimiter square = new ValueDelimiter("[", "]", DelimiterType.AsArray);
			stack.SetValue("[", square);
			ValueDelimiter quote = new ValueDelimiter("'", "'", DelimiterType.AsString);
			stack.SetValue("'", quote);

			return stack;
		}

		static Value ToValue(string s, IStack stack)
		{
			DelimiterList list = ParseLine.Do(s, stack);
			return EvalList.Do(list.Nodes, stack);
		}

		[Test]
		public void TestSetValue()
		{
			IStack stack = CreateValueStack();

			{
				Value value = ToValue("l3.setValue [ :a 5 ]", stack);
				Assert.AreEqual(5, value.AsInt);

				Value fetch = stack.GetValue(new Token("a"));
				Assert.AreEqual(5, fetch.AsInt);
			}

			{	// change a
				Value value = ToValue("l3.setValue [ :a 7.5 ]", stack);
				Assert.AreEqual(7.5, value.AsFloat);

				Value fetch = stack.GetValue(new Token("a"));
				Assert.AreEqual(7.5, fetch.AsFloat);
			}

			{	// set an array
				Value value = ToValue("l3.setValue [ :key [ a 2 false ] ]", stack);
				List<Value> array = value.AsArray;
				Assert.AreEqual(3, array.Count);
				Assert.AreEqual(7.5, array[0].AsFloat);
				Assert.AreEqual(2, array[1].AsInt);
				Assert.AreEqual(false, array[2].AsBool);

				Value fetch = stack.GetValue(new Token("key"));
				List<Value> fetchArray = fetch.AsArray;
				Assert.AreEqual(3, fetchArray.Count);
				Assert.AreEqual(7.5, fetchArray[0].AsFloat);
				Assert.AreEqual(2, fetchArray[1].AsInt);
				Assert.AreEqual(false, fetchArray[2].AsBool);
			}
		}

		[Test]
		public void TestCreateMap()
		{
			IStack stack = CreateValueStack();

			{
				Value value = ToValue("l3.createMap [ :a 5  :key true ]", stack);
				ValueMap map = value.AsMap;
				Assert.AreEqual(5, map["a"].AsInt);
				Assert.AreEqual(true, map["key"].AsBool);
			}
		}

		[Test]
		public void TestCreateFunction()
		{
			IStack stack = CreateValueStack();

			{
				// first add the + function to the current stack
				Value value = ToValue("l3.setValue [ :+ ( l3.createFunction l3.createMap [ :pre :x  :post :y :lines [ ' l3.add [ x y ] ' ] ] ) ]", stack);
				ValueFunction func = value as ValueFunction;

				// next, use it
				Value result = ToValue("5 + 7", stack);
				Assert.AreEqual(12, result.AsInt);
			}
		}
	}
}
