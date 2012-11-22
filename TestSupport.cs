using System.Collections.Generic;
using loki3.core;

namespace loki3.builtin.test
{
	/// <summary>[k1 v1 k2 v2 ...] -> map</summary>
	class CreateMap : ValueFunctionPre
	{
		internal override Value ValueCopy() { return new CreateMap(); }

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

	internal class TestSupport
	{
		static internal Value ToValue(string s, IScope scope)
		{
			DelimiterList list = ParseLine.Do(s, scope);
			return EvalList.Do(list.Nodes, scope);
		}
	}
}
