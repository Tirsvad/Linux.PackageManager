using System;
using TirsvadCLI.Linux;

namespace Example;

class Program
{
  static void Main(string[] args)
  {
    Console.WriteLine("PackageManager Example of use");
    Console.WriteLine("The default PM is " + PackageManager.packageManager);
  }
}
