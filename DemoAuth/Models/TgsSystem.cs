using System.Security.Claims;

namespace DemoAuth.Models;

public class TgsSystem
{
	private readonly IReadOnlyCollection<TgsFunction> _functions;

	public TgsSystem(params TgsFunction[] functions)
	{
		_functions = functions.AsReadOnly();
	}

	public bool TryGetFunction(HttpContext context, out TgsFunction function)
	{
		function = default!;

		foreach (var func in _functions)
		{
			if (func.IsMatch(context))
			{
				function = func;
				return true;
			}
		}

		return false;
	}

	public bool HasPermission(ClaimsPrincipal user, TgsFunction function)
	{
		if (function.AllowAnonymous)
			return true;

		var userIsAnonymous = user?.Identity is null
			|| !user.Identities.Any(i => i.IsAuthenticated);

		return !userIsAnonymous && user?.Identity?.Name == "test";
	}
}
