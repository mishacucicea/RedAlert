using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Web;
using QRCoder;

namespace RedAlert.API.BL
{
    public class QrCoderGenerator
    {
        private readonly QRCodeGenerator _qrGenerator;

        public QrCoderGenerator()
        {
            _qrGenerator = new QRCodeGenerator();
        }

        public Bitmap GetQrCodeFromString(string encodedMessage)
        {
            QRCodeData qrCodeData = _qrGenerator.CreateQrCode(encodedMessage, QRCodeGenerator.ECCLevel.Q);
            QRCode qrCode = new QRCode(qrCodeData);
            return  qrCode.GetGraphic(20);
        }
    }
}