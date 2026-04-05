using Microsoft.Extensions.Configuration;

namespace Judgy.Samples.XunitAssertions;

internal static class SampleSettingsLoader
{
    private const string SettingsFileName = "appsettings.json";
    private const string RootSectionName = "Sample";

    internal static SampleSettings Load()
    {
        var settingsPath = Path.Combine(AppContext.BaseDirectory, SettingsFileName);
        var configuration = new ConfigurationBuilder()
            .AddJsonFile(settingsPath, optional: false)
            .Build();

        var settings = configuration.GetSection(RootSectionName).Get<SampleSettings>()
            ?? throw new InvalidOperationException($"Configuration section '{RootSectionName}' could not be loaded from '{SettingsFileName}'.");

        Validate(settings.JudgeOllama);

        return settings;
    }

    private static void Validate(OllamaJudgeSettings settings)
    {
        if (string.IsNullOrWhiteSpace(settings.BaseUrl))
            throw new InvalidOperationException("JudgeOllama.BaseUrl must be configured.");

        if (string.IsNullOrWhiteSpace(settings.Model))
            throw new InvalidOperationException("JudgeOllama.Model must be configured.");

        if (settings.TimeoutSeconds <= 0)
            throw new InvalidOperationException("JudgeOllama.TimeoutSeconds must be greater than zero.");
    }
}
