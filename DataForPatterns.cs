using System;
using System.Collections.Generic;

namespace loki3.core
{
	/// <summary>
	/// Describes the metadata associated with a function parameter
	/// </summary>
	internal class DataForPatterns
	{
		/// <summary>Parameter that's a single named value</summary>
		internal static Value Single(string name)
		{
			ValueString vName = new ValueString(name);
			DataForPatterns data = new DataForPatterns(vName);
			return vName;
		}

		/// <summary>Parameter that's a single named value with a default</summary>
		internal static Value Single(string name, Value theDefault)
		{
			ValueString vName = new ValueString(name);
			DataForPatterns data = new DataForPatterns(vName);
			data.Default = theDefault;
			return vName;
		}

		/// <summary>Parameter that's a single named value of the given type</summary>
		internal static Value Single(string name, string type)
		{
			ValueString vName = new ValueString(name);
			DataForPatterns data = new DataForPatterns(vName);
			data.TypeName = new ValueString(type);
			return vName;
		}

		/// <summary>Parameter that's a single named value of the given type with a default</summary>
		internal static Value Single(string name, string type, Value theDefault)
		{
			ValueString vName = new ValueString(name);
			DataForPatterns data = new DataForPatterns(vName);
			data.TypeName = new ValueString(type);
			data.Default = theDefault;
			return vName;
		}

		/// <summary>Parameter that's the remainder of an array</summary>
		internal static Value ArrayEnd(string name)
		{
			ValueString vName = new ValueString(name);
			DataForPatterns data = new DataForPatterns(vName);
			data.RestOfArray = true;
			return vName;
		}

		/// <summary>Parameter that's the remainder of an array of a given type</summary>
		internal static Value ArrayEnd(string name, string type)
		{
			ValueString vName = new ValueString(name);
			DataForPatterns data = new DataForPatterns(vName);
			data.TypeName = new ValueString(type);
			data.RestOfArray = true;
			return vName;
		}

		/// <summary>Parameter that's the remainder of an array</summary>
		internal static Value Body()
		{
			ValueString vName = new ValueString("l3.body");
			DataForPatterns data = new DataForPatterns(vName);
			data.TypeName = new ValueString("ValueString");
			data.RestOfArray = true;
			return vName;
		}


		/// <summary>
		/// Gets the metadata from a parameter name value
		/// </summary>
		/// <param name="name">object representing name of parameter</param>
		internal DataForPatterns(ValueString name)
		{
			m_object = name;
			m_readableMetadata = name.Metadata;
			m_writableMetadata = null;
		}

		#region Keys
		internal static string keyType = "l3.param.type";
		internal static string keyOneOf = "l3.param.oneOf";
		internal static string keyRequiredKeys = "l3.param.keys";
		internal static string keyDefault = "l3.param.default";
		internal static string keyRestOfArray = "l3.param.rest";
		internal static string keyIsBody = "l3.param.body?";
		#endregion

		/// <summary>If present, required type of parameter</summary>
		internal ValueString TypeName
		{
			get { return m_readableMetadata == null ? null : m_readableMetadata.GetOptional(keyType, null) as ValueString; }
			set { WritableMetadata[keyType] = value; }
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

		/// <summary>If present, the parameter will contain the portion of the array that hasn't otherwise been used</summary>
		internal bool RestOfArray
		{
			get { return m_readableMetadata == null ? false : m_readableMetadata.GetOptional<bool>(keyRestOfArray, false); }
			set { WritableMetadata[keyRestOfArray] = new ValueBool(value); }
		}

		/// <summary>If present, the parameter will contain the function body, i.e. an array of strings</summary>
		internal bool IsBody
		{
			get { return m_readableMetadata == null ? false : m_readableMetadata.GetOptional<bool>(keyIsBody, false); }
			set { WritableMetadata[keyIsBody] = new ValueBool(value); }
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
