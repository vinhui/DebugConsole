using System;
using UnityEngine;

namespace DebugConsole
{
    public class UnityLogHandler : ILogHandler
    {
        public void LogException(Exception exception, UnityEngine.Object context)
        {
            LogFormat(LogType.Exception, context, "{0}", new object[] { exception.Message });
        }

        public void LogFormat(LogType logType, UnityEngine.Object context, string format, params object[] args)
        {
            switch (logType)
            {
                case LogType.Error:
                    DebugConsole.WriteErrorLine(string.Format(format, args));
                    break;

                case LogType.Warning:
                    DebugConsole.WriteWarningLine(string.Format(format, args));
                    break;

                case LogType.Log:
                    DebugConsole.WriteLine(string.Format(format, args));
                    break;

                case LogType.Exception:
                    DebugConsole.WriteErrorLine(string.Format(format, args));
                    break;

                case LogType.Assert:
                default:
                    throw new NotImplementedException();
            }
        }
    }
}