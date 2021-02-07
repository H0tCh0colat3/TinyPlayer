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

        public PlaybackStopType PlaybackStopType { get; set; }

        public double LengthInSeconds => _audioFileReader?.TotalTime.TotalSeconds ?? 0.0;

        public double PositionInSeconds
        {
            get => _audioFileReader?.CurrentTime.TotalSeconds ?? 0.0;
            set
            {
                if (_audioFileReader != null)
                {
                    _audioFileReader.CurrentTime = TimeSpan.FromSeconds(value);
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
            PlaybackStopType = PlaybackStopType.EndOfFile;

            _audioFileReader = new AudioFileReader(filepath) { Volume = volume };

            _output = new DirectSoundOut(200);
            _output.PlaybackStopped += OutputPlaybackStopped;

            var wc = new WaveChannel32(_audioFileReader)
            {
                PadWithZeroes = true
            };

            _output.Init(wc);
        }

        private void OutputPlaybackStopped(object sender, StoppedEventArgs e)
        {
            PlaybackStopped?.Invoke();
            Dispose();
        }

        private void TimerCallback(object o)
        {
            PositionUpdated?.Invoke();
        }

        public void Play(PlaybackState playbackState, double currentVolumeLevel)
        {
            if (playbackState == PlaybackState.Stopped || playbackState == PlaybackState.Paused)
            {
                _output.Play();
            }

            _audioFileReader.Volume = (float)currentVolumeLevel;
            PlaybackResumed?.Invoke();

            _positionUpdateTimer = new Timer(TimerCallback, null, 0, 1000);
        }

        public void Stop()
        {
            DisposeTimer();
            _output?.Stop();
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

        public void TogglePlayPause(double currentVolumeLevel)
        {
            DisposeTimer();

            if (_output != null)
            {
                if (_output.PlaybackState == PlaybackState.Playing)
                {
                    Pause();
                }
                else
                {
                    Play(_output.PlaybackState, currentVolumeLevel);
                }
            }
            else
            {
                Play(PlaybackState.Stopped, currentVolumeLevel);
            }
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