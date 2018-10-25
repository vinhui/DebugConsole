using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace DebuggingConsole
{
    public class UnityLogHandler : ILogHandler
    {
        public void LogException(Exception exception, Object context)
        {
            LogFormat(LogType.Exception, context, "{0}", exception.Message);
        }

        public void LogFormat(LogType logType, Object context, string format, params object[] args)
        {
            switch (logType)
            {
                case LogType.Error:
                    DebugConsole.WriteErrorLine(format, args);
                    break;

                case LogType.Warning:
                    DebugConsole.WriteWarningLine(format, args);
                    break;

                case LogType.Log:
                    DebugConsole.WriteLine(format, args);
                    break;

                case LogType.Exception:
                    DebugConsole.WriteErrorLine(format, args);
                    break;

                default:
                    throw new NotImplementedException();
            }
        }
    }
}