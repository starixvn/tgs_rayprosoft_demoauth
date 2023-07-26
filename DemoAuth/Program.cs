using DemoAuth.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;

var builder = WebApplication.CreateBuilder(args);

builder.Services
	.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
	.AddOpenIdConnect(oidcOptions =>
	{
		oidcOptions.AuthenticationMethod = OpenIdConnectRedirectBehavior.FormPost;
		oidcOptions.Authority = "https://dev-auth.rpgp.cc";
		oidcOptions.ClientId = "3190ea82-45c4-40da-b0ee-d9261028a868";
		oidcOptions.ClientSecret = "n5B.g1BHF9WphMNtvuPHF21HAh";
		oidcOptions.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
	})
	.AddCookie(cookieOptions =>
	{
		cookieOptions.ForwardChallenge = "Api";
		cookieOptions.Cookie.Name = "test.id";
	})
	.AddScheme<RemoteAuthenticationOptions, ApiAuthenticationHandler>(
		"Api",
		null,
		options => options.CallbackPath = "/test")
	.Services
	.AddAuthorizationBuilder()
	.AddDefaultPolicy("OpenId", policyBuilder => policyBuilder
		.RequireAuthenticatedUser());
var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();
app.UseAuthentication();
app.UseAuthorization();
//app.MapGet("/", () => "Hello World!");
app.MapGet("/api/account", () => "Hello World!")
	.RequireAuthorization();

app.Run();
