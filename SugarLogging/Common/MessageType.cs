/**
 * Copyright (c) 2021 cptea
 * 
 * CLR Version: 4.0.30319.42000
 * Namespace  : Sugar.Log.Common
 * ClassName  : MessageType
 * Guid       : 599935d3-e179-46ae-a19a-dd82fb36e5eb
 * Author     : cptea
 * Version    : V1.0.0
 * Date       : 8/31/2021
 * Description: 
 * History:
 */


namespace Sugar.Log {
  public enum MessageType {
    /// <summary>
    /// 崩溃信息
    /// </summary>
    Fatal,

    /// <summary>
    /// 错误信息
    /// </summary>
    Error,

    Warn,

    /// <summary>
    /// 普通信息
    /// </summary>
    Info,

    /// <summary>
    /// 调试信息
    /// </summary>
    Debug,
  }
}