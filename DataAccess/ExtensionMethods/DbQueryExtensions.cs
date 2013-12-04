using System.Data.Entity.Infrastructure;
using System.Data.Objects;
using System.Linq;
using System.Reflection;

namespace DataAccess.ExtensionMethods
{
	public static class IDbQueryExtensions
	{
		public static string ToTraceString<T>(this DbQuery<T> query)
		{
			var internalQueryField = query.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Instance).FirstOrDefault(f => f.Name.Equals("_internalQuery"));
			if (internalQueryField == null)
			{
				return "[Could not retrieve internal query]";
			}

			var internalQuery = internalQueryField.GetValue(query);

			var objectQueryField = internalQuery.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Instance).FirstOrDefault(f => f.Name.Equals("_objectQuery"));
			if (objectQueryField == null)
			{
				return "[Could not retrieve object query]";
			}
			
			var objectQuery = objectQueryField.GetValue(internalQuery) as ObjectQuery<T>;
			return objectQuery.ToTraceStringWithParameters();
		}

		public static string ToTraceStringWithParameters<T>(this ObjectQuery<T> query)
		{
			var parameterStrings = query.Parameters.Select(p => string.Format("{0} [{1}] = {2}", p.Name, p.ParameterType.FullName, p.Value)).ToArray();
			return query.ToTraceString() + "\n" + string.Join("\n", parameterStrings);
		}
	}
}