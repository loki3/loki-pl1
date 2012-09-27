using System;
using System.Collections.Generic;
using loki3.core;

namespace loki3.builtin
{
	/// <summary>
	/// Built-in math functions
	/// </summary>
	class Math
	{
		/// <summary>
		/// Add built-in math functions to the scope
		/// </summary>
		internal static void Register(IScope scope)
		{
			scope.SetValue("l3.add", new AddArray());
			scope.SetValue("l3.subtract", new Subtract());
			scope.SetValue("l3.multiply", new MultiplyArray());
			scope.SetValue("l3.divide", new Divide());
			scope.SetValue("l3.sqrt", new SquareRoot());
		}


		/// <summary>[a1 a2 ... an] -> a1 + a2 + ... + an</summary>
		class AddArray : ValueFunctionPre
		{
			internal AddArray() { Init(DataForPatterns.Array("a", "ValueNumber")); }

			internal override Value Eval(DelimiterNode next, IScope scope, INodeRequestor nodes)
			{
				Value numbers = EvalNode.Do(next, scope, nodes);
				List<Value> list = numbers.AsArray;

				bool isResultInt = true;
				int iResult = 0;
				double dResult = 0;
				foreach (Value val in list)
				{
					if (isResultInt && val.Type == loki3.core.ValueType.Int)
					{
						iResult += val.AsInt;
					}
					else
					{
						if (isResultInt)
						{
							isResultInt = false;
							dResult = iResult;
						}
						dResult += val.AsForcedFloat;
					}
				}

				if (isResultInt)
					return new ValueInt(iResult);
				return new ValueFloat(dResult);
			}
		}

		/// <summary>[a1 a2] -> a1 - a2</summary>
		class Subtract : ValueFunctionPre
		{
			internal Subtract() { Init(DataForPatterns.ArrayElements("a", "b", "ValueNumber")); }

			internal override Value Eval(DelimiterNode next, IScope scope, INodeRequestor nodes)
			{
				Value numbers = EvalNode.Do(next, scope, nodes);
				List<Value> list = numbers.AsArray;
				if (list.Count != 2)
					throw new WrongSizeArray(2, list.Count);

				Value v1 = list[0];
				Value v2 = list[1];

				// keep everything as ints
				if (v1.Type == loki3.core.ValueType.Int && v2.Type == loki3.core.ValueType.Int)
					return new ValueInt(v1.AsInt - v2.AsInt);

				// do math as floats
				return new ValueFloat(v1.AsForcedFloat - v2.AsForcedFloat);
			}
		}

		/// <summary>[a1 a2 ... an] -> a1 * a2 * ... * an</summary>
		class MultiplyArray : ValueFunctionPre
		{
			internal MultiplyArray() { Init(DataForPatterns.Array("a", "ValueNumber")); }

			internal override Value Eval(DelimiterNode next, IScope scope, INodeRequestor nodes)
			{
				Value numbers = EvalNode.Do(next, scope, nodes);
				List<Value> list = numbers.AsArray;

				bool isResultInt = true;
				int iResult = 1;
				double dResult = 1;
				foreach (Value val in list)
				{
					if (isResultInt && val.Type == loki3.core.ValueType.Int)
					{
						iResult *= val.AsInt;
					}
					else
					{
						if (isResultInt)
						{
							isResultInt = false;
							dResult = iResult;
						}
						dResult *= val.AsForcedFloat;
					}
				}

				if (isResultInt)
					return new ValueInt(iResult);
				return new ValueFloat(dResult);
			}
		}

		/// <summary>[a1 a2] -> a1 / a2</summary>
		class Divide : ValueFunctionPre
		{
			internal Divide() { Init(DataForPatterns.ArrayElements("a", "b", "ValueNumber")); }

			internal override Value Eval(DelimiterNode next, IScope scope, INodeRequestor nodes)
			{
				Value numbers = EvalNode.Do(next, scope, nodes);
				List<Value> list = numbers.AsArray;
				if (list.Count != 2)
					throw new WrongSizeArray(2, list.Count);

				Value v1 = list[0];
				Value v2 = list[1];

				// keep everything as ints
				if (v1.Type == loki3.core.ValueType.Int && v2.Type == loki3.core.ValueType.Int)
					return new ValueInt(v1.AsInt / v2.AsInt);

				// do math as floats
				return new ValueFloat(v1.AsForcedFloat / v2.AsForcedFloat);
			}
		}

		/// <summary>a -> sqrt(a)</summary>
		class SquareRoot : ValueFunctionPre
		{
			internal SquareRoot() { Init(DataForPatterns.Single("a", "ValueNumber")); }

			internal override Value Eval(DelimiterNode next, IScope scope, INodeRequestor nodes)
			{
				Value number = EvalNode.Do(next, scope, nodes);
				double a = number.AsForcedFloat;
				return new ValueFloat(System.Math.Sqrt(a));
			}
		}
	}
}
