using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.DirectX;
using Microsoft.DirectX.DirectSound;
using Buffer = Microsoft.DirectX.DirectSound.Buffer;
using WaveFormatRep = Microsoft.DirectX.DirectSound.WaveFormat;
using System.Threading;

namespace Audiosort.Codecs
{
    public class DirectsoundOutput : WaveOutput
    {
        const double BufferLength = 1.0; // sec

        protected IntPtr hWnd = IntPtr.Zero;
        protected static Device Device = null;
        protected SecondaryBuffer SoundBufffer = null;
        protected Notify Notify = null;
        protected WaveFormatRep WaveFormatRep;

        protected AutoResetEvent NotificationEvent = new AutoResetEvent(false);

        public DirectsoundOutput(IntPtr hwnd)
        {
            hWnd = hwnd;

            if (Device == null)
            {
                Device = new Device();
                Device.SetCooperativeLevel(hwnd, CooperativeLevel.Priority);
            }
        }

        public override void PrepareBuffer(AudioStream stream)
        {
            AudioStream = stream;
            WaveFormat = AudioStream.Format;

            if (SoundBufffer != null)
                SoundBufffer.Dispose();
            if (Notify != null)
                Notify.Dispose();

            WaveFormatRep = new WaveFormatRep();
            WaveFormatRep.FormatTag = WaveFormatTag.Pcm;
            WaveFormatRep.SamplesPerSecond = WaveFormat.SampleRate;
            WaveFormatRep.BitsPerSample = WaveFormat.BitsPerSample;
            WaveFormatRep.Channels = WaveFormat.Channels;
            WaveFormatRep.BlockAlign = (short)(WaveFormatRep.Channels * (WaveFormatRep.BitsPerSample / 8));
            WaveFormatRep.AverageBytesPerSecond = WaveFormatRep.SamplesPerSecond * WaveFormatRep.BlockAlign;

            BufferDescription BufferDesc = new BufferDescription(WaveFormatRep);
            BufferDesc.ControlPositionNotify = true;
            BufferDesc.ControlVolume = true;
            BufferDesc.ControlEffects = false;
            BufferDesc.Control3D = false;
            BufferDesc.StickyFocus = true;
            BufferDesc.BufferBytes = (int)Math.Round((double)WaveFormatRep.AverageBytesPerSecond * BufferLength);

            SoundBufffer = new SecondaryBuffer(BufferDesc, Device);
            int bufferLength = SoundBufffer.Caps.BufferBytes;
            byte[] randomData = new byte[bufferLength];
            new Random().NextBytes(randomData);

            Notify = new Notify(SoundBufffer);
            BufferPositionNotify[] bpn = new BufferPositionNotify[3];
            bpn[0] = new BufferPositionNotify();
            bpn[0].Offset = bufferLength / 2 - 1;
            bpn[0].EventNotifyHandle = NotificationEvent.Handle;

            bpn[1] = new BufferPositionNotify();
            bpn[1].Offset = bufferLength - 1;
            bpn[1].EventNotifyHandle = NotificationEvent.Handle;

            bpn[2] = new BufferPositionNotify();
            bpn[2].Offset = (int)PositionNotifyFlag.OffsetStop;
            bpn[2].EventNotifyHandle = NotificationEvent.Handle;

            Notify.SetNotificationPositions(bpn, 3);

            if (Initialized != null)
                Initialized(this, new EventArgs());
        }

        public override bool isPlaying()
        {
            return false;
        }

        public override void Play()
        {
        }

        public override void Pause()
        {
        }

        public override void Resume()
        {
        }

        public override void Stop()
        {
        }

        public override float GetPosition()
        {
            return 0.0f;
        }

        public override void SetPosition(float pos)
        {
        }

        public override void SetVolume(int volume)
        {
        }

        public override int GetVolume()
        {
            return 100;
        }
    }
}
