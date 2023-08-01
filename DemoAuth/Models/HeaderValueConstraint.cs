namespace DemoAuth.Models;

public class HeaderValueConstraint : IRouteConstraint
{
	private readonly string _headerName;

	public HeaderValueConstraint(string headerName)
	{
		if (string.IsNullOrWhiteSpace(headerName))
			throw new ArgumentException($"'{nameof(headerName)}' 不得為 Null 或空白字元。", nameof(headerName));

		_headerName = headerName;
	}

	public bool Match(HttpContext? httpContext, IRouter? route, string routeKey, RouteValueDictionary values, RouteDirection routeDirection)
	{
		if (httpContext is null)
			return false;

		if (httpContext.Request.Headers.TryGetValue(_headerName, out var headerValue))
		{
			values[_headerName] = headerValue;

			return true;
		}

		return false;
	}
}
