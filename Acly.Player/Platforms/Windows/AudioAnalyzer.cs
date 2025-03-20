using NAudio.Dsp;
using NAudio.Wave;
using Complex = NAudio.Dsp.Complex;

namespace Acly.Player.Windows
{
    internal partial class AudioAnalyzer : IAudioAnalyzer
    {
        public AudioAnalyzer()
        {
            _Capture = new();
            _Capture.DataAvailable += OnDataAvailable;
            _Capture.WaveFormat = new WaveFormat(44100, 1);

            if (Capacity == 0)
            {
                Capacity = 1024;
            }

            _FftBuffer ??= new float[1024];
            _FftComplex ??= new Complex[1024];
            _LastFft ??= new float[512];
        }
        ~AudioAnalyzer()
        {
            Dispose();
        }

        public bool IsDisposed { get; private set; }
        public int Capacity
        {
            get => _Capacity;
            set
            {
                if (value <= 0 && (value & (value - 1)) != 0)
                {
                    throw new ArgumentException("Количество захватываемых данных должно быть 2^n");
                }

                _Capacity = value;
                _FftBuffer = new float[value];
                _FftComplex = new Complex[value];
                _LastFft = new float[value];
            }
        }
        public string? AudioFile { get; set; }

        private readonly WasapiLoopbackCapture _Capture;
        private int _Capacity;
        private float[] _FftBuffer;
        private Complex[] _FftComplex;
        private float[] _LastFft;

        #region Управление

        public void Start()
        {
            _Capture.StartRecording();
        }
        public void Stop()
        {
            _Capture.StopRecording();
        }

        public float[] GetSpectrumData(int Size)
        {
            if (Size >= 512)
            {
                throw new ArgumentException("Максимальный доступный размер - 512");
            }
            if (Size == -1)
            {
                Size = _LastFft.Length;
            }

            float[] result = new float[Size];

            if (_LastFft == null)
            {
                return result;
            }

            for (int i = 0; i < Size; i++)
            {
                result[i] = _LastFft[i * 2];
            }

            return result;
        }
        /// <summary>
        /// Обосрыш
        /// </summary>
        public float[] GetWaveformData(int Size)
        {
            if (AudioFile == null)
            {
                return [];
            }

            List<float> Samples = [];

            using AudioFileReader AudioFileReader = new(AudioFile);
            var SampleProvider = AudioFileReader.ToSampleProvider();
            var Buffer = new float[AudioFileReader.WaveFormat.SampleRate * AudioFileReader.WaveFormat.Channels]; // Буфер для чтения
            int SamplesRead;

            while ((SamplesRead = SampleProvider.Read(Buffer, 0, Buffer.Length)) > 0)
            {
                Samples.AddRange(Buffer.Take(SamplesRead));
            }

            if (Size == -1)
            {
                Size = Samples.Count;
            }

            var СomplexSamples = new Complex[Size];

            for (int i = 0; i < Size; i++)
            {
                СomplexSamples[i].X = (float)(Samples[i] * FastFourierTransform.HammingWindow(i, Size)); // Применение оконной функции
                СomplexSamples[i].Y = 0;
            }
            FastFourierTransform.FFT(true, (int)Math.Log(Size, 2), СomplexSamples);

            var АftResults = new float[Size / 2];

            for (int i = 0; i < АftResults.Length; i++)
            {
                АftResults[i] = (float)Math.Sqrt(СomplexSamples[i].X * СomplexSamples[i].X + СomplexSamples[i].Y * СomplexSamples[i].Y);
            }

            return АftResults;
        }

        public void Dispose()
        {
            if (IsDisposed)
            {
                return;
            }

            IsDisposed = true;

            Array.Fill(_FftBuffer, default);
            Array.Fill(_FftComplex, default);
            Array.Fill(_LastFft, default);

            _Capture.DataAvailable -= OnDataAvailable;
            _Capture.StopRecording();
            _Capture.Dispose();

            GC.SuppressFinalize(this);
        }

        #endregion

        #region События

        private void OnDataAvailable(object? sender, WaveInEventArgs e)
        {
            for (int i = 0; i < e.BytesRecorded; i += 2)
            {
                if (i / 2 < _FftBuffer.Length)
                {
                    _FftBuffer[i / 2] = BitConverter.ToInt16(e.Buffer, i) / 32768f;
                }
            }
            for (int i = 0; i < _FftBuffer.Length; i++)
            {
                _FftComplex[i].X = _FftBuffer[i];
                _FftComplex[i].Y = 0;
            }

            FastFourierTransform.FFT(true, 10, _FftComplex);

            for (int i = 0; i < _LastFft.Length; i++)
            {
                _LastFft[i] = (float)Math.Sqrt(_FftComplex[i].X * _FftComplex[i].X + _FftComplex[i].Y * _FftComplex[i].Y);
            }
        }

        #endregion
    }
}