using Microsoft.AspNetCore.Authorization;

namespace DemoAuth.Models;

internal class TgsAuthorizationRequirement : AuthorizationHandler<TgsAuthorizationRequirement>, IAuthorizationRequirement
{
	protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, TgsAuthorizationRequirement requirement)
	{
		if (context.Resource is HttpContext httpContext)
		{
			var tgsFunction = httpContext.Features.Get<TgsFunction>();
			var tgsSystem = httpContext.RequestServices.GetRequiredService<TgsSystem>();

			if (tgsFunction is null)
			{
				context.Fail(new AuthorizationFailureReason(this, "Can't found tgs function data."));

				return Task.CompletedTask;
			}

			if (tgsSystem.HasPermission(context.User, tgsFunction))
				context.Succeed(requirement);
			else
				context.Fail(new AuthorizationFailureReason(this, "Access deny!"));
		}

		return Task.CompletedTask;
	}
}
