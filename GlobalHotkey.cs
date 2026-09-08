using Avalonia;
using Avalonia.Input;
using System.Runtime.InteropServices;

namespace Dtdash;

public static class GlobalHotkey
{
    public static IDisposable Register(Action callback)
    {
        if (OperatingSystem.IsMacOS()) return MacHotkey.Register(callback);
        return new NoopHotkey();
    }

    private sealed class NoopHotkey : IDisposable
    {
        public void Dispose() { }
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
