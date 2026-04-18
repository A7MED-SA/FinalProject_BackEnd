using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using backend_project.Models;

namespace backend_project.Authorization;

public static class TeacherRequestPolicies
{
    public const string CanSubmitTeacherRequest = "CanSubmitTeacherRequest";
    public const string CanViewOwnTeacherRequests = "CanViewOwnTeacherRequests";
    public const string CanProcessTeacherRequests = "CanProcessTeacherRequests";

    public static AuthorizationPolicy CanSubmitTeacherRequestPolicy()
    {
        return new AuthorizationPolicyBuilder()
            .RequireAuthenticatedUser()
            .RequireAssertion(context =>
            {
                // التحقق إذا كان المستخدم ليس معلم بالفعل
                var isTeacher = context.User.IsInRole("Teacher");
                return !isTeacher;
            })
            .Build();
    }

    public static AuthorizationPolicy CanViewOwnTeacherRequestsPolicy()
    {
        return new AuthorizationPolicyBuilder()
            .RequireAuthenticatedUser()
            .Build();
    }

    public static AuthorizationPolicy CanProcessTeacherRequestsPolicy()
    {
        return new AuthorizationPolicyBuilder()
            .RequireRole("Admin")
            .Build();
    }
}