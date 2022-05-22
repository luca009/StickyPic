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
        StreamWriter writer = null;
        string buffer = "";

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
        }

        public void FlushlessLog(string message, LogSeverity severity)
        {
            buffer += $"[{DateTime.Now.ToString("HH:mm:ss.FFFFF")} - {severity}] {message}\n";
        }

        public void Log(string message, LogSeverity severity)
        {
            if (!Enabled) return;

            if (writer == null) writer = new StreamWriter(LogPath) { AutoFlush = false };

            if (buffer.Length > 0)
            {
                writer.Write(buffer);
                buffer = "";
            }

            writer.Write($"[{DateTime.Now.ToString("HH:mm:ss.FFFFF")} - {severity}] {message}\n");
            writer.Flush();
        }
    }
}
