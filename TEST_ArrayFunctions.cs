using System.Collections.Generic;
using loki3.core;
using NUnit.Framework;

namespace loki3.builtin.test
{
	[TestFixture]
	class TEST_ArrayFunctions
	{
		static IScope CreateScope()
		{
			ScopeChain scope = new ScopeChain();
			ArrayFunctions.Register(scope);
			scope.SetValue("create-map", new CreateMap());

			scope.SetValue("[", new ValueDelimiter("[", "]", DelimiterType.AsArray));
			scope.SetValue("{", new ValueDelimiter("{", "}", DelimiterType.AsArray, new CreateMap()));
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

		/// <summary>Function that adds previous and next ints</summary>
		class TestSum : ValueFunction
		{
			internal override Value ValueCopy() { return new TestSum(); }

			internal TestSum() { Init(PatternData.Single("a", ValueType.Int), PatternData.Single("b", ValueType.Int)); }

			internal override Value Eval(DelimiterNode prev, DelimiterNode next, IScope scope, INodeRequestor nodes, ILineRequestor requestor)
			{
				Value value1 = EvalNode.Do(prev, scope, nodes, requestor);
				Value value2 = EvalNode.Do(next, scope, nodes, requestor);
				int sum = value1.AsInt + value2.AsInt;
				return new ValueInt(sum);
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
		public void TestCombine()
		{
			IScope scope = CreateScope();
			ArrayFunctions.Register(scope);

			{
				Value value = TestSupport.ToValue("l3.combine [ [ 1 2 ] [ 3 4 ] ]", scope);
				List<Value> array = value.AsArray;
				Assert.AreEqual(4, array.Count);
				Assert.AreEqual(1, array[0].AsInt);
				Assert.AreEqual(2, array[1].AsInt);
				Assert.AreEqual(3, array[2].AsInt);
				Assert.AreEqual(4, array[3].AsInt);
			}

			{
				Value value = TestSupport.ToValue("l3.combine [ [ 1 2 ] nil ]", scope);
				List<Value> array = value.AsArray;
				Assert.AreEqual(2, array.Count);
				Assert.AreEqual(1, array[0].AsInt);
				Assert.AreEqual(2, array[1].AsInt);
			}
		}

		[Test]
		public void TestApply()
		{
			IScope scope = CreateScope();
			ArrayFunctions.Register(scope);

			scope.SetValue("2x", new Double());
			scope.SetValue("+", new TestSum());

			{
				Value value = TestSupport.ToValue("l3.apply { :array [ 1 2 ] :function 2x }", scope);
				List<Value> array = value.AsArray;
				Assert.AreEqual(2, array.Count);
				Assert.AreEqual(2, array[0].AsInt);
				Assert.AreEqual(4, array[1].AsInt);
			}
		}

		[Test]
		public void TestFold()
		{
			IScope scope = CreateScope();
			ArrayFunctions.Register(scope);

			scope.SetValue("2x", new Double());
			scope.SetValue("+", new TestSum());

			{
				Value value = TestSupport.ToValue("l3.foldLeft { :array [ 4 1 2 ] :function + }", scope);
				Assert.AreEqual(7, value.AsInt);
			}
			{
				Value value = TestSupport.ToValue("l3.foldRight { :array [ 4 1 2 ] :function + }", scope);
				Assert.AreEqual(7, value.AsInt);
			}
		}

		[Test]
		public void TestFilter()
		{
			IScope scope = CreateScope();
			ArrayFunctions.Register(scope);

			scope.SetValue("even?", new IsEven());

			{
				Value value = TestSupport.ToValue("l3.filter { :array [ 3 4 7 8 9 11 12 ] :function even? }", scope);
				List<Value> array = value.AsArray;
				Assert.AreEqual(3, array.Count);
				Assert.AreEqual(4, array[0].AsInt);
				Assert.AreEqual(8, array[1].AsInt);
				Assert.AreEqual(12, array[2].AsInt);
			}
		}
	}
}
