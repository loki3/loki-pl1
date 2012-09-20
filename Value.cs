using System;
using System.Collections.Generic;

namespace loki3
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

		// request value as a particular type
		internal virtual bool AsBool { get { throw new WrongTypeException(ValueType.Bool, Type); } }
		internal virtual int AsInt { get { throw new WrongTypeException(ValueType.Int, Type); } }
		internal virtual double AsFloat { get { throw new WrongTypeException(ValueType.Float, Type); } }
		internal virtual string AsString { get { throw new WrongTypeException(ValueType.String, Type); } }
		internal virtual List<Value> AsArray { get { throw new WrongTypeException(ValueType.Array, Type); } }

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
				if (!m_metadata.ContainsKey(keyPrecedence))
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

		internal override bool AsBool { get { return m_val; } }
		#endregion

		bool m_val;
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

		internal override int AsInt { get { return m_val; } }
		#endregion

		int m_val;
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

		internal override double AsFloat { get { return m_val; } }
		#endregion

		double m_val;
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

		internal override string AsString { get { return m_val; } }
		#endregion

		string m_val;
	}
}
