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
		static IScope CreateValueScope()
		{
			ScopeChain scope = new ScopeChain();
			Values.Register(scope);

			scope.SetValue("(", new ValueDelimiter(")", DelimiterType.AsValue));
			scope.SetValue("[", new ValueDelimiter("]", DelimiterType.AsArray));
			scope.SetValue("'", new ValueDelimiter("'", DelimiterType.AsString));
			scope.SetValue("{", new ValueDelimiter("}", DelimiterType.AsArray, new CreateMap()));
			scope.SetValue("`", new ValueDelimiter("`", DelimiterType.AsRaw));

			return scope;
		}

		[Test]
		public void TestSetValue()
		{
			IScope scope = CreateValueScope();

			{
				Value value = TestSupport.ToValue("l3.setValue { :key :a :value 5 }", scope);
				Assert.AreEqual(5, value.AsInt);

				Value fetch = scope.GetValue(new Token("a"));
				Assert.AreEqual(5, fetch.AsInt);
			}

			{	// change a
				Value value = TestSupport.ToValue("l3.setValue { :key :a :value 7.5 }", scope);
				Assert.AreEqual(7.5, value.AsFloat);

				Value fetch = scope.GetValue(new Token("a"));
				Assert.AreEqual(7.5, fetch.AsFloat);
			}

			{	// set an array
				Value value = TestSupport.ToValue("l3.setValue { :key :key :value [ a 2 false ] }", scope);
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

			// create a nested scope
			IScope nested = new ScopeChain(scope);

			{	// explicitly say create-if-needed
				TestSupport.ToValue("l3.setValue { :key :created :value 3 :create? true }", nested);
				Value fetch = nested.GetValue(new Token("created"));
				Assert.AreEqual(3, fetch.AsInt);
			}

			{	// asking to reuse non-existent var throws
				bool bThrew = false;
				try
				{
					Value value = TestSupport.ToValue("l3.setValue { :key :doesnt-exist :value 3 :create? false }", nested);
				}
				catch (Loki3Exception e)
				{
					bThrew = true;
					Assert.AreEqual("doesnt-exist", e.Errors[Loki3Exception.keyBadToken].AsString);
				}
				Assert.IsTrue(bThrew);
			}

			{	// set var on parent scope & reuse on nested scope
				TestSupport.ToValue("l3.setValue { :key :on-parent :value 5 :create? true }", scope);
				Assert.AreEqual(5, scope.AsMap["on-parent"].AsInt);
				TestSupport.ToValue("l3.setValue { :key :on-parent :value 4 :create? false }", nested);
				Assert.AreEqual(4, scope.AsMap["on-parent"].AsInt);
				Assert.AreNotEqual(null, nested.Exists("on-parent"));
				Assert.IsFalse(nested.AsMap.ContainsKey("on-parent"));
			}

			{	// set a value on a map
				Map map = new Map();
				map["one"] = new ValueInt(1);
				map["two"] = new ValueInt(2);
				ValueMap valueMap = new ValueMap(map);
				scope.SetValue("mymap", valueMap);

				Value value = TestSupport.ToValue("l3.setValue { :key :one :value 3 :map mymap }", scope);
				Assert.AreEqual(3, value.AsInt);
				Assert.AreEqual(3, map["one"].AsInt);
			}

			{	// pattern matching: extract values out of an array
				Value value = TestSupport.ToValue("l3.setValue { :key [ :first :second :third ] :value [ 11 22 33 ] :create? true }", scope);
				Assert.AreEqual(11, scope.AsMap["first"].AsInt);
				Assert.AreEqual(22, scope.AsMap["second"].AsInt);
				Assert.AreEqual(33, scope.AsMap["third"].AsInt);
			}

			{	// special case: initialize every token in array to nil
				Value value = TestSupport.ToValue("l3.setValue { :key [ :first :second :third ] :value nil :create? true }", scope);
				Assert.IsTrue(scope.AsMap["first"].IsNil);
				Assert.IsTrue(scope.AsMap["second"].IsNil);
				Assert.IsTrue(scope.AsMap["third"].IsNil);
			}
		}

		[Test]
		public void TestGetValue()
		{
			IScope scope = CreateValueScope();

			Map map = new Map();
			map["one"] = new ValueInt(1);
			map["two"] = new ValueInt(2);
			ValueMap vMap = new ValueMap(map);
			scope.SetValue("aMap", vMap);

			List<Value> array = new List<Value>();
			array.Add(new ValueInt(2));
			array.Add(new ValueInt(4));
			scope.SetValue("anArray", new ValueArray(array));

			{
				Value value = TestSupport.ToValue("l3.getValue { :object aMap :key :two }", scope);
				Assert.AreEqual(2, value.AsInt);
			}

			{
				Value value = TestSupport.ToValue("l3.getValue { :object anArray :key 1 }", scope);
				Assert.AreEqual(4, value.AsInt);
			}

			// asking for a key that doesn't exist returns nil...
			{
				Value value = TestSupport.ToValue("l3.getValue { :object aMap :key :notThere }", scope);
				Assert.IsTrue(value.IsNil);
			}

			// ...unless the collection has a default
			{
				vMap.WritableMetadata[PatternData.keyDefault] = new ValueInt(42);
				Value value = TestSupport.ToValue("l3.getValue { :object aMap :key :notThere }", scope);
				Assert.AreEqual(42, value.AsInt);
			}
		}

		[Test]
		public void TestCreateMap()
		{
			IScope scope = CreateValueScope();

			{
				Value value = TestSupport.ToValue("l3.createMap [ :a 5  :key true ]", scope);
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
				Value value = TestSupport.ToValue("l3.createFunction { :post :x :body [ ' l3.add [ 3 x ] ' ] }", scope);
				ValueFunction func = value as ValueFunction;

				List<DelimiterNode> nodes = new List<DelimiterNode>();
				nodes.Add(new DelimiterNodeValue(func));
				nodes.Add(new DelimiterNodeValue(new ValueInt(4)));

				Value result = EvalList.Do(nodes, scope);
				Assert.AreEqual(7, result.AsInt);
			}

			{	// postfix
				Value value = TestSupport.ToValue("l3.createFunction { :pre :x :body [ ' l3.add [ 7 x ] ' ] }", scope);
				ValueFunction func = value as ValueFunction;

				List<DelimiterNode> nodes = new List<DelimiterNode>();
				nodes.Add(new DelimiterNodeValue(new ValueInt(2)));
				nodes.Add(new DelimiterNodeValue(func));

				Value result = EvalList.Do(nodes, scope);
				Assert.AreEqual(9, result.AsInt);
			}

			{	// infix
				// first add the + function to the current scope
				Value value = TestSupport.ToValue("l3.setValue { :key :+ :value ( l3.createFunction l3.createMap [ :pre :x  :post :y :body [ ' l3.add [ x y ] ' ] ] ) }", scope);
				ValueFunction func = value as ValueFunction;

				// next, use it
				Value result = TestSupport.ToValue("5 + 7", scope);
				Assert.AreEqual(12, result.AsInt);
			}
		}

		[Test]
		public void TestCreateDelimiter()
		{
			IScope scope = CreateValueScope();

			{	// {} should mean create a map from the contents
				// first add the + function to the current scope
				Value value = TestSupport.ToValue("l3.setValue { :key :{ :value ( l3.createDelimiter { :end :} :type :asArray :function l3.createMap } ) }", scope);
				ValueDelimiter delimi = value as ValueDelimiter;

				// next, use it
				Value result = TestSupport.ToValue("{ :key1 5 :key2 false }", scope);
				ValueMap map = result as ValueMap;
				Assert.AreEqual(5, map["key1"].AsInt);
				Assert.AreEqual(false, map["key2"].AsBool);
			}
		}

		[Test]
		public void TestGetMetadata()
		{
			IScope scope = CreateValueScope();

			{	// lookup metadata on a value
				scope.SetValue("a", new ValueInt(42));
				Value value = TestSupport.ToValue("l3.getMetadata { :value a }", scope);
				Assert.AreEqual(loki3.core.ValueType.Nil, value.Type);
				value = TestSupport.ToValue("l3.getMetadata { :value a :writable? true }", scope);
				Assert.AreEqual(loki3.core.ValueType.Map, value.Type);
			}

			{	// lookup a key on the current scope (so function doesn't get evaled)
				Value value = TestSupport.ToValue("l3.getMetadata { :key :l3.getValue }", scope);
				Assert.AreEqual(loki3.core.ValueType.Map, value.Type);
			}
		}

		[Test]
		public void TestGetTypeFunction()
		{
			IScope scope = CreateValueScope();

			{	// built-in type w/out metadata
				scope.SetValue("a", new ValueInt(42));
				Value value = TestSupport.ToValue("l3.getType { :value a }", scope);
				Assert.AreEqual("int", value.AsString);
				value = TestSupport.ToValue("l3.getType { :value a :builtin? true }", scope);
				Assert.AreEqual("int", value.AsString);
				value = TestSupport.ToValue("l3.getType { :key :a }", scope);
				Assert.AreEqual("int", value.AsString);
			}

			{	// metadata type
				Value b = new ValueInt(42);
				b.MetaType = "myType";
				scope.SetValue("b", b);

				Value value = TestSupport.ToValue("l3.getType { :value b }", scope);
				Assert.AreEqual("myType", value.AsString);
				value = TestSupport.ToValue("l3.getType { :value b :builtin? true }", scope);
				Assert.AreEqual("int", value.AsString);
			}
		}

		[Test]
		public void TestEval()
		{
			IScope scope = CreateValueScope();
			scope.SetValue("a", new ValueInt(42));

			{	// atomic value
				Value value = TestSupport.ToValue("l3.eval 5", scope);
				Assert.AreEqual(5, value.AsInt);
			}

			{	// string
				Value value = TestSupport.ToValue("l3.eval :a", scope);
				Assert.AreEqual(42, value.AsInt);

				value = TestSupport.ToValue("l3.eval ' 8 '", scope);
				Assert.AreEqual(8, value.AsInt);
			}

			{	// raw
				Value value = TestSupport.ToValue("l3.eval ` 8 `", scope);
				Assert.AreEqual(8, value.AsInt);
			}

			{	// array
				Value value = TestSupport.ToValue("l3.eval [ ' 8 ' ]", scope);
				Assert.AreEqual(8, value.AsInt);

				value = TestSupport.ToValue("l3.eval [ ` 8 ` ]", scope);
				Assert.AreEqual(8, value.AsInt);
			}
		}
	}
}
