using System;
using DataAccess.Models;

namespace DataAccess.Repository
{
	public interface IUnitOfWork : IDisposable
	{
		void Save();

		IGenericRepository<ExampleModel> ExampleRepository { get; }
	}
}