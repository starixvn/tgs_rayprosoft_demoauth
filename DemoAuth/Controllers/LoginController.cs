using System.Net;
using System.Net.Mime;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace DemoAuth.Controllers;

[Route("api/[controller]")]
[ApiController]
public class LoginController : ControllerBase
{
	[Consumes("application/x-www-form-urlencoded")]
	[HttpPost]
	[Produces(MediaTypeNames.Text.Html)]
	public async Task<IActionResult> LoginAsync(
		[FromServices] IMemoryCache cache,
		[FromQuery(Name = "loginChallenge")] string loginChallenge,
		[FromForm(Name = "userName")] string userName,
		[FromForm(Name = "password")] string password)
	{
		var cookieis = cache.Get<string>(loginChallenge);
		var container = new CookieContainer();
		container.Add(JsonSerializer.Deserialize<CookieCollection>(cookieis!)!);
		var httpClient = new HttpClient(new SocketsHttpHandler
		{
			CookieContainer = container
		});

		var content = await httpClient.PostAsJsonAsync(
			$"https://dev-session.rpgp.cc/login?login_challenge={loginChallenge}",
			new Dictionary<string, string>
			{
				["brandId"] = "3fa85f64-5717-4562-b3fc-2c963f66afa6",
				["userName"] = userName,
				["password"] = password
			})
			.ToAsyncEnumerable()
			.SelectMany(response => response.Content
				.ReadAsStringAsync()
				.ToAsyncEnumerable())
			.FirstAsync();

		return Content(content, MediaTypeNames.Text.Html);
	}
}
