/**
 * Copyright (c) 2021 . All rights reserved.
 * 
 * CLR Version: 4.0.30319.42000
 * Namespace  : Sugar.Log.Logger
 * ClassName  : FileLogger
 * Guid       : a9e27ef4-3c31-4bbb-8ed3-876c3d0722cf
 * Author     : Administrator
 * Version    : V1.0.0
 * Date       : 8/31/2021 2:07:30 PM
 * Description: 
 * History:
 */


using System.IO;
using System;

namespace Sugar.Log.Logger {
  internal class FileLogger: BaseLogger {
    private readonly string _directory;
    private static readonly object s_locker = new object();

    public FileLogger(string storageDirectory = null, LogLevel level = LogLevel.Debug)
      : base(level) {
      _directory = Path.Combine(storageDirectory ?? Environment.CurrentDirectory, "log");
      if(!Directory.Exists(_directory)) {
        _ = Directory.CreateDirectory(_directory);
      }
      using(StreamWriter writer = new StreamWriter(
        Path.Combine(_directory, $"{DateTime.Now:yyyy-MM-dd}.log"), 
        true)) { }
    }

    public override bool Write(string message, MessageType type) {
      if((int)type > (int)Level) {
        return false;
      }
      var path = Path.Combine(_directory, $"{DateTime.Now:yyyy-MM-dd}.log");
      lock(s_locker) {
        if((int)type > (int)Level) {
          return false;
        }
        using(StreamWriter writer = new StreamWriter(path, true)) {
          _ = writer.WriteLineAsync(message);
        }
      }
      return true;
    }

    public override bool Clear(int duration = 30) {
      try {
        var files = Directory.GetFiles(_directory);
        foreach(var file in files) {
          var dateTimeStr = Path.GetFileNameWithoutExtension(file);
          var dateTime = Convert.ToDateTime(dateTimeStr);
          if((DateTime.Now - dateTime).Days > duration) {
            File.Delete(file);
          }
        }
      } catch (Exception e) {
        LoggerManager.WriteFatal(e.Message);
        return false;
      }
      
      return true;
    }
  }
}