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
			scope.SetValue("l3.add", new Add());
			scope.SetValue("l3.addArray", new AddArray());
			scope.SetValue("l3.subtract", new Subtract());
			scope.SetValue("l3.multiply", new Multiply());
			scope.SetValue("l3.multiplyArray", new MultiplyArray());
			scope.SetValue("l3.divide", new Divide());
			scope.SetValue("l3.modulo", new Modulo());
			scope.SetValue("l3.sqrt", new SquareRoot());
			scope.SetValue("l3.lt", new LessThan());
			scope.SetValue("l3.gt", new GreaterThan());
		}


		/// <summary>[a1 a2] -> a1 + a2</summary>
		class Add : ValueFunctionPre
		{
			internal override Value ValueCopy() { return new Add(); }

			internal Add()
			{
				SetDocString("Return [0] + [1].");
				List<Value> list = new List<Value>();
				list.Add(PatternData.Single("a", ValueType.Number));
				list.Add(PatternData.Single("b", ValueType.Number));
				ValueArray array = new ValueArray(list);
				Init(array);
			}

			internal override Value Eval(Value arg, IScope scope)
			{
				List<Value> list = arg.AsArray;
				Value v1 = list[0];
				Value v2 = list[1];

				// keep everything as ints
				if (v1.Type == loki3.core.ValueType.Int && v2.Type == loki3.core.ValueType.Int)
					return new ValueInt(v1.AsInt + v2.AsInt);

				// do math as floats
				return new ValueFloat(v1.AsForcedFloat + v2.AsForcedFloat);
			}
		}

		/// <summary>[a1 a2 ... an] -> a1 + a2 + ... + an</summary>
		class AddArray : ValueFunctionPre
		{
			internal override Value ValueCopy() { return new AddArray(); }

			internal AddArray()
			{
				SetDocString("Return sum of the numbers in the array.");
				Init(PatternData.Rest("a", ValueType.Number));
			}

			internal override Value Eval(Value arg, IScope scope)
			{
				List<Value> list = arg.AsArray;

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
			internal override Value ValueCopy() { return new Subtract(); }

			internal Subtract()
			{
				SetDocString("Return [0] - [1].");
				List<Value> list = new List<Value>();
				list.Add(PatternData.Single("a", ValueType.Number));
				list.Add(PatternData.Single("b", ValueType.Number));
				ValueArray array = new ValueArray(list);
				Init(array);
			}

			internal override Value Eval(Value arg, IScope scope)
			{
				List<Value> list = arg.AsArray;
				Value v1 = list[0];
				Value v2 = list[1];

				// keep everything as ints
				if (v1.Type == loki3.core.ValueType.Int && v2.Type == loki3.core.ValueType.Int)
					return new ValueInt(v1.AsInt - v2.AsInt);

				// do math as floats
				return new ValueFloat(v1.AsForcedFloat - v2.AsForcedFloat);
			}
		}

		/// <summary>[a1 a2] -> a1 * a2</summary>
		class Multiply : ValueFunctionPre
		{
			internal override Value ValueCopy() { return new Multiply(); }

			internal Multiply()
			{
				SetDocString("Return [0] * [1].");
				List<Value> list = new List<Value>();
				list.Add(PatternData.Single("a", ValueType.Number));
				list.Add(PatternData.Single("b", ValueType.Number));
				ValueArray array = new ValueArray(list);
				Init(array);
			}

			internal override Value Eval(Value arg, IScope scope)
			{
				List<Value> list = arg.AsArray;
				Value v1 = list[0];
				Value v2 = list[1];

				// keep everything as ints
				if (v1.Type == loki3.core.ValueType.Int && v2.Type == loki3.core.ValueType.Int)
					return new ValueInt(v1.AsInt * v2.AsInt);

				// do math as floats
				return new ValueFloat(v1.AsForcedFloat * v2.AsForcedFloat);
			}
		}

		/// <summary>[a1 a2 ... an] -> a1 * a2 * ... * an</summary>
		class MultiplyArray : ValueFunctionPre
		{
			internal override Value ValueCopy() { return new MultiplyArray(); }

			internal MultiplyArray()
			{
				SetDocString("Return product of the numbers in the array.");
				Init(PatternData.Rest("a", ValueType.Number));
			}

			internal override Value Eval(Value arg, IScope scope)
			{
				List<Value> list = arg.AsArray;

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
			internal override Value ValueCopy() { return new Divide(); }

			internal Divide()
			{
				SetDocString("Return [0] / [1].");

				List<Value> list = new List<Value>();
				list.Add(PatternData.Single("a", ValueType.Number));
				list.Add(PatternData.Single("b", ValueType.Number));
				ValueArray array = new ValueArray(list);
				Init(array);
			}

			internal override Value Eval(Value arg, IScope scope)
			{
				List<Value> list = arg.AsArray;
				Value v1 = list[0];
				Value v2 = list[1];

				// keep everything as ints
				if (v1.Type == loki3.core.ValueType.Int && v2.Type == loki3.core.ValueType.Int)
					return new ValueInt(v1.AsInt / v2.AsInt);

				// do math as floats
				return new ValueFloat(v1.AsForcedFloat / v2.AsForcedFloat);
			}
		}

		/// <summary>[a1 a2] -> a1 % a2</summary>
		class Modulo : ValueFunctionPre
		{
			internal override Value ValueCopy() { return new Modulo(); }

			internal Modulo()
			{
				SetDocString("Return [0] % [1].");

				List<Value> list = new List<Value>();
				list.Add(PatternData.Single("a", ValueType.Number));
				list.Add(PatternData.Single("b", ValueType.Number));
				ValueArray array = new ValueArray(list);
				Init(array);
			}

			internal override Value Eval(Value arg, IScope scope)
			{
				List<Value> list = arg.AsArray;
				Value v1 = list[0];
				Value v2 = list[1];

				// keep everything as ints
				if (v1.Type == loki3.core.ValueType.Int && v2.Type == loki3.core.ValueType.Int)
					return new ValueInt(v1.AsInt % v2.AsInt);

				// do math as floats
				return new ValueFloat(v1.AsForcedFloat % v2.AsForcedFloat);
			}
		}

		/// <summary>a -> sqrt(a)</summary>
		class SquareRoot : ValueFunctionPre
		{
			internal override Value ValueCopy() { return new SquareRoot(); }

			internal SquareRoot()
			{
				SetDocString("Return square root of number.");
				Init(PatternData.Single("a", ValueType.Number));
			}

			internal override Value Eval(Value arg, IScope scope)
			{
				double a = arg.AsForcedFloat;
				return new ValueFloat(System.Math.Sqrt(a));
			}
		}

		/// <summary>[a1 a2] -> a1 < a2</summary>
		class LessThan : ValueFunctionPre
		{
			internal override Value ValueCopy() { return new LessThan(); }

			internal LessThan()
			{
				SetDocString("Return [0] < [1].");

				List<Value> list = new List<Value>();
				list.Add(PatternData.Single("a", ValueType.Number));
				list.Add(PatternData.Single("b", ValueType.Number));
				ValueArray array = new ValueArray(list);
				Init(array);
			}

			internal override Value Eval(Value arg, IScope scope)
			{
				List<Value> list = arg.AsArray;
				Value v1 = list[0];
				Value v2 = list[1];

				// keep everything as ints
				if (v1.Type == loki3.core.ValueType.Int && v2.Type == loki3.core.ValueType.Int)
					return new ValueBool(v1.AsInt < v2.AsInt);

				// do math as floats
				return new ValueBool(v1.AsForcedFloat < v2.AsForcedFloat);
			}
		}

		/// <summary>[a1 a2] -> a1 > a2</summary>
		class GreaterThan : ValueFunctionPre
		{
			internal override Value ValueCopy() { return new GreaterThan(); }

			internal GreaterThan()
			{
				SetDocString("Return [0] > [1].");

				List<Value> list = new List<Value>();
				list.Add(PatternData.Single("a", ValueType.Number));
				list.Add(PatternData.Single("b", ValueType.Number));
				ValueArray array = new ValueArray(list);
				Init(array);
			}

			internal override Value Eval(Value arg, IScope scope)
			{
				List<Value> list = arg.AsArray;
				Value v1 = list[0];
				Value v2 = list[1];

				// keep everything as ints
				if (v1.Type == loki3.core.ValueType.Int && v2.Type == loki3.core.ValueType.Int)
					return new ValueBool(v1.AsInt > v2.AsInt);

				// do math as floats
				return new ValueBool(v1.AsForcedFloat > v2.AsForcedFloat);
			}
		}
	}
}