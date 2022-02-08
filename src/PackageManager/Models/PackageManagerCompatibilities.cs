using System;
using System.Collections.Generic;

using YamlDotNet.Serialization;

namespace TirsvadCLI.Linux;

public class PackageManagerCompatibilities
{
  [YamlMember(Alias = "os")]
  public List<Distribution> Distributions { get; set; }

  public class Distribution
  {
    [YamlMember(Alias = "distribution")]
    public string Name { get; set; }

    [YamlMember(Alias = "versions")]
    public Dictionary<float, PackageManagerSettings> Versions { get; set; }
  }

  public class PackageManagerSettings
  {
    [YamlMember(Alias = "silentMode")]
    public string silentMode { get; set; }
    [YamlMember(Alias = "defaultPackageManager")]
    public string defaultPackageManager { get; set; }

    [YamlMember(Alias = "packageManagerCommands")]
    public List<PackageManagerCmd> packageManagerCommands { get; set; }
  }

  public class PackageManagerCmd
  {
    [YamlMember(Alias = "cmd")]
    public string cmd { get; set; }
    [YamlMember(Alias = "pm")]
    public string pm { get; set; }
    [YamlMember(Alias = "arg")]
    public string arg { get; set; }
  }

}
