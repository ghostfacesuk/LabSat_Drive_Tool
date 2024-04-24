using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Management;
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

                // Sleep is generally not recommended for use in production code; consider alternative synchronization.
                Thread.Sleep(2000);

                RunCommand($"mke2fs.exe -t ext4 PHYSICALDRIVE{GetDiskNumber(selectedDiskDrive)}");
            }
        }

        private void BackgroundWorker_RunWorkerCompleted(object? sender, RunWorkerCompletedEventArgs e)
        {
            label1.Text = "Formatting complete.";
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

                // Send command to cmd.exe
                cmdProcess.StandardInput.WriteLine(command);
                cmdProcess.StandardInput.WriteLine("exit");
                cmdProcess.StandardInput.Close(); // Ensure no more input is going to be sent

                // Read the output as it is produced
                string output = cmdProcess.StandardOutput.ReadToEnd();

                // Wait for the process to finish
                cmdProcess.WaitForExit();

                // After process ends
                Console.WriteLine(output);
                MessageBox.Show("Formatting completed.");
            }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Add any necessary logic here
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // Add any necessary logic here
        }

        private void label1_Click(object sender, EventArgs e)
        {
            // Add any necessary logic here
        }
    }
}
