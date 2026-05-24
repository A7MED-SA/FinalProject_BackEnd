using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using backend_project.Models;

namespace backend_project.Data;

public class ApplicationDbContext : IdentityDbContext<User, Role, Guid, 
    Microsoft.AspNetCore.Identity.IdentityUserClaim<Guid>,
    UserRole,
    Microsoft.AspNetCore.Identity.IdentityUserLogin<Guid>,
    Microsoft.AspNetCore.Identity.IdentityRoleClaim<Guid>,
    Microsoft.AspNetCore.Identity.IdentityUserToken<Guid>>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    // User & Authentication (Users, Roles, UserRoles are inherited from IdentityDbContext)
    public DbSet<UserPhone> UserPhones { get; set; }
    public DbSet<Session> Sessions { get; set; }
    public DbSet<VerificationToken> VerificationTokens { get; set; }
    public DbSet<Address> Addresses { get; set; }
    public DbSet<TeacherRequest> TeacherRequests { get; set; }
    public DbSet<TeacherRequestDocument> TeacherRequestDocuments { get; set; }

    // Permissions
    public DbSet<Permission> Permissions { get; set; }
    public DbSet<RolePermission> RolePermissions { get; set; }

    // Courses
    public DbSet<Category> Categories { get; set; }
    public DbSet<Course> Courses { get; set; }
    public DbSet<CourseRequirement> CourseRequirements { get; set; }
    public DbSet<CourseLearningOutcome> CourseLearningOutcomes { get; set; }
    public DbSet<Section> Sections { get; set; }
    public DbSet<SectionItem> SectionItems { get; set; }

    // Commerce
    public DbSet<Wishlist> Wishlists { get; set; }
    public DbSet<Cart> Carts { get; set; }
    public DbSet<CartItem> CartItems { get; set; }
    public DbSet<Review> Reviews { get; set; }
    public DbSet<ReviewHelpful> ReviewHelpfuls { get; set; }
    public DbSet<Certificate> Certificates { get; set; }
    public DbSet<Coupon> Coupons { get; set; }
    public DbSet<CouponCourse> CouponCourses { get; set; }
    public DbSet<CouponUsage> CouponUsages { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }
    public DbSet<PaymentMethod> PaymentMethods { get; set; }
    public DbSet<Payment> Payments { get; set; }
    public DbSet<Refund> Refunds { get; set; }
    public DbSet<TransactionLog> TransactionLogs { get; set; }

    // Content & Learning
    public DbSet<Video> Videos { get; set; }
    public DbSet<Document> Documents { get; set; }
    public DbSet<VideoComment> VideoComments { get; set; }
    public DbSet<CommentLike> CommentLikes { get; set; }
    public DbSet<Quiz> Quizzes { get; set; }
    public DbSet<Question> Questions { get; set; }
    public DbSet<Option> Options { get; set; }
    public DbSet<QuizAttempt> QuizAttempts { get; set; }
    public DbSet<UserAnswer> UserAnswers { get; set; }
    public DbSet<LiveSession> LiveSessions { get; set; }
    public DbSet<LiveAttendance> LiveAttendances { get; set; }
    public DbSet<Enrollment> Enrollments { get; set; }
    public DbSet<ContentProgress> ContentProgresses { get; set; }

    // System
    public DbSet<UploadedFile> Files { get; set; }
    public DbSet<Notification> Notifications { get; set; }
    public DbSet<Message> Messages { get; set; }
    public DbSet<Announcement> Announcements { get; set; }
    public DbSet<Report> Reports { get; set; }
    public DbSet<ActivityLog> ActivityLogs { get; set; }
    public DbSet<CourseLog> CourseLogs { get; set; }
    public DbSet<CourseEditRequest> CourseEditRequests { get; set; }
    public DbSet<SystemSetting> SystemSettings { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure Identity table names
        modelBuilder.Entity<User>().ToTable("users");
        modelBuilder.Entity<Role>().ToTable("roles");
        modelBuilder.Entity<UserRole>().ToTable("user_roles");
        modelBuilder.Entity<Microsoft.AspNetCore.Identity.IdentityUserClaim<Guid>>().ToTable("user_claims");
        modelBuilder.Entity<Microsoft.AspNetCore.Identity.IdentityUserLogin<Guid>>().ToTable("user_logins");
        modelBuilder.Entity<Microsoft.AspNetCore.Identity.IdentityUserToken<Guid>>().ToTable("user_tokens");
        modelBuilder.Entity<Microsoft.AspNetCore.Identity.IdentityRoleClaim<Guid>>().ToTable("role_claims");

        // Configure ID generation - Application generates IDs, NOT database
        modelBuilder.Entity<User>()
            .Property(u => u.Id)
            .ValueGeneratedNever(); // Database will NOT generate ID

        modelBuilder.Entity<Role>()
            .Property(r => r.Id)
            .ValueGeneratedNever(); // Database will NOT generate ID

        // User Relationships
        modelBuilder.Entity<User>()
            .HasMany(u => u.UserPhones)
            .WithOne(up => up.User)
            .HasForeignKey(up => up.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<User>()
            .HasMany(u => u.Sessions)
            .WithOne(s => s.User)
            .HasForeignKey(s => s.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<User>()
            .HasMany(u => u.Addresses)
            .WithOne(a => a.User)
            .HasForeignKey(a => a.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // VerificationToken Relationships
        modelBuilder.Entity<VerificationToken>()
            .HasOne(vt => vt.User)
            .WithMany()
            .HasForeignKey(vt => vt.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Teacher Request Relationships
        modelBuilder.Entity<TeacherRequest>()
            .HasOne(tr => tr.User)
            .WithMany(u => u.SubmittedTeacherRequests)
            .HasForeignKey(tr => tr.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<TeacherRequest>()
            .HasOne(tr => tr.ProcessedByUser)
            .WithMany(u => u.ProcessedTeacherRequests)
            .HasForeignKey(tr => tr.ProcessedBy)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<TeacherRequest>()
            .HasMany(tr => tr.Documents)
            .WithOne(trd => trd.Request)
            .HasForeignKey(trd => trd.RequestId)
            .OnDelete(DeleteBehavior.Cascade);

        // Role & Permission Relationships
        modelBuilder.Entity<RolePermission>()
            .HasOne(rp => rp.Role)
            .WithMany(r => r.RolePermissions)
            .HasForeignKey(rp => rp.RoleId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<RolePermission>()
            .HasOne(rp => rp.Permission)
            .WithMany(p => p.RolePermissions)
            .HasForeignKey(rp => rp.PermissionId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<UserRole>()
            .HasOne(ur => ur.User)
            .WithMany(u => u.UserRoles)
            .HasForeignKey(ur => ur.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<UserRole>()
            .HasOne(ur => ur.Role)
            .WithMany(r => r.UserRoles)
            .HasForeignKey(ur => ur.RoleId)
            .OnDelete(DeleteBehavior.Cascade);

        // Category Self-Referencing Relationship
        modelBuilder.Entity<Category>()
            .HasOne(c => c.ParentCategory)
            .WithMany(c => c.SubCategories)
            .HasForeignKey(c => c.ParentCategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        // Course Relationships
        modelBuilder.Entity<Course>()
            .HasOne(c => c.Creator)
            .WithMany(u => u.CreatedCourses)
            .HasForeignKey(c => c.CreatedBy)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Course>()
            .HasOne(c => c.Approver)
            .WithMany(u => u.ApprovedCourses)
            .HasForeignKey(c => c.ApprovedBy)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Course>()
            .HasOne(c => c.Category)
            .WithMany(cat => cat.Courses)
            .HasForeignKey(c => c.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Course>()
            .HasMany(c => c.CourseRequirements)
            .WithOne(cr => cr.Course)
            .HasForeignKey(cr => cr.CourseId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Course>()
            .HasMany(c => c.CourseLearningOutcomes)
            .WithOne(clo => clo.Course)
            .HasForeignKey(clo => clo.CourseId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Course>()
            .HasMany(c => c.Sections)
            .WithOne(s => s.Course)
            .HasForeignKey(s => s.CourseId)
            .OnDelete(DeleteBehavior.Cascade);

        // Section Relationships
        modelBuilder.Entity<Section>()
            .HasMany(s => s.SectionItems)
            .WithOne(si => si.Section)
            .HasForeignKey(si => si.SectionId)
            .OnDelete(DeleteBehavior.Cascade);

        // SectionItem Polymorphic Relationships (No FK constraints - handled at app level)
        // These are navigation-only relationships based on ItemType and ItemId
        modelBuilder.Entity<SectionItem>()
            .HasOne(si => si.Video)
            .WithOne(v => v.SectionItem)
            .HasForeignKey<SectionItem>(si => si.ItemId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<SectionItem>()
            .HasOne(si => si.Quiz)
            .WithOne(q => q.SectionItem)
            .HasForeignKey<SectionItem>(si => si.ItemId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<SectionItem>()
            .HasOne(si => si.Document)
            .WithOne(d => d.SectionItem)
            .HasForeignKey<SectionItem>(si => si.ItemId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<SectionItem>()
            .HasOne(si => si.LiveSession)
            .WithOne(ls => ls.SectionItem)
            .HasForeignKey<SectionItem>(si => si.ItemId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.NoAction);

        // Wishlist Relationships
        modelBuilder.Entity<Wishlist>()
            .HasOne(w => w.User)
            .WithMany()
            .HasForeignKey(w => w.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Wishlist>()
            .HasOne(w => w.Course)
            .WithMany()
            .HasForeignKey(w => w.CourseId)
            .OnDelete(DeleteBehavior.Cascade);

        // Cart Relationships
        modelBuilder.Entity<Cart>()
            .HasOne(c => c.User)
            .WithMany()
            .HasForeignKey(c => c.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Cart>()
            .HasMany(c => c.CartItems)
            .WithOne(ci => ci.Cart)
            .HasForeignKey(ci => ci.CartId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<CartItem>()
            .HasOne(ci => ci.Course)
            .WithMany()
            .HasForeignKey(ci => ci.CourseId)
            .OnDelete(DeleteBehavior.Restrict);

        // Review Relationships
        modelBuilder.Entity<Review>()
            .HasOne(r => r.User)
            .WithMany()
            .HasForeignKey(r => r.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Review>()
            .HasOne(r => r.Course)
            .WithMany()
            .HasForeignKey(r => r.CourseId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Review>()
            .HasOne(r => r.Moderator)
            .WithMany()
            .HasForeignKey(r => r.ModeratedBy)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Review>()
            .HasOne(r => r.FlaggedByUser)
            .WithMany()
            .HasForeignKey(r => r.FlaggedBy)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Review>()
            .HasMany(r => r.ReviewHelpfuls)
            .WithOne(rh => rh.Review)
            .HasForeignKey(rh => rh.ReviewId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Review>()
            .HasIndex(r => new { r.UserId, r.CourseId })
            .IsUnique();

        modelBuilder.Entity<Review>()
            .HasIndex(r => r.IsFlagged);

        modelBuilder.Entity<ReviewHelpful>()
            .HasOne(rh => rh.User)
            .WithMany()
            .HasForeignKey(rh => rh.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        // Certificate Relationships
        modelBuilder.Entity<Certificate>()
            .HasOne(c => c.User)
            .WithMany()
            .HasForeignKey(c => c.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Certificate>()
            .HasOne(c => c.Course)
            .WithMany()
            .HasForeignKey(c => c.CourseId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Certificate>()
            .HasOne(c => c.Enrollment)
            .WithMany()
            .HasForeignKey(c => c.EnrollmentId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Certificate>()
            .HasOne(c => c.RevokedByUser)
            .WithMany()
            .HasForeignKey(c => c.RevokedBy)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Certificate>()
            .HasIndex(c => c.VerificationCode)
            .IsUnique();

        // Coupon Relationships
        modelBuilder.Entity<Coupon>()
            .HasOne(c => c.Creator)
            .WithMany()
            .HasForeignKey(c => c.CreatedBy)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Coupon>()
            .HasMany(c => c.CouponCourses)
            .WithOne(cc => cc.Coupon)
            .HasForeignKey(cc => cc.CouponId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Coupon>()
            .HasMany(c => c.CouponUsages)
            .WithOne(cu => cu.Coupon)
            .HasForeignKey(cu => cu.CouponId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<CouponCourse>()
            .HasOne(cc => cc.Course)
            .WithMany()
            .HasForeignKey(cc => cc.CourseId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<CouponUsage>()
            .HasOne(cu => cu.User)
            .WithMany()
            .HasForeignKey(cu => cu.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<CouponUsage>()
            .HasOne(cu => cu.Order)
            .WithMany(o => o.CouponUsages)
            .HasForeignKey(cu => cu.OrderId)
            .OnDelete(DeleteBehavior.Restrict);

        // Order Relationships
        modelBuilder.Entity<Order>()
            .HasOne(o => o.User)
            .WithMany()
            .HasForeignKey(o => o.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Order>()
            .HasOne(o => o.BillingAddress)
            .WithMany()
            .HasForeignKey(o => o.BillingAddressId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Order>()
            .HasOne(o => o.Coupon)
            .WithMany(c => c.Orders)
            .HasForeignKey(o => o.CouponId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Order>()
            .HasMany(o => o.OrderItems)
            .WithOne(oi => oi.Order)
            .HasForeignKey(oi => oi.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Order>()
            .HasMany(o => o.Payments)
            .WithOne(p => p.Order)
            .HasForeignKey(p => p.OrderId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<OrderItem>()
            .HasOne(oi => oi.Course)
            .WithMany()
            .HasForeignKey(oi => oi.CourseId)
            .OnDelete(DeleteBehavior.Restrict);

        // Payment Relationships
        modelBuilder.Entity<Payment>()
            .HasOne(p => p.User)
            .WithMany()
            .HasForeignKey(p => p.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Payment>()
            .HasOne(p => p.PaymentMethod)
            .WithMany(pm => pm.Payments)
            .HasForeignKey(p => p.PaymentMethodId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Payment>()
            .HasMany(p => p.TransactionLogs)
            .WithOne(tl => tl.Payment)
            .HasForeignKey(tl => tl.PaymentId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Payment>()
            .HasMany(p => p.Refunds)
            .WithOne(r => r.Payment)
            .HasForeignKey(r => r.PaymentId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Refund>()
            .HasOne(r => r.ProcessedByUser)
            .WithMany()
            .HasForeignKey(r => r.ProcessedBy)
            .OnDelete(DeleteBehavior.Restrict);

        // Video & Comment Relationships
        modelBuilder.Entity<Video>()
            .HasMany(v => v.VideoComments)
            .WithOne(vc => vc.Video)
            .HasForeignKey(vc => vc.VideoId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<VideoComment>()
            .HasOne(vc => vc.User)
            .WithMany()
            .HasForeignKey(vc => vc.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<VideoComment>()
            .HasOne(vc => vc.ParentComment)
            .WithMany(vc => vc.Replies)
            .HasForeignKey(vc => vc.ParentCommentId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<VideoComment>()
            .HasMany(vc => vc.CommentLikes)
            .WithOne(cl => cl.Comment)
            .HasForeignKey(cl => cl.CommentId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<CommentLike>()
            .HasOne(cl => cl.User)
            .WithMany()
            .HasForeignKey(cl => cl.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        // Quiz Relationships
        modelBuilder.Entity<Quiz>()
            .HasMany(q => q.Questions)
            .WithOne(qu => qu.Quiz)
            .HasForeignKey(qu => qu.QuizId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Quiz>()
            .HasMany(q => q.QuizAttempts)
            .WithOne(qa => qa.Quiz)
            .HasForeignKey(qa => qa.QuizId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Question>()
            .HasMany(q => q.Options)
            .WithOne(o => o.Question)
            .HasForeignKey(o => o.QuestionId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Question>()
            .HasMany(q => q.UserAnswers)
            .WithOne(ua => ua.Question)
            .HasForeignKey(ua => ua.QuestionId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<QuizAttempt>()
            .HasOne(qa => qa.Enrollment)
            .WithMany(e => e.QuizAttempts)
            .HasForeignKey(qa => qa.EnrollmentId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<QuizAttempt>()
            .HasMany(qa => qa.UserAnswers)
            .WithOne(ua => ua.Attempt)
            .HasForeignKey(ua => ua.AttemptId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<UserAnswer>()
            .HasOne(ua => ua.SelectedOption)
            .WithMany(o => o.UserAnswers)
            .HasForeignKey(ua => ua.SelectedOptionId)
            .OnDelete(DeleteBehavior.Restrict);

        // Live Session Relationships
        modelBuilder.Entity<LiveSession>()
            .HasOne(ls => ls.Course)
            .WithMany()
            .HasForeignKey(ls => ls.CourseId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<LiveSession>()
            .HasMany(ls => ls.LiveAttendances)
            .WithOne(la => la.Session)
            .HasForeignKey(la => la.SessionId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<LiveAttendance>()
            .HasOne(la => la.User)
            .WithMany()
            .HasForeignKey(la => la.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        // Enrollment Relationships
        modelBuilder.Entity<Enrollment>()
            .HasOne(e => e.User)
            .WithMany()
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Enrollment>()
            .HasOne(e => e.Course)
            .WithMany()
            .HasForeignKey(e => e.CourseId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Enrollment>()
            .HasOne(e => e.Certificate)
            .WithMany()
            .HasForeignKey(e => e.CertificateId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Enrollment>()
            .HasMany(e => e.ContentProgresses)
            .WithOne(cp => cp.Enrollment)
            .HasForeignKey(cp => cp.EnrollmentId)
            .OnDelete(DeleteBehavior.Cascade);

        // System Relationships
        modelBuilder.Entity<UploadedFile>()
            .HasOne(f => f.Uploader)
            .WithMany()
            .HasForeignKey(f => f.UploadedBy)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Notification>()
            .HasOne(n => n.User)
            .WithMany()
            .HasForeignKey(n => n.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Message>()
            .HasOne(m => m.Sender)
            .WithMany()
            .HasForeignKey(m => m.SenderId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Message>()
            .HasOne(m => m.Receiver)
            .WithMany()
            .HasForeignKey(m => m.ReceiverId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Announcement>()
            .HasOne(a => a.Creator)
            .WithMany()
            .HasForeignKey(a => a.CreatedBy)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Report>()
            .HasOne(r => r.Reporter)
            .WithMany()
            .HasForeignKey(r => r.ReporterId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Report>()
            .HasOne(r => r.Resolver)
            .WithMany()
            .HasForeignKey(r => r.ResolvedBy)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<ActivityLog>()
            .HasOne(al => al.User)
            .WithMany()
            .HasForeignKey(al => al.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<CourseLog>()
            .HasOne(cl => cl.Course)
            .WithMany()
            .HasForeignKey(cl => cl.CourseId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<CourseLog>()
            .HasOne(cl => cl.User)
            .WithMany()
            .HasForeignKey(cl => cl.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        // CourseEditRequest Relationships
        modelBuilder.Entity<CourseEditRequest>()
            .HasOne(r => r.Course)
            .WithMany()
            .HasForeignKey(r => r.CourseId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<CourseEditRequest>()
            .HasOne(r => r.RequestedByUser)
            .WithMany()
            .HasForeignKey(r => r.RequestedBy)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<CourseEditRequest>()
            .HasOne(r => r.ReviewedByUser)
            .WithMany()
            .HasForeignKey(r => r.ReviewedBy)
            .OnDelete(DeleteBehavior.Restrict);

        // CourseEditRequest Indexes
        modelBuilder.Entity<CourseEditRequest>()
            .HasIndex(r => new { r.CourseId, r.Status })
            .HasDatabaseName("IX_CourseEditRequests_CourseId_Status");

        modelBuilder.Entity<CourseEditRequest>()
            .HasIndex(r => r.RequestedAt)
            .HasDatabaseName("IX_CourseEditRequests_RequestedAt");

        modelBuilder.Entity<CourseEditRequest>()
            .HasIndex(r => r.IsEmergency)
            .HasFilter("is_emergency = 1")
            .HasDatabaseName("IX_CourseEditRequests_IsEmergency");

        modelBuilder.Entity<SystemSetting>()
            .HasOne(ss => ss.UpdatedByUser)
            .WithMany()
            .HasForeignKey(ss => ss.UpdatedBy)
            .OnDelete(DeleteBehavior.Restrict);

        // Unique Indexes
        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();

        modelBuilder.Entity<Session>()
            .HasIndex(s => s.RefreshTokenHash)
            .IsUnique();

        modelBuilder.Entity<Role>()
            .HasIndex(r => r.Name)
            .IsUnique();

        modelBuilder.Entity<Permission>()
            .HasIndex(p => p.Name)
            .IsUnique();

        modelBuilder.Entity<Category>()
            .HasIndex(c => c.Slug)
            .IsUnique();

        modelBuilder.Entity<Course>()
            .HasIndex(c => c.Slug)
            .IsUnique();

        modelBuilder.Entity<Certificate>()
            .HasIndex(c => c.VerificationCode)
            .IsUnique();

        modelBuilder.Entity<Coupon>()
            .HasIndex(c => c.Code)
            .IsUnique();

        modelBuilder.Entity<Order>()
            .HasIndex(o => o.OrderNumber)
            .IsUnique();

        modelBuilder.Entity<Payment>()
            .HasIndex(p => p.TransactionRef)
            .IsUnique();

        modelBuilder.Entity<SystemSetting>()
            .HasIndex(ss => ss.Key)
            .IsUnique();

        modelBuilder.Entity<Cart>()
            .HasIndex(c => c.UserId)
            .IsUnique();

        modelBuilder.Entity<VerificationToken>()
            .HasIndex(vt => vt.TokenHash)
            .IsUnique();

        modelBuilder.Entity<VerificationToken>()
            .HasIndex(vt => new { vt.UserId, vt.TokenType })
            .HasDatabaseName("IX_VerificationTokens_UserId_TokenType");

        modelBuilder.Entity<Enrollment>()
            .HasIndex(e => new { e.UserId, e.CourseId })
            .IsUnique();

        modelBuilder.Entity<CommentLike>()
            .HasIndex(cl => new { cl.CommentId, cl.UserId })
            .IsUnique();

        modelBuilder.Entity<ContentProgress>()
            .HasIndex(cp => new { cp.EnrollmentId, cp.ContentType, cp.ContentId })
            .IsUnique();
    }
}
