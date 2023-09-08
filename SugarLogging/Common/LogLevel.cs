/**
 * Copyright (c) 2021 cptea
 * 
 * CLR Version: 4.0.30319.42000
 * Namespace  : Sugar.Log
 * ClassName  : LogLevel
 * Guid       : 1b5d2886-fbb5-44fb-bc01-90f2dadcd90e
 * Author     : cptea
 * Version    : V1.0.0
 * Date       : 8/31/2021 2:02:12 PM
 * Description: 
 * History:
 */

namespace Sugar.Log {
  public enum LogLevel {
    /// <summary>
    /// 崩溃等级
    /// </summary>
    Fatal,

    /// <summary>
    /// 错误等级
    /// </summary>
    Error,

    Warn,

    /// <summary>
    /// 普通等级
    /// </summary>
    Info,

    /// <summary>
    /// 调试等级
    /// </summary>
    Debug,
  }
}