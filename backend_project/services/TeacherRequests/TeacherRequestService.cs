using Microsoft.EntityFrameworkCore;
using backend_project.Data;
using backend_project.DTOs.TeacherRequests.Requests;
using backend_project.DTOs.TeacherRequests.Responses;
using backend_project.Models;
using Microsoft.AspNetCore.Identity;
using backend_project.Services.Notifications;

namespace backend_project.Services.TeacherRequests;

public class TeacherRequestService : ITeacherRequestService
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<User> _userManager;
    private readonly IEmailService _emailService;
    private readonly INotificationService _notificationService;

    public TeacherRequestService(
        ApplicationDbContext context, 
        UserManager<User> userManager,
        IEmailService emailService,
        INotificationService notificationService)
    {
        _context = context;
        _userManager = userManager;
        _emailService = emailService;
        _notificationService = notificationService;
    }

    // ========================================
    // للمستخدم العادي (Student)
    // ========================================

    public async Task<bool> CanSubmitRequestAsync(string userId)
    {
        // التحقق إذا كان المستخدم لديه طلب معلق
        var hasPending = await _context.TeacherRequests
            .AnyAsync(r => r.UserId == Guid.Parse(userId) && 
                          r.Status == TeacherRequestStatus.Pending);

        // التحقق إذا كان المستخدم معلم بالفعل
        var user = await _userManager.FindByIdAsync(userId);
        var isTeacher = await _userManager.IsInRoleAsync(user!, "Teacher");

        return !hasPending && !isTeacher;
    }

    public async Task<TeacherRequestDto> SubmitRequestAsync(string userId, SubmitTeacherRequestDto dto)
    {
        // التحقق من إمكانية التقديم
        if (!await CanSubmitRequestAsync(userId))
            throw new InvalidOperationException("لا يمكنك تقديم طلب جديد حالياً");

        // إنشاء الطلب
        var request = new TeacherRequest
        {
            UserId = Guid.Parse(userId),
            Message = dto.Message,
            Status = TeacherRequestStatus.Pending,
            SubmittedAt = DateTime.UtcNow
        };

        _context.TeacherRequests.Add(request);
        await _context.SaveChangesAsync();

        // إضافة المستندات
        foreach (var docDto in dto.Documents)
        {
            var document = new TeacherRequestDocument
            {
                RequestId = request.Id,
                DocumentType = docDto.DocumentType,
                FileId = docDto.FileId,
                UrlValue = docDto.UrlValue,
                UploadedAt = DateTime.UtcNow
            };

            _context.TeacherRequestDocuments.Add(document);
        }

        await _context.SaveChangesAsync();

        // إرسال إشعار للمستخدم
        await _notificationService.CreateAndSendNotificationAsync(
            request.UserId,
            "تم تقديم طلبك بنجاح",
            "تم استلام طلبك لتصبح معلم. سيتم مراجعته قريباً.",
            NotificationType.TeacherRequest,
            "/teacher-requests",
            "check-circle");

        return await MapToDtoAsync(request);
    }

    public async Task<TeacherRequestDto> UpdateRequestAsync(
        Guid requestId, 
        string userId, 
        UpdateTeacherRequestDto dto)
    {
        var request = await _context.TeacherRequests
            .Include(r => r.Documents)
            .FirstOrDefaultAsync(r => r.Id == requestId && 
                                     r.UserId == Guid.Parse(userId) && 
                                     r.Status == TeacherRequestStatus.Pending);

        if (request == null)
            throw new KeyNotFoundException("الطلب غير موجود أو لا يمكن تعديله");

        // تحديث الرسالة
        request.Message = dto.Message;

        // حذف المستندات القديمة
        _context.TeacherRequestDocuments.RemoveRange(request.Documents);
        request.Documents.Clear();

        // إضافة المستندات الجديدة
        foreach (var docDto in dto.Documents)
        {
            var document = new TeacherRequestDocument
            {
                RequestId = request.Id,
                DocumentType = docDto.DocumentType,
                FileId = docDto.FileId,
                UrlValue = docDto.UrlValue,
                UploadedAt = DateTime.UtcNow
            };

            _context.TeacherRequestDocuments.Add(document);
        }

        _context.TeacherRequests.Update(request);
        await _context.SaveChangesAsync();

        // إرسال إشعار للمستخدم
        await _notificationService.CreateAndSendNotificationAsync(
            request.UserId,
            "تم تحديث طلبك",
            "تم تحديث طلبك بنجاح.",
            NotificationType.TeacherRequest,
            "/teacher-requests",
            "edit");

        return await MapToDtoAsync(request);
    }

    public async Task<bool> AddDocumentAsync(
        Guid requestId, 
        string userId, 
        AddDocumentToRequestDto dto)
    {
        var request = await _context.TeacherRequests
            .FirstOrDefaultAsync(r => r.Id == requestId && 
                                     r.UserId == Guid.Parse(userId) && 
                                     r.Status == TeacherRequestStatus.Pending);

        if (request == null)
            return false;

        // التحقق من عدم تجاوز الحد الأقصى للمستندات (10)
        var currentDocumentsCount = await _context.TeacherRequestDocuments
            .CountAsync(d => d.RequestId == requestId);

        if (currentDocumentsCount >= 10)
            throw new InvalidOperationException("لا يمكن إرفاق أكثر من 10 مستندات");

        // إضافة المستند الجديد
        var document = new TeacherRequestDocument
        {
            RequestId = requestId,
            DocumentType = dto.DocumentType,
            FileId = dto.FileId,
            UrlValue = dto.UrlValue,
            UploadedAt = DateTime.UtcNow
        };

        _context.TeacherRequestDocuments.Add(document);
        await _context.SaveChangesAsync();

        // إرسال إشعار للمستخدم
        await _notificationService.CreateAndSendNotificationAsync(
            Guid.Parse(userId),
            "تم إضافة مستند جديد",
            "تم إضافة مستند جديد لطلبك.",
            NotificationType.TeacherRequest,
            "/teacher-requests",
            "paperclip");

        return true;
    }

    public async Task<List<TeacherRequestDto>> GetMyRequestsAsync(string userId)
    {
        var requests = await _context.TeacherRequests
            .Where(r => r.UserId == Guid.Parse(userId))
            .OrderByDescending(r => r.SubmittedAt)
            .ToListAsync();

        return requests.Select(MapToDto).ToList();
    }

    public async Task<TeacherRequestDetailDto> GetMyRequestByIdAsync(string userId, Guid requestId)
    {
        var request = await _context.TeacherRequests
            .Include(r => r.Documents)
            .Include(r => r.ProcessedByUser)
            .FirstOrDefaultAsync(r => r.Id == requestId && r.UserId == Guid.Parse(userId));

        if (request == null)
            throw new KeyNotFoundException("الطلب غير موجود");

        return MapToDetailDtoAsync(request);
    }

    public async Task<bool> CancelRequestAsync(string userId, Guid requestId)
    {
        var request = await _context.TeacherRequests
            .FirstOrDefaultAsync(r => r.Id == requestId && 
                                     r.UserId == Guid.Parse(userId) && 
                                     r.Status == TeacherRequestStatus.Pending);

        if (request == null)
            return false;

        request.Status = TeacherRequestStatus.Rejected;
        request.RejectionReason = "تم الإلغاء من قبل المستخدم";
        request.ProcessedAt = DateTime.UtcNow;

        _context.TeacherRequests.Update(request);
        await _context.SaveChangesAsync();

        // إرسال إشعار للمستخدم
        await _notificationService.CreateAndSendNotificationAsync(
            request.UserId,
            "تم إلغاء طلبك",
            "تم إلغاء طلبك بنجاح.",
            NotificationType.TeacherRequest,
            "/teacher-requests",
            "x-circle");

        return true;
    }

    // ========================================
    // للأدمن
    // ========================================

    public async Task<List<TeacherRequestDto>> GetPendingRequestsAsync()
    {
        var requests = await _context.TeacherRequests
            .Where(r => r.Status == TeacherRequestStatus.Pending)
            .Include(r => r.User)
            .OrderBy(r => r.SubmittedAt)
            .ToListAsync();

        return requests.Select(MapToDto).ToList();
    }

    public async Task<TeacherRequestDetailDto> GetRequestByIdAsync(Guid requestId)
    {
        var request = await _context.TeacherRequests
            .Include(r => r.User)
            .Include(r => r.ProcessedByUser)
            .Include(r => r.Documents)
                .ThenInclude(d => d.File)
            .FirstOrDefaultAsync(r => r.Id == requestId);

        if (request == null)
            throw new KeyNotFoundException("الطلب غير موجود");

        return MapToDetailDtoAsync(request);
    }

    public async Task<TeacherRequestDto> ProcessRequestAsync(
        Guid requestId, 
        string adminId, 
        ProcessTeacherRequestDto dto)
    {
        var request = await _context.TeacherRequests
            .Include(r => r.User)
            .FirstOrDefaultAsync(r => r.Id == requestId);

        if (request == null)
            throw new KeyNotFoundException("الطلب غير موجود");

        // تحديث حالة الطلب
        request.Status = dto.Status;
        request.AdminNotes = dto.AdminNotes;
        request.RejectionReason = dto.RejectionReason;
        request.ProcessedAt = DateTime.UtcNow;
        request.ProcessedBy = Guid.Parse(adminId);

        // إذا تمت الموافقة - ترقية المستخدم لمعلم
        if (dto.Status == TeacherRequestStatus.Approved)
        {
            var user = await _userManager.FindByIdAsync(request.UserId.ToString());
            if (user != null)
            {
                await _userManager.AddToRoleAsync(user, "Teacher");
            }
        }

        _context.TeacherRequests.Update(request);
        await _context.SaveChangesAsync();

        // إرسال الإشعارات (بريد إلكتروني + داخل التطبيق)
        await SendNotificationEmailAsync(request);

        return await MapToDtoAsync(request);
    }

    public async Task<bool> DeleteRequestAsync(Guid requestId)
    {
        var request = await _context.TeacherRequests.FindAsync(requestId);
        if (request == null)
            return false;

        _context.TeacherRequests.Remove(request);
        await _context.SaveChangesAsync();

        return true;
    }

    // ========================================
    // إرسال الإشعارات
    // ========================================

    private async Task SendNotificationEmailAsync(TeacherRequest request)
    {
        try
        {
            var user = await _userManager.FindByIdAsync(request.UserId.ToString());
            if (user == null)
                return;

            var userName = user.FullName ?? user.UserName ?? "مستخدم";

            // إرسال إشعار داخل التطبيق
            await SendInAppNotificationAsync(request, user);

            // إرسال بريد إلكتروني
            switch (request.Status)
            {
                case TeacherRequestStatus.Approved:
                    await _emailService.SendTeacherRequestApprovedAsync(
                        user.Email!,
                        userName,
                        request.AdminNotes);
                    break;

                case TeacherRequestStatus.Rejected:
                    await _emailService.SendTeacherRequestRejectedAsync(
                        user.Email!,
                        userName,
                        request.RejectionReason,
                        request.AdminNotes);
                    break;

                case TeacherRequestStatus.RequiresMoreInfo:
                    await _emailService.SendTeacherRequestMoreInfoAsync(
                        user.Email!,
                        userName,
                        request.AdminNotes);
                    break;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"فشل إرسال الإشعارات: {ex.Message}");
        }
    }

    private async Task SendInAppNotificationAsync(TeacherRequest request, User user)
    {
        try
        {
            string title, message, icon;
            string? linkUrl = "/teacher-requests";

            switch (request.Status)
            {
                case TeacherRequestStatus.Approved:
                    title = "🎉 تمت الموافقة على طلبك!";
                    message = request.AdminNotes ?? "تمت الموافقة على طلبك لتصبح معلم في المنصة.";
                    icon = "check-circle";
                    linkUrl = "/dashboard/teacher";
                    break;

                case TeacherRequestStatus.Rejected:
                    title = "❌ تم رفض طلبك";
                    message = request.RejectionReason ?? "للأسف تم رفض طلبك. يرجى مراجعة البريد الإلكتروني للتفاصيل.";
                    icon = "x-circle";
                    break;

                case TeacherRequestStatus.RequiresMoreInfo:
                    title = "📋 نحتاج معلومات إضافية";
                    message = "طلبك يحتاج إلى مزيد من المعلومات قبل المتابعة.";
                    icon = "info";
                    break;

                default:
                    return;
            }

            // إرسال إشعار عبر SignalR (Realtime)
            await _notificationService.CreateAndSendNotificationAsync(
                request.UserId,
                title,
                message,
                NotificationType.TeacherRequest,
                linkUrl,
                icon);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"فشل إرسال إشعار داخل التطبيق: {ex.Message}");
        }
    }

    // ========================================
    // Mapping Methods
    // ========================================

    private TeacherRequestDto MapToDto(TeacherRequest request)
    {
        return new TeacherRequestDto
        {
            Id = request.Id,
            Status = request.Status,
            Message = request.Message,
            SubmittedAt = request.SubmittedAt,
            ProcessedAt = request.ProcessedAt,
            DocumentsCount = request.Documents.Count,
            UserName = request.User?.FullName ?? "Unknown",
            UserEmail = request.User?.Email ?? "Unknown",
            ProcessedByUserName = request.ProcessedByUser?.FullName
        };
    }

    private async Task<TeacherRequestDto> MapToDtoAsync(TeacherRequest request)
    {
        await _context.Entry(request).Reference(r => r.User).LoadAsync();
        return MapToDto(request);
    }

    private TeacherRequestDetailDto MapToDetailDtoAsync(TeacherRequest request)
    {
        var dto = new TeacherRequestDetailDto
        {
            Id = request.Id,
            Status = request.Status,
            Message = request.Message,
            SubmittedAt = request.SubmittedAt,
            ProcessedAt = request.ProcessedAt,
            AdminNotes = request.AdminNotes,
            RejectionReason = request.RejectionReason,
            DocumentsCount = request.Documents.Count,
            UserName = request.User?.FullName ?? "Unknown",
            UserEmail = request.User?.Email ?? "Unknown",
            ProcessedByUserName = request.ProcessedByUser?.FullName,
            Documents = request.Documents.Select(d => new TeacherRequestDocumentDto
            {
                DocumentType = d.DocumentType,
                FileId = d.FileId,
                UrlValue = d.UrlValue
            }).ToList()
        };

        return dto;
    }
}