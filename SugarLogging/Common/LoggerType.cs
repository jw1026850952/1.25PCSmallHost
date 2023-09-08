/**
 * Copyright (c) 2021 cptea
 * 
 * CLR Version: 4.0.30319.42000
 * Namespace  : Sugar.Log
 * ClassName  : LoggerType
 * Guid       : 811ee732-6e0d-47eb-b71e-3f41fb6bd9a2
 * Author     : cptea
 * Version    : V1.0.0
 * Date       : 8/31/2021
 * Description: 
 * History:
 */



using System;

namespace Sugar.Log {
  [Flags]
  public enum LoggerType {
    /// <summary>
    /// 调试日志记录器
    /// </summary>
    Debug = 1,

    /// <summary>
    /// 终端日志记录器
    /// </summary>
    Console = 2,

    /// <summary>
    /// 文件日志记录器
    /// </summary>
    File = 4,
  }
}
