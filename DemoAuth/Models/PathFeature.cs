using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Routing.Constraints;
using Microsoft.AspNetCore.Routing.Patterns;
using Microsoft.AspNetCore.Routing.Template;
using Microsoft.Extensions.Options;

namespace DemoAuth.Models;

public class PathFeature : IHttpFeature
{
	private readonly TemplateMatcher _matcher;
	private readonly Dictionary<string, CompositeRouteConstraint> _constraints;

	public PathFeature([StringSyntax("Route")] string pathPattern, IServiceProvider serviceProvider)
	{
		_matcher = new TemplateMatcher(
			new RouteTemplate(RoutePatternFactory.Parse(pathPattern)),
			new RouteValueDictionary());

		var constraintResolver = new DefaultInlineConstraintResolver(
			new OptionsWrapper<RouteOptions>(new RouteOptions()),
			serviceProvider);

		_constraints = _matcher.Template.Parameters.ToDictionary(
			p => p.Name!,
			p => new CompositeRouteConstraint(p.InlineConstraints
				.Select(inline => constraintResolver.ResolveConstraint(inline.Constraint))
				.Where(c => c is not null)
				.Select(c => c!)))!;
	}

	public bool IsMatch(HttpContext httpContext)
	{
		var dict = new RouteValueDictionary();

		return _matcher.TryMatch(httpContext.Request.Path, dict)
			&& MatchConstraints(httpContext, dict);
	}

	private bool MatchConstraints(HttpContext ctx, RouteValueDictionary routeValues)
	{
		foreach (var kvp in _constraints)
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
}
