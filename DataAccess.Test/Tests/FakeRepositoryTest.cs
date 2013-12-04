using System;
using System.Linq;
using DataAccess.Models;
using DataAccess.Repository;
using DataAccess.Test.Repository;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DataAccess.Test.Tests
{
	[TestClass]
	public class FakeRepositoryTest
	{
		private IUnitOfWork unitOfWork;
		private ExampleModel entity;
		private Guid entityGuid;

		[ClassInitialize]
		public static void InitializeClass(TestContext context)
		{
		}

		// Use TestInitialize to run code before running each test 
		[TestInitialize]
		public void InitializeTest()
		{
			unitOfWork = new FakeUnitOfWork();
			((FakeUnitOfWork)unitOfWork).CreateEmptyRepositories();
			entity = CreateEntity();
			entityGuid = entity.ModelGuid;
		}

		// Use TestCleanup to run code after each test has run
		[TestCleanup]
		public void CleanupTest()
		{
		}

		public ExampleModel CreateEntity()
		{
			return new ExampleModel { ModelGuid = Guid.NewGuid(), Name = "Test"};
		}

		[TestMethod]
		public void WhenInsertEntityAndQueryWithoutSaving_ThenEntityIsNotAvailable()
		{
			//arrange
			unitOfWork.CatalogRepository.Insert(entity);

			//act
			var result = unitOfWork.CatalogRepository.GetById(entityGuid);

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
			var result = unitOfWork.CatalogRepository.GetById(entityGuid);

			//assert
			Assert.IsNotNull(result);
		}

		[TestMethod]
		public void WhenInsertMultipleEntitiesAndQueryAfterSaving_ThenAllEntitiesAreAvailable()
		{
			//arrange
			unitOfWork.CatalogRepository.Insert(entity);
			unitOfWork.CatalogRepository.Insert(CreateEntity());
			unitOfWork.Save();

			//act
			var result = unitOfWork.CatalogRepository.Get().Count();

			//assert
			Assert.AreEqual(2, result);
		}

		[TestMethod]
		public void WhenInsertEntityWithIndexKeyAndQueryAfterSaving_ThenEntityIndexValueIsSet()
		{
			//arrange
			var newEntity = new UserQuestionResponse();
			unitOfWork.UserQuestionResponseRepository.Insert(newEntity);
			unitOfWork.Save();

			//act
			var result = unitOfWork.UserQuestionResponseRepository.Get().FirstOrDefault();

			//assert
			Assert.IsNotNull(result);
			Assert.AreNotEqual(0, result.ResponseId);
		}

		[TestMethod]
		public void WhenUpdateEntityAndQueryWithoutSaving_ThenEntityIsNotUpdated()
		{
			//arrange
			unitOfWork.CatalogRepository.Insert(entity);
			unitOfWork.Save();
			entity.Status = AuthoringStatus.Deactivated;
			unitOfWork.CatalogRepository.Update(entity);

			//act
			var result = unitOfWork.CatalogRepository.GetById(entityGuid);

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
			var result = unitOfWork.CatalogRepository.GetById(entityGuid);

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
			var result = unitOfWork.CatalogRepository.Get(c => c.Status == AuthoringStatus.Deactivated).Count();

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
			var result = unitOfWork.CatalogRepository.GetById(entityGuid);

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
			var result = unitOfWork.CatalogRepository.GetById(entityGuid);

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
			var result = unitOfWork.CatalogRepository.Get().Count();

			//assert
			Assert.AreEqual(0, result);
		}

		[TestMethod]
		public void WhenDeleteEntityByIdAndQueryAfterSaving_ThenEntityIsDeleted()
		{
			unitOfWork.CatalogRepository.Insert(entity);
			unitOfWork.CatalogRepository.Delete(entityGuid);
			unitOfWork.Save();

			//act
			var result = unitOfWork.CatalogRepository.GetById(entityGuid);

			//assert
			Assert.IsNull(result);
		}

		[TestMethod]
		public void WhenDeleteEntityWhichHasBeenRetrievedFromQueryAndQueryAfterSaving_ThenEntityIsDeleted()
		{
			unitOfWork.CatalogRepository.Insert(entity);
			unitOfWork.Save();
			var queryEntity = unitOfWork.CatalogRepository.GetById(entityGuid);
			unitOfWork.CatalogRepository.Delete(queryEntity);
			unitOfWork.Save();

			//act
			var result = unitOfWork.CatalogRepository.GetById(entityGuid);

			//assert
			Assert.IsNull(result);
		}

		[TestMethod]
		public void WhenInsertSaveAndQueryEntity_ThenEntityObjectsAreNotTheSame()
		{
			unitOfWork.CatalogRepository.Insert(entity);
			unitOfWork.Save();

			//act
			var queryEntity = unitOfWork.CatalogRepository.GetById(entityGuid);

			//assert
			Assert.AreNotEqual(entity, queryEntity);
		}
	}
}
