using Avalonia;
using Avalonia.Input;
using System.Runtime.InteropServices;

namespace Dtdash;

public static class GlobalHotkey
{
    public static IDisposable Register(Action callback)
    {
        if (OperatingSystem.IsMacOS()) return MacHotkey.Register(callback);
        if (OperatingSystem.IsWindows()) return WindowsHotkey.Register(callback);
        return new NoopHotkey();
    }

    private sealed class NoopHotkey : IDisposable
    {
        public void Dispose() { }
    }

    private static class WindowsHotkey
    {
        private const int WmHotKey = 0x0312;
        private const int ModControl = 0x0002;
        private const int ModShift = 0x0004;
        private const int VkN = 0x4E;
        private const int HotkeyId = 1;
        private static readonly WndProc WindowProc = OnWindowMessage;

        public static IDisposable Register(Action callback)
        {
            var handle = GetMainWindowHandle();
            if (handle == IntPtr.Zero || !RegisterHotKey(handle, HotkeyId, ModControl | ModShift, VkN))
                return new NoopHotkey();

            var previous = SetWindowLongPtr(handle, -4, Marshal.GetFunctionPointerForDelegate(WindowProc));
            return new HotkeyRegistration(handle, previous, callback);
        }

        private static IntPtr OnWindowMessage(IntPtr handle, uint message, IntPtr wParam, IntPtr lParam)
        {
            if (registrations.TryGetValue(handle, out var active))
            {
                if (message == WmHotKey && wParam.ToInt32() == HotkeyId)
                    active.Invoke();
                return CallWindowProc(active.Previous, handle, message, wParam, lParam);
            }
            return DefWindowProc(handle, message, wParam, lParam);
        }

        private static readonly Dictionary<IntPtr, HotkeyRegistration> registrations = new();

        private sealed class HotkeyRegistration : IDisposable
        {
            public IntPtr Previous { get; }
            private readonly IntPtr _handle;
            private readonly Action _callback;

            public HotkeyRegistration(IntPtr handle, IntPtr previous, Action callback)
            {
                _handle = handle;
                Previous = previous;
                _callback = callback;
                registrations[handle] = this;
            }

            public void Dispose()
            {
                UnregisterHotKey(_handle, HotkeyId);
                SetWindowLongPtr(_handle, -4, Previous);
                registrations.Remove(_handle);
            }

            public void Invoke() => _callback();
        }

        private static IntPtr GetMainWindowHandle()
        {
            return App.Current?.ApplicationLifetime is Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime desktop
                && desktop.MainWindow?.TryGetPlatformHandle()?.Handle is { } handle ? handle : IntPtr.Zero;
        }

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        private delegate IntPtr WndProc(IntPtr handle, uint message, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", SetLastError = true)] private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint modifiers, uint key);
        [DllImport("user32.dll", SetLastError = true)] private static extern bool UnregisterHotKey(IntPtr hWnd, int id);
        [DllImport("user32.dll")] private static extern IntPtr CallWindowProc(IntPtr previous, IntPtr handle, uint message, IntPtr wParam, IntPtr lParam);
        [DllImport("user32.dll")] private static extern IntPtr DefWindowProc(IntPtr handle, uint message, IntPtr wParam, IntPtr lParam);
        [DllImport("user32.dll", EntryPoint = "SetWindowLongPtrW")] private static extern IntPtr SetWindowLongPtr(IntPtr handle, int index, IntPtr value);
    }

    private static class MacHotkey
    {
        private const uint KeyN = 45;
        private const uint ControlKey = 1 << 12;
        private const uint ShiftKey = 1 << 9;
        private const uint KeyboardEventClass = 0x6B657962;
        private const uint HotKeyPressedEvent = 5;

        private static readonly EventHandlerProc Handler = OnHotkey;

        public static IDisposable Register(Action callback)
        {
            var target = GetApplicationEventTarget();
            var eventTypes = new[]
            {
                new EventTypeSpec { EventClass = KeyboardEventClass, EventKind = HotKeyPressedEvent }
            };
            var status = InstallEventHandler(target, Handler, 1, eventTypes, IntPtr.Zero, out var handlerRef);
            if (status != 0) return new NoopHotkey();

            var id = new EventHotKeyId { Signature = 0x64744473, Id = 1 };
            status = RegisterEventHotKey(KeyN, ControlKey | ShiftKey, id, target, 0, out var hotkeyRef);
            if (status != 0)
            {
                RemoveEventHandler(handlerRef);
                return new NoopHotkey();
            }

            HotkeyRegistration registration = new(hotkeyRef, handlerRef, callback);
            activeRegistration = registration;
            return registration;
        }

        private static HotkeyRegistration? activeRegistration;

        private static int OnHotkey(IntPtr nextHandler, IntPtr eventRef, IntPtr userData)
        {
            activeRegistration?.Callback();
            return 0;
        }

        private sealed class HotkeyRegistration : IDisposable
        {
            private readonly IntPtr _hotkeyRef;
            private readonly IntPtr _handlerRef;
            public Action Callback { get; }

            public HotkeyRegistration(IntPtr hotkeyRef, IntPtr handlerRef, Action callback)
            {
                _hotkeyRef = hotkeyRef;
                _handlerRef = handlerRef;
                Callback = callback;
            }

            public void Dispose()
            {
                UnregisterEventHotKey(_hotkeyRef);
                RemoveEventHandler(_handlerRef);
                if (ReferenceEquals(activeRegistration, this)) activeRegistration = null;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct EventTypeSpec
        {
            public uint EventClass;
            public uint EventKind;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct EventHotKeyId
        {
            public uint Signature;
            public uint Id;
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int EventHandlerProc(IntPtr nextHandler, IntPtr eventRef, IntPtr userData);

        [DllImport("/System/Library/Frameworks/Carbon.framework/Carbon")]
        private static extern IntPtr GetApplicationEventTarget();

        [DllImport("/System/Library/Frameworks/Carbon.framework/Carbon")]
        private static extern int InstallEventHandler(IntPtr target, EventHandlerProc handler, uint eventCount, EventTypeSpec[] eventTypes, IntPtr userData, out IntPtr handlerRef);

        [DllImport("/System/Library/Frameworks/Carbon.framework/Carbon")]
        private static extern int RemoveEventHandler(IntPtr handlerRef);

        [DllImport("/System/Library/Frameworks/Carbon.framework/Carbon")]
        private static extern int RegisterEventHotKey(uint keyCode, uint modifiers, EventHotKeyId hotKeyId, IntPtr target, uint options, out IntPtr hotkeyRef);

        [DllImport("/System/Library/Frameworks/Carbon.framework/Carbon")]
        private static extern int UnregisterEventHotKey(IntPtr hotkeyRef);
    }
}
