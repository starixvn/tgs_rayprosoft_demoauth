namespace DemoAuth.Models;

public class HeaderValueConstraint : IRouteConstraint
{
	private readonly string m_HeaderName;

	public HeaderValueConstraint(string headerName)
	{
		if (string.IsNullOrWhiteSpace(headerName))
			throw new ArgumentException($"'{nameof(headerName)}' 不得為 Null 或空白字元。", nameof(headerName));

		m_HeaderName = headerName;
	}

	public bool Match(HttpContext? httpContext, IRouter? route, string routeKey, RouteValueDictionary values, RouteDirection routeDirection)
	{
		if (httpContext is null)
			return false;

		if (httpContext.Request.Headers.TryGetValue(m_HeaderName, out var headerValue))
		{
			values[m_HeaderName] = headerValue;

			return true;
		}

		return false;
	}
}
