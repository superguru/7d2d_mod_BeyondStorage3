using System.Text;
using BeyondStorage.Harmony.Commands;
using BeyondStorage.Infrastructure;

namespace BeyondStorage.Configuration;

/// <summary>
/// Helper class for displaying configuration information to the console.
/// Provides reusable methods for config display across multiple commands.
/// </summary>
public static class ConfigDisplayHelper
{
    /// <summary>
    /// Displays the current active config settings using ConfigSnapshot.
    /// </summary>
    public static void ShowConfig()
    {
        var config = ModConfig.ClientConfig;
        var props = BsConfigPropertyRegistry.RegisteredProperties;

        var output = new StringBuilder();
        output.AppendLine("Current config snapshot:");
        output.AppendLine("{");
        foreach (var prop in props)
        {
            output.AppendLine($"  '{prop.PropertyName}': {prop.Type} = {prop.GetValue(config)}; // {prop.Description}");
        }
        output.AppendLine("}");
        output.AppendLine("Do not copy and paste this into the config.json file. The values above are formatted for reading in the console.");

        ModLogger.Info(output.ToString());
    }
}