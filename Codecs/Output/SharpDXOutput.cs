using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using SharpDX;
using SharpDX.DirectSound;
using SharpDX.Multimedia;
using WaveFormatRep = SharpDX.Multimedia.WaveFormat;

namespace Audiosort.Codecs
{
    public class SharpDXOutput : WaveOutput
    {
        protected const double BufferLength = 1.0;

        protected DirectSound DirectSound = null;
        protected PrimarySoundBuffer PrimarySoundBuffer = null;
        protected SecondarySoundBuffer SecondarySoundBuffer = null;
        protected WaveFormatRep WaveFormatRep;
        protected int VolumeRep = 100;

        public SharpDXOutput(IntPtr handle)
        {
            DirectSound = new DirectSound();
            DirectSound.SetCooperativeLevel(handle, CooperativeLevel.Priority);
        }

        public override void PrepareBuffer(AudioStream stream)
        {
            if (AudioStream != null)
                AudioStream.Close();

            AudioStream = stream;
            WaveFormat = AudioStream.Format;

            if (PrimarySoundBuffer != null)
                PrimarySoundBuffer.Dispose();
            if (SecondarySoundBuffer != null)
                SecondarySoundBuffer.Dispose();

            if (WaveFormat.Channels == -1)
                WaveFormat.Channels = 2;

            WaveFormatRep = WaveFormatRep.CreateCustomFormat(
                WaveFormatEncoding.Pcm,
                WaveFormat.SampleRate,
                WaveFormat.Channels,
                WaveFormat.SampleRate * WaveFormat.BlockAlign,
                WaveFormat.BlockAlign,
                WaveFormat.BitsPerSample
                );

            SoundBufferDescription BufferDesc = new SoundBufferDescription();
            BufferDesc.Flags = BufferFlags.PrimaryBuffer;
            BufferDesc.AlgorithmFor3D = Guid.Empty;

            PrimarySoundBuffer = new PrimarySoundBuffer(DirectSound, BufferDesc);

            BufferDesc = new SoundBufferDescription();
            //BufferDesc.BufferBytes = (int)Math.Round((double)WaveFormatRep.AverageBytesPerSecond * BufferLength);
            BufferDesc.BufferBytes = (int)AudioStream.Length;
            BufferDesc.Format = WaveFormatRep;
            BufferDesc.Flags = BufferFlags.GetCurrentPosition2 | BufferFlags.ControlPositionNotify | BufferFlags.GlobalFocus |
                BufferFlags.ControlVolume | BufferFlags.StickyFocus;
            BufferDesc.AlgorithmFor3D = Guid.Empty;

            SecondarySoundBuffer = new SecondarySoundBuffer(DirectSound, BufferDesc);
            Volume = VolumeRep;

            fillBuffer();

            if (Initialized != null)
                Initialized(this, new EventArgs());
        }

        protected void fillBuffer()
        {
            MemoryStream stream = new MemoryStream();
            AudioStream.CopyTo(stream);
            byte[] data = stream.ToArray();

            DataStream buffer, buffer2;
            buffer = SecondarySoundBuffer.Lock(0, data.Length, LockFlags.EntireBuffer, out buffer2);
            buffer.WriteRange(data);
            SecondarySoundBuffer.Unlock(buffer, buffer2);
        }

        public override bool isPlaying()
        {
            if (SecondarySoundBuffer != null)
                return SecondarySoundBuffer.Status > 0;
            return false;
        }

        public override void Play()
        {
            if (SecondarySoundBuffer != null)
            {
                SecondarySoundBuffer.CurrentPosition = 0;
                SecondarySoundBuffer.Play(0, Loop ? PlayFlags.Looping : PlayFlags.None);
            }
        }

        public override void Pause()
        {
            if (SecondarySoundBuffer != null)
            {
                SecondarySoundBuffer.Stop();
            }
        }

        public override void Resume()
        {
            if (SecondarySoundBuffer != null)
            {
                SecondarySoundBuffer.Play(0, Loop ? PlayFlags.Looping : PlayFlags.None);
            }
        }

        public override void Stop()
        {
            if (SecondarySoundBuffer != null)
            {
                SecondarySoundBuffer.Stop();
                SecondarySoundBuffer.CurrentPosition = 0;
            }
        }

        public override float GetPosition()
        {
            if (SecondarySoundBuffer != null)
            {
                int playCursor, writeCursor;
                SecondarySoundBuffer.GetCurrentPosition(out playCursor, out writeCursor);
                return (float)playCursor / SecondarySoundBuffer.Capabilities.BufferBytes;
            }
            return 0.0f;
        }

        public override void SetPosition(float pos)
        {
            if (SecondarySoundBuffer != null)
            {
                SecondarySoundBuffer.CurrentPosition = (int)(pos * WaveFormat.BytesPerSecond);
            }
        }

        public override void SetVolume(int volume)
        {
            if (SecondarySoundBuffer != null)
            {
                try
                {
                    SecondarySoundBuffer.Volume = volume;
                }
                catch (Exception)
                {
                    VolumeRep = volume;
                }
            }
            else
                VolumeRep = volume;
        }

        public override int GetVolume()
        {
            if (SecondarySoundBuffer != null)
                return SecondarySoundBuffer.Volume;
            return VolumeRep;
        }
    }
}
