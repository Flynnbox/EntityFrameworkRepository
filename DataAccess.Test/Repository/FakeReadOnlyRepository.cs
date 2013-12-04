using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using DataAccess.Repository;

namespace DataAccess.Test.Repository
{
	public class FakeReadOnlyRepository<TEntity> : IGenericReadOnlyRepository<TEntity> where TEntity : class
	{
		private readonly List<TEntity> entities;
		private readonly Func<TEntity, object> findById;


		public FakeReadOnlyRepository()
		{
			
		}

		public FakeReadOnlyRepository(List<TEntity> fakeEntities, Func<TEntity, object> getId)
		{
			entities = fakeEntities;
			findById = getId;
		}

		public IEnumerable<TEntity> Get(Expression<Func<TEntity, bool>> filter = null, Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
		                                params Expression<Func<TEntity, object>>[] includeProperties)
		{
			return GetAsQueryable(filter, orderBy, includeProperties).ToList();
		}

		public IQueryable<TEntity> GetAsQueryable(Expression<Func<TEntity, bool>> filter = null, Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
		                                          params Expression<Func<TEntity, object>>[] includeProperties)
		{
			IQueryable<TEntity> query = entities.AsQueryable();

			if (filter != null)
			{
				query = query.Where(filter);
			}

			return orderBy != null ? orderBy(query) : query;
		}

		public TEntity GetById(object id)
		{
			return entities.FirstOrDefault(e => findById(e).Equals(id));
		}
	}
}