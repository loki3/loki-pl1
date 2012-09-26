using System;
using System.Collections.Generic;

namespace loki3.core
{
	/// <summary>
	/// Describes the metadata associated with a function parameter
	/// </summary>
	internal class DataForParameter
	{
		/// <summary>
		/// Gets the metadata from a parameter name value
		/// </summary>
		/// <param name="name">object representing name of parameter</param>
		internal DataForParameter(ValueString name)
		{
			m_metadata = name.Metadata;
		}

		#region Keys
		internal static string keyType = "l3.param.type";
		internal static string keyOneOf = "l3.param.oneOf";
		internal static string keyRequiredKeys = "l3.param.keys";
		internal static string keyDefault = "l3.param.default";
		#endregion

		/// <summary>If present, required type of parameter</summary>
		internal ValueString TypeName
		{
			get { return m_metadata == null ? null : m_metadata.GetOptional(keyType, null) as ValueString; }
		}

		/// <summary>If present, value must be one of the values in the array</summary>
		internal ValueArray OneOf
		{
			get { return m_metadata == null ? null : m_metadata.GetOptional(keyOneOf, null) as ValueArray; }
		}

		/// <summary>If present, value must be a map with all the listed keys</summary>
		internal ValueArray RequiredKeys
		{
			get { return m_metadata == null ? null : m_metadata.GetOptional(keyRequiredKeys, null) as ValueArray; }
		}

		/// <summary>If present, the parameter will default to this value if not passed</summary>
		internal Value Default
		{
			get { return m_metadata == null ? null : m_metadata.GetOptional(keyDefault, null); }
		}

		private Map m_metadata;
	}
}
