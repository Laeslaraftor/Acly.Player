using System.Diagnostics;

namespace Test
{
    public class SpectrumDrawable : IDrawable
    {
        public float[]? Fft { get; set; }

        public void Draw(ICanvas canvas, RectF dirtyRect)
        {
            canvas.FillColor = Colors.Transparent;
            canvas.FillRectangle(dirtyRect);

            if (Fft == null)
            {
                return;
            }

            Debug.WriteLine(Fft[0]);

            float interval = 5;
            float columnWidth = (dirtyRect.Width - Fft.Length * interval) / Fft.Length;
            canvas.FillColor = Colors.Red;

            for (int i = 0; i < Fft.Length; i++)
            {
                float offset = i * (interval + columnWidth);
                float height = Fft[i] * dirtyRect.Height;
                float top = dirtyRect.Height - height;

                canvas.FillRectangle(offset, top, columnWidth, height);
            }
        }
    }
}
