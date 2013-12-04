using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataAccess.Repository;

namespace DataAccess.Test.Repository
{
	public class FakeStoredProcedures : IStoredProcedures
	{
		public List<CertificateProgram> GetCertificateProgramsForCourseThatUserHasNotBegunResult { get; set; }
		public IUnitOfWork unitOfWork;
		
		public FakeStoredProcedures(IUnitOfWork unitOfWork)
		{
			this.unitOfWork = unitOfWork;
			GetCertificateProgramsForCourseThatUserHasNotBegunResult = new List<CertificateProgram>();
		}

		public bool AreAllLessonsForCourseCompleted(Guid userGuid, Guid courseGuid, Guid? lessonGuidToIgnore)
		{
			return unitOfWork.CourseLessonMapRepository.GetAsQueryable(m => m.CourseGuid == courseGuid && (!lessonGuidToIgnore.HasValue || m.LessonGuid != lessonGuidToIgnore.Value))
			          .Join(unitOfWork.LessonRepository.GetAsQueryable(l => l.Status == AuthoringStatus.Activated || l.Status == AuthoringStatus.Revising), m => m.LessonGuid, l => l.LessonGuid,
			                (m, l) => l.LessonGuid)
			          .GroupJoin(unitOfWork.UserLessonProgressRepository.GetAsQueryable(u => u.UserGuid == userGuid), l => l, u => u.LessonGuid,
			                     (l, u) => new {LessonGuid = l, IsCompleted = u.Any(p => p.IsCompleted)}).Any(u => !u.IsCompleted) == false;
		}

		public List<LessonPage> GetLessonPagesInOrder(Guid lessonGuid)
		{
			return unitOfWork.LessonPageRepository.GetAsQueryable(p => p.LessonGuid == lessonGuid && p.IsActive, q => q.OrderBy(p => p.DisplayOrder).ThenBy(p => p.MinimumRetryVersion)).ToList();
		}

		public List<Catalog> GetAuthorizedCatalogGuids(Guid userGuid)
		{
			return new List<Catalog>();
		}

		public List<CertificateProgram> GetCertificateProgramsForCourseThatUserHasNotBegun(Guid catalogGuid, Guid courseGuid, Guid userGuid)
		{
			return GetCertificateProgramsForCourseThatUserHasNotBegunResult;
		}

		public List<UserQuestionResponse> GetUserResponsesForLessonPage(Guid surveyGuid, Guid userGuid)
		{
			var session = unitOfWork.UserSurveySessionRepository.Get(s => s.SurveyGuid == surveyGuid && s.ParticipantId == userGuid.ToString()).FirstOrDefault();
			return session != null ? unitOfWork.UserQuestionResponseRepository.Get(r => r.SessionGuid == session.SessionGuid).ToList() : new List<UserQuestionResponse>();
		}

		public List<LessonPageContent> GetLessonPageContentAndQuestions(Guid lessonPageGuid)
		{
			var content = unitOfWork.LessonContentBlockRepository.Get(c => c.LessonPageGuid == lessonPageGuid, q => q.OrderBy(c => c.Order)).ToList();
			var questionGuids = content.Where(c => c.ContentTypeId == 60 || c.ContentTypeId == 61 || c.ContentTypeId == 62).Select(c => c.ContentGuid).ToList();
			var questions = unitOfWork.QuestionRepository.Get(q => questionGuids.Contains(q.QuestionGuid)).ToList();
			var choices = unitOfWork.QuestionChoiceRepository.Get(c => questionGuids.Contains(c.QuestionGuid)).ToList();
			
			var result = new List<LessonPageContent>();
			foreach (var c in content)
			{
				var item = new LessonPageContent
				{
					LessonPageGuid = lessonPageGuid,
					ContentGuid = c.ContentGuid,
					Content = c.Content,
					ContentOrder = c.Order,
					ContentTypeId = c.ContentType
				};

				Question question = questions.FirstOrDefault(q => q.QuestionGuid == c.ContentGuid);
				if (question != null)
				{
					item.QuestionGuid = question.QuestionGuid;
					item.Question = question.QuestionText;
					item.ChoiceList = choices.Where(o => o.QuestionGuid == question.QuestionGuid);
				}
				result.Add(item);
			}
			return result;
		}

		public Guid GetFirstLessonPageGuid(Guid lessonGuid)
		{
			var page = unitOfWork.LessonPageRepository.Get(p => p.LessonGuid == lessonGuid && p.IsActive && p.DisplayOrder == 1).FirstOrDefault();
			return page != null ? page.LessonPageGuid : Guid.Empty;
		}

		public DataSet GetCatalogDetail(Guid catalogGuid, Guid? userGuid)
		{
			return new DataSet();
		}

		public DataSet GetCourseDetail(Guid courseGuid, Guid catalogGuid, Guid? userGuid)
		{
			return new DataSet();
		}

		public DataTable GetCourseStatus(Guid courseGuid)
		{
			return new DataTable();
		}

		public DataSet GetLessonDetail(Guid lessonGuid, Guid courseGuid, Guid catalogGuid, Guid? userGuid)
		{
			return new DataSet();
		}

		public DataSet GetLessonContent(Guid lessonGuid, Guid courseGuid, Guid catalogGuid, Guid? userGuid)
		{
			return new DataSet();
		}

		public DataTable GetLessonStatus(Guid lessonGuid)
		{
			return new DataTable();
		}

		public DataSet GetSurveyWithQuestionAndChoices(Guid surveyGuid)
		{
			return new DataSet();
		}

		public void RestartLesson(Guid userGuid, Guid lessonGuid)
		{
			var userLessonProgress = unitOfWork.UserLessonProgressRepository.Get(p => p.UserGuid == userGuid && p.LessonGuid == lessonGuid).FirstOrDefault();
			if (userLessonProgress == null)
			{
				return;
			}

			var now = DateTime.Now;
			userLessonProgress.StartDate = now;
			userLessonProgress.LastActionDate = now;
			userLessonProgress.RetryCount = userLessonProgress.RetryCount + 1;
			userLessonProgress.CurrentLessonPageGuid = null;
			userLessonProgress.ElapsedTime = null;
			userLessonProgress.PostAssessmentScore = null;
			userLessonProgress.IsCompleted = false;
			unitOfWork.UserLessonProgressRepository.Update(userLessonProgress);
			 
			//delete all sessions & responses for this user that are for lesson pages within this lesson
			var participantId = userGuid.ToString();
			var lessonPageSurveyGuids = unitOfWork.LessonPageRepository.GetAsQueryable(l => l.LessonGuid == lessonGuid).Select(l => l.SurveyGuid).ToList();
			var sessionGuids = unitOfWork.UserSurveySessionRepository.GetAsQueryable(s => s.ParticipantId == participantId && lessonPageSurveyGuids.Contains(s.SurveyGuid)).Select(s => s.SessionGuid).ToList();
			var responseIds = unitOfWork.UserQuestionResponseRepository.GetAsQueryable(r => sessionGuids.Contains(r.SessionGuid)).ToList();
			responseIds.ForEach(r => unitOfWork.UserQuestionResponseRepository.Delete(r.ResponseId));
			sessionGuids.ForEach(s => unitOfWork.UserSurveySessionRepository.Delete(s));
		}

		public void RestartCourse(Guid userGuid, Guid courseGuid)
		{
		}

		public void StartAndCompleteCourseByUserGuid(Guid userGuid, Guid catalogGuid)
		{
		}

		public void StartAndCompleteCertificateProgramsByUserGuid(Guid userGuid, Guid catalogGuid)
		{
		}

		public void PopulateCourseSurvey(Guid courseGuid, Guid surveyGuid, Guid userGuid)
		{
		}
	}
}