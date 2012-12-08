using System.Collections.Generic;
using loki3.core;
using NUnit.Framework;

namespace loki3.builtin.test
{
	[TestFixture]
	class TEST_MapFunctions
	{
		static IScope CreateScope()
		{
			ScopeChain scope = new ScopeChain();
			MapFunctions.Register(scope);
			scope.SetValue("create-map", new CreateMap());

			scope.SetValue("[", new ValueDelimiter("]", DelimiterType.AsArray));
			scope.SetValue("{", new ValueDelimiter("}", DelimiterType.AsArray, new CreateMap()));
			return scope;
		}

		/// <summary>Function that doubles an int</summary>
		class Double : ValueFunctionPre
		{
			internal override Value ValueCopy() { return new Double(); }

			internal Double()
			{
				Init(PatternData.Single("a", ValueType.Int));
			}

			internal override Value Eval(Value arg, IScope scope)
			{
				return new ValueInt(arg.AsInt * 2);
			}
		}

		/// <summary>Infix function that doubles the 'next' param</summary>
		class DoubleIn : ValueFunction
		{
			internal override Value ValueCopy() { return new DoubleIn(); }

			internal DoubleIn() { Init(PatternData.Single("a"), PatternData.Single("b", ValueType.Int)); }

			internal override Value Eval(DelimiterNode prev, DelimiterNode next, IScope scope, INodeRequestor nodes, ILineRequestor requestor)
			{
				Value value2 = EvalNode.Do(next, scope, nodes, requestor);
				return new ValueInt(value2.AsInt * 2);
			}
		}

		/// <summary>Function that checks if a value is even</summary>
		class IsEven : ValueFunctionPre
		{
			internal override Value ValueCopy() { return new IsEven(); }

			internal IsEven()
			{
				Init(PatternData.Single("a", ValueType.Int));
			}

			internal override Value Eval(Value arg, IScope scope)
			{
				return new ValueBool(arg.AsInt % 2 == 0);
			}
		}


		[Test]
		public void TestMapApply()
		{
			IScope scope = CreateScope();
			MapFunctions.Register(scope);

			scope.SetValue("2x", new Double());
			scope.SetValue("2x-in", new DoubleIn());

			{	// function on a map can be infix...
				Value value = TestSupport.ToValue("l3.mapToMap { :map { :a 3 :b 8 } :transform 2x-in }", scope);
				Map map = value.AsMap;
				Assert.AreEqual(2, map.Count);
				Assert.AreEqual(6, map["a"].AsInt);
				Assert.AreEqual(16, map["b"].AsInt);
			}

			{	// ...or prefix as for an array, which only applies to the value
				Value value = TestSupport.ToValue("l3.mapToMap { :map { :a 3 :b 8 } :transform 2x }", scope);
				Map map = value.AsMap;
				Assert.AreEqual(2, map.Count);
				Assert.AreEqual(6, map["a"].AsInt);
				Assert.AreEqual(16, map["b"].AsInt);
			}
		}

		[Test]
		public void TestMapFilter()
		{
			IScope scope = CreateScope();
			MapFunctions.Register(scope);

			scope.SetValue("even?", new IsEven());

			{
				Value value = TestSupport.ToValue("l3.mapToMap { :map { :a 3 :b 4 :c 7 :d 8 } :filter? even? }", scope);
				Map map = value.AsMap;
				Assert.AreEqual(2, map.Count);
				Assert.AreEqual(4, map["b"].AsInt);
				Assert.AreEqual(8, map["d"].AsInt);
			}
		}
	}
}
