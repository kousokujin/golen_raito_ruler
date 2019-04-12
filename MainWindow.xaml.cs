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

namespace Golden_Raito_ruler
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        bool isFlameMode = true;
        private double GoldenRaito = 1.618034;

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
            InitializeComponent();
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            var hwndSource = (HwndSource)HwndSource.FromVisual(this);
            hwndSource.AddHook(WndHookProc);
        }

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
                    dw = (int)(h * GoldenRaito + 0.5) - w;
                    dh = (int)(w / GoldenRaito + 0.5) - h;
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

                if(isFlameMode == true)
                {
                    goled_flame.Height = this.Height;
                    goled_flame.Width = this.Width / GoldenRaito;
                }
            }
            return IntPtr.Zero;
        }


        private void Flame_rectangle_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        private void Main_flame_Loaded(object sender, RoutedEventArgs e)
        {
            this.SizeToContent = SizeToContent.Manual;
        }

        private void Close_button_Click_1(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Mode_change_Click(object sender, RoutedEventArgs e)
        {
            isFlameMode = !isFlameMode;
            
            if(isFlameMode == true)
            {
                goled_flame.Visibility = Visibility.Visible;
            }
            else
            {
                goled_flame.Visibility = Visibility.Hidden;
            }
        }
    }

    public class flame_size : INotifyPropertyChanged
    {
        private double _width;
        private double _height;
        public event PropertyChangedEventHandler PropertyChanged;

        public double width
        {
            get
            {
                return _width;
            }
            set
            {
                _width = value;
                OnPropertyChanged("width");
            }
        }

        public double height
        {
            get
            {
                return _height;
            }
            set
            {
                _height = value;
                OnPropertyChanged("Height");
            }
        }

        private void OnPropertyChanged(string propertyName)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
