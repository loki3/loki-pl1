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
		String,
		Array,
		Map,
		Function,
		Delimiter,
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
		internal virtual bool AsBool { get { throw new WrongTypeException(ValueType.Bool, Type); } }
		internal virtual int AsInt { get { throw new WrongTypeException(ValueType.Int, Type); } }
		internal virtual double AsFloat { get { throw new WrongTypeException(ValueType.Float, Type); } }
		internal virtual double AsForcedFloat { get { throw new WrongTypeException(ValueType.Float, Type); } }
		internal virtual string AsString { get { throw new WrongTypeException(ValueType.String, Type); } }
		internal virtual List<Value> AsArray { get { throw new WrongTypeException(ValueType.Array, Type); } }
		internal virtual ValueMap AsMap { get { throw new WrongTypeException(ValueType.Map, Type); } }

		/// <summary>Get this value's metadata.  May be null.</summary>
		internal Dictionary<string, Value> Metadata { get { return m_metadata; } }

		/// <summary>Get this value's metadata, creating it if it doesn't already exist</summary>
		internal Dictionary<string, Value> WritableMetadata
		{
			get
			{
				if (m_metadata == null)
					m_metadata = new Dictionary<string, Value>();
				return m_metadata;
			}
		}

		#region Keys
		internal static string keyPrecedence = "l3.value.precedence";
		#endregion

		/// <summary>Evaluation precedence of this token</summary>
		internal Precedence Precedence
		{
			get
			{
				if (m_metadata == null || !m_metadata.ContainsKey(keyPrecedence))
					return Precedence.Low;
				return (Precedence)m_metadata[keyPrecedence].AsInt;
			}
		}

		private Dictionary<string, Value> m_metadata = null;
	}

	/// <summary>
	/// Nil value
	/// </summary>
	internal class ValueNil : Value
	{
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
	}

	/// <summary>
	/// Boolean value
	/// </summary>
	internal class ValueBool : Value
	{
		internal ValueBool(bool val)
		{
			m_val = val;
		}

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

		private bool m_val;
	}

	/// <summary>
	/// Int value
	/// </summary>
	internal class ValueInt : Value
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

		private int m_val;
	}

	/// <summary>
	/// Float value
	/// </summary>
	internal class ValueFloat : Value
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

		private double m_val;
	}

	/// <summary>
	/// String value
	/// </summary>
	internal class ValueString : Value
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

		public override string ToString() { return m_val; }

		private string m_val;
	}

	/// <summary>
	/// Map value
	/// </summary>
	internal class ValueMap : Value
	{
		internal ValueMap(Dictionary<string, Value> val)
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
			int count = m_val.Count;
			if (count != other.m_val.Count)
				return false;
			Dictionary<string, Value>.KeyCollection keys = m_val.Keys;
			foreach (string key in keys)
			{
				Value val;
				if (!other.m_val.TryGetValue(key, out val))
					return false;
				if (!m_val[key].Equals(val))
					return false;
			}
			return true;
		}

		internal override ValueMap AsMap { get { return this; } }
		#endregion

		/// <summary>Get a value by key</summary>
		internal Value this[string key]
		{
			get { return m_val[key]; }
		}

		/// <summary>Return value if present, else return a default</summary>
		internal Value GetOptional(string key, Value ifmissing)
		{
			Value value;
			if (m_val.TryGetValue(key, out value))
				return value;
			return ifmissing;
		}

		/// <summary>Get underlying map</summary>
		internal Dictionary<string, Value> Map
		{
			get { return m_val; }
		}

		public override string ToString() { return m_val.ToString(); }

		private Dictionary<string, Value> m_val;
	}
}
