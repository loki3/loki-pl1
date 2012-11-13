using System;
using System.Collections.Generic;

namespace loki3.core
{
	internal enum ValueType
	{
		Nil,
		Bool,
		Int,
		Float,
		Number,		// pseudo-type: Int or Float
		String,
		Array,
		Map,
		Function,
		Delimiter,
		Raw,		// DelimiterNode
		RawList,	// DelimiterList
	}

	/// <summary>
	/// Represents a number, string, bool, map, array, function, etc.
	/// and can contain metadata
	/// </summary>
	internal abstract class Value
	{
		internal abstract ValueType Type { get; }
		internal abstract bool Equals(Value v);

		// request value as a particular type
		internal virtual bool IsNil { get { return false; } }
		internal virtual bool AsBool { get { throw new Loki3Exception().AddWrongType(ValueType.Bool, Type); } }
		internal virtual int AsInt { get { throw new Loki3Exception().AddWrongType(ValueType.Int, Type); } }
		internal virtual double AsFloat { get { throw new Loki3Exception().AddWrongType(ValueType.Float, Type); } }
		internal virtual double AsForcedFloat { get { throw new Loki3Exception().AddWrongType(ValueType.Float, Type); } }
		internal virtual string AsString { get { throw new Loki3Exception().AddWrongType(ValueType.String, Type); } }
		internal virtual List<Value> AsArray { get { throw new Loki3Exception().AddWrongType(ValueType.Array, Type); } }
		internal virtual Map AsMap { get { throw new Loki3Exception().AddWrongType(ValueType.Map, Type); } }
		internal virtual List<DelimiterList> AsLine { get { throw new Loki3Exception().AddWrongType(ValueType.RawList, Type); } }

		/// <summary>Get this value's metadata.  May be null.</summary>
		internal Map Metadata { get { return m_metadata; } }

		/// <summary>Get this value's metadata, creating it if it doesn't already exist</summary>
		internal Map WritableMetadata
		{
			get
			{
				if (m_metadata == null)
					m_metadata = new Map();
				return m_metadata;
			}
		}

		/// <summary>Add a description of this value</summary>
		internal void SetDocString(string doc)
		{
			Map meta = WritableMetadata;
			meta[keyDoc] = new ValueString(doc);
		}

		#region Keys
		internal static string keyOrder = "l3.value.order";
		internal static string keyDoc = "l3.value.doc";
		#endregion

		/// <summary>Evaluation precedence of this token</summary>
		internal Order Order
		{
			get
			{
				return m_metadata == null ? Order.Low : (Order)m_metadata.GetOptionalT<int>(keyOrder, (int)Order.Low);
			}
		}

		private Map m_metadata = null;
	}

	/// <summary>
	/// Nil value
	/// </summary>
	internal class ValueNil : Value
	{
		internal static ValueNil Nil { get { return m_nil; } }
		internal override bool IsNil { get { return true; } }

		#region Value
		internal override ValueType Type
		{
			get { return ValueType.Nil; }
		}

		internal override bool Equals(Value v)
		{
			return v is ValueNil;
		}
		#endregion

		private static ValueNil m_nil = new ValueNil();
	}


	/// <summary>
	/// Generic base class for value of a specific type
	/// </summary>
	internal abstract class ValueBase<T> : Value
	{
		internal T GetValue() { return m_val; }

		protected T m_val;
	}

	/// <summary>
	/// Boolean value
	/// </summary>
	internal class ValueBool : ValueBase<bool>
	{
		internal ValueBool(bool val)
		{
			m_val = val;
		}

		internal static ValueBool True { get { return m_true; } }
		internal static ValueBool False { get { return m_false; } }

		#region Value
		internal override ValueType Type
		{
			get { return ValueType.Bool; }
		}

		internal override bool Equals(Value v)
		{
			ValueBool other = v as ValueBool;
			return (other == null ? false : m_val == other.m_val);
		}

		internal override bool AsBool { get { return m_val; } }
		#endregion

		public override string ToString() { return m_val.ToString(); }

		private static ValueBool m_true = new ValueBool(true);
		private static ValueBool m_false = new ValueBool(false);
	}

	/// <summary>
	/// Int value
	/// </summary>
	internal class ValueInt : ValueBase<int>
	{
		internal ValueInt(int val)
		{
			m_val = val;
		}

		#region Value
		internal override ValueType Type
		{
			get { return ValueType.Int; }
		}

		internal override bool Equals(Value v)
		{
			ValueInt other = v as ValueInt;
			return (other == null ? false : m_val == other.m_val);
		}

		internal override int AsInt { get { return m_val; } }
		internal override double AsForcedFloat { get { return m_val; } }
		#endregion

		public override string ToString() { return m_val.ToString(); }
	}

	/// <summary>
	/// Float value
	/// </summary>
	internal class ValueFloat : ValueBase<double>
	{
		internal ValueFloat(double val)
		{
			m_val = val;
		}

		#region Value
		internal override ValueType Type
		{
			get { return ValueType.Float; }
		}

		internal override bool Equals(Value v)
		{
			ValueFloat other = v as ValueFloat;
			return (other == null ? false : m_val == other.m_val);
		}

		internal override double AsFloat { get { return m_val; } }
		internal override double AsForcedFloat { get { return m_val; } }
		#endregion

		public override string ToString() { return m_val.ToString(); }
	}

	/// <summary>
	/// String value
	/// </summary>
	internal class ValueString : ValueBase<string>
	{
		internal ValueString(string val)
		{
			m_val = val;
		}

		#region Value
		internal override ValueType Type
		{
			get { return ValueType.String; }
		}

		internal override bool Equals(Value v)
		{
			ValueString other = v as ValueString;
			return (other == null ? false : m_val == other.m_val);
		}

		internal override string AsString { get { return m_val; } }
		#endregion

		public override string ToString() { return "\"" + m_val + "\""; }
	}

	/// <summary>
	/// Map value
	/// </summary>
	internal class ValueMap : ValueBase<Map>
	{
		internal ValueMap(Map val)
		{
			m_val = val;
		}

		#region Value
		internal override ValueType Type
		{
			get { return ValueType.Map; }
		}

		internal override bool Equals(Value v)
		{
			ValueMap other = v as ValueMap;
			if (other == null)
				return false;
			return m_val.Equals(other.m_val);
		}

		internal override Map AsMap { get { return m_val; } }
		#endregion

		/// <summary>Get a value by key</summary>
		internal Value this[string key]
		{
			get { return m_val[key]; }
		}

		/// <summary>Combine a and b into a new map</summary>
		internal static ValueMap Combine(ValueMap a, ValueMap b)
		{
			Map map = new Map();
			foreach (string key in a.AsMap.Raw.Keys)
				map[key] = a.AsMap[key];
			foreach (string key in b.AsMap.Raw.Keys)
				map[key] = b.AsMap[key];
			return new ValueMap(map);
		}

		public override string ToString() { return m_val.ToString(); }
	}

	/// <summary>
	/// Unevaled nodes
	/// </summary>
	internal class ValueRaw : ValueBase<DelimiterList>
	{
		internal ValueRaw(DelimiterList val)
		{
			m_val = val;
		}

		#region Value
		internal override ValueType Type
		{
			get { return ValueType.Raw; }
		}

		internal override bool Equals(Value v)
		{
			ValueRaw other = v as ValueRaw;
			if (other == null)
				return false;
			return m_val.Equals(other.m_val);
		}
		#endregion
	}
}
