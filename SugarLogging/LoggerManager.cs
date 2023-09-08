/**
 * Copyright (c) 2021 cptea
 * 
 * CLR Version: 4.0.30319.42000
 * Namespace  : Sugar.Log
 * ClassName  : LoggerManager
 * Guid       : c97c63d4-16d5-4326-b563-7b78e160a41b
 * Author     : cptea
 * Version    : V1.0.0
 * Date       : 8/31/2021
 * Description: 
 * History:
 */


using Sugar.Log.Logger;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Sugar.Log
{
    public static class LoggerManager
    {
        static readonly List<BaseLogger> s_loggers = new List<BaseLogger>();
        private static readonly object s_locker = new object();

        public static void Enable(LoggerType type, LogLevel level = LogLevel.Debug)
        {
            s_loggers.Clear();
            //if (type.HasFlag(LoggerType.Debug))
            //if (type.HasFlag(LoggerType.Console))      

            if (type.HasFlag(LoggerType.File))
            {
                s_loggers.Add(new FileLogger(level: level));
            }
        }

        public static void Disable()
        {
            s_loggers.Clear();
        }

        public async static Task ClearHistories(int duration = 30)
        {
            await Task.Run(() =>
            {
                bool isFinished = false;
                lock (s_locker)
                {
                    if (s_loggers.Any())
                    {
                        isFinished = true;
                        s_loggers.ForEach(logger => isFinished &= logger.Clear(duration));
                    }
                }
            }).ConfigureAwait(false);
        }

        private static bool Write(
            MessageType type,
            string message,
            string callerName,
            string callerPath,
            int callerLine)
        {
            bool isFinished = false;
            lock (s_locker)
            {
                string msg = BaseLogger.FormatMessage(type, message, callerName, callerPath, callerLine);
                if (s_loggers.Any())
                {
                    isFinished = true;
                    s_loggers.ForEach(logger => isFinished &= logger.Write(msg, type));
                }
            }
            return isFinished;
        }

        public static bool WriteDebug(
            string message,
            [CallerMemberName] string callerName = null,
            [CallerFilePath] string callerPath = null,
            [CallerLineNumber] int callerLine = 0) =>
            Write(MessageType.Debug, message, callerName, callerPath, callerLine);

        public static bool WriteInfo(
            string message,
            [CallerMemberName] string callerName = null,
            [CallerFilePath] string callerPath = null,
            [CallerLineNumber] int callerLine = 0) =>
            Write(MessageType.Info, message, callerName, callerPath, callerLine);

        public static bool WriteWarn(
            string message,
            [CallerMemberName] string callerName = null,
            [CallerFilePath] string callerPath = null,
            [CallerLineNumber] int callerLine = 0) =>
            Write(MessageType.Warn, message, callerName, callerPath, callerLine);

        public static bool WriteError(
            string message,
            [CallerMemberName] string callerName = null,
            [CallerFilePath] string callerPath = null,
            [CallerLineNumber] int callerLine = 0) =>
            Write(MessageType.Error, message, callerName, callerPath, callerLine);

        public static bool WriteFatal(
            string message,
            [CallerMemberName] string callerName = null,
            [CallerFilePath] string callerPath = null,
            [CallerLineNumber] int callerLine = 0) =>
            Write(MessageType.Fatal, message, callerName, callerPath, callerLine);
    }
}