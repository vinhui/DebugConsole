using System;
using System.Reflection;

namespace DebugConsole
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public class ConsoleCommandAttribute : Attribute
    {
        public string command { get; private set; }
        public string help { get; set; }
        public bool excludeFromCommandList { get; private set; }
        public bool showTypesInUsage { get; private set; }

        public ConsoleCommandAttribute(string command)
        {
            this.command = command;
        }

        public ConsoleCommandAttribute(string command, string help)
        {
            this.command = command;
            this.help = help;
        }

        public ConsoleCommandAttribute(string command, bool excludeFromCommandList)
        {
            this.command = command;
            this.excludeFromCommandList = excludeFromCommandList;
        }

        public ConsoleCommandAttribute(string command, string help, bool excludeFromCommandList)
        {
            this.command = command;
            this.help = help;
            this.excludeFromCommandList = excludeFromCommandList;
        }

        public ConsoleCommandAttribute(string command,
            string help = null,
            bool excludeFromCommandList = false,
            bool showTypesInUsage = false)
        {
            this.command = command;
            this.help = help;
            this.excludeFromCommandList = excludeFromCommandList;
            this.showTypesInUsage = showTypesInUsage;
        }
    }
}