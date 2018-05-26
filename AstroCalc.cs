using System;

class AstroCalc 
{

    const double Rad2Deg = 180 / Math.PI;
    const double Deg2Rad =  Math.PI / 180;

    #region Time Functions
    DateTime m_Epoch2010UTCDate = new DateTime(2010, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    public double DayNumberSince2010Epoch(DateTime localDateTime) {
        var diff = localDateTime.ToUniversalTime() - m_Epoch2010UTCDate;
        return diff.TotalDays;
    }

    public double CalculateJulianDate(DateTime date) {
        var utc = date.ToUniversalTime();
        
        double y = utc.Year;
        double m = utc.Month;
        double d = utc.Day + utc.TimeOfDay.TotalDays;

        if (m <= 2) {
            y -=1;
            m = 12;
        }

        var a = Math.Floor(y / 100);
        var b = 2 - a + Math.Floor(a / 4);
        var c = Math.Floor(365.25 * y);
        var e = Math.Floor(30.6001 * (m + 1));

        return b + c + e + d + 1720994.5;
    }

    public DateTime CalculateUTCFromJulianDate(double julianDate) {
        var t = julianDate + 0.5;
        var I = Math.Floor(t);
        var F = t - I;

        var B = I;
        if (I > 2299160) {
            var A = Math.Floor( (I - 1867216.25) / 36524.25 );
            B = I + A - Math.Floor(A / 4) + 1;
        }

        var C = B + 1524;
        var D = Math.Floor( (C - 122.1) / 365.25);
        var E = Math.Floor(365.25 * D);
        var G = Math.Floor( (C - E) / 30.6001 );

        var d = C - E + F - Math.Floor(30.6001 * G);
        var m = G < 13.5 ? G - 1 : G - 13;
        var y = m > 2.5 ? D - 4716 : D - 4715;

        var date = new DateTime((int)y, (int)m, 1, 0, 0, 0, DateTimeKind.Utc);
        date = date.AddDays(d);

        return date;
    }


    /// Converts a date time to Greenwich Sidereal Time (GST)
    /// Date time is converted to UTC
    public TimeSpan ConvertDateTimeToGST(DateTime date) {
        var utc = date.ToUniversalTime();
        var zeroHourUTC = new DateTime(utc.Year, utc.Month, utc.Day, 0, 0, 0, DateTimeKind.Utc);
        var julianDate = CalculateJulianDate(zeroHourUTC);

        var S = julianDate - 2451545.0;
        var T = S / 36525.0;
        var T0 = 6.697374558 + (2400.051336 * T);
        T0 -= 24 * Math.Floor(T0 / 24);

        var A = utc.TimeOfDay.TotalHours * 1.002737909;
        T0 += A;
        T0 -= 24 * Math.Floor(T0 / 24);

        return TimeSpan.FromHours(T0);
    }

    /// Converts a Greenwich Sidereal Time (GST) time to Local Sidereal Time (LST)
    public TimeSpan ConvertGSTToLST(TimeSpan gst, double longitude) {
        var totalHours = gst.TotalHours;
        var lonToHours = longitude / 15;

        totalHours += lonToHours;
        totalHours -= 24 * Math.Floor(totalHours / 24);

        return TimeSpan.FromHours(totalHours);
    }

    /// Converts a Local Sidereal Time (LST) time to Greenwich Sidereal Time (GST)
    public TimeSpan ConvertLSTToGST(TimeSpan lst, double longitude) {
        var totalHours = lst.TotalHours;
        var lonToHours = longitude / 15;

        totalHours -= lonToHours;
        totalHours -= 24 * Math.Floor(totalHours / 24);

        return TimeSpan.FromHours(totalHours);
    }

    #endregion

    #region Coordinate Systems

    public struct Angle {

        private double decimalDegrees;

        public double DecimalDegrees { get => decimalDegrees; set => decimalDegrees = value; }

        public double DeciamlRadians { get { return decimalDegrees * Deg2Rad; } }

        public DegreesMinutesSeconds DegreesMinutesSeconds {
            get {

                var degrees = Math.Floor(decimalDegrees);
                var temp = (decimalDegrees - degrees) * 60;
                var mins = Math.Floor(temp);
                var seconds = (temp - mins) * 60;

                return new DegreesMinutesSeconds(degrees, mins, seconds);
            }
        }

        public Angle(double decimalDegrees) {
            this.decimalDegrees = decimalDegrees;
        }

        public static Angle FromRadians(double radians) {
            return new Angle(radians * Rad2Deg);
        }
    }

    public struct HourMinutesSeconds {
        private double hour, minutes, seconds;

        internal HourMinutesSeconds(double hour, double minutes, double seconds)
        {
            this.hour = hour;
            this.minutes = minutes;
            this.seconds = seconds;
        }

        public double Hour { get => hour; }
        public double Minutes { get => minutes; }
        public double Seconds { get => seconds; }

        public override string ToString() {

            return string.Format("{0}h {1}m {2}s", hour, minutes, seconds);
        }
    }

    public struct DegreesMinutesSeconds {
        private double degrees, minutes, seconds;

        internal DegreesMinutesSeconds(double degrees, double minutes, double seconds)
        {
            this.degrees = degrees;
            this.minutes = minutes;
            this.seconds = seconds;
        }

        public double Degrees { get => degrees; }
        public double Minutes { get => minutes; }
        public double Seconds { get => seconds; }

        public override string ToString() {

            return string.Format("{0}° {1}ʹ {2}ʺ", degrees, minutes, seconds);
        }
    }

    public struct HorizonCoordinates {
        Angle altitude;
        Angle azimuth;

        public HorizonCoordinates(double altitude, double azimuth)
        {
            this.altitude = new Angle(altitude);
            this.azimuth = new Angle(azimuth);
        }

        public override string ToString() {

            return string.Format("Altitude: {0} | Azimuth: {1}", altitude.DegreesMinutesSeconds.ToString(), azimuth.DegreesMinutesSeconds.ToString());
        }
    }

    ///
    public double ConvertRightAscensionToHourAngle(double rightAsc, TimeSpan lst) {
        var hourAngle = lst.TotalHours - rightAsc;

        if (hourAngle < 0) hourAngle += 24;

        return hourAngle;
    }

    // Hour angle is in decimal hours
    // Declination is in decimal degrees
    public HorizonCoordinates ConvertEquatorialToHorizon(double hourAngle, double declination, double latitude) {

        var hRadians = hourAngle *  15 * Deg2Rad;
        var decRadians = declination * Deg2Rad;
        var latRadians = latitude * Deg2Rad;

        var altRadians = Math.Sin(decRadians) * Math.Sin(latRadians) + Math.Cos(decRadians) * Math.Cos(latRadians) * Math.Cos(hRadians);
        altRadians = Math.Asin(altRadians);

        var azimuthRadians = (Math.Sin(decRadians) - Math.Sin(latRadians) * Math.Sin(altRadians)) / (Math.Cos(latRadians) * Math.Cos(altRadians));
        azimuthRadians = Math.Acos(azimuthRadians);

        var azimuth = azimuthRadians * Rad2Deg;

        if (Math.Sin(hRadians) >= 0) {
            azimuth = 360 - azimuth;
        }

        return new HorizonCoordinates(altRadians * Rad2Deg, azimuth);
    }

    #endregion
}