namespace DemoAuth.Models;

public interface IHttpFeature
{
	bool IsMatch(HttpContext httpContext);
}