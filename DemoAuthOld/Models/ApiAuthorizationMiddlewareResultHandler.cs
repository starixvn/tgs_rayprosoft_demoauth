using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;

namespace DemoAuth.Models;

public class ApiAuthorizationMiddlewareResultHandler : IAuthorizationMiddlewareResultHandler
{
	public Task HandleAsync(RequestDelegate next, HttpContext context, AuthorizationPolicy policy, PolicyAuthorizationResult authorizeResult)
	{
		return authorizeResult.Succeeded ? next(context) : Handle();

		async Task Handle()
		{
			if (authorizeResult.Challenged)
			{
				await context.ChallengeAsync("Api");
				await context.ChallengeAsync(OpenIdConnectDefaults.AuthenticationScheme);
			}
			else if (authorizeResult.Forbidden)
			{
				if (policy.AuthenticationSchemes.Count > 0)
				{
					foreach (var scheme in policy.AuthenticationSchemes)
					{
						await context.ForbidAsync(scheme);
					}
				}
				else
				{
					await context.ForbidAsync();
				}
			}
		}
	}
}
