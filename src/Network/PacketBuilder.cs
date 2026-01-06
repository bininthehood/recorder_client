using System;
using System.Text;

namespace VoiceLog.Network
{
    public static class PacketBuilder
    {
        // Server: headerSize = 117
        // type(2) + code(3) + key(100) + subKey(6) + bodySize(6) = 117
        public const int HeaderSize = 117;

        public static byte[] Build(int type, int code, string key, int subKey, byte[] body)
        {
            if (body == null) body = Array.Empty<byte>();

            // 숫자 필드는 고정 길이(0 padding), key는 오른쪽 공백 padding
            string sType = PadNumber(type, 2);       // 2 bytes
            string sCode = PadNumber(code, 3);       // 3 bytes
            string sKey  = PadRight(key, 100);       // 100 bytes
            string sSub  = PadNumber(subKey, 6);     // 6 bytes
            string sLen  = PadNumber(body.Length, 6);// 6 bytes

            // offset 0..116 
            string header = sType + sCode + sKey + sSub + sLen; // total 117

            byte[] headerBytes = Encoding.UTF8.GetBytes(header);
            if (headerBytes.Length != HeaderSize)
                throw new InvalidOperationException("Header size mismatch: " + headerBytes.Length);

            byte[] packet = new byte[HeaderSize + body.Length];
            Buffer.BlockCopy(headerBytes, 0, packet, 0, HeaderSize);
            if (body.Length > 0)
                Buffer.BlockCopy(body, 0, packet, HeaderSize, body.Length);

            return packet;
        }

        private static string PadNumber(int n, int width)
        {
            string s = Math.Max(0, n).ToString();
            if (s.Length > width) s = s.Substring(s.Length - width, width);
            return s.PadLeft(width, '0');
        }

        private static string PadRight(string s, int width)
        {
            s = s ?? "";
            if (s.Length > width) s = s.Substring(0, width);
            return s.PadRight(width, ' ');
        }
    }
}
