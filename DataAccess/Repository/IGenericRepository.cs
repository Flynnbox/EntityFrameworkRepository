using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace DataAccess.Repository
{
	public interface IGenericRepository<TEntity> where TEntity : class
	{
		IEnumerable<TEntity> Get(
			Expression<Func<TEntity, bool>> filter = null,
			Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
			params Expression<Func<TEntity, object>>[] includeProperties);

		IQueryable<TEntity> GetAsQueryable(
			Expression<Func<TEntity, bool>> filter = null,
			Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
			params Expression<Func<TEntity, object>>[] includeProperties);

		TEntity GetById(object id);

		void Insert(TEntity entity);

		void Delete(object id);

		void Delete(TEntity entity);

		void Update(TEntity entity);
	}
}