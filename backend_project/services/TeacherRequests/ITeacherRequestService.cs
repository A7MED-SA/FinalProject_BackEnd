using backend_project.DTOs.TeacherRequests.Requests;
using backend_project.DTOs.TeacherRequests.Responses;

namespace backend_project.Services.TeacherRequests;

public interface ITeacherRequestService
{
    // للمستخدم العادي (Student)
    Task<bool> CanSubmitRequestAsync(string userId);
    Task<TeacherRequestDto> SubmitRequestAsync(string userId, SubmitTeacherRequestDto dto);
    Task<TeacherRequestDto> UpdateRequestAsync(Guid requestId, string userId, UpdateTeacherRequestDto dto);
    Task<bool> AddDocumentAsync(Guid requestId, string userId, AddDocumentToRequestDto dto);
    Task<List<TeacherRequestDto>> GetMyRequestsAsync(string userId);
    Task<TeacherRequestDetailDto> GetMyRequestByIdAsync(string userId, Guid requestId);
    Task<bool> CancelRequestAsync(string userId, Guid requestId);

    // للأدمن
    Task<List<TeacherRequestDto>> GetPendingRequestsAsync();
    Task<TeacherRequestDetailDto> GetRequestByIdAsync(Guid requestId);
    Task<TeacherRequestDto> ProcessRequestAsync(Guid requestId, string adminId, ProcessTeacherRequestDto dto);
    Task<bool> DeleteRequestAsync(Guid requestId);
}