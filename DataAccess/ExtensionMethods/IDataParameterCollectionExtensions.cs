using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace DataAccess.ExtensionMethods
{
	public static class IDataParameterCollectionExtensions
	{
		public static string GetParameterString(this IDataParameterCollection collection)
		{
			string parameterStrings = "[no parameters]";
			if (collection != null && collection.Count > 0)
			{
				IDataParameter[] sqlParams = new IDataParameter[collection.Count];
				collection.CopyTo(sqlParams, 0);
				return sqlParams.GetParameterString();
			}
			return parameterStrings;
		}
	}

	public static class IDataParameterExtensions
	{
		public static string GetParameterString(this IEnumerable<IDataParameter> parameters)
		{
			string parameterStrings = "[no parameters]";
			if (parameters.Any())
			{
				var strings = parameters.Select(p => string.Format("@{0} = '{1}'", p.ParameterName, p.Value));
				parameterStrings = string.Join(", ", strings);
			}
			return parameterStrings;
		}
	}
}