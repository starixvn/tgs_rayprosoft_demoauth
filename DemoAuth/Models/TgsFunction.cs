namespace DemoAuth.Models;

public class TgsFunction
{
	private readonly IReadOnlyCollection<IHttpFeature> _features;

	public bool AllowAnonymous { get; }

	public TgsFunction(bool allowAnonymous, params IHttpFeature[] features)
	{
		_features = features.AsReadOnly();
		AllowAnonymous = allowAnonymous;
	}

	public bool IsMatch(HttpContext httpContext)
	{
		foreach (var feature in _features)
			if (feature.IsMatch(httpContext))
				return true;

		return false;
	}
}
