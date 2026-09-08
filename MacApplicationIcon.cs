using System.Runtime.InteropServices;

namespace Dtdash;

public static class MacApplicationIcon
{
    public static void Set()
    {
        if (!OperatingSystem.IsMacOS()) return;

        try
        {
            var path = Path.Combine(AppContext.BaseDirectory, "redicon.png");
            if (!File.Exists(path)) return;

            var nsApplication = MsgSend(GetClass("NSApplication"), Selector("sharedApplication"));
            var image = MsgSend(
                MsgSend(GetClass("NSImage"), Selector("alloc")),
                Selector("initWithContentsOfFile:"),
                CreateString(path));
            if (image != IntPtr.Zero)
                MsgSend(nsApplication, Selector("setApplicationIconImage:"), image);
        }
        catch
        {
        }
    }

    private static IntPtr CreateString(string value)
    {
        var bytes = Marshal.StringToCoTaskMemUTF8(value);
        try
        {
            return MsgSend(GetClass("NSString"), Selector("stringWithUTF8String:"), bytes);
        }
        finally
        {
            Marshal.FreeCoTaskMem(bytes);
        }
    }

    private static IntPtr GetClass(string name) => objc_getClass(name);
    private static IntPtr Selector(string name) => sel_registerName(name);

    [DllImport("/usr/lib/libobjc.A.dylib")]
    private static extern IntPtr objc_getClass(string name);

    [DllImport("/usr/lib/libobjc.A.dylib")]
    private static extern IntPtr sel_registerName(string name);

    [DllImport("/usr/lib/libobjc.A.dylib", EntryPoint = "objc_msgSend")]
    private static extern IntPtr MsgSend(IntPtr receiver, IntPtr selector);

    [DllImport("/usr/lib/libobjc.A.dylib", EntryPoint = "objc_msgSend")]
    private static extern IntPtr MsgSend(IntPtr receiver, IntPtr selector, IntPtr arg1);
}
