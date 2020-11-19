// <copyright file="Program.cs" company="Kjell Skogsrud">
// Copyright (c) Kjell Skogsrud. BSD 3-Clause License
// </copyright>

using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Hosting.WindowsServices;

namespace BlazorChores
{
    /// <summary>
    /// The program.
    /// </summary>
    public class Program
    {
        /// <summary>
        /// The main method.
        /// </summary>
        /// <param name="args">Commandline args.</param>
        public static void Main(string[] args)
        {
            if (args != null)
            { 
                for (int i = 0; i < args.Length; i++)
                {
                    System.Console.WriteLine(args[i]);
                }
            }

            System.IO.Directory.SetCurrentDirectory(@"E:\Repos\ChoreWorkerServer\BlazorChores\bin\Debug\netcoreapp3.1");
            CreateHostBuilder(args).Build().Run();
        }

        /// <summary>
        /// Creator of great hosts since 2020.
        /// </summary>
        /// <param name="args">Arguments.</param>
        /// <returns>Something that implments IHostBuilder.</returns>
        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseContentRoot(@"E:\Repos\ChoreWorkerServer\BlazorChores\bin\Debug\netcoreapp3.1")
                .UseWindowsService()
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
