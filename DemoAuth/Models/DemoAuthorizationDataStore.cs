using Valhalla.Authorization;

namespace DemoAuth.Models;

public class DemoAuthorizationDataStore : AuthorizationDataStore<string>
{
	private readonly Lazy<IReadOnlyDictionary<string, IAuthorizationFunction>> m_FunctionFactories;

	public DemoAuthorizationDataStore(IEnumerable<IAuthorizationFunctionFactory<string>> functionFactories)
		: base(functionFactories)
	{
		m_FunctionFactories = new Lazy<IReadOnlyDictionary<string, IAuthorizationFunction>>(() =>
		{
			var functions = new[]
			{
				"test1",
				"test2",
				"test3",
				"aaa"
			};

			return functions
				.Select(f => TryCreateFunction("Demo", f))
				.Where(f => f is not null)
				.Select(f => f!)
				.ToDictionary(f => f.Name);
		});
	}

	public override IAsyncEnumerable<IAuthorizationFunction> GetFunctionsAsync(string system, CancellationToken cancellationToken = default)
		=> m_FunctionFactories.Value.Values.ToAsyncEnumerable();

	public override ValueTask<IAuthorizationFunction?> FindFunctionAsync(string system, string functionName, CancellationToken cancellationToken = default)
		=> m_FunctionFactories.Value.TryGetValue(functionName, out var function)
			? ValueTask.FromResult((IAuthorizationFunction?)function)
			: ValueTask.FromResult(default(IAuthorizationFunction));

	public override ValueTask<bool> CheckHasPermissionAsync(
		string system,
		IAuthorizationFunction function,
		IEnumerable<IAuthorizationIdentity> identities,
		CancellationToken cancellationToken = default)
	{
		var hasPermission = false;
		if (function.Name == "test1")
		{
			hasPermission = identities.OfType<DemoIdentity>()
				.Any(id => id.Name == "test");
		}

		return ValueTask.FromResult(hasPermission);
	}
}
