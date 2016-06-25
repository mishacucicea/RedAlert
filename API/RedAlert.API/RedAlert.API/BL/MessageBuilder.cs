using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RedAlert.API.BL
{
    public class MessageBuilder
    {
        private System.Drawing.Color _colorRGB;
        //waves  is the default pattern
        private byte _pattern = 1;
        private uint _timeout = 0;

        public bool TrySetColor(string color)
        {
            try
            {
                _colorRGB = System.Drawing.ColorTranslator.FromHtml("#" + color);
            }
            catch
            {
                _colorRGB = System.Drawing.Color.FromName(color);

                if (_colorRGB.ToArgb() == 0)
                {
                    return false;
                }
            }

            return true;
        }

        public bool TrySetPattern(string pattern)
        {
            if (string.Compare(pattern, "waves", true) == 0)
            {
                //PATTERN_WAVES
                _pattern = 1;
            }
            else
            {
                return false;
            }

            return true;
        }

        public void SetTimeout(uint timeout)
        {
            _timeout = timeout;
        }

        public byte[] GetBytes()
        {
            //1 byte is the type of the message
            //3 bytes is the RGB value
            //1 bytes is the pattern number
            //4 bytes is the timeout
            byte[] message = new byte[9];

            //version 1
            message[0] = 1;

            message[1] = _colorRGB.R;
            message[2] = _colorRGB.G;
            message[3] = _colorRGB.B;

            message[4] = _pattern;

            byte[] timeoutBytes = BitConverter.GetBytes(_timeout);

            //because we're running on a little indian machine
            Array.Reverse(timeoutBytes);
            Array.Copy(timeoutBytes, 0, message, 4, 3);

            return message;
        }
    }
}