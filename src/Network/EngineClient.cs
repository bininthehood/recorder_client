using System;
using VoiceLog.Network;

namespace VoiceLog.Network
{
    /// <summary>
    /// 엔진 서버로 messageSend(TCP) 하는 클라이언트
    /// 프로토콜: 117바이트 고정 헤더 + Body(bodySize)
    /// - type 1: 시작 + 데이터(PCM)
    /// - type 2: 종료
    /// </summary>
    public class EngineClient : IDisposable
    {
        private readonly TcpMessageSender _sender = new TcpMessageSender();

        public void Connect(string host, int port) => _sender.Connect(host, port);

        /// <summary>
        /// 녹취 시작 (Body 없음)
        /// </summary>
        public void SendStart(string key, int subKey, int code = 100)
        {
            // type=1, bodySize=0
            var packet = PacketBuilder.Build(type: 1, code: code, key: key, subKey: subKey, body: Array.Empty<byte>());
            _sender.Send(packet);
        }

        /// <summary>
        /// 녹취 데이터 전송 (PCM Body 포함)
        /// - 규격상 "데이터도 type=1" 이므로 type=1로 전송
        /// </summary>
        public void SendAudio(string key, int subKey, byte[] pcm, int code = 200)
        {
            if (pcm == null || pcm.Length == 0) return;

            // type=1, bodySize=pcm.Length
            var packet = PacketBuilder.Build(type: 1, code: code, key: key, subKey: subKey, body: pcm);
            _sender.Send(packet);
        }

        /// <summary>
        /// 녹취 종료 (Body 필요 시 사용: 예) 파일사이즈/결과코드 등)
        /// </summary>
        public void SendStop(string key, int subKey, byte[] body = null, int code = 900)
        {
            // type=2
            var packet = PacketBuilder.Build(type: 2, code: code, key: key, subKey: subKey, body: body ?? Array.Empty<byte>());
            _sender.Send(packet);
        }

        public void Dispose() => _sender.Dispose();
    }
}
