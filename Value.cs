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
		Number,	// pseudo-type: Int or Float
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
		internal virtual Map AsMap { get { throw new WrongTypeException(ValueType.Map, Type); } }

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

		#region Keys
		internal static string keyPrecedence = "l3.value.precedence";
		internal static string keyDoc = "l3.value.doc";
		#endregion

		/// <summary>Evaluation precedence of this token</summary>
		internal Precedence Precedence
		{
			get
			{
				return m_metadata == null ? Precedence.Low : (Precedence)m_metadata.GetOptional<int>(keyPrecedence, (int)Precedence.Low);
			}
		}

		private Map m_metadata = null;
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

		internal override Map AsMap { get { return m_val; } }
		#endregion

		/// <summary>Get a value by key</summary>
		internal Value this[string key]
		{
			get { return m_val[key]; }
		}

		public override string ToString() { return m_val.ToString(); }
	}
}
