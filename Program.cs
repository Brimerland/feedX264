using System;
using System.Diagnostics;

namespace feedX264
{
    class Program
    {
        static void Main(string[] args)
        {
            int frames = 10000;
            int resX = 1920;
            int resY = 1080;

            try
            {
                var sw = new Stopwatch();
                sw.Start();

                var process = new Process();
                process.StartInfo.Arguments = string.Format("--input-res {0}x{1} -o test.mkv -", resX, resY);
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.CreateNoWindow = true;
                
                process.StartInfo.RedirectStandardInput = true;
                process.StartInfo.FileName = "x264/x264-r2538-121396c.exe";

                if (process.Start())
                {
                    process.ErrorDataReceived += process_ErrorDataReceived;
                    process.OutputDataReceived += process_OutputDataReceived;
                    var inStream = process.StandardInput.BaseStream;

                    var yPlane = new byte[resX*resY];
                    var uPlane = new byte[resX * resY / 4];
                    var vPlane = new byte[resX * resY / 4];

                    int i;
                    i = yPlane.Length;
                    while (i-- > 0)
                        yPlane[i] = 0xeb;

                    i = uPlane.Length;
                    while (i-- > 0)
                        uPlane[i] = 0x80;

                    i = vPlane.Length;
                    while (i-- > 0)
                        vPlane[i] = 0x80;

                    Random rnd = new Random();
                    int frameCount = frames;
                    while (frameCount-- > 0)
                    {
                        inStream.Write(yPlane, 0, yPlane.Length);
                        inStream.Write(uPlane, 0, uPlane.Length);
                        inStream.Write(vPlane, 0, vPlane.Length);
                        if (frameCount%500 == 0)
                            Console.Out.WriteLine("Frames left: " + frameCount);
                    }

                    Console.Out.WriteLine("fps: " + frames / sw.Elapsed.TotalSeconds);
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
        }

        static void process_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            Console.Out.WriteLine(e.Data);
        }

        static void process_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            Console.Out.WriteLine(e.Data);
        }
    }
}
