using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.Deployment.Compression;
using Microsoft.Deployment.Compression.Cab;

namespace PopCapInstallGenerator
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("==== PopCap Installer Generator ====");

            if (args.Length < 1)
            {
                Console.WriteLine("Usage: PopCapInstallGenerator <inputDir>");
                return;
            }
            
            Console.WriteLine("Making Installer...");
            MakeInstaller(Path.GetFullPath(args[0]).Replace("\\", "/") + "/");
        }

        private static void MakeInstaller(string inputDir)
        {
            using var s = File.Open("OutputSetup.exe", FileMode.Create);
            using var bw = new BinaryWriter(s);
            
            Console.WriteLine("Writing Installer Base...");
            bw.Write(GetInstallerBase());
            
            Console.WriteLine("Packing Cab File from: '" + inputDir + "'");
            byte[] cab = MakeCab(inputDir);
            
            int cabOffset = (int)s.Position;
            Console.WriteLine("Writing Cab File At: '0x" + cabOffset.ToString("X8") + "'");
            bw.Write(cab);
            
            Console.WriteLine("Writing Signature...");
            WriteSignature(bw, cabOffset);
            bw.Flush();
            
            Console.WriteLine("Done!");
        }

        private static byte[] GetInstallerBase()
        {
            return InstallerBase.File;
        }

        private static byte[] MakeCab(string dir)
        {
            string temp = Path.GetTempFileName();
            
            CabInfo cab = new CabInfo(temp);
            cab.Pack(dir, true, CompressionLevel.Max, (sender, args) =>
            {
                int percent = args.CurrentFileNumber * 100 / args.TotalFiles;
                long percent2 = args.CurrentFileBytesProcessed * 100 / args.TotalFileBytes;
                Console.Write("\rPacking: " + percent + "% - " + percent2 + "%");
            });
            
            Console.WriteLine("\rPacking: 100%");

            return File.ReadAllBytes(temp);
        }

        private static void WriteSignature(BinaryWriter bw, int target)
        {
            bw.Write(new byte[0x4C4F]);
            int offset = (int)bw.BaseStream.Position;
            bw.Write(offset - target);
            bw.Write((int)1);
            bw.Write(Encoding.ASCII.GetBytes("!popcapinstallersig!"));
            bw.Write(new byte[0x1555]);
            bw.Flush();
        }
    }
}