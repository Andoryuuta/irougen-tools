namespace IrougenTools.Core;

public class SvnVersionInfo
{
    public required long Version { get; set; }
    public required string Branch { get; set; }
    public required DateTimeOffset Timestamp { get; set; }

    public static SvnVersionInfo Parse(string versionString)
    {
        if (string.IsNullOrEmpty(versionString))
            throw new ArgumentNullException(nameof(versionString));

        var parts = versionString.Split('|');
        if (parts.Length != 3)
            throw new FormatException("Invalid SVN version string format. Expected format: 'version|branch|timestamp'");

        if (!long.TryParse(parts[0], out var version))
            throw new FormatException("Invalid version number format");

        if (!DateTimeOffset.TryParse(parts[2], out var timestamp))
            throw new FormatException("Invalid timestamp format");

        return new SvnVersionInfo
        {
            Version = version,
            Branch = parts[1],
            Timestamp = timestamp
        };
    }

    public override string ToString()
    {
        return $"{Version}|{Branch}|{Timestamp:yyyy-MM-ddTHH:mm:ss.ffffffZ}";
    }
}