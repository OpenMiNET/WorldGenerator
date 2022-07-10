using System;
using System.IO;
using System.Reflection;
using Microsoft.Xna.Framework;
using MiMap.Viewer.DesktopGL.Graphics;
using NLog;

namespace MiMap.Viewer.DesktopGL
{
    public static class Program
    {
        private static ILogger Log;
        [STAThread]
        static void Main(string[] args)
        {
            ConfigureNLog(Path.GetFullPath(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)));

            try
            {
                Log.Info($"Starting {Assembly.GetExecutingAssembly().GetName().Name}");

                using (var game = new MiMapViewer())
                {
                    game.Run(GameRunBehavior.Synchronous);
                }
            }
            catch (Exception ex)
            {
                // NLog: catch any exception and log it.
                Log.Error(ex, "Stopped program because of exception");
                throw;
            }
            finally
            {
                SpriteBatchExtensions.Dispose();
                // Ensure to flush and stop internal timers/threads before application-exit (Avoid segmentation fault on Linux)
                LogManager.Shutdown();
            }
        }

        private static void ConfigureNLog(string baseDir)
        {
            string loggerConfigFile = Path.Combine(baseDir, "NLog.config");

            string logsDir = Path.Combine(baseDir, "logs");
            if (!Directory.Exists(logsDir))
            {
                Directory.CreateDirectory(logsDir);
            }

            LogManager.ThrowConfigExceptions = false;
            LogManager.LoadConfiguration(loggerConfigFile);
//			LogManager.Configuration = new XmlLoggingConfiguration(loggerConfigFile);
            LogManager.Configuration.Variables["basedir"] = baseDir;

            Log = LogManager.GetCurrentClassLogger();

            AppDomain.CurrentDomain.FirstChanceException += (sender, args) => Log.Error(args.Exception, "FirstChanceException");
            AppDomain.CurrentDomain.UnhandledException += (sender, args) => Log.Error(args.ExceptionObject as Exception, "Unhandled exception");
        }
    }
}