using ThuInfoWeb.DBModels;

namespace ThuInfoWeb;

public static class DeepSeekModelNames
{
    public const string DefaultModel = "DeepSeek-V4.1-Flash";

    public static string[] Parse(string? names) => names?
        .Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
        .Distinct(StringComparer.Ordinal)
        .ToArray() ?? [];

    public static string Normalize(string? names) => string.Join(',', Parse(names));

    // Auto-sync adds a nullable column to existing rows. Only null is a migration marker:
    // an administrator's deliberately empty list must survive subsequent starts.
    public static void Initialize(IFreeSql database) => database.Update<Misc>()
        .Set(misc => misc.DeepSeekModels, DefaultModel)
        .Where(misc => misc.DeepSeekModels == null)
        .ExecuteAffrows();
}
