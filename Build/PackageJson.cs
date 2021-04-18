sealed class PackageJson
{
    public PackageJson(string version) => Version = version;

    public string Version { get; }
}
