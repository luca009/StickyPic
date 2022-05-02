using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace StickyPic.Classes
{
    public enum LogSeverity
    {
        Message, Warning, Error
    }

    public class Logger
    {
        public bool Enabled { get; set; }
        public string LogPath { get; set; }
        StreamWriter writer;

        public Logger(bool enabled, string path = "auto")
        {
            Enabled = enabled;

            if (path == "auto")
            {
                DirectoryInfo directory = new DirectoryInfo(System.Reflection.Assembly.GetEntryAssembly().Location);
                LogPath = $"{directory.Parent.FullName}\\StickyPic-{DateTime.Now.ToString("MM-dd-HH-mm-ss")}.log";
            }
            else
            {
                LogPath = path;
            }

            writer = new StreamWriter(LogPath);
        }

        public void FlushlessLog(string message, LogSeverity severity)
        {
            writer.Write($"[{DateTime.Now.ToString("HH:mm:ss.FFFFF")} - {severity}] {message}\n");
        }

        public void Log(string message, LogSeverity severity)
        {
            if (!Enabled)
                return;

            writer.Write($"[{DateTime.Now.ToString("HH:mm:ss.FFFFF")} - {severity}] {message}\n");
            writer.Flush();
        }
    }
}
