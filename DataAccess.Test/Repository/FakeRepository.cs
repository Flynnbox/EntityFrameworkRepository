using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.Serialization;
using DataAccess.Repository;

namespace DataAccess.Test.Repository
{
	public class FakeRepository<TEntity> : IGenericRepository<TEntity> where TEntity : class
	{
		private readonly List<TEntity> pendingEntityChanges;
		private readonly List<TEntity> entities;
		private readonly Func<TEntity, object> findById;
		private int fakeDatabaseIndexValue = 1;

		public FakeRepository()
		{
			entities = new List<TEntity>();
			pendingEntityChanges = new List<TEntity>();
		}

		public FakeRepository(Func<TEntity, object> getId)
		{
			entities = new List<TEntity>();
			pendingEntityChanges = new List<TEntity>();
			findById = getId;
		}

		public FakeRepository(List<TEntity> fakeEntities, Func<TEntity, object> getId)
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

		public void Insert(TEntity entity)
		{
			pendingEntityChanges.Add(entity);
		}

		public void Delete(object id)
		{
			var entity = pendingEntityChanges.FirstOrDefault(e => findById(e).Equals(id));
			Delete(entity);
		}

		public void Delete(TEntity entity)
		{
			var modifiedEntity = pendingEntityChanges.FirstOrDefault(e => findById(e).Equals(findById(entity)));
			pendingEntityChanges.Remove(modifiedEntity);
		}

		public void Update(TEntity entity)
		{
			var index = pendingEntityChanges.FindIndex(0, e => findById(e).Equals(findById(entity)));
			if (index == -1)
			{
				throw new InvalidOperationException(string.Format("Update of {0} failed as matching entity could not be found.", typeof (TEntity).Name));
			}
			pendingEntityChanges[index] = entity;
		}

		internal void Save()
		{
			//delete any missing entities
			foreach (var entity in entities.ToList())
			{
				var index = pendingEntityChanges.FindIndex(0, e => findById(e).Equals(findById(entity)));
				if (index == -1)
				{
					entities.Remove(entity);
				}
			}

			//insert or update changed entities
			foreach (var entity in pendingEntityChanges)
			{
				var index = entities.FindIndex(0, e => findById(e).Equals(findById(entity)));
				if (index == -1)
				{
					SetIndexFieldValues(entity);
					entities.Add(entity.Clone());
				}
				else
				{
					entities[index] = entity.Clone();
				}
			}
		}

		private void SetIndexFieldValues(TEntity entity)
		{
			PropertyInfo[] properties = entity.GetType().GetProperties();
			foreach (var propertyInfo in properties.Where(p => Attribute.IsDefined(p, typeof(KeyAttribute))))
			{
				if (propertyInfo.PropertyType == typeof(int) && ((int)propertyInfo.GetValue(entity, null)) == default(int))
				{
					propertyInfo.SetValue(entity, fakeDatabaseIndexValue++, null);
				}
			}
		}
	}

	public static class EntityObjectExensions
	{
		public static T Clone<T>(this T source) where T : class
		{
			var ser = new DataContractSerializer(typeof (T));
			using (var stream = new MemoryStream())
			{
				ser.WriteObject(stream, source);
				stream.Seek(0, SeekOrigin.Begin);
				return (T) ser.ReadObject(stream);
			}
		}
	}
}