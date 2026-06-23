using Athary.Application.Interfaces.Authentication;
using Athary.Application.Interfaces.Category;
using Athary.Application.Interfaces.Certificate;
using Athary.Application.Interfaces.Communication;
using Athary.Application.Interfaces.Contact;
using Athary.Application.Interfaces.Courses;
using Athary.Application.Interfaces.Commerce;
using Athary.Application.Interfaces.Admin;
using Athary.Application.Interfaces.Dashboard;
using Athary.Application.Interfaces.InstructorRequests;
using Athary.Application.Interfaces.LiveSession;
using Athary.Application.Interfaces.Media;
using Athary.Application.Interfaces.Notification;
using Athary.Application.Interfaces.Profile;
using Athary.Application.Interfaces.Public;
using Athary.Application.Interfaces.Review;
using Athary.Application.Interfaces.VideoComment;
using Athary.Application.Interfaces.Wishlist;
using Athary.Domain.Interfaces;
using Athary.Infrastructure.Data;
using Athary.Infrastructure.Repositories;
using Athary.Infrastructure.Services.Authentication;
using Athary.Infrastructure.Services.Category;
using Athary.Infrastructure.Services.Certificate;
using Athary.Infrastructure.Helpers;
using Athary.Infrastructure.Services.Communication;
using Athary.Infrastructure.Services.Commerce;
using Athary.Infrastructure.Services.Commerce.PaymentGateway;
using Athary.Infrastructure.Services.Courses;
using Athary.Infrastructure.Services.Admin;
using Athary.Infrastructure.Services.Dashboard;
using Athary.Infrastructure.Services.InstructorRequests;
using Athary.Infrastructure.Services.LiveSession;
using Athary.Infrastructure.Services.Media;
using Athary.Infrastructure.Services.Notification;
using Athary.Infrastructure.Services.Profile;
using Athary.Infrastructure.Services.Public;
using Athary.Infrastructure.Services.Contact;
using Athary.Infrastructure.Services.Review;
using Athary.Infrastructure.Services.VideoComment;
using Athary.Infrastructure.Services.Wishlist;
using Athary.Infrastructure.Settings;
using Athary.Infrastructure.Workers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Minio;

namespace Athary.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.GetName().Name)));

        services.AddScoped(typeof(IRepository<>), typeof(GenericRepository<>));
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Settings
        services.Configure<EmailSettings>(configuration.GetSection("EmailSettings"));
        services.Configure<MinioSettings>(configuration.GetSection("MinioSettings"));

        // Auth Services
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<ISessionService, SessionService>();
        services.AddScoped<IVerificationService, VerificationService>();
        services.AddScoped<IPermissionService, PermissionService>();
        services.AddScoped<IAuthenticationService, AuthenticationService>();
        services.AddScoped<IOAuthService, OAuthService>();

        // Communication Services
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<IActivityLogService, ActivityLogService>();
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<IMessageService, MessageService>();
        services.AddScoped<ISystemSettingService, SystemSettingService>();
        services.AddScoped<IAnnouncementService, AnnouncementService>();
        services.AddScoped<IReportService, ReportService>();
        services.AddScoped<EnrollmentGuard>();

        // MinIO Object Storage
        services.AddScoped<IMinioClient>(sp =>
        {
            var settings = sp.GetRequiredService<IOptions<MinioSettings>>().Value;
            return new MinioClient()
                .WithEndpoint(settings.Endpoint)
                .WithCredentials(settings.AccessKey, settings.SecretKey)
                .WithSSL(settings.UseSsl)
                .Build();
        });
        services.AddScoped<IObjectStorage, MinioObjectStorage>();

        // Media Services
        services.AddScoped<IMediaService, MediaService>();
        services.AddScoped<IAdminMediaService, AdminMediaService>();
        services.AddScoped<IVideoProcessingService, VideoProcessingService>();

        // Profile Services
        services.AddScoped<IProfileService, ProfileService>();

        // Course Services
        services.AddScoped<ICourseService, CourseService>();
        services.AddScoped<ISectionService, SectionService>();
        services.AddScoped<IEnrollmentService, EnrollmentService>();
        services.AddScoped<IContentProgressService, ContentProgressService>();
        services.AddScoped<IVideoService, VideoService>();
        services.AddScoped<IDocumentService, DocumentService>();
        services.AddScoped<IQuizService, QuizService>();
        services.AddScoped<IQuizAttemptService, QuizAttemptService>();
        services.AddScoped<IPublicCourseService, PublicCourseService>();
        services.AddScoped<ICourseEditApprovalService, CourseEditApprovalService>();

        // Certificate Services
        services.AddScoped<ICertificateService, CertificateService>();

        // Instructor Request Services
        services.AddScoped<IInstructorRequestService, InstructorRequestService>();

        // Category Services
        services.AddScoped<ICategoryService, CategoryService>();

        // Review Services
        services.AddScoped<IReviewService, ReviewService>();

        // Video Comment Services
        services.AddScoped<IVideoCommentService, VideoCommentService>();

        // Wishlist Services
        services.AddScoped<IWishlistService, WishlistService>();

        // Live Session Services
        services.AddScoped<IStreamingProvider, DefaultStreamingProvider>();
        services.AddScoped<ILiveSessionService, LiveSessionService>();
        services.AddScoped<ILiveAttendanceService, LiveAttendanceService>();

        // Public Services (Backend Modifications)
        services.AddScoped<IPublicService, PublicService>();
        services.AddScoped<ITestimonialService, TestimonialService>();
        services.AddScoped<ILegalPageService, LegalPageService>();

        // Contact Services
        services.AddScoped<IContactService, ContactService>();

        // Notification Preference Services
        services.AddScoped<INotificationPreferenceService, NotificationPreferenceService>();

        // Commerce Services
        services.AddScoped<IPaymentGateway, MockPaymentGateway>();
        services.AddScoped<ICartService, CartService>();
        services.AddScoped<ICouponService, CouponService>();
        services.AddScoped<IOrderService, OrderService>();
        services.AddScoped<IPaymentService, PaymentService>();
        services.AddScoped<IRefundService, RefundService>();

        // Admin Services
        services.AddScoped<IAdminUserService, AdminUserService>();

        // Dashboard Services
        services.AddScoped<IAdminDashboardService, AdminDashboardService>();
        services.AddScoped<IInstructorDashboardService, InstructorDashboardService>();
        services.AddScoped<IStudentDashboardService, StudentDashboardService>();

        // SignalR
        services.AddSignalR();

        // Background Workers
        services.AddHostedService<VideoProcessingWorker>();
        services.AddHostedService<EditRequestCleanupService>();
        services.AddHostedService<ScheduledDeletionService>();

        return services;
    }
}
