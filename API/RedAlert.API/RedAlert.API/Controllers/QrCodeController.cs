using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using RedAlert.API.BL;

namespace RedAlert.API.Controllers
{
    public class QrCodeController : Controller
    {
        // GET: QrCode
        public ActionResult GenerateQr()
        {

            var apiUrl = "http://redalert.com/";
            var token = "sdasfrgrgdfgdf344554dff";
            var message = apiUrl + token;
            var generator = new QrCoderGenerator();

            var image = generator.GetQrCodeFromString(message);

            var bitmapBytes = BitmapToBytes(image); //Convert bitmap into a byte array
            return File(bitmapBytes, "image/jpeg");
          
        }

        public ActionResult Index()
        {
            return View();
        }

        private static byte[] BitmapToBytes(Bitmap img)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                img.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
                return stream.ToArray();
            }
        }
    }
}