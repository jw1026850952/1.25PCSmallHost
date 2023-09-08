/**
 * Copyright (c) 2021 cptea
 * 
 * CLR Version: 4.0.30319.42000
 * Namespace  : Sugar.Log.Logger
 * ClassName  : ILogger
 * Guid       : e10af374-2367-4ba5-aed5-f5e6d2f213af
 * Author     : cptea
 * Version    : V1.0.0
 * Date       : 8/31/2021
 * Description: 
 * History:
 */

namespace Sugar.Log.Logger {

  interface ILogger {
    bool Write(MessageType type, string msg, string callerName, string callerPath, int callerLine);
  }

}