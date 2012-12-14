using System.Collections.Generic;
using loki3.core;
using loki3.builtin;
using NUnit.Framework;

namespace loki3.core.test
{
	[TestFixture]
	class TEST_FunctionOverload
	{
		/// <summary>Add two ints, floats or numbers</summary>
		class Add : ValueFunctionPre
		{
			internal override Value ValueCopy() { return null; }

			internal Add(ValueType type1, ValueType type2, int additional)
			{
				m_additional = additional;
				List<Value> list = new List<Value>();
				list.Add(PatternData.Single("a", type1));
				list.Add(PatternData.Single("b", type2));
				Init(new ValueArray(list));
			}

			internal override Value Eval(Value arg, IScope scope)
			{
				List<Value> list = arg.AsArray;
				Value v1 = list[0];
				Value v2 = list[1];
				if (v1.Type == loki3.core.ValueType.Int && v2.Type == loki3.core.ValueType.Int)
					return new ValueInt(v1.AsInt + v2.AsInt + m_additional);
				return new ValueFloat(v1.AsForcedFloat + v2.AsForcedFloat + m_additional);
			}

			private int m_additional;
		}

		/// <summary>Empty postfix function</summary>
		class Post : ValueFunctionPost
		{
			internal override Value ValueCopy() { return null; }
			internal Post() { Init(PatternData.Single("blah")); }
			internal override Value Eval(Value arg, IScope scope) { return null; }
		}

		/// <summary>
		/// Make a parameter that's an array of two values,
		/// each of which could be an int or float
		/// </summary>
		private DelimiterNode MakePair(double a, double b, bool bInt1, bool bInt2)
		{
			List<Value> list = new List<Value>();
			Value value1, value2;
			if (bInt1)
				value1 = new ValueInt((int)a);
			else
				value1 = new ValueFloat(a);
			list.Add(value1);
			if (bInt2)
				value2 = new ValueInt((int)b);
			else
				value2 = new ValueFloat(b);
			list.Add(value2);
			return new DelimiterNodeValue(new ValueArray(list));
		}


		[Test]
		public void TestBasic()
		{
			FunctionOverload overload = new FunctionOverload();
			IScope scope = new ScopeChain();

			{	// add a single function & make sure it's called at right time & fails at right time
				ValueFunction numInt = new Add(ValueType.Number, ValueType.Int, 0);
				overload.Add(numInt);
				Value a = overload.Eval(null, MakePair(3, 4, true, true), scope, null, null);
				Assert.AreEqual(7, a.AsInt);
				Value b = overload.Eval(null, MakePair(3, 5, false, true), scope, null, null);
				Assert.AreEqual(8, b.AsFloat);

				bool bThrew = false;
				try
				{
					overload.Eval(null, MakePair(3, 4, false, false), scope, null, null);
				}
				catch (Loki3Exception)
				{
					bThrew = true;
				}
				Assert.IsTrue(bThrew);
			}

			{	// add a second function & make sure both succeed & fail at right time
				// this one is more specific so should get called first when signature matches
				ValueFunction intInt = new Add(ValueType.Int, ValueType.Int, 1);
				overload.Add(intInt);
				Value a = overload.Eval(null, MakePair(3, 4, true, true), scope, null, null);
				Assert.AreEqual(8, a.AsInt);	// calls 2nd version
				Value b = overload.Eval(null, MakePair(3, 5, false, true), scope, null, null);
				Assert.AreEqual(8, b.AsFloat);	// calls 1st version

				bool bThrew = false;
				try
				{	// still no match for this one
					overload.Eval(null, MakePair(3, 4, false, false), scope, null, null);
				}
				catch (Loki3Exception)
				{
					bThrew = true;
				}
				Assert.IsTrue(bThrew);
			}

			{	// try adding a postfix function to the overload
				bool bThrew = false;
				try
				{
					ValueFunction post = new Post();
					overload.Add(post);
				}
				catch (Loki3Exception e)
				{
					Assert.AreEqual("prefix", e.Errors[Loki3Exception.keyExpectedFix].ToString());
					Assert.AreEqual("postfix", e.Errors[Loki3Exception.keyActualFix].ToString());
					bThrew = true;
				}
				Assert.IsTrue(bThrew);
			}
		}
	}
}
