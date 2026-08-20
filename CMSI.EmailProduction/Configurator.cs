using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;

namespace CMSI.EmailProduction
{
    internal static class Configurator
    {
        public static string LogPath = ConfigurationManager.AppSettings["LogPath"];
        public static string IP = ConfigurationManager.AppSettings["IP"];
        public static string Port = ConfigurationManager.AppSettings["Port"];
        public static string TempPath = ConfigurationManager.AppSettings["TempFolder"];
        public static string Secret = ConfigurationManager.AppSettings["Secret"];
        public static string GPGPath = ConfigurationManager.AppSettings["GPGPath"];
        public static string BCCPath = ConfigurationManager.AppSettings["BCCPath"];
        public static string BatchDirectory = ConfigurationManager.AppSettings["BatchDirectory"];
        public static string BCCListDirectory = ConfigurationManager.AppSettings["BCCListDirectory"];
        public static string BCCJobDirectory = ConfigurationManager.AppSettings["BCCJobDirectory"];
        public static string BCCSystemId = ConfigurationManager.AppSettings["BCCSystemId"];
        public static string PrinterDuplex = ConfigurationManager.AppSettings["PrinterDuplex"];
        public static string PrinterSimplex = ConfigurationManager.AppSettings["PrinterSimplex"];
        public static string PdfPrinter = ConfigurationManager.AppSettings["PdfPrinter"];
        public static string ReportPath = ConfigurationManager.AppSettings["ReportPath"];
    }
}
