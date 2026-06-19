using System.Windows;
using System.Windows.Interop;
using System.Runtime.InteropServices;
using Promplet.Win32;

namespace Promplet.Services;

public sealed class GlobalHotKeyService : IDisposable
{
    private readonly List<GlobalHotKeyRegistrationResult> _registrationResults = [];
    private readonly Dictionary<int, GlobalHotKeyAction> _registeredActions = [];
    private readonly Window _window;
    private IReadOnlyList<GlobalHotKeyDefinition> _definitions;
    private IntPtr _handle;
    private HwndSource? _source;
    private bool _disposed;

    public GlobalHotKeyService(Window window, IReadOnlyList<GlobalHotKeyDefinition>? definitions = null)
    {
        _window = window;
        _definitions = definitions ?? GlobalHotKeyDefinitions.CreateDefault();
        _window.SourceInitialized += Window_SourceInitialized;
        _window.Closed += (_, _) => Dispose();
    }

    public event EventHandler<GlobalHotKeyPressedEventArgs>? HotKeyPressed;

    public IReadOnlyCollection<GlobalHotKeyAction> RegisteredActions => _registeredActions.Values;

    public IReadOnlyList<GlobalHotKeyRegistrationResult> RegistrationResults => _registrationResults;

    public void ApplyHotKeys(IReadOnlyList<GlobalHotKeyDefinition> definitions)
    {
        _definitions = definitions;

        if (_handle == IntPtr.Zero)
        {
            return;
        }

        UnregisterHotKeys();
        RegisterHotKeys();
    }

    private void Window_SourceInitialized(object? sender, EventArgs e)
    {
        _handle = new WindowInteropHelper(_window).Handle;
        _source = HwndSource.FromHwnd(_handle);
        _source?.AddHook(WindowProcedure);
        RegisterHotKeys();
    }

    private void RegisterHotKeys()
    {
        _registrationResults.Clear();
        _registeredActions.Clear();

        foreach (var hotkey in _definitions)
        {
            var registered = NativeMethods.RegisterHotKey(
                _handle,
                hotkey.Id,
                (uint)hotkey.Modifiers,
                (uint)hotkey.VirtualKey);

            if (registered)
            {
                _registeredActions[hotkey.Id] = hotkey.Action;
                _registrationResults.Add(new GlobalHotKeyRegistrationResult(hotkey, true, 0));
            }
            else
            {
                _registrationResults.Add(new GlobalHotKeyRegistrationResult(
                    hotkey,
                    false,
                    Marshal.GetLastWin32Error()));
            }
        }
    }

    private void UnregisterHotKeys()
    {
        foreach (var id in _registeredActions.Keys.ToArray())
        {
            NativeMethods.UnregisterHotKey(_handle, id);
        }

        _registeredActions.Clear();
        _registrationResults.Clear();
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
        UnregisterHotKeys();
        _window.SourceInitialized -= Window_SourceInitialized;
    }
}

public sealed record GlobalHotKeyRegistrationResult(
    GlobalHotKeyDefinition Definition,
    bool IsRegistered,
    int ErrorCode);

public sealed class GlobalHotKeyPressedEventArgs : EventArgs
{
    public GlobalHotKeyPressedEventArgs(GlobalHotKeyAction action)
    {
        Action = action;
    }

    public GlobalHotKeyAction Action { get; }
}
