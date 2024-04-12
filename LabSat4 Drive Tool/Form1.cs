using System;
using System.Diagnostics;
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
                comboBox1.Items.Add(drive["Caption"]);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (comboBox1.SelectedIndex != -1)
            {
                string selectedDiskDrive = comboBox1.SelectedItem.ToString();

                ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_DiskDrive WHERE Caption='" + selectedDiskDrive + "'");
                foreach (ManagementObject drive in searcher.Get())
                {
                    string physicalDriveNumber = drive["Index"].ToString();

                    Console.WriteLine($"Selected Disk Drive: {selectedDiskDrive}");
                    Console.WriteLine($"Physical Drive Number: {physicalDriveNumber}");

                    ProcessStartInfo processInfo = new ProcessStartInfo
                    {
                        FileName = "mke2fs.exe",
                        RedirectStandardInput = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    };

                    Process process = new Process { StartInfo = processInfo };
                    process.Start();

                    Console.WriteLine("mke2fs.exe process started.");

                    process.StandardInput.WriteLine($"-t ext4 PHYSICALDRIVE{physicalDriveNumber}");
                    process.StandardInput.Flush();

                    Console.WriteLine($"Command sent to mke2fs.exe: -t ext4 PHYSICALDRIVE{physicalDriveNumber}");

                    // Wait for the process to complete
                    process.WaitForExit();

                    Console.WriteLine("mke2fs.exe process completed.");

                    MessageBox.Show($"Disk Drive {selectedDiskDrive} (Physical Drive {physicalDriveNumber}) has been formatted as EXT4.");
                }
            }
            else
            {
                MessageBox.Show("Please select a disk drive first.");
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
