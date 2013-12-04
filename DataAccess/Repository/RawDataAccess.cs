using System;
using System.Data;
using System.Data.Common;
using System.Data.Entity;
using System.Reflection;
using DataAccess.ExtensionMethods;
using log4net;

namespace DataAccess.Repository
{
	public class RawDataAccess
	{
		private readonly DbContext context;
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		public RawDataAccess(DbContext context)
		{
			this.context = context;
		}

		public DbCommand GetCommand(string sql)
		{
			var command = context.Database.Connection.CreateCommand();
			command.CommandText = sql;
			command.CommandType = CommandType.StoredProcedure;
			return command;
		}

		public DbCommand GetSqlText(string sql)
		{
			var command = context.Database.Connection.CreateCommand();
			command.CommandText = sql;
			command.CommandType = CommandType.Text;
			return command;
		}

		public void Execute(IDbCommand command)
		{
			ExecuteCommand(command, cmd => cmd.ExecuteNonQuery());
		}

		public T GetScalar<T>(IDbCommand command)
		{
			return ExecuteCommand(command, cmd =>
			                               {
				                               object result = cmd.ExecuteScalar();
				                               if (!(result is T))
				                               {
					                               throw new InvalidOperationException(string.Format("Scalar result {0} cannot be cast to Type {1}", result, typeof (T)));
				                               }
				                               return (T) result;
			                               }
				);
		}

		public DataTable GetDataTable(IDbCommand command)
		{
			return ExecuteCommand(command, cmd =>
			                               {
				                               using (var reader = cmd.ExecuteReader())
				                               {
					                               if (reader == null)
					                               {
						                               throw new InvalidOperationException("Cannot load DataTable as DataReader is null.");
					                               }

					                               var dataTable = new DataTable();
					                               dataTable.Load(reader);
					                               return dataTable;
				                               }
			                               });
		}

		public DataSet GetDataSet(IDbCommand command)
		{
			return ExecuteCommand(command, cmd =>
			                               {
				                               using (var reader = cmd.ExecuteReader())
				                               {
					                               if (reader == null)
					                               {
						                               throw new InvalidOperationException("Cannot load DataTable as DataReader is null.");
					                               }
					                               var dataSet = new DataSet();

					                               while (!reader.IsClosed)
					                               {
						                               var dataTable = new DataTable();
						                               dataTable.Load(reader);
						                               dataSet.Tables.Add(dataTable);
					                               }
					                               return dataSet;
				                               }
			                               });
		}

		private T ExecuteCommand<T>(IDbCommand command, Func<IDbCommand, T> commandAction)
		{
			if (command == null)
			{
				throw new ArgumentNullException("command");
			}
			if (log.IsDebugEnabled)
			{
				log.DebugFormat("SQL ExecuteCommand [{0}] {1}", command.CommandText, command.Parameters.GetParameterString());
			}

			try
			{
				context.Database.Connection.Open();
				return commandAction(command);
			}
			catch (Exception ex)
			{
				log.Error(string.Format("SQL ExecuteCommand [{0}] {1}", command.CommandText, command.Parameters.GetParameterString()), ex);
				throw;
			}
			finally
			{
				context.Database.Connection.Close();
			}
		}
	}
}