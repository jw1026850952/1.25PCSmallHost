using System;
using Checkers;

namespace FireLinkage {
  class CRC {
    /// <summary>
    /// 计算CRC16
    /// </summary>
    /// <param name="data"></param>
    /// <returns>大端存储</returns>
    private static byte[] CRC16(byte[] data) {
      int length = data.Length;
      if(length <= 0) return new byte[] { 0, 0 };

      ushort crc = 0xFFFF;
      for(int i = 0; i < length; ++i) {
        crc ^= data[i];
        for(int j = 0; j < 8; ++j) {
          crc = (crc & 0x01) != 0 ? (ushort)((crc >> 1) ^ 0xA001) : (ushort)(crc >> 1);
        }
      }

      byte high = (byte)((crc & 0xFF00) >> 8);
      byte low = (byte)(crc & 0x00FF);

      return new byte[] { high, low };
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="data"></param>
    /// <param name="isBigEndian">是否为大端存储</param>
    /// <returns></returns>
    private static string CRC16ToString(byte[] data, bool isBigEndian = true) {
      try {
        return Convert.ToString((CRC16BytesToUInt16(data)), 16)
          .ToUpper().PadLeft(4, '0');
      } catch(Exception ex) { throw (ex); }
    }

    private static ushort CRC16BytesToUInt16(byte[] bytes, bool isBigEndian = true) {
      return isBigEndian ?
        (ushort)(bytes[1] << 8 | bytes[0]) :
        (ushort)(bytes[0] << 8 | bytes[1]);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="data">数据</param>
    /// <param name="isBigEndian">是否转换为大端存储</param>
    /// <returns></returns>
    internal static string ToCRC16(byte[] data, bool isBigEndian = true) {
      return CRC16ToString(CRC16(data), isBigEndian);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="data"></param>
    /// <param name="isBigEndian"></param>
    /// <returns></returns>
    internal static byte[] ToCRC16Bytes(byte[] data, bool isBigEndian = true) {
      var crc16 = CRC16(data);
      if (!isBigEndian) {
        byte high = crc16[1];
        crc16[1] = crc16[0];
        crc16[0] = high;
      }
      return crc16;
    }

    internal static bool CheckWithCRC16(byte[] data, byte[] crc16, bool isBigEndian = true) {
      var crc16x = CRC16BytesToUInt16(CRC16(data), true);
      var crc16y = CRC16BytesToUInt16(crc16, isBigEndian);
      return crc16x == crc16y;
    }
  }
}
