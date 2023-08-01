using System.Security.Claims;
using DemoAuth.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing.Constraints;
using Microsoft.AspNetCore.Routing.Patterns;
using Microsoft.AspNetCore.Routing.Template;
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
		oidcOptions.AuthenticationMethod = OpenIdConnectRedirectBehavior.FormPost;

		oidcOptions.Scope.Clear();
		oidcOptions.Scope.Add("openid");
		oidcOptions.Scope.Add("offline_access");

		oidcOptions.TokenValidationParameters.NameClaimType = ClaimTypes.NameIdentifier;

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
	.AddTransient<IAuthorizationMiddlewareResultHandler, ApiAuthorizationMiddlewareResultHandler>()
	.AddAuthorizationBuilder()
	.AddFallbackPolicy("Fallback", new AuthorizationPolicyBuilder()
		.AddRequirements(new TgsAuthorizationRequirement())
		.Build())
	.AddDefaultPolicy("OpenId", policyBuilder => policyBuilder
		.RequireAuthenticatedUser())
	.Services
	.Configure<ForwardedHeadersOptions>(options => options.ForwardedHeaders =
		ForwardedHeaders.All)
	.AddSingleton(sp => new TgsSystem(
		new TgsFunction(
			false,
			new PathFeature("api/info/test/{id:int}", sp))));

var app = builder.Build();

app.UseForwardedHeaders();
app.UseDefaultFiles();
app.UseStaticFiles();
app.UseAuthentication();
app.Use((HttpContext ctx, RequestDelegate next) =>
{
	var system = ctx.RequestServices.GetRequiredService<TgsSystem>();

	if (system.TryGetFunction(ctx, out var function))
		ctx.Features.Set(function);

	return next(ctx);
});
app.UseAuthorization();
//app.MapGet("/", () => "Hello World!");
app.MapGet("/api/account", () => "Hello World!")
	.RequireAuthorization();

app.MapGet("/api/info/{*pathinfo}", (HttpContext ctx, [FromServices] IServiceProvider sp, ILogger<Program> logger) =>
{
	var pattern = RoutePatternFactory.Parse("/api/info/test/{id:int}");
	var template = new RouteTemplate(pattern);
	var matcher = new TemplateMatcher(template, new RouteValueDictionary());

	var dict = new RouteValueDictionary();
	var constraintResolver = new DefaultInlineConstraintResolver(new OptionsWrapper<RouteOptions>(new RouteOptions()), sp);

	var constraints = template.Parameters
		.ToDictionary(
			p => p.Name!,
			p => new CompositeRouteConstraint(p.InlineConstraints
				.Select(inline => constraintResolver.ResolveConstraint(inline.Constraint))
				.Where(c => c is not null)
				.Select(c => c!)))!;
	constraints.Add("aaa", new CompositeRouteConstraint(new[] { new HeaderValueConstraint("aaa") }));

	bool MatchConstraints(RouteValueDictionary routeValues)
	{
		foreach (var kvp in constraints!)
		{
			var constraint = kvp.Value;

			if (!constraint.Match(
				ctx,
				null,
				kvp.Key,
				routeValues,
				RouteDirection.IncomingRequest))
				return false;
		}

		return true;
	}

	return matcher.TryMatch(ctx.Request.Path, dict)
		&& MatchConstraints(dict)
		? (new
		{
			Match = true,
			Dict = dict
		})
		: (new
		{
			Match = false,
			Dict = dict
		});
});

app.Run();
