using System;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using UnityEngine;
using Object = UnityEngine.Object;

namespace DebuggingConsole
{
    public class ConsoleCommand
    {
        public string command { get; }
        public string help { get; protected set; }

        protected int parameterCount;
        protected string[] parameterNames;
        protected Type[] parameterTypes;

        public bool excludeFromCommandList { get; set; }
        public bool showTypesInUsage { get; set; }
        
        public bool captureAllParamsAsOne { get; protected set; }

        protected readonly MethodInfo method;

        public ConsoleCommand()
        {
        }

        public ConsoleCommand([NotNull] ConsoleCommandAttribute attribute, [NotNull] MethodInfo method)
        {
            if (attribute == null)
                throw new ArgumentNullException(nameof(attribute));
            if (method == null)
                throw new ArgumentNullException(nameof(method));

            command = attribute.command.ToLower();
            help = attribute.help;
            excludeFromCommandList = attribute.excludeFromCommandList;
            showTypesInUsage = attribute.showTypesInUsage;
            this.method = method;
            ParametersFromMethodInfo();
        }

        public ConsoleCommand([NotNull] string command, Delegate method)
            : this(command, method.Method)
        {
        }

        public ConsoleCommand([NotNull] string command, MethodInfo method)
        {
            if (command == null)
                throw new ArgumentNullException(nameof(command));
            if (method == null)
                throw new ArgumentNullException(nameof(method));

            this.command = command.ToLower();
            this.method = method;
            ParametersFromMethodInfo();
        }

        /// <summary>
        /// Method to fill in the parameter fields from the <see cref="method"/>
        /// The <see cref="method"/> should not be null at this point
        /// </summary>
        private void ParametersFromMethodInfo()
        {
            ParameterInfo[] parameters = method.GetParameters();
            parameterCount = parameters.Length;
            parameterNames = new string[parameterCount];
            parameterTypes = new Type[parameterCount];
            for (int i = 0; i < parameters.Length; i++)
            {
                ParameterInfo param = parameters[i];
                parameterNames[i] = param.Name;
                parameterTypes[i] = param.ParameterType;
            }
        }

        /// <summary>
        /// Gets called to check if the command that the user entered matches this command
        /// </summary>
        /// <param name="command">The command the user entered</param>
        /// <returns>Should return true if it matches</returns>
        public virtual bool MatchesEnteredCommand(string command)
        {
            return this.command == command;
        }

        /// <summary>
        /// Gets called to check if the parameters are valid for this command
        /// This method will be used if <see cref="captureAllParamsAsOne"/> is true
        /// </summary>
        /// <param name="parameters">The parameters the user filled in</param>
        /// <returns>Should return true if it matches</returns>
        public virtual bool MatchesParameters(string parameters)
        {
            return MatchesParameters(SplitOnSpaces(parameters));
        }

        /// <summary>
        /// Gets called to check if the parameters are valid for this command
        /// This method will be used if <see cref="captureAllParamsAsOne"/> is false
        /// </summary>
        /// <param name="parameters">The parameters the user filled in</param>
        /// <returns>Should return true if it matches</returns>
        public virtual bool MatchesParameters(string[] parameters)
        {
            return parameterCount == parameters.Length;
        }

        public virtual void Invoke(string command, string[] parameters)
        {
            try
            {
                object[] @params = new object[parameters.Length];
                for (int i = 0; i < parameters.Length; i++)
                {
                    bool success;
                    @params[i] = ConvertToParameterType(i, parameters[i], out success);
                    if (!success)
                        return;
                }

                // Call the method with the parameters
                object returnValue = method.Invoke(
                    method.DeclaringType != null && method.DeclaringType.IsSubclassOf(typeof(MonoBehaviour))
                        ? Object.FindObjectOfType(method.DeclaringType)
                        : null, @params);

                // If there's a return value, output it to the console
                if (returnValue != null)
                    DebugConsole.WriteLine(returnValue.ToString());
            }
            catch (Exception e)
            {
                // Show the user running the command failed and for what reason
                DebugConsole.WriteErrorLine("Failed to run command: ");
                if (e is TargetParameterCountException)
                    DebugConsole.WriteErrorLine("Parameters are invalid");
                else
                {
                    DebugConsole.WriteErrorLine(e.Message);
                    throw;
                }

                // Show the help info for the command if its available
                if (!string.IsNullOrEmpty(help))
                {
                    DebugConsole.WriteLine(help);
                }
            }
        }

        protected virtual object ConvertToParameterType(int parameterIndex, string value, out bool success)
        {
            string paramName = parameterNames[parameterIndex];
            Type paramType = parameterTypes[parameterIndex];
            object returnVal = null;
            try
            {
                if (paramType == typeof(object))
                    returnVal = value;
                else if (paramType.IsEnum)
                    returnVal = Enum.Parse(paramType, value, true);
                else
                    returnVal = Convert.ChangeType(value, paramType);

                success = true;
            }
            catch (Exception)
            {
                DebugConsole.WriteErrorLine("Failed to cast '{0}' to '{1}' (for parameter '{2}')", value, paramType,
                    paramName);
                success = false;
            }

            return returnVal;
        }

        public override string ToString()
        {
            return $"{command}\r\n\t{help}";
        }

        public static string[] SplitOnSpaces(string cmd)
        {
            return cmd.Split('"')
                .Select((element, index) => index % 2 == 0 // If even index
                    ? element.Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries) // Split the item
                    : new[] {element}) // Keep the entire item
                .SelectMany(element => element)
                .ToArray();
        }
    }
}