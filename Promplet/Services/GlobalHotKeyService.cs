using System.Windows;
using System.Windows.Interop;
using Promplet.Win32;

namespace Promplet.Services;

public sealed class GlobalHotKeyService : IDisposable
{
    private readonly Dictionary<int, GlobalHotKeyAction> _registeredActions = [];
    private readonly Window _window;
    private IntPtr _handle;
    private HwndSource? _source;
    private bool _disposed;

    public GlobalHotKeyService(Window window)
    {
        _window = window;
        _window.SourceInitialized += Window_SourceInitialized;
        _window.Closed += (_, _) => Dispose();
    }

    public event EventHandler<GlobalHotKeyPressedEventArgs>? HotKeyPressed;

    public IReadOnlyCollection<GlobalHotKeyAction> RegisteredActions => _registeredActions.Values;

    private void Window_SourceInitialized(object? sender, EventArgs e)
    {
        _handle = new WindowInteropHelper(_window).Handle;
        _source = HwndSource.FromHwnd(_handle);
        _source?.AddHook(WindowProcedure);
        RegisterDefaultHotKeys();
    }

    private void RegisterDefaultHotKeys()
    {
        foreach (var hotkey in GlobalHotKeyDefinitions.CreateDefault())
        {
            var registered = NativeMethods.RegisterHotKey(
                _handle,
                hotkey.Id,
                (uint)hotkey.Modifiers,
                (uint)hotkey.VirtualKey);

            if (registered)
            {
                _registeredActions[hotkey.Id] = hotkey.Action;
            }
        }
    }

    private IntPtr WindowProcedure(
        IntPtr hwnd,
        int message,
        IntPtr wParam,
        IntPtr lParam,
        ref bool handled)
    {
        if (message == NativeMethods.WM_HOTKEY && _registeredActions.TryGetValue(wParam.ToInt32(), out var action))
        {
            handled = true;
            HotKeyPressed?.Invoke(this, new GlobalHotKeyPressedEventArgs(action));
        }

        return IntPtr.Zero;
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        _source?.RemoveHook(WindowProcedure);

        foreach (var id in _registeredActions.Keys.ToArray())
        {
            NativeMethods.UnregisterHotKey(_handle, id);
        }

        _registeredActions.Clear();
        _window.SourceInitialized -= Window_SourceInitialized;
    }
}

public sealed class GlobalHotKeyPressedEventArgs : EventArgs
{
    public GlobalHotKeyPressedEventArgs(GlobalHotKeyAction action)
    {
        Action = action;
    }

    public GlobalHotKeyAction Action { get; }
}
