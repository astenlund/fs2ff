// ReSharper disable InconsistentNaming

using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Interop;

namespace fs2ff
{
    public partial class MainWindow
    {
        private const uint WM_USER_SIMCONNECT = 0x0402;

        public MainWindow()
        {
            InitializeComponent();
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);

            HwndSource hwndSource = (HwndSource) PresentationSource.FromVisual(this)!;
            hwndSource.AddHook(WndProc);
            ((MainViewModel) DataContext).SetWindowHandle(hwndSource.Handle);
        }

        private IntPtr WndProc(IntPtr hWnd, int iMsg, IntPtr hWParam, IntPtr hLParam, ref bool bHandled)
        {
            if (iMsg == WM_USER_SIMCONNECT)
            {
                ((MainViewModel) DataContext).ReceiveFlightSimMessage();
            }

            return IntPtr.Zero;
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            ((MainViewModel) DataContext).Dispose();
        }
    }
}
