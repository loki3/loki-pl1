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

			internal Add(ValueType type1, ValueType type2)
			{
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
					return new ValueInt(v1.AsInt + v2.AsInt);
				return new ValueFloat(v1.AsForcedFloat + v2.AsForcedFloat);
			}
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
				ValueFunction numInt = new Add(ValueType.Number, ValueType.Int);
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
		}
	}
}
