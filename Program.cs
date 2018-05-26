using System;

class Program
{
    static AstroCalc calc = new AstroCalc();
    static void Main(string[] args)
    {
        var testDate = new DateTime(2009, 6, 19, 18, 0, 0, DateTimeKind.Utc);
        Console.WriteLine("Test Date = {0}", testDate.ToString("HH:mm dd/MM/yyyy"));

        var jd = calc.CalculateJulianDate(testDate);
        Console.WriteLine("JD = {0}", jd);
        
        var utc = calc.CalculateUTCFromJulianDate(jd);
        Console.WriteLine("UTC Date = {0}", utc.ToString("HH:mm dd/MM/yyyy"));

        var testDate2 = new DateTime(1980, 4, 22, 14, 36, 51, 670, DateTimeKind.Utc);
        Console.WriteLine("Test Date = {0}", testDate2.ToString("HH:mm:ss:ffff dd/MM/yyyy"));

        var gst = calc.ConvertDateTimeToGST(testDate2);
        Console.WriteLine("G Sidereal Time = {0}", gst.ToString("G"));
        
        var lst = calc.ConvertGSTToLST(gst, -64);
        Console.WriteLine("L Sidereal Time = {0}", lst.ToString("G"));
        
        var gst2 = calc.ConvertLSTToGST(lst, -64);
        Console.WriteLine("G Sidereal Time = {0}", gst2.ToString("G"));
        
        var hor = calc.ConvertEquatorialToHorizon(5.862222, 23.219444, 52.0);
        Console.WriteLine("Horizon Coord = {0}", hor.ToString());

        var d = new DateTime(2003, 7, 27, 0, 0, 0, DateTimeKind.Utc);
        Console.WriteLine("{0}", d.DayOfYear);
        var t = calc.DayNumberSince2010Epoch(d);
        Console.WriteLine("{0}", t);
    }
}