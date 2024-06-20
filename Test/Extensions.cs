namespace Test
{
    public static class Extensions
    {
        public static string ToStringFormat(this TimeSpan Time)
        {
            double Seconds = Time.TotalSeconds;
            var Hours = Math.Floor(Seconds / 3600);
            var Min = Math.Floor((Seconds - (Hours * 3600)) / 60);
            var Sec = Math.Floor(Seconds - (Hours * 3600) - (Min * 60));

            string ResultHrs = Hours.ToString();
            string ResultMin = Min.ToString();
            string ResultSec = Sec.ToString();

            if (Hours < 10)
            {
                ResultHrs = "0" + Hours;
            }
            if (Min < 10)
            {
                ResultMin = "0" + Min;
            }
            if (Sec < 10)
            {
                ResultSec = "0" + Sec;
            }

            if (Hours == 0)
            {
                return ResultMin + ':' + ResultSec;
            }
            else
            {
                return ResultHrs + ':' + ResultSec + ':' + ResultMin;
            }
        }
    }
}
