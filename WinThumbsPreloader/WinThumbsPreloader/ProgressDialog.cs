using System.ComponentModel;
using System.Threading;

namespace WinThumbsPreloader
{
    //Based on https://www.brad-smith.info/blog/archives/523 with a ProgressBar tweak
    /// <summary>
    /// Managed wrapper for the COM ProgressDialog component. Displays a 
    /// dialog box to track the progress of a long-running operation.
    /// </summary>
    public class ProgressDialog : Component
    {

        const string CLSID_ProgressDialog = "{F8383852-FCD3-11d1-A6B9-006097DF5BD4}";
        const string IDD_ProgressDialog = "EBBC7C04-315E-11d2-B62F-006097DF5BD4";

        Type _progressDialogType;
        IProgressDialog _nativeProgressDialog;
        string _title;
        string _cancelMessage;
        string _line1;
        string _line2;
        string _line3;
        int _maximum;
        int _value;
        bool _compactPaths;
        PROGDLG _flags;
        ProgressDialogState _state;
        bool _autoClose;
        IntPtr _progressBarHandler = IntPtr.Zero;

        /// <summary>
        /// Gets or sets whether the progress dialog is automatically closed when the Value property equals or exceeds the value of Maximum.
        /// </summary>
        [DefaultValue(true), Category("Behaviour"), Description("whether the progress dialog is automatically closed when the Value property equals or exceeds the value of Maximum.")]
        public bool AutoClose
        {
            get
            {
                return _autoClose;
            }
            set
            {
                _autoClose = value;
            }
        }
        /// <summary>
        /// Gets the current state of the progress dialog.
        /// </summary>
        [ReadOnly(true), Browsable(false)]
        public ProgressDialogState State
        {
            get
            {
                return _state;
            }
        }
        /// <summary>
        /// Gets or sets the title displayed on the progress dialog.
        /// </summary>
        [DefaultValue("Working..."), Category("Appearance"), Description("Indicates the title displayed on the progress dialog.")]
        public string Title
        {
            get
            {
                return _title;
            }
            set
            {
                _title = value;
                if (_nativeProgressDialog != null) _nativeProgressDialog.SetTitle(_title);
            }
        }
        /// <summary>
        /// Gets or sets the message to be displayed when the user clicks the cancel button on the progress dialog.
        /// </summary>
        [DefaultValue("Aborting..."), Category("Appearance"), Description("Indicates the message to be displayed when the user clicks the cancel button on the progress dialog.")]
        public string CancelMessage
        {
            get
            {
                return _cancelMessage;
            }
            set
            {
                _cancelMessage = value;
                if (_nativeProgressDialog != null) _nativeProgressDialog.SetCancelMsg(_cancelMessage, null);
            }
        }
        /// <summary>
        /// Gets or sets whether to have path strings compacted if they are too large to fit on a line.
        /// </summary>
        [DefaultValue(false), Category("Appearance"), Description("Indicates whether to have path strings compacted if they are too large to fit on a line.")]
        public bool CompactPaths
        {
            get
            {
                return _compactPaths;
            }
            set
            {
                bool diff = (_compactPaths != value);
                _compactPaths = value;

                if (diff && (_nativeProgressDialog != null))
                {
                    _nativeProgressDialog.SetLine(1, _line1, _compactPaths, IntPtr.Zero);
                    _nativeProgressDialog.SetLine(2, _line1, _compactPaths, IntPtr.Zero);
                    _nativeProgressDialog.SetLine(3, _line1, _compactPaths, IntPtr.Zero);
                }
            }
        }
        /// <summary>
        /// Gets or sets the text displayed on the first line of the progress dialog.
        /// </summary>
        [DefaultValue(""), Browsable(false)]
        public string Line1
        {
            get
            {
                return _line1;
            }
            set
            {
                if (_state == ProgressDialogState.Stopped)
                    throw new InvalidOperationException("Timer is not running.");
                else if (_nativeProgressDialog != null)
                    _nativeProgressDialog.SetLine(1, _line1 = value, _compactPaths, IntPtr.Zero);
            }
        }
        /// <summary>
        /// Gets or sets the text displayed on the second line of the progress dialog.
        /// </summary>
        [DefaultValue(""), Browsable(false)]
        public string Line2
        {
            get
            {
                return _line2;
            }
            set
            {
                if (_state == ProgressDialogState.Stopped)
                    throw new InvalidOperationException("Timer is not running.");
                else if (_nativeProgressDialog != null)
                    _nativeProgressDialog.SetLine(2, _line2 = value, _compactPaths, IntPtr.Zero);
            }
        }
        /// <summary>
        /// Gets or sets the text displayed on the third line of the progress dialog. This property cannot be set if the ShowTimeRemaining property is set to true.
        /// </summary>
        [DefaultValue(""), Browsable(false)]
        public string Line3
        {
            get
            {
                return _line3;
            }
            set
            {
                if (_state == ProgressDialogState.Stopped)
                    throw new InvalidOperationException("Timer is not running.");
                else if (_nativeProgressDialog != null)
                {
                    if (ShowTimeRemaining)
                        throw new InvalidOperationException("Line3 cannot be set if ShowTimeRemaining is set to true.");
                    else
                        _nativeProgressDialog.SetLine(3, _line3 = value, _compactPaths, IntPtr.Zero);
                }
            }
        }
        /// <summary>
        /// Gets or sets the Value property will be equal to when the operation has completed.
        /// </summary>
        [DefaultValue(100), Category("Behaviour"), Description("Indicates what the Value property will be equal to when the operation has completed.")]
        public int Maximum
        {
            get
            {
                return _maximum;
            }
            set
            {
                _maximum = value;
                if (_state != ProgressDialogState.Stopped) UpdateProgress();
            }
        }
        /// <summary>
        /// Gets or sets a value indicating the proportion of the operation has been completed.
        /// </summary>
        [DefaultValue(0), Browsable(false)]
        public int Value
        {
            get
            {
                return _value;
            }
            set
            {
                _value = value;
                if (_state != ProgressDialogState.Stopped)
                {
                    UpdateProgress();
                    if (_autoClose && (_value >= _maximum)) Close();
                }
            }
        }
        /// <summary>
        /// Gets or sets whether the progress dialog box will be modal to the parent window. By default, a progress dialog box is modeless.
        /// </summary>
        [DefaultValue(false), Category("Behaviour"), Description("Indicates whether the progress dialog box will be modal to the parent window. By default, a progress dialog box is modeless.")]
        public bool Modal
        {
            get
            {
                return (_flags & PROGDLG.Modal) == PROGDLG.Modal;
            }
            set
            {
                if (value)
                    _flags |= PROGDLG.Modal;
                else
                    _flags &= ~PROGDLG.Modal;
            }
        }
        /// <summary>
        /// Gets or sets whether to automatically estimate the remaining time and display the estimate on line 3.
        /// </summary>
        [DefaultValue(true), Category("Behaviour"), Description("Automatically estimate the remaining time and display the estimate on line 3.")]
        public bool ShowTimeRemaining
        {
            get
            {
                return (_flags & PROGDLG.AutoTime) == PROGDLG.AutoTime;
            }
            set
            {
                if (value)
                {
                    _flags &= ~PROGDLG.NoTime;
                    _flags |= PROGDLG.AutoTime;
                }
                else
                {
                    _flags &= ~PROGDLG.AutoTime;
                    _flags |= PROGDLG.NoTime;
                }
            }
        }
        /// <summary>
        /// Gets or sets whether to display a minimize button on the dialog box's caption bar.
        /// </summary>
        [DefaultValue(true), Category("Appearance"), Description("Display a minimize button on the dialog box's caption bar.")]
        public bool MinimizeButton
        {
            get
            {
                return (_flags & PROGDLG.NoMinimize) != PROGDLG.NoMinimize;
            }
            set
            {
                if (value)
                    _flags &= ~PROGDLG.NoMinimize;
                else
                    _flags |= PROGDLG.NoMinimize;
            }
        }
        /// <summary>
        /// Gets or sets whether to display a progress bar on the dialog box.
        /// </summary>
        [DefaultValue(true), Category("Appearance"), Description("Display a progress bar on the dialog box.")]
        public bool ProgressBar
        {
            get
            {
                return (_flags & PROGDLG.NoProgressBar) != PROGDLG.NoProgressBar;
            }
            set
            {
                if (value)
                    _flags &= ~PROGDLG.NoProgressBar;
                else
                    _flags |= PROGDLG.NoProgressBar;
            }
        }
        /// <summary>
        /// Gets or sets whether the operation can be cancelled. You should always show a cancel button unless absolutely necessary
        /// </summary>
        [DefaultValue(true), Category("Behaviour"), Description("Indicates whether the operation can be cancelled. You should always show a cancel button unless absolutely necessary.")]
        public bool CancelButton
        {
            get
            {
                return (_flags & PROGDLG.NoCancel) != PROGDLG.NoCancel;
            }
            set
            {
                if (value)
                {
                    _flags &= ~PROGDLG.NoCancel;
                }
                else
                {
                    if (Environment.OSVersion.Version.Major < 6) throw new NotSupportedException("This option is only available on Windows Vista or greater.");
                    _flags |= PROGDLG.NoCancel;
                }
            }
        }
        /// <summary>
        /// Sets the progress bar to marquee mode. This causes the progress bar to scroll horizontally, similar to a marquee display. Use this when you wish to indicate that progress is being made, but the time required for the operation is unknown.
        /// </summary>
        [DefaultValue(false), Category("Behaviour"), Description("Sets the progress bar to marquee mode.")]
        public bool Marquee
        {
            get
            {
                return (_flags & PROGDLG.MarqueeProgress) == PROGDLG.MarqueeProgress;
            }
            set
            {
                if (value)
                {
                    if (Environment.OSVersion.Version.Major < 6) throw new NotSupportedException("This option is only available on Windows Vista or greater.");
                    _flags |= PROGDLG.MarqueeProgress;
                }
                else
                {
                    _flags &= ~PROGDLG.MarqueeProgress;
                }
                if (_state != ProgressDialogState.Stopped)
                {
                    int style = (int)GetWindowLongPtr(_progressBarHandler, GWL.GWL_STYLE);
                    if (value)
                    {
                        style |= (int)PBS.PBS_MARQUEE;
                    }
                    else
                    {
                        style &= ~(int)PBS.PBS_MARQUEE;
                    }
                    SetWindowLongPtr(_progressBarHandler, GWL.GWL_STYLE, (IntPtr)style);
                    SendMessage(_progressBarHandler, 0x40A, value ? 1 : 0, 0);
                }
            }
        }
        /// <summary>
        /// Gets a value indicating whether the user has cancelled the operation.
        /// </summary>
        [ReadOnly(true), Browsable(false)]
        public bool HasUserCancelled
        {
            get
            {
                if (_nativeProgressDialog != null)
                    return _nativeProgressDialog.HasUserCancelled();
                else
                    return false;
            }
        }

        /// <summary>
        /// Initialises a new instance of the ProgressDialog class, using default values.
        /// </summary>
        public ProgressDialog()
        {
            _progressDialogType = Type.GetTypeFromCLSID(new Guid(CLSID_ProgressDialog));

            // default/initial values
            _autoClose = true;
            _state = ProgressDialogState.Stopped;
            _maximum = 100;
            _line1 = _line2 = _line3 = String.Empty;
            _flags = PROGDLG.Normal | PROGDLG.AutoTime;
            _title = "Working...";
            _cancelMessage = "Aborting...";
        }

        /// <summary>
        /// Initialises a new instance of the ProgressDialog class and adds it to the specified IContainer.
        /// </summary>
        /// <param name="container"></param>
        public ProgressDialog(IContainer container)
            : this()
        {
            container.Add(this);
        }

        /// <summary>
        /// Updates the progress displayed on the dialog box.
        /// </summary>
        private void UpdateProgress()
        {
            _nativeProgressDialog.SetProgress((uint)_value, (uint)_maximum);
        }

        /// <summary>
        /// Displays the progress dialog and starts the timer.
        /// </summary>
        public void Show()
        {
            Show(null);
        }

        /// <summary>
        /// Displays the progress dialog and starts the timer.
        /// </summary>
        /// <param name="parent">The dialog box's parent window.</param>
        public void Show(IWin32Window parent)
        {
            if (_state != ProgressDialogState.Stopped) throw new InvalidOperationException("Timer is already running.");

            if (parent == null) parent = Form.ActiveForm;
            IntPtr handle = (parent == null) ? IntPtr.Zero : parent.Handle;

            _nativeProgressDialog = (IProgressDialog)Activator.CreateInstance(_progressDialogType);
            _nativeProgressDialog.SetCancelMsg(_cancelMessage, null);
            if (ShowTimeRemaining) _nativeProgressDialog.SetLine(3, "Estimating time remaining...", false, IntPtr.Zero);
            //Temporary title for progressbar handler detection
            string guidTitle = Guid.NewGuid().ToString();
            _nativeProgressDialog.SetTitle(guidTitle);
            _nativeProgressDialog.StartProgressDialog(handle, null, _flags, IntPtr.Zero);
            //Workaround to manipulate progressbar style
            IntPtr handler = IntPtr.Zero;
            while (true)
            {
                handler = FindWindow(null, guidTitle);
                if (handler == IntPtr.Zero) Thread.Sleep(25); else break;
            }
            handler = FindWindowEx(handler, IntPtr.Zero, "DirectUIHWND", null);
            IntPtr childHandler = FindWindowEx(handler, IntPtr.Zero, "CtrlNotifySink", null);
            childHandler = FindWindowEx(handler, childHandler, "CtrlNotifySink", null);
            childHandler = FindWindowEx(handler, childHandler, "CtrlNotifySink", null);
            _progressBarHandler = FindWindowEx(childHandler, IntPtr.Zero, "msctls_progress32", null);
            //Real title
            _nativeProgressDialog.SetTitle(_title);
            _value = 0;
            _state = ProgressDialogState.Running;
            _nativeProgressDialog.Timer(PDTIMER.Reset, null);
        }

        /// <summary>
        /// Pauses the timer on the progress dialog.
        /// </summary>
        public void Pause()
        {
            if (_state == ProgressDialogState.Stopped) throw new InvalidOperationException("Timer is not running.");
            if (_state == ProgressDialogState.Running)
            {
                _nativeProgressDialog.Timer(PDTIMER.Pause, null);
                _state = ProgressDialogState.Paused;
            }
        }

        /// <summary>
        /// Resumes the timer on the progress dialog.
        /// </summary>
        public void Resume()
        {
            if (_state != ProgressDialogState.Paused) throw new InvalidOperationException("Timer is not paused.");
            _nativeProgressDialog.Timer(PDTIMER.Resume, null);
            _state = ProgressDialogState.Running;
        }

        /// <summary>
        /// Stops the timer and closes the progress dialog.
        /// </summary>
        public void Close()
        {
            if (_state != ProgressDialogState.Stopped)
            {
                try
                {
                    _nativeProgressDialog.StopProgressDialog();
                }
                catch { }
                _state = ProgressDialogState.Stopped;
            }

            CleanUp();
        }

        /// <summary>
        /// Releases the RCW to the native IProgressDialog component.
        /// </summary>
        private void CleanUp()
        {
            if (_nativeProgressDialog != null)
            {
                if (_state != ProgressDialogState.Stopped)
                {
                    try
                    {
                        _nativeProgressDialog.StopProgressDialog();
                    }
                    catch { }
                }

                Marshal.FinalReleaseComObject(_nativeProgressDialog);
                _nativeProgressDialog = null;
            }

            _state = ProgressDialogState.Stopped;
        }

        /// <summary>
        /// Releases the RCW to the native IProgressDialog component.
        /// </summary>
        /// <param name="disposing"></param>
        protected override void Dispose(bool disposing)
        {
            CleanUp();
            base.Dispose(disposing);
        }

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);
        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr FindWindowEx(IntPtr parentHandle, IntPtr childAfter, string className, string windowTitle);

        [DllImport("user32.dll", EntryPoint = "GetWindowLong")]
        private static extern IntPtr GetWindowLongPtr32(IntPtr hWnd, GWL nIndex);
        [DllImport("user32.dll", EntryPoint = "GetWindowLongPtr")]
        private static extern IntPtr GetWindowLongPtr64(IntPtr hWnd, GWL nIndex);
        // This static method is required because Win32 does not support
        // GetWindowLongPtr directly
        public static IntPtr GetWindowLongPtr(IntPtr hWnd, GWL nIndex)
        {
            if (IntPtr.Size == 8)
                return GetWindowLongPtr64(hWnd, nIndex);
            else
                return GetWindowLongPtr32(hWnd, nIndex);
        }

        [DllImport("user32.dll", EntryPoint = "SetWindowLong")]
        private static extern int SetWindowLong32(IntPtr hWnd, GWL nIndex, int dwNewLong);
        [DllImport("user32.dll", EntryPoint = "SetWindowLongPtr")]
        private static extern IntPtr SetWindowLongPtr64(IntPtr hWnd, GWL nIndex, IntPtr dwNewLong);
        // This static method is required because legacy OSes do not support
        // SetWindowLongPtr 
        public static IntPtr SetWindowLongPtr(IntPtr hWnd, GWL nIndex, IntPtr dwNewLong)
        {
            if (IntPtr.Size == 8)
                return SetWindowLongPtr64(hWnd, nIndex, dwNewLong);
            else
                return new IntPtr(SetWindowLong32(hWnd, nIndex, dwNewLong.ToInt32()));
        }

        public enum GWL
        {
            GWL_WNDPROC = (-4),
            GWL_HINSTANCE = (-6),
            GWL_HWNDPARENT = (-8),
            GWL_STYLE = (-16),
            GWL_EXSTYLE = (-20),
            GWL_USERDATA = (-21),
            GWL_ID = (-12)
        }

        public enum PBS
        {
            PBS_MARQUEE = (8)
        }

        public enum PBM
        {
            PBM_SETMARQUEE = (1034)
        }
        

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        static extern IntPtr SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);

        #region COM Interop

        /// <summary>
        /// Flags that control the operation of the progress dialog box.
        /// </summary>
        [Flags]
        private enum PROGDLG : uint
        {
            /// <summary>
            /// Normal progress dialog box behavior.
            /// </summary>
            Normal = 0x00000000,
            /// <summary>
            /// The progress dialog box will be modal to the window specified by hwndParent. By default, a progress dialog box is modeless.
            /// </summary>
            Modal = 0x00000001,
            /// <summary>
            /// Automatically estimate the remaining time and display the estimate on line 3.
            /// </summary>
            /// <remarks>
            /// If this flag is set, IProgressDialog::SetLine can be used only to display text on lines 1 and 2.
            /// </remarks>
            AutoTime = 0x00000002,
            /// <summary>
            /// Do not show the "time remaining" text.
            /// </summary>
            NoTime = 0x00000004,
            /// <summary>
            /// Do not display a minimize button on the dialog box's caption bar.
            /// </summary>
            NoMinimize = 0x00000008,
            /// <summary>
            /// Do not display a progress bar.
            /// </summary>
            /// <remarks>
            /// Typically, an application can quantitatively determine how much of the operation remains and periodically pass that value to IProgressDialog::SetProgress. The progress dialog box uses this information to update its progress bar. This flag is typically set when the calling application must wait for an operation to finish, but does not have any quantitative information it can use to update the dialog box.
            /// </remarks>
            NoProgressBar = 0x00000010,
            /// <summary>
            /// Sets the progress bar to marquee mode.
            /// </summary>
            /// <remarks>
            /// This causes the progress bar to scroll horizontally, similar to a marquee display. Use this when you wish to indicate that progress is being made, but the time required for the operation is unknown.
            /// </remarks>
            MarqueeProgress = 0x00000020,
            /// <summary>
            /// Do not display a cancel button.
            /// </summary>
            /// <remarks>
            /// The operation cannot be canceled. Use this only when absolutely necessary.
            /// </remarks>
            NoCancel = 0x00000040
        }

        /// <summary>
        /// Flags that indicate the action to be taken by the ProgressDialog.SetTime() method.
        /// </summary>
        private enum PDTIMER : uint
        {
            /// <summary>
            /// Resets the timer to zero. Progress will be calculated from the time this method is called.
            /// </summary>
            Reset = (0x01),
            /// <summary>
            /// Progress has been suspended.
            /// </summary>
            Pause = (0x02),
            /// <summary>
            /// Progress has been resumed.
            /// </summary>
            Resume = (0x03)
        }

        [ComImport, Guid(IDD_ProgressDialog), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface IProgressDialog
        {

            /// <summary>
            /// Starts the progress dialog box.
            /// </summary>
            /// <param name="hwndParent">A handle to the dialog box's parent window.</param>
            /// <param name="punkEnableModless">Reserved. Set to null.</param>
            /// <param name="dwFlags">Flags that control the operation of the progress dialog box.</param>
            /// <param name="pvResevered">Reserved. Set to IntPtr.Zero</param>
            void StartProgressDialog(IntPtr hwndParent, [MarshalAs(UnmanagedType.IUnknown)] object punkEnableModless, PROGDLG dwFlags, IntPtr pvResevered);

            /// <summary>
            /// Stops the progress dialog box and removes it from the screen.
            /// </summary>
            void StopProgressDialog();

            /// <summary>
            /// Sets the title of the progress dialog box.
            /// </summary>
            /// <param name="pwzTitle">A pointer to a null-terminated Unicode string that contains the dialog box title.</param>
            void SetTitle([MarshalAs(UnmanagedType.LPWStr)] string pwzTitle);

            /// <summary>
            /// Specifies an Audio-Video Interleaved (AVI) clip that runs in the dialog box. Note: Note  This method is not supported in Windows Vista or later versions.
            /// </summary>
            /// <param name="hInstAnimation">An instance handle to the module from which the AVI resource should be loaded.</param>
            /// <param name="idAnimation">An AVI resource identifier. To create this value, use the MAKEINTRESOURCE macro. The control loads the AVI resource from the module specified by hInstAnimation.</param>
            void SetAnimation(IntPtr hInstAnimation, ushort idAnimation);

            /// <summary>
            /// Checks whether the user has canceled the operation.
            /// </summary>
            /// <returns>TRUE if the user has cancelled the operation; otherwise, FALSE.</returns>
            /// <remarks>
            /// The system does not send a message to the application when the user clicks the Cancel button.
            /// You must periodically use this function to poll the progress dialog box object to determine
            /// whether the operation has been canceled.
            /// </remarks>
            [PreserveSig]
            [return: MarshalAs(UnmanagedType.Bool)]
            bool HasUserCancelled();

            /// <summary>
            /// Updates the progress dialog box with the current state of the operation.
            /// </summary>
            /// <param name="dwCompleted">An application-defined value that indicates what proportion of the operation has been completed at the time the method was called.</param>
            /// <param name="dwTotal">An application-defined value that specifies what value dwCompleted will have when the operation is complete.</param>
            void SetProgress(uint dwCompleted, uint dwTotal);

            /// <summary>
            /// Updates the progress dialog box with the current state of the operation.
            /// </summary>
            /// <param name="ullCompleted">An application-defined value that indicates what proportion of the operation has been completed at the time the method was called.</param>
            /// <param name="ullTotal">An application-defined value that specifies what value ullCompleted will have when the operation is complete.</param>
            void SetProgress64(ulong ullCompleted, ulong ullTotal);

            /// <summary>
            /// Displays a message in the progress dialog.
            /// </summary>
            /// <param name="dwLineNum">The line number on which the text is to be displayed. Currently there are three lines—1, 2, and 3. If the PROGDLG_AUTOTIME flag was included in the dwFlags parameter when IProgressDialog::StartProgressDialog was called, only lines 1 and 2 can be used. The estimated time will be displayed on line 3.</param>
            /// <param name="pwzString">A null-terminated Unicode string that contains the text.</param>
            /// <param name="fCompactPath">TRUE to have path strings compacted if they are too large to fit on a line. The paths are compacted with PathCompactPath.</param>
            /// <param name="pvResevered"> Reserved. Set to IntPtr.Zero.</param>
            /// <remarks>This function is typically used to display a message such as "Item XXX is now being processed." typically, messages are displayed on lines 1 and 2, with line 3 reserved for the estimated time.</remarks>
            void SetLine(uint dwLineNum, [MarshalAs(UnmanagedType.LPWStr)] string pwzString, [MarshalAs(UnmanagedType.VariantBool)] bool fCompactPath, IntPtr pvResevered);

            /// <summary>
            /// Sets a message to be displayed if the user cancels the operation.
            /// </summary>
            /// <param name="pwzCancelMsg">A pointer to a null-terminated Unicode string that contains the message to be displayed.</param>
            /// <param name="pvResevered">Reserved. Set to NULL.</param>
            /// <remarks>Even though the user clicks Cancel, the application cannot immediately call
            /// IProgressDialog::StopProgressDialog to close the dialog box. The application must wait until the
            /// next time it calls IProgressDialog::HasUserCancelled to discover that the user has canceled the
            /// operation. Since this delay might be significant, the progress dialog box provides the user with
            /// immediate feedback by clearing text lines 1 and 2 and displaying the cancel message on line 3.
            /// The message is intended to let the user know that the delay is normal and that the progress dialog
            /// box will be closed shortly.
            /// It is typically is set to something like "Please wait while ...". </remarks>
            void SetCancelMsg([MarshalAs(UnmanagedType.LPWStr)] string pwzCancelMsg, object pvResevered);

            /// <summary>
            /// Resets the progress dialog box timer to zero.
            /// </summary>
            /// <param name="dwTimerAction">Flags that indicate the action to be taken by the timer.</param>
            /// <param name="pvResevered">Reserved. Set to NULL.</param>
            /// <remarks>
            /// The timer is used to estimate the remaining time. It is started when your application
            /// calls IProgressDialog::StartProgressDialog. Unless your application will start immediately,
            /// it should call Timer just before starting the operation.
            /// This practice ensures that the time estimates will be as accurate as possible. This method
            /// should not be called after the first call to IProgressDialog::SetProgress.</remarks>
            void Timer(PDTIMER dwTimerAction, object pvResevered);
        }

        #endregion
    }

    /// <summary>
    /// Represents the various states in which the ProgressDialog component can be.
    /// </summary>
    public enum ProgressDialogState
    {
        /// <summary>
        /// The progress dialog is not showing.
        /// </summary>
        Stopped,
        /// <summary>
        /// The progress dialog is showing and the timer is running.
        /// </summary>
        Running,
        /// <summary>
        /// The progress dialog is showing and the timer is paused.
        /// </summary>
        Paused
    }
}
