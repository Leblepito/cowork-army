using CoworkArmy.Application.Interfaces;

namespace CoworkArmy.Infrastructure.LLM;

public class LlmProviderFactory : ILlmProviderFactory
{
    private readonly Dictionary<string, ILlmProvider> _providers;
    private readonly string _defaultProvider;

    public LlmProviderFactory(IEnumerable<ILlmProvider> providers, IConfiguration config)
    {
        _providers = providers.ToDictionary(p => p.Name, StringComparer.OrdinalIgnoreCase);
        _defaultProvider = config["LLM_PROVIDER"]
            ?? Environment.GetEnvironmentVariable("LLM_PROVIDER")
            ?? "gemini"; // Default to gemini (free tier available)
    }

    public ILlmProvider GetByName(string name)
    {
        if (_providers.TryGetValue(name, out var provider)) return provider;
        // Fallback to configured default
        if (_providers.TryGetValue(_defaultProvider, out var fallback)) return fallback;
        return _providers.Values.First();
    }

    public ILlmProvider GetByTier(string tier)
    {
        // Use LLM_PROVIDER env var as the primary provider for all tiers
        // This ensures we use the provider that has active API credits
        return GetByName(_defaultProvider);
    }
}
