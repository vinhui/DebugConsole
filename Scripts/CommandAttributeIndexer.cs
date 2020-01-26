using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace DebuggingConsole
{
    public class CommandAttributeIndexer
    {
        private readonly Assembly assembly;
        public Dictionary<ConsoleCommandAttribute, MethodInfo> commands { get; protected set; }

        public CommandAttributeIndexer(Assembly assembly)
        {
            this.assembly = assembly;
        }

        public virtual void StartIndexing()
        {
            commands = new Dictionary<ConsoleCommandAttribute, MethodInfo>();

            // Loop through all the types in the assembly
            foreach (var type in assembly.GetTypes())
            {
                // Loop through all the methods in the type
                foreach (var method in type.GetMethods(
                    BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static))
                {
                    ParseMethod(method);
                }
            }
        }

        protected virtual void ParseMethod(MethodInfo method)
        {
            // And finally, loop through all the attributes of type ConsoleCommandAttribute on the method
            foreach (var attribute in method.GetCustomAttributes(typeof(ConsoleCommandAttribute), true))
            {
                try
                {
                    ParseAttribute(attribute as ConsoleCommandAttribute, method);
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }
        }

        protected virtual void ParseAttribute(ConsoleCommandAttribute attr, MethodInfo method)
        {
            var parameters = method.GetParameters();

            attr.help = GenerateHelpForAttribute(attr, parameters);

            // Add it to the commands
            commands.Add(attr, method.ContainsGenericParameters ? method.MakeGenericMethod(typeof(object)) : method);
        }

        protected virtual string GenerateHelpForAttribute(ConsoleCommandAttribute attr, ParameterInfo[] parameters)
        {
            var help = attr.help ?? string.Empty;

            if (parameters.Length > 0)
            {
                if (!string.IsNullOrEmpty(help))
                    help += "\n";

                help += string.Format("Usage:\n\t{0}", attr.command);

                // Go through all the parameters of the method
                foreach (var parameterInfo in parameters)
                {
                    help += GenerateHelpFromParameter(attr, parameterInfo);
                }
            }

            return help;
        }

        protected virtual string GenerateHelpFromParameter(ConsoleCommandAttribute attr, ParameterInfo parameter)
        {
            var help = " <";
            help += parameter.Name;

            // Add some extra info for some types
            if (parameter.ParameterType == typeof(bool))
            {
                help += " (true|false)";
            }
            else if (parameter.ParameterType.IsEnum)
            {
                help += string.Format(" ({0})", string.Join("|", Enum.GetNames(parameter.ParameterType)));
            }
            else if(attr.showTypesInUsage)
            {
                help += string.Format(" ({0})", parameter.ParameterType.Name);
            }

            help += ">";

            return help;
        }
    }
}