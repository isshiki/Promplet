using System.Runtime.InteropServices;
using Promplet.Win32;

namespace Promplet.Services;

public sealed class KeyboardInputService
{
    public void SendCtrlV()
    {
        var inputs = new[]
        {
            KeyDown(NativeMethods.VK_CONTROL),
            KeyDown(NativeMethods.VK_V),
            KeyUp(NativeMethods.VK_V),
            KeyUp(NativeMethods.VK_CONTROL)
        };

        var sent = NativeMethods.SendInput(
            (uint)inputs.Length,
            inputs,
            Marshal.SizeOf<NativeMethods.INPUT>());

        if (sent != inputs.Length)
        {
            throw new InvalidOperationException("Failed to send Ctrl+V.");
        }
    }

    private static NativeMethods.INPUT KeyDown(ushort key)
    {
        return new NativeMethods.INPUT
        {
            type = NativeMethods.INPUT_KEYBOARD,
            union = new NativeMethods.INPUTUNION
            {
                keyboardInput = new NativeMethods.KEYBDINPUT
                {
                    virtualKey = key
                }
            }
        };
    }

    private static NativeMethods.INPUT KeyUp(ushort key)
    {
        var input = KeyDown(key);
        input.union.keyboardInput.flags = NativeMethods.KEYEVENTF_KEYUP;
        return input;
    }
}
