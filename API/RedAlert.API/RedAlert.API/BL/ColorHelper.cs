using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Web;

namespace RedAlert.API.BL
{
    public static class ColorHelper
    {

        public static int TotalResponse =0;
        public static int YesAnswer=0;
        public static int NoAnswer=0;

        public static string GetColorFromPercent(int percent)
        {
            if (TotalResponse != 0)
            {
                var red = (255*percent)/100;
                var green = (255*(100 - percent))/100;
                var blue = 0;
                var colorFromRgb = Color.FromArgb(red, green, blue);
                var colorHex = GetHexFromRgb(colorFromRgb);
                return colorHex;
            }
            else
            {
                return GetHexFromRgb(Color.FromArgb(128,128,0));
            }

        }

        public static string GetHexFromRgb(Color color)
        {
            string hex = color.R.ToString("X2") + color.G.ToString("X2") + color.B.ToString("X2");
            return hex;
        }

        public static int GetPercentFromAnswers()
        {
            if (TotalResponse > 0)
            {
                double percent = (double)NoAnswer / TotalResponse * 100;
                return (int)percent;
            }
            else
            {
                return 0;
            }
            












        }

        public static string CreateColorFromPercent()
        {
           var percent= GetPercentFromAnswers();
           return GetColorFromPercent(percent);
        }
    }
}