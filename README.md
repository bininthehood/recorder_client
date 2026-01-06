🎧 VoiceLog Client

금융권 상담 녹취를 위한 C# 기반 데스크톱 녹취 프로그램입니다.
마이크 입력부터 파일 저장, 서버 전송까지 녹취 전체 흐름을 단독으로 설계·구현했습니다.

1️⃣ 프로젝트 개요

프로젝트 유형: 금융권 내부 사용 녹취 클라이언트

개발 형태: 단독 개발

플랫폼: Windows Desktop

주요 역할:

오디오 녹취 로직 설계

장치 제어 / 파일 처리

실시간 상태 관리

서버 연동 구조 구현

단순 녹음 프로그램이 아니라
**“실무 환경에서 안정적으로 동작하는 녹취 클라이언트”**를 목표로 개발했습니다.

2️⃣ 주요 기능
🎙 오디오 녹취

마이크 입력 실시간 캡처

WAV / MP3 포맷 저장

스테레오 입력 시 좌/우 채널 분리 저장

녹취 시작 / 일시정지 / 중지 제어

🎛 장치 관리

다중 입력 장치 목록 조회

실시간 장치 변경 대응

기본 장치 변경 감지

📊 실시간 모니터링

입력 음성 파형 시각화

녹취 상태 (Recording / Pause / Stop) 표시

녹취 시간 타이머 관리

🌐 서버 연동

녹취 파일 서버 전송

전송 실패 시 재시도 구조

클라이언트 ↔ 서버 역할 분리

3️⃣ 시스템 흐름
[ Audio Device ]
       ↓
[ NAudio Capture ]
       ↓
[ Buffer Processing ]
       ↓
[ Channel Split / Encoding ]
       ↓
[ File Save (WAV / MP3) ]
       ↓
[ Server Upload (Optional) ]

4️⃣ 핵심 구현 포인트
🔹 1. 안정적인 오디오 캡처

NAudio (WaveInEvent) 기반 캡처

버퍼 단위 수신 (OnDataAvailable)

UI 스레드와 녹취 스레드 분리

UI Thread  ← 상태 표시
Audio Thread ← 실시간 녹취 처리


👉 장시간 녹취 시에도 UI 멈춤 없이 동작하도록 설계

🔹 2. 좌 / 우 채널 분리 처리

스테레오 PCM 데이터에서
L / R 채널 바이트 직접 분리

각각 독립 파일로 저장 가능

[L][R][L][R] → Left.wav / Right.wav


👉 금융 녹취 환경에서 채널 단위 관리 요구 대응

🔹 3. 파일 저장 안정성

녹취 중 강제 종료 대비

스트림 기반 파일 쓰기

녹취 종료 시 헤더 정합성 보장

🔹 4. UI와 로직 분리

WPF 기반 UI

녹취 / 장치 / 네트워크 로직 분리

상태 변경은 Dispatcher를 통해 UI 반영

5️⃣ 기술 스택

Language: C#

Framework: .NET / WPF

Audio: NAudio

File Format: WAV / MP3

OS: Windows

6️⃣ 프로젝트 구조 (요약)
voicelog-client
├─ src/
│   ├─ Audio/
│   │   ├─ Recorder.cs
│   │   ├─ ChannelSplitter.cs
│   │   └─ AudioEncoder.cs
│   │
│   ├─ Device/
│   │   └─ AudioDeviceManager.cs
│   │
│   ├─ Network/
│   │   └─ UploadClient.cs
│   │
│   ├─ UI/
│   │   ├─ MainWindow.xaml
│   │   └─ ViewModels/
│   │
│   └─ Common/
│       └─ Logger.cs
│
├─ assets/
│   └─ screenshot.png
└─ README.md

7️⃣ 문제 상황 & 해결 경험
❗ 장시간 녹취 시 UI 멈춤

원인: UI 스레드에서 녹취 처리

해결: 녹취 로직을 별도 스레드로 분리

❗ 스테레오 녹취 시 채널 데이터 꼬임

원인: PCM 바이트 처리 순서 문제

해결: 프레임 단위로 좌/우 명확 분리

❗ 녹취 중 예외 발생 시 파일 손상

해결: 스트림 종료 시점 제어 및 예외 처리 강화

8️⃣ 배운 점

오디오 처리는 정확성과 안정성이 최우선

UI / 로직 분리가 유지보수에 결정적

“녹음된다”보다 **“운영 환경에서 버틴다”**가 중요

📌 한 줄 요약 (면접용)

금융권 환경에서 사용되는
C# 기반 데스크톱 녹취 프로그램을 단독으로 설계·구현한 프로젝트
