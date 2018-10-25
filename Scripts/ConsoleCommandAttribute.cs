using System;

namespace DebuggingConsole
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class ConsoleCommandAttribute : Attribute
    {
        public string command { get; }
        public string help { get; set; }
        public bool excludeFromCommandList { get; set; }
        public bool showTypesInUsage { get; set; }

        public ConsoleCommandAttribute(string command)
        {
            this.command = command;
        }

        public ConsoleCommandAttribute(string command, string help)
        {
            this.command = command;
            this.help = help;
        }
    }
}