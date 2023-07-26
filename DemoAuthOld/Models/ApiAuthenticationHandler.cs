using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Extensions.Options;

namespace DemoAuth.Models;

public class ApiAuthenticationHandler : RemoteAuthenticationHandler<OpenIdConnectOptions>
{
	public ApiAuthenticationHandler(
		IOptionsMonitor<OpenIdConnectOptions> options,
		ILoggerFactory logger,
		UrlEncoder encoder,
		ISystemClock clock)
		: base(options, logger, encoder, clock)
	{
	}

	protected override Task<HandleRequestResult> HandleRemoteAuthenticateAsync()
		=> Task.FromResult(HandleRequestResult.NoResult());

	protected override Task HandleChallengeAsync(AuthenticationProperties properties)
	{
		Context.Response.StatusCode = 401;

		return Task.CompletedTask;
	}
}
