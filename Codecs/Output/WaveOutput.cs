using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Audiosort.Codecs
{
    public abstract class WaveOutput
    {
        public bool BufferReady { get; protected set; }
        public bool Loop;

        public WaveFormat WaveFormat;
        public AudioStream AudioStream = null;

        public abstract void PrepareBuffer(AudioStream stream);

        public abstract bool isPlaying();
        public abstract void Play();
        public abstract void Pause();
        public abstract void Resume();
        public abstract void Stop();
        public abstract float GetPosition();
        public abstract void SetPosition(float pos);
        public abstract void SetVolume(int volume);
        public abstract int GetVolume();

        public float Position
        {
            get
            {
                return GetPosition();
            }
            set
            {
                SetPosition(value);
            }
        }
        public int Volume
        {
            get
            {
                return GetVolume();
            }
            set
            {
                SetVolume(value);
            }
        }
        public float Duration = -1;

        public EventHandler Initialized;
    }
}
