using System;
using System.Diagnostics;

using System.IO;
using System.Linq;
using System.Reflection;
using Xunit;
using Xunit.Abstractions;
using XUnit.Project.Attributes;
using TirsvadCLI.Linux;

namespace XUnit.Project;

[TestCaseOrderer("XUnit.Project.Orderers.PriorityOrderer", "XUnit.Project")]
public class TestPackageManager
{
  private readonly ITestOutputHelper output;
  public TestPackageManager(ITestOutputHelper output)
  {
    this.output = output;
  }


  [Fact, TestPriority(0)]
  public void Can_I_Find_PackageManager_In_OS()
  {
    string result = "";
    try
    {
      using (Process ps = new Process())
      {
        ps.StartInfo.UseShellExecute = false;
        ps.StartInfo.RedirectStandardOutput = true;
        ps.StartInfo.FileName = "which";
        ps.StartInfo.Arguments = PackageManager.packageManager;
        ps.Start();

        result = ps.StandardOutput.ReadToEnd();

        ps.WaitForExit();

      }
    }
    catch (AggregateException e)
    {
      Console.WriteLine("WARNING: {0}", e.Message);
    }
    Assert.NotEmpty(result);
  }

  [Fact, TestPriority(1)]
  public void PackageManager_Cmd_Update()
  {
    Assert.True(PackageManager.PmUpdate());
  }

  [Fact, TestPriority(1)]
  public void PackageManager_Cmd_Upgrade()
  {
    Assert.True(PackageManager.PmUpgrade());
  }

  [Fact, TestPriority(1)]
  public void PackageManager_Cmd_Install_Curl()
  {
    if (ApplicationExist("curl"))
    {
      PackageManager.PmUnInstall("curl");
    }
    PackageManager.PmInstall("curl");
    Assert.True(ApplicationExist("curl"), "Application curl should have exist");
  }

  [Fact, TestPriority(2)]
  public void PackageManager_Cmd_UnInstall_Curl()
  {
    if (!ApplicationExist("curl"))
    {
      PackageManager.PmInstall("curl");
    }
    PackageManager.PmUnInstall("curl");
    Assert.False(ApplicationExist("curl"), "Application curl shouldn't have exist");
  }

  public bool ApplicationExist(string app)
  {
    try
    {
      using (Process ps = new Process())
      {
        ps.StartInfo.UseShellExecute = false;
        ps.StartInfo.RedirectStandardOutput = true;
        ps.StartInfo.FileName = "which";
        ps.StartInfo.Arguments = app;
        ps.Start();

        string result = ps.StandardOutput.ReadToEnd();

        ps.WaitForExit();

        if (result == "")
        {
          return false;
        }
      }
    }
    catch (AggregateException e)
    {
      Console.WriteLine("WARNING: {0}", e.Message);
      return false;
    }
    return true;
  }
}
