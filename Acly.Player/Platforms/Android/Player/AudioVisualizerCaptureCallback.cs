using Android.Media.Audiofx;

namespace Acly.Player.Android
{
    internal class AudioVisualizerCaptureCallback : Java.Lang.Object, Visualizer.IOnDataCaptureListener
    {
        public event DataCaptured? FftCaptured;
        public event DataCaptured? WaveFormCaptured;

        public void OnFftDataCapture(Visualizer? visualizer, byte[]? fft, int samplingRate)
        {
            FftCaptured?.Invoke(visualizer, fft, samplingRate);
        }
        public void OnWaveFormDataCapture(Visualizer? visualizer, byte[]? waveform, int samplingRate)
        {
            WaveFormCaptured?.Invoke(visualizer, waveform, samplingRate);
        }

        #region Делегаты

        public delegate void DataCaptured(Visualizer? Visualizer, byte[]? Data, int SamplingRate);

        #endregion
    }
}
