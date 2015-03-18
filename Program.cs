using System;
using System.Diagnostics;
using System.IO.Pipes;
using System.Threading.Tasks;

namespace feedX264
{
    class Program
    {
        static void Main(string[] args)
        {
            int frames = 10000;
            int resX = 1280;
            int resY = 720;
            int fps = 60;

            try
            {
                var sw = new Stopwatch();
                sw.Start();

                Task.Run(() => { OpenNamedPipe(); });

                var process = new Process();
                process.StartInfo.Arguments = string.Format("--input-res {0}x{1} --fps 60000/1001 --preset ultrafast -o \\\\.\\pipe\\test.mkv -", resX, resY);
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.CreateNoWindow = true;
                
                process.StartInfo.RedirectStandardInput = true;
                process.StartInfo.FileName = "x264/x264-r2538-121396c.exe";

                if (process.Start())
                {
                    process.ErrorDataReceived += process_ErrorDataReceived;
                    process.OutputDataReceived += process_OutputDataReceived;
                    var inStream = process.StandardInput.BaseStream;

                    var yPlane = new byte[resX * resY];
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
                        rnd.NextBytes(yPlane);
                        inStream.Write(yPlane, 0, yPlane.Length);
                        inStream.Write(uPlane, 0, uPlane.Length);
                        inStream.Write(vPlane, 0, vPlane.Length);
                        if (frameCount % fps == 0)
                        {
                            Console.Out.WriteLine("Frames left: " + frameCount + "  fps: " + fps / sw.Elapsed.TotalSeconds);
                            sw.Restart();
                        }
                    }
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

            Console.In.ReadLine();
        }

        private static async void OpenNamedPipe()
        {
            try
            {
                using (var outStream = System.IO.File.Open("outpipe.mkv", System.IO.FileMode.Create, System.IO.FileAccess.Write, System.IO.FileShare.Read))
                using (NamedPipeServerStream pipeServer =
                new NamedPipeServerStream("test.mkv", PipeDirection.In))
                {
                    Console.WriteLine("NamedPipeServerStream object created.");

                    // Wait for a client to connect
                    Console.Write("Waiting for client connection...");
                    pipeServer.WaitForConnection();

                    Console.WriteLine("Client connected.");
                    try
                    {
                        await pipeServer.CopyToAsync(outStream);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Exception on pipe read: " + e);
                    }

                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception on pipe task: " + e);
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
