using System;
using System.Collections.Generic;

namespace loki3.core
{
	/// <summary>built-in types of values</summary>
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

	/// <summary>built-in types of values</summary>
	internal class ValueClasses
	{
		/// <summary>get string representing class of value type</summary>
		static public string ClassOf(ValueType type) { return m_classes[(int)type]; }

		// NOTE: must match order of enum ValueType
		static private string[] m_classes = {
			"nil",
			"bool",
			"int",
			"float",
			"number",	// pseudo-type: int or float
			"string",
			"array",
			"map",
			"function",
			"delimiter",
			"rawLines",
			"rawLine",
		};
	}

	/// <summary>
	/// Represents a number, string, bool, map, array, function, etc.
	/// and can contain metadata
	/// </summary>
	internal abstract class Value
	{
		internal string Class { get { return ValueClasses.ClassOf(Type); } }
		internal abstract ValueType Type { get; }
		internal abstract bool Equals(Value v);

		/// <summary>Make a copy of the value (w/out metadata)</summary>
		internal abstract Value ValueCopy();

		/// <summary>Make a full copy of the value (w/ metadata)</summary>
		internal Value Copy()
		{
			Value other = ValueCopy();
			if (m_metadata != null)
				other.m_metadata = m_metadata.Copy();
			return other;
		}

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

		// dealing with values that are collections
		internal virtual int Count { get { return 1; } }

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
		internal static string keyCat = "l3.value.cat";
		internal static string keyType = "l3.value.type";
		internal static string keyEnumKey = "l3.enum.key";
		#endregion

		/// <summary>Evaluation precedence of this token</summary>
		internal Order Order
		{
			get
			{
				return m_metadata == null ? Order.Low : (Order)m_metadata.GetOptionalT<int>(keyOrder, (int)Order.Low);
			}
		}

		/// <summary>If this is an instance of a class or enum, return name of class, else return built-in type</summary>
		internal string MetaType
		{
			get
			{
				string s = null;
				if (m_metadata != null)
					s = m_metadata.GetOptionalT<string>(keyType, null);
				if (s == null)
					s = ValueClasses.ClassOf(Type);
				return s;
			}
			set
			{
				Map meta = WritableMetadata;
				meta[keyType] = new ValueString(value);
			}
		}

		/// <summary>If this is an enum value, return the key</summary>
		internal string EnumKey
		{
			get
			{
				return m_metadata == null ? "" : m_metadata.GetOptionalT<string>(keyEnumKey, "");
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

		internal override Value ValueCopy() { return new ValueNil(); }
		#endregion

		public override string ToString() { return "nil"; }

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

		internal override Value ValueCopy() { return new ValueBool(m_val); }

		internal override bool AsBool { get { return m_val; } }
		#endregion

		public override string ToString() { return m_val ? "true" : "false"; }

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
			if (other != null)
				return m_val == other.m_val;
			ValueFloat otherF = v as ValueFloat;
			return (otherF == null ? false : m_val == otherF.GetValue());
		}

		internal override Value ValueCopy() { return new ValueInt(m_val); }

		internal override int AsInt { get { return m_val; } }
		internal override double AsForcedFloat { get { return m_val; } }
		#endregion

		public override string ToString()
		{
			string enumkey = EnumKey;
			if (enumkey != "")
				return enumkey + "=" + m_val.ToString();
			return m_val.ToString();
		}
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
			if (other != null)
				return m_val == other.m_val;
			ValueInt otherI = v as ValueInt;
			return (otherI == null ? false : m_val == otherI.GetValue());
		}

		internal override Value ValueCopy() { return new ValueFloat(m_val); }

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

		internal override Value ValueCopy() { return new ValueString(m_val); }

		internal override string AsString { get { return m_val; } }
		#endregion

		public override string ToString() { return m_val; }
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

		internal override Value ValueCopy()
		{
			return new ValueMap(m_val.Copy());
		}

		internal override Map AsMap { get { return m_val; } }

		internal override int Count { get { return m_val.Count; } }
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
			if (a.AsMap.Raw != null)
				foreach (string key in a.AsMap.Raw.Keys)
					map[key] = a.AsMap[key];
			if (b.AsMap.Raw != null)
				foreach (string key in b.AsMap.Raw.Keys)
					map[key] = b.AsMap[key];
			return new ValueMap(map);
		}

		public override string ToString() { return m_val.ToString(); }

		#region Scope
		/// <summary>
		/// If this map represents a scope, get the corresponding IScope
		/// </summary>
		internal IScope Scope
		{
			get { return m_scope; }
			set { m_scope = value; }
		}
		private IScope m_scope = null;
		#endregion
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

		internal override Value ValueCopy() { return new ValueRaw(m_val); }
		#endregion

		public override string ToString()
		{
			return m_val.ToString();
		}
	}
}
