using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

using YamlDotNet.Serialization;

namespace TirsvadCLI.Linux;
public class PackageManager
{
  private static string _scriptPath { get; }
  private static string _embeddedResFileSettingsYaml { get; }
  private static Assembly _assembly { get; }

  private static string _settingsFileDirectory { get; } = "conf";
  private static string _settingsFile { get; } = "settings.yaml";
  private static List<string> _listOfPackageManager { get; } = new List<string>() { "apt", "dnf" };
  private static Dictionary<string, Dictionary<string, string>> _packageManagerCommand = new Dictionary<string, Dictionary<string, string>>();
  private static PackageManagerCompatibilities _settings = null;

  public static string settingsFile { get; } = "";
  public static string packageManager { get; } = "";
  public static List<string> listOfAvaiblePackageManager { get; } = new List<string>();


  static PackageManager()
  {
    _scriptPath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
    _assembly = Assembly.GetCallingAssembly();
    string[] embeddedResources = _assembly.GetManifestResourceNames();
    _embeddedResFileSettingsYaml = "PackageManager.embedded." + _settingsFile;

    _PackageManagerAsync().Wait();

    foreach (PackageManagerCompatibilities.Distribution distro in _settings.Distributions)
    {
      if (distro.Name == Distribution.DistributionName)
      {
        foreach (KeyValuePair<float, PackageManagerCompatibilities.PackageManagerSettings> ver in distro.Versions)
        {
          if (ver.Key.ToString() == Distribution.DistributionVersion)
          {
            packageManager = ver.Value.defaultPackageManager;
            List<string> cmd = new List<string>();
            foreach (PackageManagerCompatibilities.PackageManagerCmd index in ver.Value.packageManagerCommands)
            {
              _packageManagerCommand[index.cmd] = new Dictionary<string, string>();
              _packageManagerCommand[index.cmd]["pm"] = index.pm;
              _packageManagerCommand[index.cmd]["arg"] = ver.Value.silentMode + " " + index.arg;
            }
          }
        }
      }
    }
  }

  private static async Task _PackageManagerAsync()
  {
    List<Task> tasks = new List<Task>();
    foreach (var manager in _listOfPackageManager)
    {
      tasks.Add(_CheckAvaiblePackageManager(manager.ToString()));
    }

    tasks.Add(_LoadConfigFileYaml(_scriptPath + "/" + _settingsFileDirectory + "/" + settingsFile));

    await Task.WhenAll(tasks);

  }

  private static Task<bool> _CheckAvaiblePackageManager(string manager)
  {
    string result = "";
    try
    {
      Process ps;
      ps = new Process();
      ps.StartInfo.UseShellExecute = false;
      ps.StartInfo.RedirectStandardOutput = true;
      ps.StartInfo.FileName = "which";
      ps.StartInfo.Arguments = manager;
      ps.Start();

      result = ps.StandardOutput.ReadToEnd();

      ps.WaitForExit();

    }
    catch (AggregateException e)
    {
      Console.WriteLine("WARNING: {0}", e.Message);
      return Task.FromResult(false);
    }
    if (result != "")
    {
      listOfAvaiblePackageManager.Add(manager);
    }
    return Task.FromResult(true);
  }

  private static async Task _LoadConfigFileYaml(string configurationFile = "")
  {
    Console.WriteLine("_embeddedResFileSettingsYaml " + _embeddedResFileSettingsYaml);
    using (Stream s = _assembly.GetManifestResourceStream(_embeddedResFileSettingsYaml))
    {
      using (StreamReader input = new StreamReader(s))
      {
        DeserializerBuilder deserializerBuilder = new DeserializerBuilder();
        IDeserializer deserializer = deserializerBuilder.Build();
        _settings = deserializer.Deserialize<PackageManagerCompatibilities>(input);
      }
    }
    await Task.CompletedTask;
  }

  public static bool PmUpdate()
  {
    try
    {
      using (Process ps = new Process())
      {
        ps.StartInfo.UseShellExecute = false;
        ps.StartInfo.RedirectStandardOutput = true;
        ps.StartInfo.FileName = _packageManagerCommand["update"]["pm"];
        ps.StartInfo.Arguments = _packageManagerCommand["update"]["arg"]; ;
        ps.Start();

        string result = ps.StandardOutput.ReadToEnd();

        ps.WaitForExit();

      }
    }
    catch (AggregateException e)
    {
      Console.WriteLine("WARNING: " + e.Message);
      return false;
    }
    return true;
  }

  public static bool PmUpgrade()
  {
    try
    {
      using (Process ps = new Process())
      {
        ps.StartInfo.UseShellExecute = false;
        ps.StartInfo.RedirectStandardOutput = true;
        ps.StartInfo.FileName = _packageManagerCommand["upgrade"]["pm"];
        ps.StartInfo.Arguments = _packageManagerCommand["upgrade"]["arg"];
        ps.Start();

        string result = ps.StandardOutput.ReadToEnd();

        ps.WaitForExit();

      }
    }
    catch (AggregateException e)
    {
      Console.WriteLine("WARNING: " + e.Message);
      return false;
    }
    return true;
  }
  public static bool PmInstall(string package)
  {
    try
    {
      using (Process ps = new Process())
      {
        ps.StartInfo.UseShellExecute = false;
        ps.StartInfo.RedirectStandardOutput = true;
        ps.StartInfo.FileName = _packageManagerCommand["install"]["pm"];
        ps.StartInfo.Arguments = _packageManagerCommand["install"]["arg"] + " " + package;
        ps.Start();

        string result = ps.StandardOutput.ReadToEnd();

        ps.WaitForExit();

      }
    }
    catch (AggregateException e)
    {
      Console.WriteLine("WARNING: " + e.Message);
      return false;
    }
    return true;
  }
  public static bool PmUnInstall(string package)
  {
    try
    {
      using (Process ps = new Process())
      {
        ps.StartInfo.UseShellExecute = false;
        ps.StartInfo.RedirectStandardOutput = true;
        ps.StartInfo.FileName = _packageManagerCommand["uninstall"]["pm"];
        ps.StartInfo.Arguments = _packageManagerCommand["uninstall"]["arg"] + " " + package;
        ps.Start();

        string result = ps.StandardOutput.ReadToEnd();

        ps.WaitForExit();

      }
    }
    catch (AggregateException e)
    {
      Console.WriteLine("WARNING: " + e.Message);
      return false;
    }
    return true;
  }


}
