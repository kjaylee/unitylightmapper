using System;
using System.Text;
using System.Collections;
using System.Runtime.InteropServices;
using UnityEditor;

/*
    Coded for Unity Summer of Code 2009 by Michal Mandrysz
    Feel free to modify for your own needs
    http://masteranza.wordpress.com/unity/lightmapping/
    http://unity3d.com
*/

public class SwitchWindows
{
    const int MAXTITLE = 255;
    private static ArrayList mTitlesList;
    private static string seekFor;
    public static bool found=false;

    private delegate bool EnumDelegate(IntPtr hWnd, int lParam);

    [DllImport("user32.dll", EntryPoint = "EnumDesktopWindows",
     ExactSpelling = false, CharSet = CharSet.Auto, SetLastError = true)]
    private static extern bool _EnumDesktopWindows(IntPtr hDesktop,
    EnumDelegate lpEnumCallbackFunction, IntPtr lParam);

    [DllImport("user32.dll", EntryPoint = "GetWindowText",
     ExactSpelling = false, CharSet = CharSet.Auto, SetLastError = true)]
    private static extern int _GetWindowText(IntPtr hWnd,
    StringBuilder lpWindowText, int nMaxCount);

    [DllImport("user32.dll", SetLastError = true)]
    static extern bool BringWindowToTop(IntPtr hWnd);

	[DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
    static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

    
    private static bool EnumWindowsProc(IntPtr hWnd, int lParam)
    {
        GetWindowText(hWnd);
        return true;
    }
    
    public static void GetWindowText(IntPtr hWnd)
    {   
        StringBuilder title = new StringBuilder(MAXTITLE);
        int titleLength = _GetWindowText(hWnd, title, title.Capacity + 1);
        title.Length = titleLength;

        if (title.ToString().Contains(seekFor))
        {
            ShowWindow(hWnd, 10);
            //normal state resizes the window, so better keep it maximized
            ShowWindow(hWnd, 3);
            BringWindowToTop(hWnd);
            found = true;
        }
    }

    public static bool TurnTo(string app)
    {
        seekFor = app;
        found = false;
        EnumDelegate enumfunc = new EnumDelegate(EnumWindowsProc);
        IntPtr hDesktop = IntPtr.Zero; // current desktop
        bool success = _EnumDesktopWindows(hDesktop, enumfunc, IntPtr.Zero);
        if (success)
        {
            return found;
        }
        else
        {
            int errorCode = Marshal.GetLastWin32Error();
            string errorMessage = String.Format(
            "EnumDesktopWindows failed with code {0}.", errorCode);
            throw new Exception(errorMessage);
        }
    }
}