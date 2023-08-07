using Valhalla.Authorization;

namespace DemoAuth.Models;

public class DemoIdentityResolveProvider : IAuthorizationIdentityResolveProvider
{
	private readonly HttpContext m_HttpContext;

	public DemoIdentityResolveProvider(IHttpContextAccessor httpContextAccessor)
	{
		if (httpContextAccessor is null)
			throw new ArgumentNullException(nameof(httpContextAccessor));
		m_HttpContext = httpContextAccessor.HttpContext ?? throw new NullReferenceException(nameof(HttpContext));
	}

	public IAsyncEnumerable<IAuthorizationIdentity> GetIdentitiesAsync(CancellationToken cancellationToken = default)
		=> m_HttpContext.User?.Identity?.IsAuthenticated ?? false
			? new[] { new DemoIdentity(m_HttpContext.User!.Identity!.Name!) }.Cast<IAuthorizationIdentity>().ToAsyncEnumerable()
			: Array.Empty<IAuthorizationIdentity>().ToAsyncEnumerable();
}
