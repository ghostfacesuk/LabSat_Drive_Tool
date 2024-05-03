using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Management;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using FormsTimer = System.Windows.Forms.Timer; // This line disambiguates the Timer

namespace LabSat4_Drive_Tool
{
    public partial class Form1 : Form
    {
        private BackgroundWorker backgroundWorker = new BackgroundWorker();
        private FormsTimer progressTimer = new FormsTimer(); // Use the alias
        private int progressStep = 0;

        public Form1()
        {
            InitializeComponent();
            PopulateDriveList();
            CheckAndCopyExt2fsd();

            backgroundWorker.WorkerReportsProgress = true;
            backgroundWorker.DoWork += BackgroundWorker_DoWork;
            backgroundWorker.ProgressChanged += BackgroundWorker_ProgressChanged;
            backgroundWorker.RunWorkerCompleted += BackgroundWorker_RunWorkerCompleted;

            progressTimer.Interval = 10000; // Update progress every second
            progressTimer.Tick += ProgressTimer_Tick;  // Ensure this is correctly attached

            progressBar1.Maximum = 100;
            progressBar1.Style = ProgressBarStyle.Continuous;
        }

        private void ProgressTimer_Tick(object sender, EventArgs e)
        {
            progressStep += 2;  // Incrementing by 2% each tick
            if (progressStep > 100)
            {
                progressStep = 100;
            }
            backgroundWorker.ReportProgress(progressStep);
        }

        private void PopulateDriveList()
        {
            int attempts = 0;
            bool populated = false;

            while (!populated && attempts < 5)
            {
                ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_DiskDrive");
                foreach (ManagementObject drive in searcher.Get())
                {
                    string model = drive["Model"]?.ToString() ?? "Unknown Model";
                    AddDriveToList(model);
                    populated = true;
                }

                if (!populated)
                {
                    System.Threading.Thread.Sleep(1000);  // Wait for 1 second before retrying
                    attempts++;
                }
            }

            if (!populated)
            {
                MessageBox.Show("Unable to detect drives. Please try reloading.");
            }
        }

        private void AddDriveToList(string driveDescription)
        {
            if (comboBox1.InvokeRequired)
            {
                comboBox1.Invoke(new Action(() => comboBox1.Items.Add(driveDescription)));
            }
            else
            {
                comboBox1.Items.Add(driveDescription);
            }
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
                    progressBar1.Value = 0;
                    progressStep = 0;
                    backgroundWorker.RunWorkerAsync(selectedDiskDrive);
                    progressTimer.Start();
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
                RunCommand($"mke2fs.exe -t ext4 -E lazy_itable_init=0,lazy_journal_init=0 PHYSICALDRIVE{GetDiskNumber(selectedDiskDrive)}");
            }
        }

        private void BackgroundWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            // Check if the progress bar is not disposed and update its value
            if (!progressBar1.IsDisposed)
            {
                progressBar1.Value = Math.Min(e.ProgressPercentage, progressBar1.Maximum);
            }
        }

        private void BackgroundWorker_RunWorkerCompleted(object? sender, RunWorkerCompletedEventArgs e)
        {
            progressTimer.Stop();
            progressBar1.Value = 100;
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
                RedirectStandardError = true, // Redirect standard error to capture any errors
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

                // Read the outputs
                string output = cmdProcess.StandardOutput.ReadToEnd();
                string errors = cmdProcess.StandardError.ReadToEnd();  // Capture any errors

                cmdProcess.WaitForExit();

                // Log the outputs for debugging
                Console.WriteLine("Output: " + output);
                if (!string.IsNullOrEmpty(errors))
                {
                    Console.WriteLine("Errors: " + errors);
                }
            }
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

        private void progressBar1_Click(object sender, EventArgs e)
        {
            // Event handler logic here
        }

        private void button2_Click(object sender, EventArgs e)
        {
            comboBox1.Items.Clear();
            PopulateDriveList();
        }
    }
}
