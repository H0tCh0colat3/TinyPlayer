using System;
using System.Threading;
using NAudio.Wave;
using TinyPlayer.Enums;

namespace TinyPlayer.Audio
{
    public class AudioPlayer : IDisposable
    {
        private AudioFileReader _audioFileReader;

        private DirectSoundOut _output;

        private Timer _positionUpdateTimer;

        public bool EndOfFile { get; private set; } = false;

        public double LengthInSeconds => _audioFileReader?.TotalTime.TotalSeconds ?? 0.0;

        public double PositionInSeconds
        {
            get => _audioFileReader?.CurrentTime.TotalSeconds ?? 0.0;
            set
            {
                if (_audioFileReader != null)
                {
                    _audioFileReader.CurrentTime = TimeSpan.FromSeconds(Math.Min(value, LengthInSeconds));
                }
            }
        }

        public float Volume
        {
            get => _audioFileReader?.Volume ?? 1;
            set
            {
                if (_output != null && _audioFileReader != null)
                {
                    _audioFileReader.Volume = value;
                }
            }
        }

        public event Action PlaybackResumed;
        public event Action PlaybackStopped;
        public event Action PlaybackPaused;
        public event Action PositionUpdated;

        public AudioPlayer(string filepath, float volume)
        {
            _audioFileReader = new AudioFileReader(filepath) { Volume = volume };

            _output = new DirectSoundOut(200);
            _output.PlaybackStopped += OutputPlaybackStopped;

            var wc = new WaveChannel32(_audioFileReader)
            {
                PadWithZeroes = false
            };

            _output.Init(wc);
        }

        private void OutputPlaybackStopped(object sender, StoppedEventArgs e)
        {
            EndOfFile = PositionInSeconds >= LengthInSeconds;
            PlaybackStopped?.Invoke();
            Dispose();
        }

        private void TimerCallback(object o)
        {
            PositionUpdated?.Invoke();
        }

        public void Play()
        {
            if (_output != null)
            {
                _output.Play();
                PlaybackResumed?.Invoke();
            }

            _positionUpdateTimer = new Timer(TimerCallback, null, 0, 1000);
        }

        public void Pause()
        {
            DisposeTimer();
            if (_output != null)
            {
                _output.Pause();
                PlaybackPaused?.Invoke();
            }
        }

        public void Stop()
        {
            DisposeTimer();
            _output?.Stop();
        }

        public void Dispose()
        {
            if (_output != null)
            {
                if (_output.PlaybackState == PlaybackState.Playing)
                {
                    _output.Stop();
                }
                _output.Dispose();
                _output = null;
            }
            if (_audioFileReader != null)
            {
                _audioFileReader.Dispose();
                _audioFileReader = null;
            }

            DisposeTimer();
        }

        private void DisposeTimer()
        {
            if (_positionUpdateTimer != null)
            {
                _positionUpdateTimer.Dispose();
                _positionUpdateTimer = null;
            }
        }
    }
}