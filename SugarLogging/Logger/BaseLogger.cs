/**
 * Copyright (c) 2021 cptea
 * 
 * CLR Version: 4.0.30319.42000
 * Namespace  : Sugar.Log.Logger
 * ClassName  : BaseLogger
 * Guid       : fe8688ed-d2f0-4aed-b2b8-0aaec04fd840
 * Author     : cptea
 * Version    : V1.0.0
 * Date       : 8/31/2021 2:05:26 PM
 * Description: 
 * History:
 */



using Sugar.Log;
using System.IO;
using System;

namespace Sugar.Log.Logger {
  internal abstract class BaseLogger {
    public BaseLogger(LogLevel level = LogLevel.Debug) {
      Level = level;
    }

    public LogLevel Level { get; private set; }
    
    public static string FormatMessage(
        MessageType type,
        string message,
        string callerName,
        string callerPath,
        int callerLine) {

      // example: [2021-08-31 24:00:00] Error  0x0000ffff Filename.Function: rrror message placeholder
      string msg = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {type, -5} {callerLine:x8} {Path.GetFileNameWithoutExtension(callerPath)}.{callerName}: {message}";
      return msg;
    }

    public abstract bool Write(string message, MessageType type);

    public abstract bool Clear(int duration = 30);
  }
}