// ReSharper disable InconsistentNaming

using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Interop;
using fs2ff.SimConnect;

namespace fs2ff
{
    public partial class MainWindow
    {
        private const uint WM_USER_SIMCONNECT = 0x0402;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            ((ISimConnectMessageHandler) DataContext).Dispose();
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            HwndSource hwndSource = (HwndSource) PresentationSource.FromVisual(this)!;
            hwndSource.AddHook(WndProc);
            ((ISimConnectMessageHandler) DataContext).WindowHandle = hwndSource.Handle;
        }

        private IntPtr WndProc(IntPtr hWnd, int iMsg, IntPtr hWParam, IntPtr hLParam, ref bool bHandled)
        {
            if (iMsg == WM_USER_SIMCONNECT)
            {
                ((ISimConnectMessageHandler) DataContext).ReceiveFlightSimMessage();
            }

            return IntPtr.Zero;
        }
    }
}
