using Microsoft.AspNetCore.Authorization;

namespace Athary.API.Authorization;

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
                var isInstructor = context.User.IsInRole("Instructor");
                return !isInstructor;
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
