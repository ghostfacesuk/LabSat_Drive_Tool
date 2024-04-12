using System;
using System.Diagnostics;
using System.Management;
using System.IO;
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

        // Form
        private void Form1_Load(object sender, EventArgs e)
        {

        }

        // Drive Dropdown 
        private void PopulateDriveList()
        {
            ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_DiskDrive");
            foreach (ManagementObject drive in searcher.Get())
            {
                comboBox1.Items.Add(drive["Caption"]);
            }
        }

        // Drive select dropdown 
        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        // Format drive button
        private void button1_Click(object sender, EventArgs e)
        {
            // Check if a drive is selected
            if (comboBox1.SelectedIndex != -1)
            {
                // Get the selected disk drive
                string? selectedDiskDrive = comboBox1.SelectedItem?.ToString();

                // Check if selectedDiskDrive is not null before proceeding
                if (selectedDiskDrive != null)
                {
                    // Get the physical drive number of the selected disk drive
                    ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_DiskDrive WHERE Caption='" + selectedDiskDrive + "'");
                    foreach (ManagementObject drive in searcher.Get())
                    {
                        string? physicalDriveNumber = drive["Index"]?.ToString();

                        // Check if physicalDriveNumber is not null before proceeding
                        if (physicalDriveNumber != null)
                        {
                            // Format the disk drive as EXT4 using mke2fs.exe (provided by Ext2Fsd)
                            ProcessStartInfo processInfo = new ProcessStartInfo
                            {
                                FileName = "cmd.exe",
                                RedirectStandardInput = true,
                                UseShellExecute = false,
                                CreateNoWindow = true,
                                WorkingDirectory = @"C:\Program Files\Ext2Fsd\" // Update the path accordingly
                            };

                            Process process = new Process { StartInfo = processInfo };
                            process.Start();

                            // Command to format the disk drive as EXT4
                            process.StandardInput.WriteLine($"mke2fs -t ext4 \\\\.\\PhysicalDrive{physicalDriveNumber}");

                            // Close the cmd window after execution
                            process.StandardInput.WriteLine("exit");
                            process.WaitForExit();

                            MessageBox.Show($"Disk Drive {selectedDiskDrive} (Physical Drive {physicalDriveNumber}) has been formatted as EXT4.");
                        }
                        else
                        {
                            MessageBox.Show("Failed to retrieve physical drive number.");
                        }
                    }
                }
                else
                {
                    MessageBox.Show("Please select a disk drive first.");
                }
            }
        }
    }
}
