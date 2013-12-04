using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;

namespace DataAccess.Repository
{
	public class GenericReadOnlyRepository<TEntity> : IGenericReadOnlyRepository<TEntity> where TEntity : class
	{
		internal DbContext context;
		internal DbSet<TEntity> dbSet;

		public GenericReadOnlyRepository(DbContext context)
		{
			this.context = context;
			dbSet = context.Set<TEntity>();
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
			return query;
		}

		public virtual TEntity GetById(object id)
		{
			return dbSet.Find(id);
		}
	}
}