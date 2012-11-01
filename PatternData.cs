using System;
using System.Collections.Generic;

namespace loki3.core
{
	/// <summary>
	/// Describes the metadata associated with a function parameter
	/// </summary>
	internal class PatternData
	{
		/// <summary>Parameter that's a single named value</summary>
		internal static Value Single(string name)
		{
			ValueString vName = new ValueString(name);
			PatternData data = new PatternData(vName);
			return vName;
		}

		/// <summary>Parameter that's a single named value with a default</summary>
		internal static Value Single(string name, Value theDefault)
		{
			ValueString vName = new ValueString(name);
			PatternData data = new PatternData(vName);
			data.Default = theDefault;
			return vName;
		}

		/// <summary>Parameter that's a single named value of the given type</summary>
		internal static Value Single(string name, ValueType type)
		{
			ValueString vName = new ValueString(name);
			PatternData data = new PatternData(vName);
			data.ValueType = type;
			return vName;
		}

		/// <summary>Parameter that's a single named value of the given type with a default</summary>
		internal static Value Single(string name, ValueType type, Value theDefault)
		{
			ValueString vName = new ValueString(name);
			PatternData data = new PatternData(vName);
			data.ValueType = type;
			data.Default = theDefault;
			return vName;
		}

		/// <summary>Parameter that's the remainder of an array</summary>
		internal static Value ArrayEnd(string name)
		{
			ValueString vName = new ValueString(name);
			PatternData data = new PatternData(vName);
			data.UseRest = true;
			return vName;
		}

		/// <summary>Parameter that indicates that a "body" is required, i.e. an array of strings</summary>
		internal static Value Body()
		{
			ValueString vName = new ValueString("l3.body");
			PatternData data = new PatternData(vName);
			data.ValueType = ValueType.String;
			return vName;
		}

		/// <summary>Parameter that's the remainder of an array or map</summary>
		internal static Value Rest(string name)
		{
			ValueString vName = new ValueString(name);
			PatternData data = new PatternData(vName);
			data.UseRest = true;
			return vName;
		}

		/// <summary>Parameter that's the remainder of an array of a given type</summary>
		internal static Value Rest(string name, ValueType type)
		{
			ValueString vName = new ValueString(name);
			PatternData data = new PatternData(vName);
			data.ValueType = type;
			data.UseRest = true;
			return vName;
		}


		/// <summary>
		/// Gets the metadata from a parameter name value
		/// </summary>
		/// <param name="name">object representing name of parameter</param>
		internal PatternData(ValueString name)
		{
			m_object = name;
			m_readableMetadata = (name == null ? null : name.Metadata);
			m_writableMetadata = null;
		}

		#region Keys
		internal static string keyType = "l3.param.type";
		internal static string keyOneOf = "l3.param.oneOf";
		internal static string keyRequiredKeys = "l3.param.keys";
		internal static string keyDefault = "l3.param.default";
		internal static string keyRest = "l3.param.rest";
		internal static string keyIsBody = "l3.param.body?";
		#endregion

		/// <summary>If present, required type of parameter</summary>
		internal ValueType ValueType
		{
			get { return m_readableMetadata == null ? ValueType.Nil : (ValueType)m_readableMetadata.GetOptionalT<int>(keyType, (int)ValueType.Nil); }
			set { WritableMetadata[keyType] = new ValueInt((int)value); }
		}

		/// <summary>If present, value must be one of the values in the array</summary>
		internal ValueArray OneOf
		{
			get { return m_readableMetadata == null ? null : m_readableMetadata.GetOptional(keyOneOf, null) as ValueArray; }
			set { WritableMetadata[keyOneOf] = value; }
		}

		/// <summary>If present, value must be a map with all the listed keys</summary>
		internal ValueArray RequiredKeys
		{
			get { return m_readableMetadata == null ? null : m_readableMetadata.GetOptional(keyRequiredKeys, null) as ValueArray; }
			set { WritableMetadata[keyRequiredKeys] = value; }
		}

		/// <summary>If present, the parameter will default to this value if not passed</summary>
		internal Value Default
		{
			get { return m_readableMetadata == null ? null : m_readableMetadata.GetOptional(keyDefault, null); }
			set { WritableMetadata[keyDefault] = value; }
		}

		/// <summary>If present, the parameter will contain the function body, i.e. an array of strings</summary>
		internal bool IsBody
		{
			get { return m_readableMetadata == null ? false : m_readableMetadata.GetOptionalT<bool>(keyIsBody, false); }
			set { WritableMetadata[keyIsBody] = new ValueBool(value); }
		}

		/// <summary>If present, the parameter will contain the portion of the array or map that hasn't otherwise been used</summary>
		internal bool UseRest
		{
			get { return m_readableMetadata == null ? false : m_readableMetadata.GetOptionalT<bool>(keyRest, false); }
			set { WritableMetadata[keyRest] = new ValueBool(value); }
		}

		/// <summary>Make metadata writable if it isn't already</summary>
		private Map WritableMetadata
		{
			get
			{
				if (m_writableMetadata == null)
					m_writableMetadata = m_object.WritableMetadata;
				return m_writableMetadata;
			}
		}

		private ValueString m_object;
		private Map m_readableMetadata;
		private Map m_writableMetadata;
	}
}
