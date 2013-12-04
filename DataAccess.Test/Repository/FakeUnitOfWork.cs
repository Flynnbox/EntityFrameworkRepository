using System;
using DataAccess.Models;
using DataAccess.Repository;

namespace DataAccess.Test.Repository
{
	public class FakeUnitOfWork : IUnitOfWork
	{
		private IGenericRepository<ExampleModel> exampleRepository;
		public IGenericRepository<ExampleModel> ExampleRepository
		{
			get { return GetTypedRepository(exampleRepository); }
			set { exampleRepository = value; }
		}
		
		public void CreateEmptyRepositories()
		{
			exampleRepository = new FakeRepository<ExampleModel>(m => m.ModelGuid);
		}
		
		public void Save()
		{
			((FakeRepository<ExampleModel>)exampleRepository).Save();
		}

		private bool disposed;

		protected virtual void Dispose(bool disposing)
		{
			if (!disposed)
			{
				if (disposing)
				{
				}
			}
			disposed = true;
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}
		
		private IGenericRepository<T> GetTypedRepository<T>(IGenericRepository<T> repository) where T : class
		{
			return repository ?? (repository = new FakeRepository<T>());
		}
	}
}