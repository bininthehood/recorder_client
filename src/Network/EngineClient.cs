using System;
using VoiceLog.Network;

namespace VoiceLog.Network
{
    public class EngineClient : IDisposable
    {
        private readonly TcpMessageSender _sender = new TcpMessageSender();

        public void Connect(string host, int port) => _sender.Connect(host, port);

        public void SendAudio(string key, int subKey, byte[] pcm)
        {
            var packet = PacketBuilder.Build(type: 3, code: 200, key: key, subKey: subKey, body: pcm);
            _sender.Send(packet);
        }

        public void SendStart(string key, int subKey)
        {
            var packet = PacketBuilder.Build(type: 1, code: 100, key: key, subKey: subKey, body: Array.Empty<byte>());
            _sender.Send(packet);
        }

        public void SendStop(string key, int subKey, int fileSizeOr0 = 0)
        {
            var body = System.Text.Encoding.UTF8.GetBytes(fileSizeOr0.ToString());
            var packet = PacketBuilder.Build(type: 2, code: 900, key: key, subKey: subKey, body: body);
            _sender.Send(packet);
        }

        public void Dispose() => _sender.Dispose();
    }
}
