namespace Web.Features.Auth;

public class AuthProviders
{
    private readonly HashSet<string> _providers = [];

    public IReadOnlySet<string> Providers => _providers;

    public void Register(string provider)
    {
        _providers.Add(provider);
    }
}
