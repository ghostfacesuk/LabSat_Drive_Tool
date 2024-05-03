using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Management;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

namespace LabSat4_Drive_Tool
{
    public partial class Form1 : Form
    {
        private BackgroundWorker backgroundWorker = new BackgroundWorker();

        public Form1()
        {
            InitializeComponent();
            PopulateDriveList();
            CheckAndCopyExt2fsd();

            backgroundWorker.DoWork += BackgroundWorker_DoWork;
            backgroundWorker.RunWorkerCompleted += BackgroundWorker_RunWorkerCompleted;
        }

        private void CheckAndCopyExt2fsd()
        {
            string sourceFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ext2fsd.sys");
            string destinationFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "drivers", "ext2fsd.sys");

            if (!File.Exists(destinationFilePath))
            {
                try
                {
                    File.Copy(sourceFilePath, destinationFilePath);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error copying ext2fsd.sys: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void PopulateDriveList()
        {
            ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_DiskDrive");
            foreach (ManagementObject drive in searcher.Get())
            {
                comboBox1.Items.Add(drive["Caption"]);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string? selectedDiskDrive = comboBox1.SelectedItem?.ToString();
            if (selectedDiskDrive != null)
            {
                DialogResult result = MessageBox.Show($"Are you sure you want to format: \n{selectedDiskDrive} as EXT4?\n\nAll data will be lost.",
                                                       "Warning",
                                                       MessageBoxButtons.YesNo,
                                                       MessageBoxIcon.Warning);

                if (result == DialogResult.Yes)
                {
                    label1.Text = "Please wait, drive is formatting...";
                    backgroundWorker.RunWorkerAsync(selectedDiskDrive);
                }
            }
            else
            {
                MessageBox.Show("Please select a disk drive first.");
            }
        }

        private void BackgroundWorker_DoWork(object? sender, DoWorkEventArgs e)
        {
            string? selectedDiskDrive = e.Argument as string;
            if (selectedDiskDrive != null)
            {
                RunDiskPart($"select disk {GetDiskNumber(selectedDiskDrive)}", "clean");
                Thread.Sleep(2000); // Consider replacing with a more robust synchronization method.
                // Updated mke2fs command to fully initialize the disk during formatting
                RunCommand($"mke2fs.exe -t ext4 -E lazy_itable_init=0,lazy_journal_init=0 PHYSICALDRIVE{GetDiskNumber(selectedDiskDrive)}");
                EjectDrive($"PHYSICALDRIVE{GetDiskNumber(selectedDiskDrive)}");
            }
        }

        private void BackgroundWorker_RunWorkerCompleted(object? sender, RunWorkerCompletedEventArgs e)
        {
            label1.Text = "Formatting complete. Please remove drive.";
            MessageBox.Show("Formatting completed. Please disconnect and connect to LabSat4");
        }

        private int GetDiskNumber(string driveCaption)
        {
            ManagementObjectSearcher searcher = new ManagementObjectSearcher($"SELECT * FROM Win32_DiskDrive WHERE Caption='{driveCaption}'");
            foreach (ManagementObject drive in searcher.Get())
            {
                return Convert.ToInt32(drive["Index"]);
            }
            return -1;
        }

        private void RunDiskPart(params string[] commands)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = "diskpart.exe",
                RedirectStandardInput = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                Verb = "runas"
            };

            Process process = new Process { StartInfo = startInfo };
            process.Start();

            foreach (string command in commands)
            {
                process.StandardInput.WriteLine(command);
            }

            process.StandardInput.WriteLine("exit");
            process.WaitForExit();
        }

        private void RunCommand(string command)
        {
            ProcessStartInfo cmdProcessInfo = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                Verb = "runas"
            };

            using (Process cmdProcess = new Process { StartInfo = cmdProcessInfo })
            {
                cmdProcess.Start();
                cmdProcess.StandardInput.WriteLine(command);
                cmdProcess.StandardInput.WriteLine("exit");
                cmdProcess.StandardInput.Close();
                string output = cmdProcess.StandardOutput.ReadToEnd();
                cmdProcess.WaitForExit();
                Console.WriteLine(output);
            }
        }

        private void EjectDrive(string driveLetter)
        {
            // This method should be implemented as discussed, using the CreateFile and DeviceIoControl functions.
            // Implementation details for DeviceIoControl and CreateFile can be complex and require proper understanding of P/Invoke.
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Add any necessary logic here.
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // Add any necessary logic here.
        }

        private void label1_Click(object sender, EventArgs e)
        {
            // Add any necessary logic here.
        }
    }
}
