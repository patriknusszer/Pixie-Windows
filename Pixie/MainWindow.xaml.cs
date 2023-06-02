using System;
using System.Windows;
using System.Windows.Media;
using System.ComponentModel;
using System.Threading;
using System.Runtime.InteropServices;
using Microsoft.Win32;

namespace NussPosition
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 
    public partial class MainWindow : Window
    {
        [DllImport("user32.dll")]
        public static extern int GetAsyncKeyState(Int32 vKey);

        [DllImport("user32.dll")]
        private static extern bool SetCursorPos(Int32 X, Int32 Y);

        [DllImport("user32.dll")]
        static extern bool GetCursorPos(out POINT lpPoint);

        [DllImport("user32.dll")]
        static extern IntPtr GetDC(IntPtr hwnd);

        [DllImport("user32.dll")]
        static extern Int32 ReleaseDC(IntPtr hwnd, IntPtr hdc);

        [DllImport("gdi32.dll")]
        static extern uint GetPixel(IntPtr hdc, int nXPos, int nYPos);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool SystemParametersInfo(uint uiAction, uint uiParam, int vParam, uint fWinIni);

        /*[DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool SystemParametersInfo(SPI uiAction, uint uiParam, ref T pvParam, SPIF fWinIni); // T = any type */

        [StructLayout(LayoutKind.Sequential)]

        public struct POINT
        {
            public int X;

            public int Y;
        }

        const uint SPI_SETCURSORS = 0x0057;

        const uint SPIF_SENDCHANGE = 0x02;

        const uint SPIF_UPDATEINIFILE = 0x01;

        BackgroundWorker backgroundworkerWorker = new BackgroundWorker();

        POINT pointMousePosition = new POINT();

        int intSleepAmount = 1;

        IntPtr hdc = GetDC(IntPtr.Zero);

        public MainWindow()
        {
            InitializeComponent();
            backgroundworkerWorker.DoWork += backgroundworkerWorker_DoWork;
            backgroundworkerWorker.ProgressChanged += backgroundworkerWorker_ProgressChanged;
            backgroundworkerWorker.WorkerReportsProgress = true;
            backgroundworkerWorker.WorkerSupportsCancellation = true;
            backgroundworkerWorker.RunWorkerAsync();
        }

        public void backgroundworkerWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            while (true)
            {
                if (GetAsyncKeyState(122) == -32767)
                {
                    while (true)
                    {
                        if (GetAsyncKeyState(123) == -32767)
                        {
                            break;
                        }
                    }
                }

                GetCursorPos(out pointMousePosition);

                backgroundworkerWorker.ReportProgress(0);

                Thread.Sleep(intSleepAmount);
            }
        }

        public void backgroundworkerWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            textboxX.Text = pointMousePosition.X.ToString();

            textboxY.Text = pointMousePosition.Y.ToString();

            if (checkboxPickColor.IsChecked == true)
            {
                intSleepAmount = 20;

                checkboxUsePen.IsEnabled = true;
                uint uintColor = GetPixel(hdc, pointMousePosition.X, pointMousePosition.Y);
                byte[] bytearrayColor = BitConverter.GetBytes(uintColor);
                textboxR.Text = Convert.ToInt16(bytearrayColor[0]).ToString();
                textboxG.Text = Convert.ToInt16(bytearrayColor[1]).ToString();
                textboxB.Text = Convert.ToInt16(bytearrayColor[2]).ToString();
                textboxDec.Text = uintColor.ToString();
                string stringHex = null;

                for (int i = 0; i <= 2; i++ )
                {
                    stringHex += Convert.ToString((uint)bytearrayColor[i], 16);
                }

                textboxHex.Text = stringHex;
                Color colorColor = Color.FromRgb(bytearrayColor[0], bytearrayColor[1], bytearrayColor[2]);
                rectangleColor.Fill = new SolidColorBrush(colorColor);
            }
            else if (checkboxPickColor.IsChecked == false)
            {
                checkboxUsePen.IsEnabled = false;

                intSleepAmount = 1;
            }
        }

        private void buttonGoToPosition_Click(object sender, RoutedEventArgs e)
        {
            if (!String.IsNullOrWhiteSpace(textboxSetX.Text) && !String.IsNullOrWhiteSpace(textboxSetY.Text))
            {
                SetCursorPos(Convert.ToInt32(textboxSetX.Text), Convert.ToInt32(textboxSetY.Text));
            }
        }

        private void checkboxUsePen_Checked(object sender, RoutedEventArgs e)
        {
            Registry.SetValue(@"HKEY_CURRENT_USER\Control Panel\Cursors\", "Arrow", @"%SystemRoot%\cursors\aero_pen.cur");
            SystemParametersInfo(SPI_SETCURSORS, 0, 0, SPIF_SENDCHANGE);
        }

        private void checkboxUsePen_Unchecked(object sender, RoutedEventArgs e)
        {
            Registry.SetValue(@"HKEY_CURRENT_USER\Control Panel\Cursors\", "Arrow", @"%SystemRoot%\cursors\aero_arrow.cur");
            SystemParametersInfo(SPI_SETCURSORS, 0, 0, SPIF_SENDCHANGE);
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);

            if (checkboxUsePen.IsChecked == true)
            {
                Registry.SetValue(@"HKEY_CURRENT_USER\Control Panel\Cursors\", "Arrow", @"%SystemRoot%\cursors\aero_arrow.cur");
                SystemParametersInfo(SPI_SETCURSORS, 0, 0, SPIF_SENDCHANGE | SPIF_UPDATEINIFILE);
                ReleaseDC(IntPtr.Zero, hdc);
            }
        }
    }
}
