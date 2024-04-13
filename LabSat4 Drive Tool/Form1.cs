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
                    // Run as administrator
                    ProcessStartInfo cmdProcessInfo = new ProcessStartInfo
                    {
                        FileName = "cmd.exe",
                        RedirectStandardInput = true,
                        UseShellExecute = false,
                        CreateNoWindow = true,
                        Verb = "runas"  // Run the command prompt as administrator
                    };

                    Process cmdProcess = new Process { StartInfo = cmdProcessInfo };
                    cmdProcess.Start();

                    // Iterate over selected disk drives
                    ManagementObjectSearcher searcher = new ManagementObjectSearcher($"SELECT * FROM Win32_DiskDrive WHERE Caption='{selectedDiskDrive}'");
                    foreach (ManagementObject drive in searcher.Get())
                    {
                        string? physicalDriveNumber = drive["Index"].ToString();

                        // Run mke2fs.exe command within the command prompt
                        cmdProcess.StandardInput.WriteLine($"mke2fs.exe -t ext4 PHYSICALDRIVE{physicalDriveNumber}");
                        cmdProcess.StandardInput.WriteLine();  // Send ENTER key press to confirm
                        cmdProcess.StandardInput.Flush();
                    }

                    // Close input stream to allow cmd.exe to exit
                    cmdProcess.StandardInput.Close();

                    // Wait for cmd.exe process to exit
                    cmdProcess.WaitForExit();

                    MessageBox.Show($"Selected drive {selectedDiskDrive} has been formatted as EXT4.\nPlease disconnect and connect to LabSat4");
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
