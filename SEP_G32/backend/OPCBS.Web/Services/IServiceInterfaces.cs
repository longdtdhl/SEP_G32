using OPCBS.Web.DTOs;

namespace OPCBS.Web.Services;

public interface IScheduleApiService
{
    Task<(List<ScheduleDto> Data, string? Error)> GetMySchedulesAsync();
    Task<(ScheduleDto? Data, string? Error)> GetByIdAsync(Guid id);
    Task<(bool Success, string? Error)> CreateAsync(CreateScheduleDto dto);
    Task<(bool Success, string? Error)> UpdateAsync(Guid id, UpdateScheduleDto dto);
    Task<(bool Success, string? Error)> DeleteAsync(Guid id);
    Task<(List<DayOffDto> Data, string? Error)> GetDaysOffAsync();
    Task<(bool Success, string? Error)> CreateDayOffAsync(CreateDayOffDto dto);
    Task<(bool Success, string? Error)> DeleteDayOffAsync(Guid id);
    Task<(AvailableSlotsDto? Data, string? Error)> GetMySlotsAsync(DateOnly? date = null);
    Task<(bool Success, string? Error)> ToggleBlockSlotAsync(Guid slotId);
    Task<(AppointmentSlotDto? Data, string? Error)> CreateSlotAsync(CreateSlotDto dto);
    Task<(bool Success, string? Error)> DeleteSlotAsync(Guid slotId);
    Task<(bool Success, string? Error)> UpdateSlotNotesAsync(Guid slotId, string? notes);
    Task<(bool Success, string? Error)> UpdateSlotAsync(Guid slotId, UpdateSlotDto dto);
}

public interface IPatientRecordApiService
{
    Task<(List<PatientRecordDto> Data, string? Error)> GetAllAsync();
    Task<(List<PatientRecordDto> Data, string? Error)> GetMyPatientsAsync();
    Task<(List<PatientRecordDto> Data, string? Error)> GetSystemPatientsAsync();
    Task<(List<PatientRecordDto> Data, string? Error)> GetGuestPatientsAsync();
    Task<(PatientRecordDto? Data, string? Error)> GetByIdAsync(Guid id);
    Task<(PatientRecordDto? Data, string? Error)> GetByUserIdAsync(Guid userId);
    Task<(bool Success, string? Error)> CreateAsync(CreatePatientRecordDto dto);
    Task<(bool Success, string? Error)> UpdateAsync(Guid id, UpdatePatientRecordDto dto);
}

public interface IConsultationNoteApiService
{
    Task<(List<ConsultationNoteDto> Data, PaginationDto? Pagination, string? Error)> GetAllAsync(int page = 1, int pageSize = 10);
    Task<(List<ConsultationNoteDto> Data, PaginationDto? Pagination, string? Error)> GetMyRecordsAsync(int page = 1, int pageSize = 10);
    Task<(List<ConsultationNoteDto> Data, PaginationDto? Pagination, string? Error)> GetByPatientRecordIdAsync(Guid patientRecordId, int page = 1, int pageSize = 10);
    Task<(ConsultationNoteDto? Data, string? Error)> GetByIdAsync(Guid id);
    Task<(ConsultationNoteDto? Data, string? Error)> GetByAppointmentIdAsync(Guid appointmentId);
    Task<(bool Success, string? Error)> CreateAsync(CreateConsultationNoteDto dto);
    Task<(bool Success, string? Error)> UpdateAsync(Guid id, UpdateConsultationNoteDto dto);
    Task<(bool Success, string? Error)> ConfirmAsync(Guid recordId);
}

public interface ITreatmentPackageApiService
{
    Task<(List<TreatmentPackageDto> Data, PaginationDto? Pagination, string? Error)> GetAllAsync(int page = 1, int pageSize = 10);
    Task<(List<TreatmentPackageDto> Data, PaginationDto? Pagination, string? Error)> GetMyPackagesAsync(int page = 1, int pageSize = 10);
    Task<(TreatmentPackageDto? Data, string? Error)> GetByIdAsync(Guid id);
    Task<(bool Success, string? Error)> CreateAsync(CreateTreatmentPackageDto dto);
    Task<(bool Success, string? Error)> UpdateAsync(Guid id, UpdateTreatmentPackageDto dto);
    Task<(bool Success, string? Error)> DeleteAsync(Guid id);
    Task<(bool Success, string? Error)> AcceptAsync(Guid id);
    Task<(bool Success, string? Error)> RejectAsync(Guid id, string? reason = null);
    Task<(bool Success, string? Error)> CancelAsync(Guid id, string? reason = null);
}

public interface IReviewApiService
{
    Task<(bool Success, string? Error)> CreateAsync(CreateReviewDto dto);
    Task<(List<ReviewDto> Data, PaginationDto? Pagination, string? Error)> GetMyReviewsAsync(int page = 1);
}

public interface IVerificationApiService
{
    Task<(VerificationDto? Data, string? Error)> GetMyVerificationAsync();
    Task<(bool Success, string? Error)> SubmitAsync(SubmitVerificationDto dto);
    Task<(List<VerificationDto> Data, PaginationDto? Pagination, string? Error)> GetPendingAsync(int page = 1, int pageSize = 10);
    Task<(VerificationDto? Data, string? Error)> GetByIdAsync(Guid id);
    Task<(bool Success, string? Error)> ReviewAsync(Guid id, ReviewVerificationDto dto);
}

public interface IServicePackageApiService
{
    Task<(List<ServicePackageDto> Data, string? Error)> GetAllAsync();
    Task<(ServicePackageDto? Data, string? Error)> GetByIdAsync(Guid id);
    Task<(bool Success, string? Error)> CreateAsync(CreateServicePackageDto dto);
    Task<(bool Success, string? Error)> UpdateAsync(Guid id, UpdateServicePackageDto dto);
    Task<(bool Success, string? Error)> DeleteAsync(Guid id);
}

public interface ISubscriptionApiService
{
    Task<(SubscriptionDto? Data, string? Error)> GetCurrentAsync();
    Task<(List<SubscriptionDto> Data, string? Error)> GetHistoryAsync();
    Task<(bool Success, string? Error)> SubscribeAsync(CreateSubscriptionDto dto);
    Task<(SubscriptionDto? Data, string? Error)> PurchaseAsync(Guid packageId, string returnUrl);
    Task<(bool Success, string? Error)> ProcessCallbackAsync(IDictionary<string, string> queryParams);
}

public interface IAdminApiService
{
    Task<(DashboardStatsDto? Data, string? Error)> GetDashboardStatsAsync();
    Task<(List<UserListItemDto> Data, PaginationDto? Pagination, string? Error)> GetUsersAsync(UserFilterDto? filter = null);
    Task<(UserListItemDto? Data, string? Error)> GetUserByIdAsync(Guid id);
    Task<(bool Success, string? Error)> LockUserAsync(Guid id);
    Task<(bool Success, string? Error)> UnlockUserAsync(Guid id);
    Task<(List<RoleDto> Data, string? Error)> GetRolesAsync();
    Task<(List<AuditLogDto> Data, PaginationDto? Pagination, string? Error)> GetAuditLogsAsync(string? entityName = null, int page = 1, int pageSize = 20);
    Task<(Dictionary<string, string> Data, string? Error)> GetSystemSettingsAsync();
    Task<(bool Success, string? Error)> UpdateSystemSettingsAsync(Dictionary<string, string> settings);
}

public interface ICustomerSupportApiService
{
    Task<(DashboardStatsDto? Data, string? Error)> GetDashboardStatsAsync();
    Task<(List<VerificationDto> Data, PaginationDto? Pagination, string? Error)> GetDoctorApplicationsAsync(int page = 1, string? status = null);
    Task<(VerificationDto? Data, string? Error)> GetApplicationByIdAsync(Guid id);
    Task<(bool Success, string? Error)> ReviewApplicationAsync(Guid id, ReviewVerificationDto dto);
    Task<(List<BlogListItemDto> Data, PaginationDto? Pagination, string? Error)> GetBlogModerationQueueAsync(int page = 1);
    Task<(BlogDto? Data, string? Error)> GetBlogForModerationAsync(Guid id);
    Task<(bool Success, string? Error)> ApproveBlogAsync(Guid id);
    Task<(bool Success, string? Error)> RejectBlogAsync(Guid id, string? reason = null);
}

public interface IBusinessManagerApiService
{
    Task<(DashboardStatsDto? Data, string? Error)> GetDashboardStatsAsync();
    Task<(List<ServicePackageDto> Data, string? Error)> GetServicePackagesAsync();
    Task<(ServicePackageDto? Data, string? Error)> GetServicePackageByIdAsync(Guid id);
    Task<(bool Success, string? Error)> CreateServicePackageAsync(CreateServicePackageDto dto);
    Task<(bool Success, string? Error)> UpdateServicePackageAsync(Guid id, UpdateServicePackageDto dto);
    Task<(bool Success, string? Error)> DeleteServicePackageAsync(Guid id);
    Task<(List<SpecializationDto> Data, string? Error)> GetSpecializationsAsync();
    Task<(bool Success, string? Error)> CreateSpecializationAsync(CreateSpecializationDto dto);
    Task<(bool Success, string? Error)> UpdateSpecializationAsync(Guid id, CreateSpecializationDto dto);
    Task<(bool Success, string? Error)> DeleteSpecializationAsync(Guid id);
}

public interface IPsychometricApiService
{
    Task<(List<PsychometricTestDto> Data, string? Error)> GetTestsAsync();
    Task<(List<PsychometricQuestionDto> Data, string? Error)> GetQuestionsAsync(Guid testId);
    Task<(PsychometricSubmissionDto? Data, string? Error)> SubmitTestAsync(SubmitTestDto dto);
    Task<(PsychometricSubmissionDto? Data, string? Error)> GetSubmissionByAppointmentAsync(Guid appointmentId);
    Task<(PsychometricSubmissionDto? Data, string? Error)> GetSubmissionByIdAsync(Guid submissionId);
    Task<(List<PsychometricSubmissionDto> Data, string? Error)> GetMySubmissionsAsync();
}

public interface INotificationApiService
{
    Task<(List<NotificationDto> Data, PaginationDto? Pagination, string? Error)> GetNotificationsAsync(int page = 1, int pageSize = 20);
    Task<(int Count, string? Error)> GetUnreadCountAsync();
    Task<(bool Success, string? Error)> MarkAsReadAsync(Guid notificationId);
    Task<(bool Success, string? Error)> MarkAllAsReadAsync();
}

public interface ITherapyApiService
{
    Task<(List<TherapyAssignmentDto> Data, string? Error)> GetAssignmentsByPackageAsync(Guid packageId);
    Task<(TherapyAssignmentDto? Data, string? Error)> GetAssignmentByIdAsync(Guid id);
    Task<(TherapyAssignmentDto? Data, string? Error)> CreateAssignmentAsync(CreateAssignmentDto dto);
    Task<(bool Success, string? Error)> SubmitAssignmentAsync(Guid id, SubmitAssignmentDto dto);
    Task<(bool Success, string? Error)> FeedbackAssignmentAsync(Guid id, FeedbackAssignmentDto dto);
    Task<(bool Success, string? Error)> DeleteAssignmentAsync(Guid id);
    Task<(List<EmotionJournalDto> Data, string? Error)> GetMyJournalsAsync();
    Task<(List<EmotionJournalDto> Data, string? Error)> GetPatientSharedJournalsAsync(Guid patientId);
    Task<(EmotionJournalDto? Data, string? Error)> CreateJournalAsync(CreateJournalDto dto);
    Task<(bool Success, string? Error)> DeleteJournalAsync(Guid id);
}

public interface IFavoriteApiService
{
    Task<(List<FavoriteDoctorWebDto> Data, string? Error)> GetFavoritesAsync();
    Task<(bool Success, string? Error)> AddFavoriteAsync(Guid doctorId);
    Task<(bool Success, string? Error)> RemoveFavoriteAsync(Guid doctorId);
    Task<(bool IsFavorite, string? Error)> IsFavoriteAsync(Guid doctorId);
}

public interface IMessagingApiService
{
    Task<(List<ConversationWebDto> Data, string? Error)> GetConversationsAsync();
    Task<(List<MessageWebDto> Data, string? Error)> GetMessagesAsync(Guid conversationId);
    Task<(MessageWebDto? Data, string? Error)> SendMessageAsync(Guid conversationId, object dto);
    Task<(bool Success, string? Error)> MarkAsReadAsync(Guid conversationId);
    Task<(ConversationWebDto? Data, string? Error)> GetOrCreateConversationAsync(Guid doctorUserId, Guid? appointmentId = null, Guid? treatmentPackageId = null);
    Task<(ConversationWebDto? Data, string? Error)> GetOrCreateConversationByPatientAsync(Guid patientUserId, Guid? appointmentId = null, Guid? treatmentPackageId = null);
    Task<(int Count, string? Error)> GetUnreadCountAsync();
}

public interface ITreatmentCaseApiService
{
    Task<(List<TreatmentCaseListWebDto> Data, string? Error)> GetByDoctorAsync(Guid doctorUserId);
    Task<(List<TreatmentCaseListWebDto> Data, string? Error)> GetByPatientAsync(Guid patientUserId);
    Task<(TreatmentCaseWebDto? Data, string? Error)> GetByIdAsync(Guid id);
    Task<(bool Success, string? Error)> CreateAsync(object dto);
    Task<(bool Success, string? Error)> UpdateAsync(Guid id, object dto);
    Task<(bool Success, string? Error)> CloseAsync(Guid id, object dto);
    Task<(bool Success, string? Error)> GenerateScheduleAsync(object dto);
    Task<(List<TreatmentSessionWebDto> Data, string? Error)> GetSessionsAsync(Guid caseId);
    Task<(bool Success, string? Error)> CreateSessionAsync(object dto);
    Task<(bool Success, string? Error)> UpdateSessionAsync(Guid sessionId, object dto);
    Task<(bool Success, string? Error)> DeleteSessionAsync(Guid sessionId);
    Task<(bool Success, string? Error)> ReorderSessionsAsync(object dto);
    Task<(bool Success, string? Error)> CompleteSessionAsync(Guid sessionId, object dto);
    Task<(List<TreatmentGoalWebDto> Data, string? Error)> GetGoalsAsync(Guid caseId);
    Task<(bool Success, string? Error)> CreateGoalAsync(object dto);
    Task<(bool Success, string? Error)> UpdateGoalAsync(Guid goalId, object dto);
    Task<(bool Success, string? Error)> RecordGoalProgressAsync(object dto);
    Task<(List<TreatmentGoalProgressWebDto> Data, string? Error)> GetGoalProgressHistoryAsync(Guid goalId);
    Task<(List<HomeworkWebDto> Data, string? Error)> GetHomeworkAsync(Guid caseId);
    Task<(bool Success, string? Error)> CreateHomeworkAsync(object dto);
    Task<(bool Success, string? Error)> SubmitHomeworkAsync(Guid homeworkId, object dto);
    Task<(bool Success, string? Error)> ReviewHomeworkAsync(Guid homeworkId, object dto);
    Task<(List<MoodEntryWebDto> Data, string? Error)> GetMoodEntriesAsync(Guid caseId);
    Task<(bool Success, string? Error)> AddMoodEntryAsync(object dto);
    Task<(TreatmentProgressWebDto? Data, string? Error)> GetProgressAsync(Guid caseId);
    Task<(List<TreatmentTimelineWebDto> Data, string? Error)> GetTimelineAsync(Guid caseId);
}
