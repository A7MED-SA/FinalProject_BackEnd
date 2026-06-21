using Athary.Application.DTOs.InstructorRequests.Requests;
using Athary.Application.DTOs.InstructorRequests.Responses;
using Athary.Application.Interfaces.Authentication;
using Athary.Application.Interfaces.InstructorRequests;
using Athary.Application.Interfaces.Notification;
using Athary.Domain.Entities;
using Athary.Domain.Enums;
using Athary.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Athary.Infrastructure.Services.InstructorRequests;

public class InstructorRequestService : IInstructorRequestService
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<User> _userManager;
    private readonly IEmailService _emailService;
    private readonly INotificationService _notificationService;

    public InstructorRequestService(
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

    public async Task<bool> CanSubmitRequestAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var hasPending = await _context.InstructorRequests
            .AnyAsync(r => r.UserId == userId && r.Status == InstructorRequestStatus.Pending, cancellationToken);

        var user = await _userManager.FindByIdAsync(userId.ToString());
        var isInstructor = user != null && await _userManager.IsInRoleAsync(user, "Instructor");

        return !hasPending && !isInstructor;
    }

    public async Task<InstructorRequestDto> SubmitRequestAsync(Guid userId, SubmitInstructorRequestDto dto, CancellationToken cancellationToken = default)
    {
        if (!await CanSubmitRequestAsync(userId, cancellationToken))
            throw new InvalidOperationException("لا يمكنك تقديم طلب جديد حالياً");

        var request = new InstructorRequest
        {
            UserId = userId,
            Message = dto.Message,
            Status = InstructorRequestStatus.Pending,
            SubmittedAt = DateTime.UtcNow
        };

        _context.InstructorRequests.Add(request);
        await _context.SaveChangesAsync(cancellationToken);

        foreach (var docDto in dto.Documents)
        {
            var document = new InstructorRequestDocument
            {
                RequestId = request.Id,
                DocumentType = docDto.DocumentType,
                FileId = docDto.FileId,
                UrlValue = docDto.UrlValue,
                UploadedAt = DateTime.UtcNow
            };

            _context.InstructorRequestDocuments.Add(document);
        }

        await _context.SaveChangesAsync(cancellationToken);

        await _notificationService.CreateAndSendNotificationAsync(
            request.UserId,
            "تم تقديم طلبك بنجاح",
            "تم استلام طلبك لتصبح معلماً. سيتم مراجعته قريباً.",
            NotificationType.InstructorRequest,
            "/instructor-requests",
            "check-circle",
            cancellationToken);

        return await MapToDtoAsync(request, cancellationToken);
    }

    public async Task<InstructorRequestDto> UpdateRequestAsync(Guid requestId, Guid userId, UpdateInstructorRequestDto dto, CancellationToken cancellationToken = default)
    {
        var request = await _context.InstructorRequests
            .Include(r => r.Documents)
            .FirstOrDefaultAsync(r => r.Id == requestId && r.UserId == userId && r.Status == InstructorRequestStatus.Pending, cancellationToken);

        if (request == null)
            throw new KeyNotFoundException("الطلب غير موجود أو لا يمكن تعديله");

        request.Message = dto.Message;

        _context.InstructorRequestDocuments.RemoveRange(request.Documents);
        request.Documents.Clear();

        foreach (var docDto in dto.Documents)
        {
            var document = new InstructorRequestDocument
            {
                RequestId = request.Id,
                DocumentType = docDto.DocumentType,
                FileId = docDto.FileId,
                UrlValue = docDto.UrlValue,
                UploadedAt = DateTime.UtcNow
            };

            _context.InstructorRequestDocuments.Add(document);
        }

        _context.InstructorRequests.Update(request);
        await _context.SaveChangesAsync(cancellationToken);

        await _notificationService.CreateAndSendNotificationAsync(
            request.UserId,
            "تم تحديث طلبك",
            "تم تحديث طلبك بنجاح.",
            NotificationType.InstructorRequest,
            "/instructor-requests",
            "edit",
            cancellationToken);

        return await MapToDtoAsync(request, cancellationToken);
    }

    public async Task<bool> AddDocumentAsync(Guid requestId, Guid userId, AddDocumentToRequestDto dto, CancellationToken cancellationToken = default)
    {
        var request = await _context.InstructorRequests
            .FirstOrDefaultAsync(r => r.Id == requestId && r.UserId == userId && r.Status == InstructorRequestStatus.Pending, cancellationToken);

        if (request == null)
            return false;

        var currentDocumentsCount = await _context.InstructorRequestDocuments
            .CountAsync(d => d.RequestId == requestId, cancellationToken);

        if (currentDocumentsCount >= 10)
            throw new InvalidOperationException("لا يمكن إرفاق أكثر من 10 مستندات");

        var document = new InstructorRequestDocument
        {
            RequestId = requestId,
            DocumentType = dto.DocumentType,
            FileId = dto.FileId,
            UrlValue = dto.UrlValue,
            UploadedAt = DateTime.UtcNow
        };

        _context.InstructorRequestDocuments.Add(document);
        await _context.SaveChangesAsync(cancellationToken);

        await _notificationService.CreateAndSendNotificationAsync(
            userId,
            "تم إضافة مستند جديد",
            "تم إضافة مستند جديد لطلبك.",
            NotificationType.InstructorRequest,
            "/instructor-requests",
            "paperclip",
            cancellationToken);

        return true;
    }

    public async Task<List<InstructorRequestDto>> GetMyRequestsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var requests = await _context.InstructorRequests
            .Where(r => r.UserId == userId)
            .Include(r => r.Documents)
            .OrderByDescending(r => r.SubmittedAt)
            .ToListAsync(cancellationToken);

        return requests.Select(MapToDto).ToList();
    }

    public async Task<InstructorRequestDetailDto> GetMyRequestByIdAsync(Guid userId, Guid requestId, CancellationToken cancellationToken = default)
    {
        var request = await _context.InstructorRequests
            .Include(r => r.Documents)
            .Include(r => r.ProcessedByUser)
            .FirstOrDefaultAsync(r => r.Id == requestId && r.UserId == userId, cancellationToken);

        if (request == null)
            throw new KeyNotFoundException("الطلب غير موجود");

        return MapToDetailDto(request);
    }

    public async Task<bool> CancelRequestAsync(Guid userId, Guid requestId, CancellationToken cancellationToken = default)
    {
        var request = await _context.InstructorRequests
            .FirstOrDefaultAsync(r => r.Id == requestId && r.UserId == userId && r.Status == InstructorRequestStatus.Pending, cancellationToken);

        if (request == null)
            return false;

        request.Status = InstructorRequestStatus.Rejected;
        request.RejectionReason = "تم الإلغاء من قبل المستخدم";
        request.ProcessedAt = DateTime.UtcNow;

        _context.InstructorRequests.Update(request);
        await _context.SaveChangesAsync(cancellationToken);

        await _notificationService.CreateAndSendNotificationAsync(
            request.UserId,
            "تم إلغاء طلبك",
            "تم إلغاء طلبك بنجاح.",
            NotificationType.InstructorRequest,
            "/instructor-requests",
            "x-circle",
            cancellationToken);

        return true;
    }

    public async Task<List<InstructorRequestDto>> GetPendingRequestsAsync(CancellationToken cancellationToken = default)
    {
        var requests = await _context.InstructorRequests
            .Where(r => r.Status == InstructorRequestStatus.Pending)
            .Include(r => r.User)
            .Include(r => r.Documents)
            .OrderBy(r => r.SubmittedAt)
            .ToListAsync(cancellationToken);

        return requests.Select(MapToDto).ToList();
    }

    public async Task<InstructorRequestDetailDto> GetRequestByIdAsync(Guid requestId, CancellationToken cancellationToken = default)
    {
        var request = await _context.InstructorRequests
            .Include(r => r.User)
            .Include(r => r.ProcessedByUser)
            .Include(r => r.Documents)
                .ThenInclude(d => d.File)
            .FirstOrDefaultAsync(r => r.Id == requestId, cancellationToken);

        if (request == null)
            throw new KeyNotFoundException("الطلب غير موجود");

        return MapToDetailDto(request);
    }

    public async Task<InstructorRequestDto> ProcessRequestAsync(Guid requestId, Guid adminId, ProcessInstructorRequestDto dto, CancellationToken cancellationToken = default)
    {
        var request = await _context.InstructorRequests
            .Include(r => r.User)
            .FirstOrDefaultAsync(r => r.Id == requestId, cancellationToken);

        if (request == null)
            throw new KeyNotFoundException("الطلب غير موجود");

        request.Status = dto.Status;
        request.AdminNotes = dto.AdminNotes;
        request.RejectionReason = dto.RejectionReason;
        request.ProcessedAt = DateTime.UtcNow;
        request.ProcessedBy = adminId;

        if (dto.Status == InstructorRequestStatus.Approved)
        {
            var user = await _userManager.FindByIdAsync(request.UserId.ToString());
            if (user != null)
            {
                await _userManager.AddToRoleAsync(user, "Instructor");
            }
        }

        _context.InstructorRequests.Update(request);
        await _context.SaveChangesAsync(cancellationToken);

        await SendNotificationEmailAsync(request, cancellationToken);

        return await MapToDtoAsync(request, cancellationToken);
    }

    public async Task<bool> DeleteRequestAsync(Guid requestId, CancellationToken cancellationToken = default)
    {
        var request = await _context.InstructorRequests.FindAsync(new object[] { requestId }, cancellationToken);
        if (request == null)
            return false;

        _context.InstructorRequests.Remove(request);
        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }

    private async Task SendNotificationEmailAsync(InstructorRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await _userManager.FindByIdAsync(request.UserId.ToString());
            if (user == null)
                return;

            var userName = user.FullName ?? user.UserName ?? "مستخدم";

            await SendInAppNotificationAsync(request, user, cancellationToken);

            switch (request.Status)
            {
                case InstructorRequestStatus.Approved:
                    await _emailService.SendInstructorRequestApprovedAsync(user.Email!, userName, request.AdminNotes, cancellationToken);
                    break;
                case InstructorRequestStatus.Rejected:
                    await _emailService.SendInstructorRequestRejectedAsync(user.Email!, userName, request.RejectionReason, request.AdminNotes, cancellationToken);
                    break;
                case InstructorRequestStatus.RequiresMoreInfo:
                    await _emailService.SendInstructorRequestMoreInfoAsync(user.Email!, userName, request.AdminNotes, cancellationToken);
                    break;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"فشل إرسال الإشعارات: {ex.Message}");
        }
    }

    private async Task SendInAppNotificationAsync(InstructorRequest request, User user, CancellationToken cancellationToken = default)
    {
        try
        {
            string title, message, icon;
            string? linkUrl = "/instructor-requests";

            switch (request.Status)
            {
                case InstructorRequestStatus.Approved:
                    title = "تمت الموافقة على طلبك!";
                    message = request.AdminNotes ?? "تمت الموافقة على طلبك لتصبح معلماً في المنصة.";
                    icon = "check-circle";
                    linkUrl = "/dashboard/instructor";
                    break;
                case InstructorRequestStatus.Rejected:
                    title = "تم رفض طلبك";
                    message = request.RejectionReason ?? "للأسف تم رفض طلبك. يرجى مراجعة البريد الإلكتروني للتفاصيل.";
                    icon = "x-circle";
                    break;
                case InstructorRequestStatus.RequiresMoreInfo:
                    title = "نحتاج معلومات إضافية";
                    message = "طلبك يحتاج إلى مزيد من المعلومات قبل المتابعة.";
                    icon = "info";
                    break;
                default:
                    return;
            }

            await _notificationService.CreateAndSendNotificationAsync(
                request.UserId,
                title,
                message,
                NotificationType.InstructorRequest,
                linkUrl,
                icon,
                cancellationToken);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"فشل إرسال إشعار داخل التطبيق: {ex.Message}");
        }
    }

    private InstructorRequestDto MapToDto(InstructorRequest request)
    {
        return new InstructorRequestDto
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

    private async Task<InstructorRequestDto> MapToDtoAsync(InstructorRequest request, CancellationToken cancellationToken = default)
    {
        await _context.Entry(request).Reference(r => r.User).LoadAsync(cancellationToken);
        return MapToDto(request);
    }

    private InstructorRequestDetailDto MapToDetailDto(InstructorRequest request)
    {
        return new InstructorRequestDetailDto
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
            Documents = request.Documents.Select(d => new InstructorRequestDocumentDto
            {
                DocumentType = d.DocumentType,
                FileId = d.FileId,
                UrlValue = d.UrlValue
            }).ToList()
        };
    }
}
