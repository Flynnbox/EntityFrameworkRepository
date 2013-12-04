using System;
using System.Data;
using System.Linq;
using System.Transactions;
using DataAccess.Models;
using DataAccess.Repository;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DataAccess.Test.Tests
{
	[TestClass]
	public class VerifyWithDatabase
	{
		private TransactionScope transaction;
		private ExampleModel entity;
		private Guid entityGuid;
		private IUnitOfWork unitOfWork;

		[ClassInitialize]
		public static void InitializeClass(TestContext context)
		{
		}

		// Use TestInitialize to run code before running each test 
		[TestInitialize]
		public void InitializeTest()
		{
			//open a transaction
			transaction = new TransactionScope();
			unitOfWork = new UnitOfWork();
			Setup();
			unitOfWork.Save();
		}

		// Use TestCleanup to run code after each test has run
		[TestCleanup]
		public void CleanupTest()
		{
			//rollback and close transaction
			transaction.Dispose();
		}

		private void Setup()
		{
			entity = CreateEntity();
			entityGuid = entity.ModelGuid;
		}

		public ExampleModel CreateEntity()
		{
			return new ExampleModel() { ModelGuid = Guid.NewGuid(), Name = "Test"};
		}

		[TestMethod]
		public void WhenInsertEntityAndQueryWithoutSaving_ThenEntityIsNotAvailable()
		{
			//arrange
			unitOfWork.CatalogRepository.Insert(entity);

			//act
			var result = unitOfWork.CatalogRepository.Get(e => e.ModelGuid == entityGuid).FirstOrDefault();

			//assert
			Assert.IsNull(result);
		}

		[TestMethod]
		public void WhenInsertEntityAndQueryAfterSaving_ThenEntityIsAvailable()
		{
			//arrange
			unitOfWork.CatalogRepository.Insert(entity);
			unitOfWork.Save();

			//act
			var result = unitOfWork.CatalogRepository.Get(e => e.ModelGuid == entity.ModelGuid).FirstOrDefault();
			//assert
			Assert.IsNotNull(result);
		}

		[TestMethod]
		public void WhenInsertMultipleEntitiesAndQueryAfterSaving_ThenAllEntitiesAreAvailable()
		{
			//arrange
			var entity2 = CreateEntity();
			unitOfWork.CatalogRepository.Insert(entity);
			unitOfWork.CatalogRepository.Insert(entity2);
			unitOfWork.Save();

			//act
			var result = unitOfWork.CatalogRepository.Get(e => e.ModelGuid == entity.ModelGuid || e.ModelGuid == entity2.ModelGuid).Count();

			//assert
			Assert.AreEqual(2, result);
		}

		[TestMethod]
		public void WhenInsertEntityWithIndexKeyAndQueryAfterSaving_ThenEntityIndexValueIsSet()
		{
			//arrange
			var sessionGuid = Guid.NewGuid();
			var newEntity = new UserQuestionResponse
			                {
												SessionGuid = sessionGuid
			                };
			unitOfWork.UserQuestionResponseRepository.Insert(newEntity);
			unitOfWork.Save();

			//act
			var result = unitOfWork.UserQuestionResponseRepository.Get(r => r.SessionGuid == sessionGuid).FirstOrDefault();

			//assert
			Assert.IsNotNull(result);
			Assert.AreNotEqual(0, result.ResponseId);
		}

		[TestMethod]
		public void WhenUpdateEntityAndQueryWithoutSaving_ThenEntityIsNotUpdated()
		{
			//arrange
			entity.Status = AuthoringStatus.Activated;
			unitOfWork.CatalogRepository.Insert(entity);
			unitOfWork.Save();
			var tempEntity = unitOfWork.CatalogRepository.Get(e => e.ModelGuid == entityGuid).First();
			tempEntity.Status = AuthoringStatus.Deactivated;
			unitOfWork.CatalogRepository.Update(tempEntity);

			//act
			var result = unitOfWork.CatalogRepository.Get(e => e.ModelGuid == entityGuid).First();

			//assert
			Assert.AreNotEqual(AuthoringStatus.Deactivated, result.Status);
		}

		[TestMethod]
		public void WhenUpdateEntityAndQueryAfterSaving_ThenEntityIsUpdated()
		{
			unitOfWork.CatalogRepository.Insert(entity);
			unitOfWork.Save();
			entity.Status = AuthoringStatus.Deactivated;
			unitOfWork.CatalogRepository.Update(entity);
			unitOfWork.Save();

			//act
			var result = unitOfWork.CatalogRepository.Get(e => e.ModelGuid == entityGuid).First();

			//assert
			Assert.AreEqual(AuthoringStatus.Deactivated, result.Status);
		}

		[TestMethod]
		public void WhenUpdateMultipleEntitiesAndQueryAfterSaving_ThenAllEntitiesAreUpdated()
		{
			unitOfWork.CatalogRepository.Insert(entity);
			var entity2 = CreateEntity();
			unitOfWork.CatalogRepository.Insert(entity2);
			unitOfWork.Save();
			entity.Status = AuthoringStatus.Deactivated;
			entity2.Status = AuthoringStatus.Deactivated;
			unitOfWork.CatalogRepository.Update(entity);
			unitOfWork.CatalogRepository.Update(entity2);
			unitOfWork.Save();

			//act
			var result = unitOfWork.CatalogRepository.Get(e => e.ModelGuid == entity.ModelGuid || e.ModelGuid == entity2.ModelGuid).Count();

			//assert
			Assert.AreEqual(2, result);
		}

		[TestMethod]
		public void WhenDeleteEntityAndQueryWithoutSaving_ThenEntityIsNotDeleted()
		{
			//arrange
			unitOfWork.CatalogRepository.Insert(entity);
			unitOfWork.Save();
			unitOfWork.CatalogRepository.Delete(entity);

			//act
			var result = unitOfWork.CatalogRepository.Get(e => e.ModelGuid == entity.ModelGuid).FirstOrDefault();

			//assert
			Assert.IsNotNull(result);
		}

		[TestMethod]
		public void WhenDeleteEntityAndQueryAfterSaving_ThenEntityIsDeleted()
		{
			unitOfWork.CatalogRepository.Insert(entity);
			unitOfWork.Save();
			unitOfWork.CatalogRepository.Delete(entity);
			unitOfWork.Save();

			//act
			var result = unitOfWork.CatalogRepository.Get(e => e.ModelGuid == entity.ModelGuid).FirstOrDefault();

			//assert
			Assert.IsNull(result);
		}

		[TestMethod]
		public void WhenDeleteMultipleEntitiesAndQueryAfterSaving_ThenAllEntitiesAreDeleted()
		{
			unitOfWork.CatalogRepository.Insert(entity);
			var entity2 = CreateEntity();
			unitOfWork.CatalogRepository.Insert(entity2);
			unitOfWork.Save();
			unitOfWork.CatalogRepository.Delete(entity);
			unitOfWork.CatalogRepository.Delete(entity2);
			unitOfWork.Save();

			//act
			var result = unitOfWork.CatalogRepository.Get(e => e.ModelGuid == entity.ModelGuid || e.ModelGuid == entity2.ModelGuid).Any();

			//assert
			Assert.IsFalse(result);
		}

		[TestMethod]
		public void WhenDeleteEntityByIdAndQueryAfterSaving_ThenEntityIsDeleted()
		{
			unitOfWork.CatalogRepository.Insert(entity);
			unitOfWork.CatalogRepository.Delete(entityGuid);
			unitOfWork.Save();

			//act
			var result = unitOfWork.CatalogRepository.Get(e => e.ModelGuid == entity.ModelGuid).FirstOrDefault();

			//assert
			Assert.IsNull(result);
		}

		[TestMethod]
		public void WhenDeleteEntityWhichHasBeenRetrievedFromQueryAndQueryAfterSaving_ThenEntityIsDeleted()
		{
			unitOfWork.CatalogRepository.Insert(entity);
			unitOfWork.Save();
			var queryEntity = unitOfWork.CatalogRepository.Get(e => e.ModelGuid == entity.ModelGuid).First();
			unitOfWork.CatalogRepository.Delete(queryEntity);
			unitOfWork.Save();

			//act
			var result = unitOfWork.CatalogRepository.Get(e => e.ModelGuid == entity.ModelGuid).FirstOrDefault();

			//assert
			Assert.IsNull(result);
		}

		[TestMethod]
		public void WhenInsertSaveAndQueryEntity_ThenEntityObjectsAreNotTheSame()
		{
			unitOfWork.CatalogRepository.Insert(entity);
			unitOfWork.Save();

			//act
			var queryEntity = unitOfWork.CatalogRepository.Get(e => e.ModelGuid == entityGuid).First();

			//assert
			Assert.IsFalse(ReferenceEquals(entity, queryEntity));
		}

		[TestMethod]
		public void WhenEntityIsInsertedIntoRepositoryAndThenEntityIsUpdatedBeforeSaving_UpdatedValueIsInserted()
		{
			//arrange
			unitOfWork.CatalogRepository.Insert(entity);
			unitOfWork.CatalogRepository.Insert(entity);

			//act
			entity.Status = AuthoringStatus.Revising;
			unitOfWork.Save();

			//assert
			var updatedEntity = unitOfWork.CatalogRepository.Get(e => e.ModelGuid == entity.ModelGuid).First();
			Assert.AreEqual(AuthoringStatus.Revising, updatedEntity.Status);
		}

		[TestMethod]
		public void WhenEntityIsUpdatedTwiceInRepositoryBeforeSaving_ThenLatestUpdateIsSaved()
		{
			//arrange
			unitOfWork.CatalogRepository.Insert(entity);
			unitOfWork.Save();

			//act
			entity.Status = AuthoringStatus.Revising;
			unitOfWork.CatalogRepository.Update(entity);
			entity.Status = AuthoringStatus.PreConfirmed;
			unitOfWork.CatalogRepository.Update(entity);
			unitOfWork.Save();

			//assert
			Assert.AreEqual(AuthoringStatus.PreConfirmed, entity.Status);
		}

		[TestMethod]
		public void WhenFinishRawDataAccessGetScalar_ThenDBConnectionIsClosed()
		{
			//arrange

			//act
			using (var context = new DataContext())
			{
				var dataAccess = new RawDataAccess(context);
				var cmd = dataAccess.GetSqlText("select top 1 1 from lmsCatalog");
				dataAccess.GetScalar<int>(cmd);

				//assert
				Assert.AreEqual(ConnectionState.Closed, context.Database.Connection.State);
			}
		}

		[TestMethod]
		public void WhenFinishRawDataAccessGetScalar_ThenScalarValueIsAvailable()
		{
			//arrange
			int result;

			//act
			using (var context = new DataContext())
			{
				var dataAccess = new RawDataAccess(context);
				var cmd = dataAccess.GetSqlText("select top 1 1 from lmsCatalog");
				result = dataAccess.GetScalar<int>(cmd);
			}

			//assert
			Assert.AreEqual(1, result);
		}

		[TestMethod]
		public void WhenFinishRawDataAccessGetDataTable_ThenDBConnectionIsClosed()
		{
			//arrange

			//act
			using (var context = new DataContext())
			{
				var dataAccess = new RawDataAccess(context);
				var cmd = dataAccess.GetSqlText("select top 1 1 from lmsCatalog");
				dataAccess.GetDataTable(cmd);

				//assert
				Assert.AreEqual(ConnectionState.Closed, context.Database.Connection.State);
			}
		}

		[TestMethod]
		public void WhenFinishRawDataAccessGetDataTable_ThenDataTableValueIsAvailable()
		{
			//arrange
			DataTable result;

			//act
			using (var context = new DataContext())
			{
				var dataAccess = new RawDataAccess(context);
				var cmd = dataAccess.GetSqlText("select top 1 1 from lmsCatalog");
				result = dataAccess.GetDataTable(cmd);
			}

			//assert
			Assert.IsNotNull(result);
			Assert.AreEqual(1, result.Rows.Count);
			Assert.AreEqual(1, result.Rows[0].Field<int>(0));
		}

		[TestMethod]
		public void WhenFinishRawDataAccessGetDataSet_ThenDBConnectionIsClosed()
		{
			//arrange

			//act
			using (var context = new DataContext())
			{
				var dataAccess = new RawDataAccess(context);
				var cmd = dataAccess.GetSqlText("select top 1 1 from lmsCatalog");
				dataAccess.GetDataSet(cmd);

				//assert
				Assert.AreEqual(ConnectionState.Closed, context.Database.Connection.State);
			}
		}

		[TestMethod]
		public void WhenFinishRawDataAccessGetDataSet_ThenDataSetValueIsAvailable()
		{
			//arrange
			DataSet result;

			//act
			using (var context = new DataContext())
			{
				var dataAccess = new RawDataAccess(context);
				var cmd = dataAccess.GetSqlText("select top 1 1 from lmsCatalog");
				result = dataAccess.GetDataSet(cmd);
			}

			//assert
			Assert.IsNotNull(result);
			Assert.AreEqual(1, result.Tables.Count);
			Assert.AreEqual(1, result.Tables[0].Rows.Count);
			Assert.AreEqual(1, result.Tables[0].Rows[0].Field<int>(0));
		}
	}
}