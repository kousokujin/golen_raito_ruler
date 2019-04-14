using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Xml.Serialization;

namespace Golden_Raito_ruler
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        //trueだと中線で黄金比になる
        bool isFlameMode = true;

        //縦横
        byte rotation;

        //黄金比
        private double GoldenRaito = 1.618034;

        //ipcサーバ
        private IpcComunication comunication;

        private const int WM_SIZING = 0x214;
        private const int WMSZ_LEFT = 1;
        private const int WMSZ_RIGHT = 2;
        private const int WMSZ_TOP = 3;
        private const int WMSZ_TOPLEFT = 4;
        private const int WMSZ_TOPRIGHT = 5;
        private const int WMSZ_BOTTOM = 6;
        private const int WMSZ_BOTTOMLEFT = 7;
        private const int WMSZ_BOTTOMRIGHT = 8;

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int left;
            public int top;
            public int right;
            public int bottom;
        }

        public MainWindow()
        {        
            rotation = 0;
            InitializeComponent();
            loadSetting();
            resizeWindow();
            //saveSetting();

            IpcServer server = new IpcServer();
            bool server_ok = server.startServer("run");

            if(server_ok == false)
            {
                IpcClient client = new IpcClient();
                client.startClient("stop");
                this.Close();
            }
            else
            {
                server.MessageReceive += eventIpc;
                comunication = server;

            }
        }

        //ipcの値が変わったとき
        void eventIpc(object sender,EventArgs e)
        {
            this.Dispatcher.Invoke((Action)(() =>
            {
                saveSetting();
                this.Close();
            }));
            //Width = 1000;
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            var hwndSource = (HwndSource)HwndSource.FromVisual(this);
            hwndSource.AddHook(WndHookProc);
        }

        //アスペクトを同じにする処理
        private IntPtr WndHookProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == WM_SIZING)
            {
                RECT r = (RECT)Marshal.PtrToStructure(
                                          lParam, typeof(RECT));
                RECT recCopy = r;
                int w = r.right - r.left;
                int h = r.bottom - r.top;
                int dw;
                int dh;

                if (isFlameMode == false)
                {
                    if (rotation % 2 == 0)
                    {
                        dw = (int)(h * GoldenRaito + 0.5) - w;
                        dh = (int)(w / GoldenRaito + 0.5) - h;
                    }
                    else
                    {
                        dw = (int)(h / GoldenRaito + 0.5) - w;
                        dh = (int)(w * GoldenRaito + 0.5) - h;
                    }
                }
                else
                {
                    dw = 0;
                    dh = 0;
                }

                switch (wParam.ToInt32())
                {
                    case WMSZ_TOP:
                    case WMSZ_BOTTOM:
                        r.right += dw;
                        break;
                    case WMSZ_LEFT:
                    case WMSZ_RIGHT:
                        r.bottom += dh;
                        break;
                    case WMSZ_TOPLEFT:
                        if (dw > 0) r.left -= dw;
                        else r.top -= dh;
                        break;
                    case WMSZ_TOPRIGHT:
                        if (dw > 0) r.right += dw;
                        else r.top -= dh;
                        break;
                    case WMSZ_BOTTOMLEFT:
                        if (dw > 0) r.left -= dw;
                        else r.bottom += dh;
                        break;
                    case WMSZ_BOTTOMRIGHT:
                        if (dw > 0) r.right += dw;
                        else r.bottom += dh;
                        break;
                }
                Marshal.StructureToPtr(r, lParam, false);

            }
            return IntPtr.Zero;
        }


        //枠をドラッグしたときにウィンドウ位置を移動させる
        private void Flame_rectangle_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        
        private void Main_flame_Loaded(object sender, RoutedEventArgs e)
        {
            this.SizeToContent = SizeToContent.Manual;
        }

        //とじるボタン
        private void Close_button_Click(object sender, RoutedEventArgs e)
        {
            saveSetting();
            this.Close();
        }

        //モード切り替えボタン
        private void Mode_change_Click(object sender, RoutedEventArgs e)
        {
            isFlameMode = !isFlameMode;
            resizeWindow();
        }

        //枠の大きさが変わったとき
        private void Main_flame_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            grid.Width = this.Width;
            grid.Height = this.Height;
            fix_flame_position();
        }

        //中の線の位置を正しい位置にする
        private void fix_flame_position()
        {
            double shift = (flame_line.StrokeThickness)/2;
            switch (rotation) {
                case 0:
                    flame_line.X1 = (this.Width / GoldenRaito) - shift;
                    flame_line.X2 = (this.Width / GoldenRaito) - shift;
                    flame_line.Y1 = 0;
                    flame_line.Y2 = this.Height;
                    return;
                case 1:
                    flame_line.X1 = 0;
                    flame_line.X2 = this.Width;
                    flame_line.Y1 = (this.Height / GoldenRaito) - shift;
                    flame_line.Y2 = (this.Height / GoldenRaito) - shift;
                    return;
                case 2:
                    flame_line.X1 = (this.Width - (this.Width / GoldenRaito)) - shift;
                    flame_line.X2 = (this.Width - (this.Width / GoldenRaito)) - shift;
                    flame_line.Y1 = 0;
                    flame_line.Y2 = this.Height;
                    return;
                case 3:
                    flame_line.X1 = 0;
                    flame_line.X2 = this.Width;
                    flame_line.Y1 = (this.Height - (this.Height / GoldenRaito)) - shift;
                    flame_line.Y2 = (this.Height - (this.Height / GoldenRaito)) - shift;
                    return;
            }
        }

        //ウィンドウのアスペクト比を正しくする
        private void fixWindowSize()
        {
            if (rotation % 2 == 0)
            {
                this.Width = this.Height * GoldenRaito;
            }
            else
            {
                this.Width = this.Height / GoldenRaito;
            }
        }

        //最大化したら強制的に戻す
        private void Main_flame_StateChanged(object sender, EventArgs e)
        {
            if (this.WindowState == WindowState.Maximized)
                this.WindowState = WindowState.Normal;
        }

        //回転ボタンを押したとき
        private void Rotate_button_Click(object sender, RoutedEventArgs e)
        {
            rotation++;
            rotation %= 4;
            resizeWindow();
        }

        private void resizeWindow()
        {
            if(isFlameMode == false)
            {
                flame_line.Visibility = Visibility.Hidden;
                fixWindowSize();
            }
            else
            {
                flame_line.Visibility = Visibility.Visible;
                fix_flame_position();
            }
        }

        //設定ファイルの保存
        public void saveSetting()
        {
            Point pt = this.PointToScreen(new Point(0.0d, 0.0d));
            int flame = 0;
            if (isFlameMode == true) flame = 1;

            SettingData data = new SettingData
            {
                x = pt.X,
                y = pt.Y,
                width = this.Width,
                height = this.Height,
                FlameMode = flame,
                rotation = this.rotation
            };

            XmlFileIO.xmlSave(data.GetType(), "config.xml", data);
        }

        //設定ファイルの読み込み
        public void loadSetting()
        {
            object data = new SettingData();
            File_Status status =　XmlFileIO.xmlLoad(data.GetType(), "config.xml" ,out data);

            if(status == File_Status.sucsess)
            {
                SettingData castData = (SettingData)data;
                this.Left = castData.x;
                this.Top = castData.y;
                this.Width = castData.width;
                this.Height = castData.height;
                this.rotation = (byte)castData.rotation;

                if(castData.FlameMode == 1)
                {
                    this.isFlameMode = true;
                }
                else
                {
                    this.isFlameMode = false;
                }
            }
        }
    }

    [XmlRoot("SettingData")]
    public class SettingData
    {
        [XmlElement("x")]
        public double x { get; set; }

        [XmlElement("y")]
        public double y { get; set; }

        [XmlElement("width")]
        public double width { get; set; }

        [XmlElement("height")]
        public double height { get; set; }

        [XmlElement("FlameMode")]
        public int FlameMode { get; set; }

        [XmlElement("rotation")]
        public int rotation { get; set; }
    }

}
