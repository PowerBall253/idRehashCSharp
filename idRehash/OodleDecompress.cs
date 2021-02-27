using System;
using System.Runtime.InteropServices;

namespace idRehash
{
    public class LinuxOodle
    {
        [DllImport("liblinoodle.so")]
        public extern static int OodleLZ_Decompress(byte[] srcBuf, int srcLen, byte[] dst, long dstSize, int fuzz, int crc, int verbose, byte dstBase, long e, IntPtr cb, IntPtr cbCtx, IntPtr scratch, long scratchSize, int threadPhase);
    }

    public class WindowsOodle
    {
        [DllImportAttribute("..\\oo2core_8_win64.dll")]
        public extern static int OodleLZ_Decompress(byte[] srcBuf, int srcLen, byte[] dst, long dstSize, int fuzz, int crc, int verbose, byte dstBase, long e, IntPtr cb, IntPtr cbCtx, IntPtr scratch, long scratchSize, int threadPhase);
    }
}