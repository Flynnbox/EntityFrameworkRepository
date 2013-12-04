using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using DataAccess.ExtensionMethods;
using log4net;

namespace DataAccess.Repository
{
	public class GenericRepository<TEntity> : IGenericRepository<TEntity> where TEntity : class
	{
		// ReSharper disable StaticFieldInGenericType
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
		// ReSharper restore StaticFieldInGenericType

		private readonly string entityName;
		private readonly PropertyInfo[] entityKeyProperties;
		internal DbContext context;
		internal DbSet<TEntity> dbSet;

		public GenericRepository(DbContext context)
		{
			this.context = context;
			dbSet = context.Set<TEntity>();
			entityName = typeof (TEntity).Name;
			entityKeyProperties = GetKeyProperties();
		}

		public virtual IEnumerable<TEntity> Get(
			Expression<Func<TEntity, bool>> filter = null,
			Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
			params Expression<Func<TEntity, object>>[] includeProperties)
		{
			return GetAsQueryable(filter, orderBy, includeProperties).ToList();
		}

		public virtual IQueryable<TEntity> GetAsQueryable(
			Expression<Func<TEntity, bool>> filter = null,
			Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
			params Expression<Func<TEntity, object>>[] includeProperties)
		{
			IQueryable<TEntity> query = dbSet;

			if (filter != null)
			{
				query = query.Where(filter);
			}

			foreach (var includeProperty in includeProperties)
			{
				query = query.Include(includeProperty);
			}

			if (orderBy != null)
			{
				return orderBy(query);
			}

			if (log.IsDebugEnabled)
			{
				log.DebugFormat("SQL Get[{0}]: {1}", entityName, query);
			}

			return query;
		}

		public virtual TEntity GetById(object id)
		{
			if (log.IsDebugEnabled)
			{
				log.DebugFormat("SQL GetById[{0}]: {1}", entityName, id);
			}
			return dbSet.Find(id);
		}

		public virtual void Insert(TEntity entity)
		{
			if (log.IsDebugEnabled)
			{
				log.DebugFormat("SQL InsertEntity[{0}]: {1}", entityName, GetKeyValuesString(entity));
			}
			dbSet.Add(entity);
		}

		public virtual void Delete(object id)
		{
			if (log.IsDebugEnabled)
			{
				log.DebugFormat("SQL DeleteById[{0}]: {1}", entityName, id);
			}
			TEntity entityToDelete = dbSet.Find(id);
			Delete(entityToDelete);
		}

		public virtual void Delete(TEntity entity)
		{
			if (log.IsDebugEnabled)
			{
				log.DebugFormat("SQL DeleteEntity[{0}]: {1}", entityName, GetKeyValuesString(entity));
			}
			if (context.Entry(entity).State == EntityState.Detached)
			{
				dbSet.Attach(entity);
			}
			dbSet.Remove(entity);
		}

		public virtual void Update(TEntity entity)
		{
			if (log.IsDebugEnabled)
			{
				log.DebugFormat("SQL UpdateEntity[{0}]: {1}", entityName, GetKeyValuesString(entity));
			}
			dbSet.Attach(entity);
			context.Entry(entity).State = EntityState.Modified;
		}

		/// <summary>
		///   Note that entities loaded via this method will be tracked
		/// </summary>
		/// <param name="sql"></param>
		/// <param name="parameters"></param>
		/// <returns></returns>
		internal IEnumerable<TEntity> ExecuteQuery(string sql, params object[] parameters)
		{
			if (sql == null)
			{
				throw new ArgumentNullException("sql");
			}
			if (log.IsDebugEnabled)
			{
				log.DebugFormat("SQL ExecuteQuery[{0}]: [{1}] {2}", entityName, sql,
				                parameters as IDataParameter[] != null ? ((IDataParameter[]) parameters).GetParameterString() : string.Join(", ", parameters));
			}

			try
			{
				return dbSet.SqlQuery(sql, parameters);
			}
			catch (Exception ex)
			{
				log.Error(string.Format("SQL ExecuteQuery[{0}]: [{1}] {2}", entityName, sql,
					              parameters as IDataParameter[] != null ? ((IDataParameter[]) parameters).GetParameterString() : string.Join(", ", parameters)), ex);
				throw;
			}
		}

		private PropertyInfo[] GetKeyProperties()
		{
			return typeof (TEntity).GetProperties().Where(p => p.GetCustomAttributes(typeof (KeyAttribute), true).Length != 0).ToArray();
		}

		private object[] GetKeyValues<T>(T entity)
		{
			return entityKeyProperties.Select(k => k.GetValue(entity, null)).ToArray();
		}

		private string GetKeyValuesString<T>(T entity)
		{
			var keyValuesStrings = entityKeyProperties.Select(k => string.Format("[{0}] '{1}'", k.Name, k.GetValue(entity, null))).ToArray();
			return string.Join(" ; ", keyValuesStrings);
		}
	}
}