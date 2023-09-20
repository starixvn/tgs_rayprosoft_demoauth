using Valhalla.Authorization;
using Valhalla.Authorization.AspNetCore;

namespace DemoAuth.Models;

public class DemoHttpFactory : IAuthorizationFunctionFactory<string>
{
	private readonly IServiceProvider m_ServiceProvider;

	public string Name => "Demo";

	public DemoHttpFactory(IServiceProvider serviceProvider)
	{
		m_ServiceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
	}

	public IAuthorizationFunction? CreateFunction(string entity)
		=> entity switch
		{
			"test1" => new HttpFunction(Guid.NewGuid(), entity, false, new HttpPathFeature("api/info/test/{id:int}", m_ServiceProvider)),
			"test2" => new HttpFunction(Guid.NewGuid(), entity, false),
			"test3" => new HttpFunction(Guid.NewGuid(), entity, false),
			"aaa" => new HttpFunction(Guid.NewGuid(), entity, true, new HttpPathFeature("api/login", m_ServiceProvider)),
			_ => null
		};
}
