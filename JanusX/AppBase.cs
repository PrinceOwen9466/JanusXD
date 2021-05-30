using JanusXD.Shell.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JanusXD.Shell
{
    public static class AppBase
    {
        public static string PRODUCT_NAME = "Hexacom API";
        public static string SHORT_PRODUCT_NAME = "Hexacom";
        public static string PRODUCT_VERSION = "1";
        public static string FULL_PRODUCT_NAME => $"{PRODUCT_NAME} {PRODUCT_VERSION}";
        public static string AUTHOR = "Prince Owen";
        public static string COMPANY = "Dev-Lynx Technologies";

        public readonly static string SYSTEM_DATA_DIR = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        public readonly static string BASE_DIR = Directory.GetCurrentDirectory();
        public readonly static string WORK_DIR = Path.Combine(SYSTEM_DATA_DIR, PRODUCT_NAME + ".Live");
        public readonly static string DATA_DIR = Path.Combine(WORK_DIR, "Data");
        public readonly static string WWWROOT = Path.Combine(SYSTEM_DATA_DIR, "wwwroot");

        static AppBase()
        {
            IOExtensions.CreateDirectories(DATA_DIR);
        }
    }
}
