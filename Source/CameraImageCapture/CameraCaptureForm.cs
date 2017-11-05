using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using OpenCvSharp;
using System.Timers;

namespace CameraImageCapture
{
    public partial class CameraCaptureForm : Form
    {
        private VideoCapture capture;
        private Mat image;
        private System.Timers.Timer captureTimer;
        private System.Timers.Timer flashTimer;
        private ConfigInfo configInfo;
        private String saveImageName;
        private String savaImageFullPath;
        private Mat flashImage;
        private double flashAlpha = 0.0;
        private DateTime flashStartTime;
        

        public CameraCaptureForm()
        {
            InitializeComponent();
        }

        private ConfigInfo LoadConfigInfo()
        {
            ConfigInfo configInfo = null;

            //XmlSerializerオブジェクトを作成
            System.Xml.Serialization.XmlSerializer serializer = new System.Xml.Serialization.XmlSerializer(typeof(ConfigInfo));
            //読み込むファイルを開く
            System.IO.StreamReader sr = new System.IO.StreamReader(ConfigInfo.ConfigFileName, new System.Text.UTF8Encoding(false));
            //XMLファイルから読み込み、逆シリアル化する
            configInfo = (ConfigInfo)serializer.Deserialize(sr);
            //ファイルを閉じる
            sr.Close();

            return configInfo;
        }

        private void SaveConfigInfo(ConfigInfo obj)
        {
            //XmlSerializerオブジェクトを作成
            //オブジェクトの型を指定する
            System.Xml.Serialization.XmlSerializer serializer = new System.Xml.Serialization.XmlSerializer(typeof(ConfigInfo));
            //書き込むファイルを開く（UTF-8 BOM無し）
            System.IO.StreamWriter sw = new System.IO.StreamWriter(ConfigInfo.ConfigFileName, false, new System.Text.UTF8Encoding(false));
            //シリアル化し、XMLファイルに保存する
            serializer.Serialize(sw, obj);
            //ファイルを閉じる
            sw.Close();
        }

        private void CameraImageDisp_TimersTimer(object sender, ElapsedEventArgs e)
        {
            capture.Read(image);
            Mat tmp = new Mat();
            Cv2.AddWeighted(image, 1.0, flashImage, flashAlpha, 0, tmp); 

            pictureBox1.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(tmp);

            GC.Collect();
        }

        private void FlashEffect_TimersTimer(object sender, ElapsedEventArgs e)
        {
            TimeSpan tmp = DateTime.Now - flashStartTime;

            // 500ミリ秒でフラッシュエフェクト終了
            flashAlpha = 1.0 - tmp.TotalMilliseconds/500.0;
            if(flashAlpha < 0)
            {
                flashAlpha = 0;
                flashTimer.Stop();
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            SaveImage();
        }

        private void SaveImage()
        {
            // フラッシュエフェクト
            flashAlpha = 1.0;
            flashTimer.Start();
            flashStartTime = DateTime.Now;

            // キャプチャの保存
            saveImageName = "image_" + DateTime.Now.ToString("yyyyMMddHHmmss") + "." + configInfo.extension;
            savaImageFullPath = configInfo.saveImagePath + "\\" + saveImageName;
            image.SaveImage(savaImageFullPath);

            // 取得したキャプチャの表示
            this.thumbnailImage.SizeMode = PictureBoxSizeMode.Zoom;
            this.thumbnailImage.ImageLocation = savaImageFullPath;
        }

        /// <summary>
        /// アクティブへ遷移時
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CameraCaptureForm_Activated(object sender, EventArgs e)
        {
            // タイマー開始
            if(captureTimer != null)
            {
                captureTimer.Start();
            }
        }

        /// <summary>
        /// 非アクティブへ遷移時
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CameraCaptureForm_Deactivate(object sender, EventArgs e)
        {
            // タイマー停止
            if (captureTimer != null)
            {
                captureTimer.Stop();
            }
                
        }

        private void CameraCaptureForm_Shown(object sender, EventArgs e)
        {
            // configファイル読み込み
            configInfo = LoadConfigInfo();
            if (configInfo == null)
            {
                MessageBox.Show("設定ファイルを読み込めませんでした。");
                this.Close();
            }
            SaveConfigInfo(configInfo);

            capture = new VideoCapture(0);
            capture.FrameWidth = configInfo.frameWidth;
            capture.FrameHeight = configInfo.frameHeihgt;
            capture.Fps = configInfo.fps;

            //pictureBox1.Width = configInfo.frameWidth;
            //pictureBox1.Height = configInfo.frameHeihgt;
            //this.Height = configInfo.frameHeihgt + 300;
            //this.Width = configInfo.frameWidth + 40;

            image = new Mat();
            flashImage = new Mat(new OpenCvSharp.Size(configInfo.frameWidth, configInfo.frameHeihgt), MatType.CV_8UC3, new Scalar(255, 255, 255));

            // タイマーの生成
            captureTimer = new System.Timers.Timer();
            captureTimer.Elapsed += new ElapsedEventHandler(CameraImageDisp_TimersTimer);
            captureTimer.Interval = 1000.0d / configInfo.fps;

            // フラッシュエフェクト用タイマー
            flashTimer = new System.Timers.Timer();
            flashTimer.Elapsed += new ElapsedEventHandler(FlashEffect_TimersTimer);
            flashTimer.Interval = 1000.0d / 30.0d;

            // タイマーを開始
            captureTimer.Start();
        }

        private void CameraCaptureForm_KeyDown(object sender, KeyEventArgs e)
        {
            //F1キーが押されたか調べる
            if (e.KeyData == Keys.Enter)
            {
                SaveImage();
            }
                
        }
    }

}
