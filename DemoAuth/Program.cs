using System.Net;
using System.Security.Claims;
using System.Text.Json;
using DemoAuth.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing.Constraints;
using Microsoft.AspNetCore.Routing.Patterns;
using Microsoft.AspNetCore.Routing.Template;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);
{
	_ = builder.Services
		.AddMemoryCache()
		.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
		.AddOpenIdConnect(oidcOptions =>
		{
			oidcOptions.Authority = "https://dev-auth.rpgp.cc";
			oidcOptions.ClientId = "a7718b62-4d03-461f-849f-a4d12698d533";
			oidcOptions.ClientSecret = "2UeLZ0A4YNzEs.WDp.hx48h_Xd";
			oidcOptions.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
			oidcOptions.ResponseType = "code";
			oidcOptions.AuthenticationMethod = OpenIdConnectRedirectBehavior.FormPost;

			oidcOptions.Scope.Clear();
			oidcOptions.Scope.Add("openid");
			oidcOptions.Scope.Add("offline_access");

			oidcOptions.TokenValidationParameters.NameClaimType = ClaimTypes.NameIdentifier;

			oidcOptions.Events.OnRedirectToIdentityProvider = async ctx =>
			{
				if (!string.IsNullOrEmpty(ctx.ProtocolMessage.State))
					ctx.Properties.Items[OpenIdConnectDefaults.UserstatePropertiesKey] = ctx.ProtocolMessage.State;

				ctx.Properties.RedirectUri = "/";
				ctx.Properties.Items.Add(OpenIdConnectDefaults.RedirectUriForCodePropertiesKey, ctx.ProtocolMessage.RedirectUri);
				ctx.ProtocolMessage.State = ctx.Options.StateDataFormat.Protect(ctx.Properties);

				//ctx.Response.StatusCode = 401;
				//ctx.Response.Headers.Add("Location", ctx.ProtocolMessage.CreateAuthenticationRequestUrl());

				var cookieContainer = new CookieContainer();



				var httpClient = new HttpClient(new SocketsHttpHandler
				{
					CookieContainer = cookieContainer
				});

				var loginChallenge = await httpClient.GetAsync(ctx.ProtocolMessage.CreateAuthenticationRequestUrl())
					.ToAsyncEnumerable()
					.Do(response => response.EnsureSuccessStatusCode())
					.SelectMany(response => response.Content.ReadAsStringAsync()
						.ToAsyncEnumerable())
					.FirstAsync();

				ctx.Response.StatusCode = 401;
				ctx.Response.Headers.Add("Login_Challenge", loginChallenge);

				var cache = ctx.HttpContext.RequestServices.GetRequiredService<IMemoryCache>();

				cache.Set(loginChallenge, JsonSerializer.Serialize(cookieContainer.GetAllCookies()));

				ctx.HandleResponse();
			};
			oidcOptions.Events.OnRemoteFailure = ctx =>
			{
				return Task.CompletedTask;
			};
		})
		.AddCookie(cookieOptions =>
		{
			cookieOptions.ForwardChallenge = OpenIdConnectDefaults.AuthenticationScheme;
			cookieOptions.Cookie.Name = "test.id";
		})
		.Services
		.AddWebApiAuthorizationMiddlewareResultHandler()
		.AddHttpContextAccessor()
		.AddPhoenixAuthorizationKits("Demo")
		.RegisterIdentityResolveProvider<DemoIdentityResolveProvider>()
		.RegisterFunctionFactory<DemoHttpFactory, string>()
		.RegisterAuthorizationDataStore<DemoAuthorizationDataStore>()
		.Services
		.AddAuthorizationBuilder()
		.AddHttpFallbackPolicy()
		.Services
		.Configure<ForwardedHeadersOptions>(options => options.ForwardedHeaders =
			ForwardedHeaders.All)
		.AddControllers();
}

var app = builder.Build();
{
	app.UseForwardedHeaders();
	app.UseDefaultFiles();
	app.UseStaticFiles();
	app.UseAuthentication();
	app.UseCaptureFunction();
	app.UseAuthorization();
	app.MapControllers();
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
}

app.Run();
