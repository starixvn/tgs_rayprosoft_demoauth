using Valhalla.Authorization;

namespace DemoAuth.Models;

public record struct DemoIdentity(string Name) : IAuthorizationIdentity
{
}
