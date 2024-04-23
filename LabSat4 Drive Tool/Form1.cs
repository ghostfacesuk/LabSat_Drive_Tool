using System;
using System.Diagnostics;
using System.Management;
using System.Threading;
using System.Windows.Forms;

namespace LabSat4_Drive_Tool
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            PopulateDriveList();
            CheckAndCopyExt2fsd();
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
            if (comboBox1.SelectedItem != null)
            {
                string? selectedDiskDrive = comboBox1.SelectedItem.ToString();

                // Display a warning message
                DialogResult result = MessageBox.Show($"Are you sure you want to format: \n{selectedDiskDrive} as EXT4?\n\nAll data will be lost.",
                                                       "Warning",
                                                       MessageBoxButtons.YesNo,
                                                       MessageBoxIcon.Warning);

                if (result == DialogResult.Yes)
                {
                    label1.Text = "Please wait, drive is formatting...";

                    if (selectedDiskDrive != null)
                    {
                        // Initialize the disk using diskpart
                        RunDiskPart($"select disk {GetDiskNumber(selectedDiskDrive)}", "clean");

                        // Wait for 2 seconds
                        Thread.Sleep(2000);

                        // Format the disk as EXT4 using mke2fs.exe
                        RunCommand($"mke2fs.exe -t ext4 -cc PHYSICALDRIVE{GetDiskNumber(selectedDiskDrive)}");

                        // Wait for 5 seconds
                        Thread.Sleep(5000);

                        label1.Text = "Please remove drive and connect to LabSat4";

                        MessageBox.Show($"Selected drive {selectedDiskDrive} has been formatted as EXT4.\nPlease disconnect and connect to LabSat4");
                    }
                }
            }
            else
            {
                MessageBox.Show("Please select a disk drive first.");
            }
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

            Process cmdProcess = new Process { StartInfo = cmdProcessInfo };
            cmdProcess.Start();

            // Send command to cmd.exe
            cmdProcess.StandardInput.WriteLine(command);
            cmdProcess.StandardInput.WriteLine("exit");

            // Wait for the process to finish
            cmdProcess.WaitForExit();

            // Optionally, read the output (standard or error)
            string output = cmdProcess.StandardOutput.ReadToEnd();

            // After process ends
            cmdProcess.Close();

            // Update the UI or log based on the output or the fact the process has finished
            Console.WriteLine(output);
            MessageBox.Show("Formatting completed.");
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

        }
    }
}
