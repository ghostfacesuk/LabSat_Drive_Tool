using System;
using System.Diagnostics;
using System.IO;
using System.Management;
using System.Windows.Forms;

namespace LabSat4_Drive_Tool
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            PopulateDriveList();
        }

        private void PopulateDriveList()
        {
            ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_DiskDrive");
            foreach (ManagementObject drive in searcher.Get())
            {
                comboBox1.Items.Add(drive["DeviceID"]);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (comboBox1.SelectedIndex != -1)
            {
                string selectedDiskDrive = comboBox1.SelectedItem.ToString();

                string mke2fsPath = @"C:\Program Files\Ext2Fsd\mke2fs.exe";

                if (File.Exists(mke2fsPath))
                {
                    ProcessStartInfo processInfo = new ProcessStartInfo
                    {
                        FileName = mke2fsPath,
                        Arguments = $"-t ext4 {selectedDiskDrive}",
                        UseShellExecute = true,
                        Verb = "runas",
                        CreateNoWindow = true
                    };

                    using (Process process = new Process { StartInfo = processInfo })
                    {
                        process.Start();
                        process.WaitForExit();
                    }

                    MessageBox.Show($"Disk Drive {selectedDiskDrive} has been formatted as EXT4.");
                }
                else
                {
                    MessageBox.Show("Ext2Fsd tools not found. Please check the installation directory.");
                }
            }
            else
            {
                MessageBox.Show("Please select a disk drive first.");
            }
        }

        private bool IsDiskInUse(int physicalDriveNumber)
        {
            try
            {
                using (Process process = new Process())
                {
                    process.StartInfo.FileName = "diskpart.exe";
                    process.StartInfo.Arguments = $"select disk {physicalDriveNumber}";
                    process.StartInfo.UseShellExecute = false;
                    process.StartInfo.RedirectStandardOutput = true;
                    process.Start();
                    process.WaitForExit();

                    string output = process.StandardOutput.ReadToEnd();
                    return output.Contains("Disk is being used by");
                }
            }
            catch (Exception)
            {
                return true; // Assume the disk is in use if an exception occurs
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
    }
}