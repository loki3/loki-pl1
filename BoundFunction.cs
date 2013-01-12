using System.Collections.Generic;

namespace loki3.core
{
	/// <summary>
	/// A function that's bound to a scope
	/// </summary>
	internal class BoundFunction : ValueFunction
	{
		internal BoundFunction(ValueFunction function, IScope scope)
		{
			m_function = function;
			m_scope = scope;

			foreach (string key in function.Metadata.Raw.Keys)
				WritableMetadata[key] = function.Metadata[key];
		}

		#region ValueFunction
		internal override Value Eval(DelimiterNode prev, DelimiterNode next, IScope scope, INodeRequestor nodes, ILineRequestor requestor)
		{
			return m_function.Eval(prev, next, m_scope, nodes, requestor);
		}

		internal override Value ValueCopy()
		{
			return new BoundFunction(m_function, m_scope);
		}
		#endregion

		private ValueFunction m_function;
		private IScope m_scope;
	}
}
