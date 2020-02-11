using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CRT_Injector
{
    public partial class Main : Form
    {
        #region Imports
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr OpenProcess(uint dwDesiredAccess, int bInheritHandle, uint dwProcessId);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern int CloseHandle(IntPtr hObject);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr GetProcAddress(IntPtr hModule, string lpProcName);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr VirtualAllocEx(IntPtr hProcess, IntPtr lpAddress, IntPtr dwSize, uint flAllocationType, uint flProtect);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern int WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] buffer, uint size, int lpNumberOfBytesWritten);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr CreateRemoteThread(IntPtr hProcess, IntPtr lpThreadAttribute, IntPtr dwStackSize, IntPtr lpStartAddress, IntPtr lpParameter, uint dwCreationFlags, IntPtr lpThreadId);
        #endregion

        public Main()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            new Task(() =>
            {
                uint PID = (uint)Process.GetProcessesByName("GTA5")[0].Id;

                if (Inject(PID, "C:\\Target.dll"))
                    Console.Beep(800, 500);
                else
                    MessageBox.Show("Error!", "CRT Injector", MessageBoxButtons.OK, MessageBoxIcon.Warning);

                Application.Exit();

            }).Start();
        }

        private bool Inject(uint PID, string DLL)
        {
            try
            {
                IntPtr pHandle = OpenProcess(1082u, 1, PID);
                IntPtr pAddress = GetProcAddress(GetModuleHandle("kernel32.dll"), "LoadLibraryA");
                IntPtr bAddress = VirtualAllocEx(pHandle, (IntPtr)null, (IntPtr)DLL.Length, 12288u, 64u);

                byte[] DBytes = Encoding.ASCII.GetBytes(DLL);

                _ = WriteProcessMemory(pHandle, bAddress, DBytes, (uint)DBytes.Length, 0) == 0;

                _ = CreateRemoteThread(pHandle, (IntPtr)null, IntPtr.Zero, pAddress, bAddress, 0u, (IntPtr)null) == IntPtr.Zero;

                CloseHandle(pHandle);

                return true; //Sucess!
            }
            catch { return false; }
        }
    }
}
