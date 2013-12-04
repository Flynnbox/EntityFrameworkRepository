using System.Data;
using System.Data.Common;

namespace DataAccess.ExtensionMethods
{
	public static class IDbCommandExtensions
	{
		public static void AddParameter(this DbCommand command, string name, DbType type, object value, bool isNullable = false)
		{
			var parameter = command.CreateParameter();
			parameter.ParameterName = name;
			parameter.DbType = type;
			parameter.Value = value;
			parameter.IsNullable = isNullable;

			command.Parameters.Add(parameter);
		}

		public static void AddOutputParameter(this DbCommand command, string name, DbType type, object value, bool isNullable = false)
		{
			var parameter = command.CreateParameter();
			parameter.ParameterName = name;
			parameter.DbType = type;
			parameter.Value = value;
			parameter.IsNullable = isNullable;
			parameter.Direction = ParameterDirection.Output;

			command.Parameters.Add(parameter);
		}

		public static void AddInputOutputParameter(this DbCommand command, string name, DbType type, object value, bool isNullable = false)
		{
			var parameter = command.CreateParameter();
			parameter.ParameterName = name;
			parameter.DbType = type;
			parameter.Value = value;
			parameter.IsNullable = isNullable;
			parameter.Direction = ParameterDirection.Output;

			command.Parameters.Add(parameter);
		}
	}
}