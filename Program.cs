using System;
using System.Diagnostics;

namespace feedX264
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                var process = new Process();
                process.StartInfo.Arguments = "-h";
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.CreateNoWindow = true;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.FileName = "x264/x264-r2538-121396c.exe";

                if (process.Start())
                {
                    string s;
                    while ((s = process.StandardOutput.ReadLine())!=null)
                        Console.Out.WriteLine(s);
                }
                else
                {
                    Console.Out.WriteLine("unable to start process");
                }
            }
            catch (Exception e)
            {
                Console.Out.WriteLine("Exception: " + e);
            }

            Console.Out.WriteLine("Press any key");
            Console.ReadKey();
        }
    }
}
