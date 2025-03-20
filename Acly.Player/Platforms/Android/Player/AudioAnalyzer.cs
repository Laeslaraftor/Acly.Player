using Android.Media;
using Android.Media.Audiofx;

namespace Acly.Player.Android
{
    internal class AudioAnalyzer : IAudioAnalyzer
    {
        public AudioAnalyzer(MediaPlayer Player, bool Waveform, bool Fft)
        {
            _Visualizer = new(Player.AudioSessionId);
            _Equalizer = new(0, Player.AudioSessionId);
            _CaptureCallback = new();
            _CaptureCallback.FftCaptured += OnFftCaptured;
            _CaptureCallback.WaveFormCaptured += OnWaveFormCaptured;

            _Visualizer.SetScalingMode(VisualizerScalingMode.AsPlayed);
            _Visualizer.SetMeasurementMode(VisualizerMeasurementMode.PeakRms);
            _Visualizer.SetDataCaptureListener(_CaptureCallback, Visualizer.MaxCaptureRate, Waveform, Fft);

            _Visualizer.SetEnabled(true);
            _Equalizer.SetEnabled(true);
        }
        ~AudioAnalyzer()
        {
            Dispose();
        }

        public bool IsDisposed { get; private set; }
        public int Capacity
        {
            get => _Visualizer.CaptureSize;
            set => _Visualizer.SetCaptureSize(value);
        }

        private readonly Visualizer _Visualizer;
        private readonly Equalizer _Equalizer;
        private readonly AudioVisualizerCaptureCallback _CaptureCallback;
        private byte[]? _LastFft;
        private byte[]? _LastWaveform;

        #region Управление

        public float[] GetSpectrumData(int Size)
        {
            return GetData(_LastFft, Size);
        }
        public float[] GetWaveformData(int Size)
        {
            return GetData(_LastWaveform, Size);
        }

        public void Dispose()
        {
            if (IsDisposed)
            {
                return;
            }

            IsDisposed = true;

            _CaptureCallback.FftCaptured -= OnFftCaptured;
            _CaptureCallback.WaveFormCaptured -= OnWaveFormCaptured;
            
            _Equalizer.SetEnabled(false);
            _Visualizer.SetEnabled(false);
            _Visualizer.SetDataCaptureListener(null, 0, false, false);
            _CaptureCallback.Dispose();
            _Visualizer.Dispose();
            _Equalizer.Dispose();

            _LastFft = null;
            _LastWaveform = null;

            GC.SuppressFinalize(this);
        }

        private float[] GetData(byte[]? DataArray, int Size)
        {
            if (Size >= Capacity)
            {
                throw new ArgumentException($"Запрашиваемый размер больше выделенного! Запрошено {Size}, а выделено {Capacity}.");
            }
            if (Size == -1)
            {
                Size = Capacity;
            }

            float[] Result = new float[Size];

            if (DataArray == null)
            {
                return Result;
            }

            for (int i = 0; i < Size; i++)
            {
                Result[i] = (float)DataArray[i] / 255;
            }

            return Result;
        }

        #endregion

        #region События

        private void OnWaveFormCaptured(Visualizer? Visualizer, byte[]? Data, int SamplingRate)
        {
            _LastWaveform = Data;
        }
        private void OnFftCaptured(Visualizer? Visualizer, byte[]? Data, int SamplingRate)
        {
            _LastFft = Data;
        }

        #endregion
    }
}
