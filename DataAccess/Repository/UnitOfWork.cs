using System;
using System.Linq;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Reflection;
using DataAccess.Models;
using log4net;

namespace DataAccess.Repository
{
	public class UnitOfWork : IUnitOfWork
	{
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		private readonly DbContext context = new DataContext();

		private IGenericRepository<ExampleModel> exampleRepository;

		public IGenericRepository<ExampleModel> ExampleRepository
		{
			get { return GetTypedRepository(exampleRepository); }
		}
		
		public void Save()
		{
			try
			{
				context.SaveChanges();
			}
			catch (DbEntityValidationException ex)
			{
				var validationErrors = ex.EntityValidationErrors.SelectMany(e => e.ValidationErrors).Select(v => v.ErrorMessage);
				var validationErrorMessage = string.Join("\n", validationErrors);
				log.Error(string.Format("Validations Errors when saving DbContext changes within UnitOfWork: {0}", validationErrorMessage), ex);
				throw;
			}
			catch (Exception ex)
			{
				log.Error("Error when saving DbContext changes within UnitOfWork", ex);
				throw;
			}
		}

		private bool disposed;

		protected virtual void Dispose(bool disposing)
		{
			if (!disposed)
			{
				if (disposing)
				{
					context.Dispose();
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
			return repository ?? (repository = new GenericRepository<T>(context));
		}
	}
}