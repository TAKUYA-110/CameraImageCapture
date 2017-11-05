using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CameraImageCapture
{
    /// <summary>
    /// 設定情報
    /// </summary>
    public class ConfigInfo
    {
        public static readonly String ConfigFileName = "config.xml";

        public double fps = 30;
        public int frameWidth = 640;
        public int frameHeihgt = 360;
        public String saveImagePath;
        public String extension;
    }
}
