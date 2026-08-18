using Hangfire.Dashboard;

namespace AutoNest.Api.Infrastructure;

public sealed class AdminDashboardFilter : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context)
        => context.GetHttpContext().User.Identity?.IsAuthenticated == true
            && context.GetHttpContext().User.IsInRole("Admin");
}
