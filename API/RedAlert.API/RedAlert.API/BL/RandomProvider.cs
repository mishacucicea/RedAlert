using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace RedAlert.API.BL
{
    public class RandomProvider
    {
        private static Random rand;

        private string alphanumericSet = "abcdefghijklmnopqrstwuvxyz0123456789";

        static RandomProvider()
        {
            rand = new Random();
        }

        public string GetAlphaNumeric(int length)
        {
            StringBuilder sb = new StringBuilder(length);

            for (int i = 0; i < length; i++)
            {
                sb.Append(alphanumericSet[rand.Next(alphanumericSet.Length)]);
            }

            return sb.ToString();
        }
    }
}