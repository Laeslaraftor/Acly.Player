namespace Acly.Player
{
    internal interface IAudioAnalyzer : IDisposable
    {
        public bool IsDisposed { get; }
        public int Capacity { get; set; }

        public float[] GetSpectrumData(int Size);
        public float[] GetWaveformData(int Size);
    }
}
