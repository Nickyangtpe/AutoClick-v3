using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;
using MouseMovementLibraries.ddxoftSupport;

namespace AutoClick_v3
{
    public partial class MainForm : Form
    {
        private LoadForm loadForm;
        private List<Timer> timers = new List<Timer>(); // Timer 列表
        private bool isRunning = false; // 控制計時器啟動或停止的標誌

        public MainForm(bool load, ProgressBar progressBar, Label label, LoadForm loadFormInstance)
        {
            InitializeComponent();
            loadForm = loadFormInstance;

            if (load)
            {
                Hide();
                InitializeAsync(progressBar, label);
            }

            // 設置全局鍵盤掛鉤
            _proc = HookCallback;
            _hookID = SetHook(_proc);
        }

        private async void InitializeAsync(ProgressBar progressBar, Label label)
        {
            if (await DdxoftMain.Load(progressBar, label))
            {
                await Task.Delay(100);
                Show();
            }
            else
            {
                loadForm.CloseForm();
            }
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            loadForm.HideForm();
        }

        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            UnhookWindowsHookEx();
            loadForm.CloseForm();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (int.TryParse(textBox1.Text, out int CPS) && CPS >= 301)
            {
                MessageBox.Show("速度最高限制為300");
                return;
            }

            if (CPS >= 100)
            {
                DialogResult result = MessageBox.Show("您確定要執行次操作？速度過快可能會導致系統反應不過來而造成延遲。", "警告", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

                if (result != DialogResult.Yes)
                {
                    return;
                }
            }
            

            StopAllTimers(); // 清空舊的 Timer
            timers.Clear();

            if (int.TryParse(textBox1.Text, out int interval) && interval > 0)
            {
                int tickInterval = 1000 / interval; // 設定每個 Timer 的間隔

                // 根據 trackBar1 的值創建 Timer
                for (int i = 0; i < trackBar1.Value; i++)
                {
                    Timer timer = new Timer();
                    timer.Interval = tickInterval;
                    timer.Tick += (s, ev) => ClickBTN();
                    timers.Add(timer);
                    timer.Start();
                }
            }
            else
            {
                MessageBox.Show("請輸入正確數字", "AutoClick v3", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            StopAllTimers();
        }

        private void ClickBTN()
        {
            DdxoftMain.ddxoftInstance.btn(1);
            DdxoftMain.ddxoftInstance.btn(1);
        }

        // API 函數和常量
        private const int WH_KEYBOARD_LL = 13;
        private const int WM_KEYDOWN = 0x0100;
        private LowLevelKeyboardProc _proc;
        private IntPtr _hookID = IntPtr.Zero;

        // 設置鍵盤掛鉤
        private IntPtr SetHook(LowLevelKeyboardProc proc)
        {
            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule curModule = curProcess.MainModule)
            {
                return SetWindowsHookEx(WH_KEYBOARD_LL, proc,
                    GetModuleHandle(curModule.ModuleName), 0);
            }
        }

        // 解除鍵盤掛鉤
        private void UnhookWindowsHookEx()
        {
            UnhookWindowsHookEx(_hookID);
        }

        // 鍵盤回調函數
        private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0 && wParam == (IntPtr)WM_KEYDOWN)
            {
                int vkCode = Marshal.ReadInt32(lParam);
                if (vkCode == (int)Keys.F8)
                {
                    ToggleTimers(); // 切換計時器狀態
                }
            }
            return CallNextHookEx(_hookID, nCode, wParam, lParam);
        }

        private void ToggleTimers()
        {
            if (isRunning)
            {
                StopAllTimers();
            }
            else
            {
                StartAllTimers();
            }
            isRunning = !isRunning;
        }

        private void StartAllTimers()
        {
            foreach (var timer in timers)
            {
                timer.Start();
            }
        }

        private void StopAllTimers()
        {
            foreach (var timer in timers)
            {
                timer.Stop();
            }
        }



        // API 函數定義
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn,
            IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);
    }
}
