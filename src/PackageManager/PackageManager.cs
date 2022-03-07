using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

using Serilog;

using YamlDotNet.Serialization;

namespace TirsvadCLI.Linux;
public class PackageManager
{
    private static readonly ILogger _log = Log.Logger;
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
        _log.Information($"Logger in PackageManager");
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
                            if (index.cmd == "preSettings")
                            {
                                _packageManagerCommand[index.cmd] = new Dictionary<string, string>();
                                _packageManagerCommand[index.cmd]["app"] = index.app;
                                _packageManagerCommand[index.cmd]["arg"] = index.arg;
                            }
                            else
                            {
                                _packageManagerCommand[index.cmd] = new Dictionary<string, string>();
                                _packageManagerCommand[index.cmd]["app"] = index.app;
                                _packageManagerCommand[index.cmd]["arg"] = ver.Value.silentMode + " " + index.arg;
                            }
                        }
                    }
                }
            }
        }

        if (_packageManagerCommand.ContainsKey("preSettings"))
        {
            using (Process ps = new Process())
            {
                ps.StartInfo.UseShellExecute = false;
                ps.StartInfo.RedirectStandardOutput = true;
                ps.StartInfo.FileName = _packageManagerCommand["preSettings"]["app"];
                ps.StartInfo.Arguments = _packageManagerCommand["preSettings"]["arg"]; ;
                ps.Start();

                string result = ps.StandardOutput.ReadToEnd();

                ps.WaitForExit();
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

        tasks.Add(_LoadConfigFileYaml());

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
            _log.Warning("WARNING: {0}", e.Message);
            return Task.FromResult(false);
        }
        if (result != "")
        {
            listOfAvaiblePackageManager.Add(manager);
            _log.Warning($"add to listOfAvaiblePackageManager {manager}");
        }
        return Task.FromResult(true);
    }

    private static async Task _LoadConfigFileYaml(string configurationFile = "")
    {
        _log.Debug("Embedded Ressources _embeddedResFileSettingsYaml " + _embeddedResFileSettingsYaml);
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
            _log.Information("Updating system");
            using (Process ps = new Process())
            {
                ps.StartInfo.UseShellExecute = false;
                ps.StartInfo.RedirectStandardOutput = true;
                ps.StartInfo.FileName = _packageManagerCommand["update"]["app"];
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
            _log.Information("Upgrading system");
            using (Process ps = new Process())
            {
                ps.StartInfo.UseShellExecute = false;
                ps.StartInfo.RedirectStandardOutput = true;
                ps.StartInfo.FileName = _packageManagerCommand["upgrade"]["app"];
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
                ps.StartInfo.FileName = _packageManagerCommand["install"]["app"];
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
                ps.StartInfo.FileName = _packageManagerCommand["uninstall"]["app"];
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