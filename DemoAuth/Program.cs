using DemoAuth.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

builder.Services
	.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
	.AddOpenIdConnect(oidcOptions =>
	{
		oidcOptions.Authority = "https://dev-auth.rpgp.cc";
		oidcOptions.ClientId = "839720f7-1f8a-47eb-9e2f-3e19c4cd29b8";
		oidcOptions.ClientSecret = "ovO39z44UXk_1k0cDHRUlCo_Kx";
		oidcOptions.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
		oidcOptions.ResponseType = "code";


		oidcOptions.Scope.Clear();
		oidcOptions.Scope.Add("openid");
		oidcOptions.Scope.Add("offline_access");

		oidcOptions.Events.OnRedirectToIdentityProvider = ctx =>
		{
			if (!string.IsNullOrEmpty(ctx.ProtocolMessage.State))
				ctx.Properties.Items[OpenIdConnectDefaults.UserstatePropertiesKey] = ctx.ProtocolMessage.State;

			ctx.Properties.RedirectUri = "/";
			ctx.Properties.Items.Add(OpenIdConnectDefaults.RedirectUriForCodePropertiesKey, ctx.ProtocolMessage.RedirectUri);
			ctx.ProtocolMessage.State = ctx.Options.StateDataFormat.Protect(ctx.Properties);

			ctx.ProtocolMessage.SetParameter("brand_id", "3fa85f64-5717-4562-b3fc-2c963f66afa6");
			ctx.Response.StatusCode = 401;
			ctx.Response.Headers.Add("Location", ctx.ProtocolMessage.CreateAuthenticationRequestUrl());

			ctx.HandleResponse();

			return Task.CompletedTask;
		};
	})
	.AddCookie(cookieOptions =>
	{
		cookieOptions.ForwardChallenge = OpenIdConnectDefaults.AuthenticationScheme;
		cookieOptions.Cookie.Name = "test.id";
	})
	.Services
	.AddAuthorizationBuilder()
	.AddDefaultPolicy("OpenId", policyBuilder => policyBuilder
		.RequireAuthenticatedUser())
	.Services
	.Configure<ForwardedHeadersOptions>(options => options.ForwardedHeaders =
		ForwardedHeaders.All);

var app = builder.Build();

app.UseForwardedHeaders();
app.UseDefaultFiles();
app.UseStaticFiles();
app.UseAuthentication();
app.UseAuthorization();
//app.MapGet("/", () => "Hello World!");
app.MapGet("/api/account", () => "Hello World!")
	.RequireAuthorization();

app.Run();
