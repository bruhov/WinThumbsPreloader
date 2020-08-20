using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace WinThumbsPreloader
{
    //Preload one thumbnail
    class ThumbnailPreloader
    {
        private Guid iIdIShellItem;
        private IThumbnailCache TBCache;

        public ThumbnailPreloader()
        {
            iIdIShellItem = new Guid("43826d1e-e718-42ee-bc55-a1e261c37bfe");
            Guid CLSIDLocalThumbnailCache = new Guid("50ef4544-ac9f-4a8e-b21b-8a26180db13f");
            var TBCacheType = Type.GetTypeFromCLSID(CLSIDLocalThumbnailCache);
            TBCache = (IThumbnailCache)Activator.CreateInstance(TBCacheType);
        }

        public void PreloadThumbnail(string filePath)
        {
            IShellItem shellItem = null;
            ISharedBitmap bmp = null;
            WTS_CACHEFLAGS cFlags;
            WTS_THUMBNAILID bmpId;
            try
            {
                SHCreateItemFromParsingName(filePath, IntPtr.Zero, iIdIShellItem, out shellItem);
                TBCache.GetThumbnail(shellItem, 128, WTS_FLAGS.WTS_EXTRACTINPROC, out bmp, out cFlags, out bmpId);
            }
            catch (Exception)
            {
                //Do nothing
            }
            if (bmp != null) Marshal.ReleaseComObject(bmp);
            if (shellItem != null)  Marshal.ReleaseComObject(shellItem);
            bmp = null;
            shellItem = null;
        }

        //Import native functions
        [DllImport("shell32.dll", CharSet = CharSet.Unicode, PreserveSig = false)]
        static extern void SHCreateItemFromParsingName(
            [In][MarshalAs(UnmanagedType.LPWStr)] string pszPath,
            [In] IntPtr pbc,
            [In][MarshalAs(UnmanagedType.LPStruct)] Guid riid,
            [Out][MarshalAs(UnmanagedType.Interface, IidParameterIndex = 2)] out IShellItem ppv);

        [ComImport]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        [Guid("F676C15D-596A-4ce2-8234-33996F445DB1")]
        interface IThumbnailCache
        {
            uint GetThumbnail(
                [In] IShellItem pShellItem,
                [In] uint cxyRequestedThumbSize,
                [In] WTS_FLAGS flags /*default:  WTS_FLAGS.WTS_EXTRACT*/,
                [Out][MarshalAs(UnmanagedType.Interface)] out ISharedBitmap ppvThumb,
                [Out] out WTS_CACHEFLAGS pOutFlags,
                [Out] out WTS_THUMBNAILID pThumbnailID
            );

            void GetThumbnailByID(
                [In, MarshalAs(UnmanagedType.Struct)] WTS_THUMBNAILID thumbnailID,
                [In] uint cxyRequestedThumbSize,
                [Out][MarshalAs(UnmanagedType.Interface)] out ISharedBitmap ppvThumb,
                [Out] out WTS_CACHEFLAGS pOutFlags
            );
        }

        [Flags]
        enum WTS_FLAGS : uint
        {
            WTS_EXTRACT = 0x00000000,
            WTS_INCACHEONLY = 0x00000001,
            WTS_FASTEXTRACT = 0x00000002,
            WTS_SLOWRECLAIM = 0x00000004,
            WTS_FORCEEXTRACTION = 0x00000008,
            WTS_EXTRACTDONOTCACHE = 0x00000020,
            WTS_SCALETOREQUESTEDSIZE = 0x00000040,
            WTS_SKIPFASTEXTRACT = 0x00000080,
            WTS_EXTRACTINPROC = 0x00000100
        }

        [Flags]
        enum WTS_CACHEFLAGS : uint
        {
            WTS_DEFAULT = 0x00000000,
            WTS_LOWQUALITY = 0x00000001,
            WTS_CACHED = 0x00000002
        }

        [StructLayout(LayoutKind.Sequential, Size = 16), Serializable]
        struct WTS_THUMBNAILID
        {
            [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 16)]
            byte[] rgbKey;
        }

        [ComImport]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        [Guid("43826d1e-e718-42ee-bc55-a1e261c37bfe")]
        public interface IShellItem
        {
            void BindToHandler(IntPtr pbc,
                [MarshalAs(UnmanagedType.LPStruct)]Guid bhid,
                [MarshalAs(UnmanagedType.LPStruct)]Guid riid,
                out IntPtr ppv);

            void GetParent(out IShellItem ppsi);

            void GetDisplayName(SIGDN sigdnName, out IntPtr ppszName);

            void GetAttributes(uint sfgaoMask, out uint psfgaoAttribs);

            void Compare(IShellItem psi, uint hint, out int piOrder);
        };

        public enum SIGDN : uint
        {
            NORMALDISPLAY = 0,
            PARENTRELATIVEPARSING = 0x80018001,
            PARENTRELATIVEFORADDRESSBAR = 0x8001c001,
            DESKTOPABSOLUTEPARSING = 0x80028000,
            PARENTRELATIVEEDITING = 0x80031001,
            DESKTOPABSOLUTEEDITING = 0x8004c000,
            FILESYSPATH = 0x80058000,
            URL = 0x80068000
        }

        [ComImportAttribute()]
        [GuidAttribute("091162a4-bc96-411f-aae8-c5122cd03363")]
        [InterfaceTypeAttribute(ComInterfaceType.InterfaceIsIUnknown)]
        public interface ISharedBitmap
        {
            uint Detach(
                [Out] out IntPtr phbm
            );

            uint GetFormat(
                [Out]  out WTS_ALPHATYPE pat
            );

            uint GetSharedBitmap(
                [Out] out IntPtr phbm
            );

            uint GetSize(
                [Out, MarshalAs(UnmanagedType.Struct)] out SIZE pSize
            );

            uint InitializeBitmap(
                [In]  IntPtr hbm,
                [In]  WTS_ALPHATYPE wtsAT
            );
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct SIZE
        {
            public int cx;
            public int cy;

            public SIZE(int cx, int cy)
            {
                this.cx = cx;
                this.cy = cy;
            }
        }

        public enum WTS_ALPHATYPE : uint
        {
            WTSAT_UNKNOWN = 0,
            WTSAT_RGB = 1,
            WTSAT_ARGB = 2
        }
    }
}
