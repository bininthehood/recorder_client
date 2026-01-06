using System;
using NAudio.Wave;
using VoiceLog.Network;

namespace VoiceLog.Audio
{
    /// <summary>
    /// 마이크 입력 캡처 → 엔진서버로 TCP 전송(messageSend)까지 연결한 핵심 클래스
    /// - NAudio WaveInEvent 기반
    /// - 캡처 버퍼를 프레임 단위로 패킷화하여 TCP로 전송
    /// </summary>
    public class Recorder : IDisposable
    {
        private WaveInEvent _waveIn;
        private bool _isRecording;

        private readonly TcpMessageSender _sender = new TcpMessageSender();

        // (선택) 전송 누적/오류 카운트 등 모니터링용
        public long SentBytes { get; private set; }
        public long SentFrames { get; private set; }

        public bool IsRecording => _isRecording;

        /// <summary>
        /// 엔진 서버 연결
        /// </summary>
        public void ConnectEngine(string host, int port)
        {
            _sender.Connect(host, port);
            Console.WriteLine("[녹취] 엔진 서버 TCP 연결 완료");
        }

        /// <summary>
        /// 녹취 시작
        /// </summary>
        public void Start(int deviceNumber, int sampleRate = 44100, int channels = 2)
        {
            if (_isRecording) return;

            if (!_sender.IsConnected)
                throw new InvalidOperationException("엔진 서버에 먼저 ConnectEngine()으로 연결해야 합니다.");

            _waveIn = new WaveInEvent
            {
                DeviceNumber = deviceNumber,
                WaveFormat = new WaveFormat(sampleRate, channels),
                BufferMilliseconds = 50
            };

            _waveIn.DataAvailable += OnDataAvailable;
            _waveIn.RecordingStopped += OnRecordingStopped;

            _waveIn.StartRecording();
            _isRecording = true;

            Console.WriteLine("[녹취] 녹취 시작");
        }

        public void Stop()
        {
            if (!_isRecording || _waveIn == null) return;
            _waveIn.StopRecording();
        }

        private void OnDataAvailable(object sender, WaveInEventArgs e)
        {
            if (!_isRecording || e.BytesRecorded <= 0) return;

            // NAudio 내부 버퍼 보호를 위해 복사
            byte[] pcm = new byte[e.BytesRecorded];
            Buffer.BlockCopy(e.Buffer, 0, pcm, 0, e.BytesRecorded);

            try
            {
                // ✅ 여기서 “messageSend” 개념으로 패킷 만들어 TCP 전송
                byte[] packet = BuildPacket(pcm, e.BytesRecorded, _waveIn.WaveFormat);
                _sender.Send(packet);

                SentBytes += packet.Length;
                SentFrames++;
            }
            catch (Exception ex)
            {
                // 실무에서는 재연결/재시도/큐잉 등을 넣을 수 있음
                Console.WriteLine("[녹취] 엔진 서버 전송 실패: " + ex.Message);
                // 필요 시 녹취 중단 등 정책 적용
                // Stop();
            }
        }

        /// <summary>
        /// ⚠️ 공개용 포인트:
        /// - 실제 사내 프로토콜/패킷 구조는 여기서만 바꿔끼우면 됨
        /// - 예시는 "길이(4바이트) + PCM" 형태로 아주 단순화
        /// </summary>
        private byte[] BuildPacket(byte[] pcm, int len, WaveFormat fmt)
        {
            // 예시 프로토콜:
            // [4 bytes length (little endian)] + [PCM bytes]
            // 실제 프로젝트에서는:
            // - command/type
            // - sampleRate/channels/bits
            // - timestamp/seq
            // - checksum
            // 등을 붙였을 가능성이 높음

            int payloadLen = len;
            byte[] packet = new byte[4 + payloadLen];

            // length
            packet[0] = (byte)(payloadLen & 0xFF);
            packet[1] = (byte)((payloadLen >> 8) & 0xFF);
            packet[2] = (byte)((payloadLen >> 16) & 0xFF);
            packet[3] = (byte)((payloadLen >> 24) & 0xFF);

            Buffer.BlockCopy(pcm, 0, packet, 4, payloadLen);
            return packet;
        }

        private void OnRecordingStopped(object sender, StoppedEventArgs e)
        {
            Cleanup();
            Console.WriteLine("[녹취] 녹취 종료");
        }

        private void Cleanup()
        {
            _isRecording = false;

            if (_waveIn != null)
            {
                _waveIn.DataAvailable -= OnDataAvailable;
                _waveIn.RecordingStopped -= OnRecordingStopped;
                _waveIn.Dispose();
                _waveIn = null;
            }
        }

        public void Dispose()
        {
            Cleanup();
            _sender.Dispose();
        }
    }
}
