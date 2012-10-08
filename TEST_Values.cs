using System;
using System.Collections.Generic;
using loki3.core;
using loki3.builtin;
using NUnit.Framework;

namespace loki3.builtin.test
{
	[TestFixture]
	class TEST_Values
	{
		/// <summary>[k1 v1 k2 v2 ...] -> map</summary>
		class CreateMap : ValueFunctionPre
		{
			internal CreateMap() { Init(PatternData.ArrayEnd("a")); }

			internal override Value Eval(Value arg, IScope scope)
			{
				List<Value> list = arg.AsArray;

				Map map = new Map();
				int count = list.Count;
				for (int i = 0; i < count; i += 2)
				{
					string key = list[i].AsString;
					Value value = list[i + 1];
					map[key] = value;
				}
				return new ValueMap(map);
			}
		}

		static IScope CreateValueScope()
		{
			ScopeChain scope = new ScopeChain(null);
			Values.Register(scope);

			scope.SetValue("(", new ValueDelimiter("(", ")", DelimiterType.AsValue));
			scope.SetValue("[", new ValueDelimiter("[", "]", DelimiterType.AsArray));
			scope.SetValue("'", new ValueDelimiter("'", "'", DelimiterType.AsString));
			scope.SetValue("{", new ValueDelimiter("{", "}", DelimiterType.AsArray, new CreateMap()));

			return scope;
		}

		static Value ToValue(string s, IScope scope)
		{
			DelimiterList list = ParseLine.Do(s, scope);
			return EvalList.Do(list.Nodes, scope);
		}

		[Test]
		public void TestSetValue()
		{
			IScope scope = CreateValueScope();

			{
				Value value = ToValue("l3.setValue { :key :a :value 5 }", scope);
				Assert.AreEqual(5, value.AsInt);

				Value fetch = scope.GetValue(new Token("a"));
				Assert.AreEqual(5, fetch.AsInt);
			}

			{	// change a
				Value value = ToValue("l3.setValue { :key :a :value 7.5 }", scope);
				Assert.AreEqual(7.5, value.AsFloat);

				Value fetch = scope.GetValue(new Token("a"));
				Assert.AreEqual(7.5, fetch.AsFloat);
			}

			{	// set an array
				Value value = ToValue("l3.setValue { :key :key :value [ a 2 false ] }", scope);
				List<Value> array = value.AsArray;
				Assert.AreEqual(3, array.Count);
				Assert.AreEqual(7.5, array[0].AsFloat);
				Assert.AreEqual(2, array[1].AsInt);
				Assert.AreEqual(false, array[2].AsBool);

				Value fetch = scope.GetValue(new Token("key"));
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
			IScope scope = CreateValueScope();

			{
				Value value = ToValue("l3.createMap [ :a 5  :key true ]", scope);
				Map map = value.AsMap;
				Assert.AreEqual(5, map["a"].AsInt);
				Assert.AreEqual(true, map["key"].AsBool);
			}
		}

		[Test]
		public void TestCreateFunction()
		{
			IScope scope = CreateValueScope();
			Math.Register(scope);

			{	// prefix
				Value value = ToValue("l3.createFunction { :post :x :body [ ' l3.add [ 3 x ] ' ] }", scope);
				ValueFunction func = value as ValueFunction;

				List<DelimiterNode> nodes = new List<DelimiterNode>();
				nodes.Add(new DelimiterNodeValue(func));
				nodes.Add(new DelimiterNodeValue(new ValueInt(4)));

				Value result = EvalList.Do(nodes, scope);
				Assert.AreEqual(7, result.AsInt);
			}

			{	// postfix
				Value value = ToValue("l3.createFunction { :pre :x :body [ ' l3.add [ 7 x ] ' ] }", scope);
				ValueFunction func = value as ValueFunction;

				List<DelimiterNode> nodes = new List<DelimiterNode>();
				nodes.Add(new DelimiterNodeValue(new ValueInt(2)));
				nodes.Add(new DelimiterNodeValue(func));

				Value result = EvalList.Do(nodes, scope);
				Assert.AreEqual(9, result.AsInt);
			}

			{	// infix
				// first add the + function to the current scope
				Value value = ToValue("l3.setValue { :key :+ :value ( l3.createFunction l3.createMap [ :pre :x  :post :y :body [ ' l3.add [ x y ] ' ] ] ) }", scope);
				ValueFunction func = value as ValueFunction;

				// next, use it
				Value result = ToValue("5 + 7", scope);
				Assert.AreEqual(12, result.AsInt);
			}
		}

		[Test]
		public void TestCreateDelimiter()
		{
			IScope scope = CreateValueScope();

			{	// {} should mean create a map from the contents
				// first add the + function to the current scope
				Value value = ToValue("l3.setValue { :key :{ :value ( l3.createDelimiter { :start :{  :end :} :type :asArray :function l3.createMap } ) }", scope);
				ValueDelimiter delimi = value as ValueDelimiter;

				// next, use it
				Value result = ToValue("{ :key1 5 :key2 false }", scope);
				ValueMap map = result as ValueMap;
				Assert.AreEqual(5, map["key1"].AsInt);
				Assert.AreEqual(false, map["key2"].AsBool);
			}
		}
	}
}
