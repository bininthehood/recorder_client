using System;
using System.Net.Sockets;

namespace VoiceLog.Network
{
    public class TcpMessageSender : IDisposable
    {
        private TcpClient _client;
        private NetworkStream _stream;
        private readonly object _lock = new object();
        private volatile bool _connected;

        public bool IsConnected => _connected;

        public void Connect(string host, int port, int timeoutMs = 3000)
        {
            lock (_lock)
            {
                Cleanup();

                _client = new TcpClient();
                var ar = _client.BeginConnect(host, port, null, null);
                if (!ar.AsyncWaitHandle.WaitOne(timeoutMs))
                {
                    Cleanup();
                    throw new TimeoutException("엔진 서버 TCP 연결 타임아웃");
                }

                _client.EndConnect(ar);
                _stream = _client.GetStream();
                _connected = true;

                Console.WriteLine("[TCP] 엔진 서버 연결 완료");
            }
        }

        public void Send(byte[] payload)
        {
            if (payload == null || payload.Length == 0) return;

            lock (_lock)
            {
                if (!_connected || _stream == null)
                    throw new InvalidOperationException("TCP 연결이 되어있지 않습니다.");

                _stream.Write(payload, 0, payload.Length);
            }
        }

        public void Dispose()
        {
            lock (_lock) Cleanup();
        }

        private void Cleanup()
        {
            _connected = false;
            try { _stream?.Close(); } catch { }
            try { _client?.Close(); } catch { }
            _stream = null;
            _client = null;
        }
    }
}
