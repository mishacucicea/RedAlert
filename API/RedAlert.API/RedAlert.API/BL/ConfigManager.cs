using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

namespace RedAlert.API.BL
{
    public static class ConfigManager
    {
        public static string FacebookClientId { get; } = ConfigurationManager.AppSettings["FacebookClientId"];
        public static string FacebookSecretKey { get; } = ConfigurationManager.AppSettings["FacebookSecretKey"];
        public static string GoogleClientId { get; } = ConfigurationManager.AppSettings["GoogleClientId"];
        public static string GoogleSecretKey { get; } = ConfigurationManager.AppSettings["GoogleSecretKey"];
        public static string MicrosoftClientId { get; } = ConfigurationManager.AppSettings["MicrosoftClientId"];
        public static string MicrosoftSecretKey { get; } = ConfigurationManager.AppSettings["MicrosoftSecretKey"];
      
    }
}