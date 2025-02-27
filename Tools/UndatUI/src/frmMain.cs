﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace undat_ui
{
    public partial class frmMain : Form
    {
        [DllImport("user32.dll")]
        static extern IntPtr LoadIcon(IntPtr hInstance, IntPtr iconName);
        [DllImport("kernel32.dll")]
        static extern IntPtr GetModuleHandle(string moduleName);

        bool isDone = false;
        bool fromCLI = false;

        public frmMain(string fileList, string master, string moddir)
        {
            InitializeComponent();

            // Use same icon as the one used for .exe
            IntPtr hInstance = GetModuleHandle(null);
            IntPtr hIcon = LoadIcon(hInstance, new IntPtr(32512));
            if (hIcon != IntPtr.Zero) this.Icon = Icon.FromHandle(hIcon);

            if(moddir != null)
                txtDestination.Text = moddir;

            fromCLI = fileList != null;

            if (fromCLI)
            {
                txtDestination.Enabled = false;
                txtMaster.Text = master;
                txtMaster.Enabled = false;
                BeginExtract(() => Environment.Exit(0), () => { txtDestination.Enabled = true; txtMaster.Enabled = true; }, fileList);
            }

        }

        private void MsgError(string error)
            => MessageBox.Show(error, "FO1 DAT extractor", MessageBoxButtons.OK, MessageBoxIcon.Error);

        private void BeginExtract(Action onSuccess, Action onError, string filesPath)
        {
            this.btnBrowseDestination.Enabled = false;
            this.btnBrowseMaster.Enabled = false;
            this.lblExtracting.Visible = true;
            this.progressBar.Value = 0;

            this.lblExtracting.Text = "";
            this.btnExtract.Enabled = false;
            var extract = new Extractor((err) =>
            {
                MessageBox.Show(err, "FO1 DAT extractor", MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.btnExtract.Enabled = true;
            },
            ((currentFile, cur, max) =>
            {
                try
                {
                    this.Invoke((MethodInvoker)delegate
                    {
                        this.progressBar.Value = cur;
                        this.progressBar.Maximum = max;
                        this.lblExtracting.Text = $"[{cur}/{max}] " + currentFile;
                    });
                }
                catch(System.InvalidOperationException)
                {
                    Environment.Exit(1);
                }

                if (cur == max)
                {
                    this.isDone = true;
                    this.Invoke((MethodInvoker)delegate
                    {
                        this.btnExtract.Text = "Done!";
                        this.btnExtract.Enabled = true;
                    });
                }
            }),
            onSuccess,
            onError,
            this.txtMaster.Text,
            filesPath,
            this.txtDestination.Text);

            extract.Begin();
        }

        private void BtnExtract_Click(object sender, EventArgs e)
        {
            if(isDone)
                Environment.Exit(0);

            var undatFilesPath = Misc.ExeDirectory() + "\\undat_files.txt";
            if (!File.Exists(undatFilesPath))
            {
                MsgError("Unable to find " + undatFilesPath);
                return;
            }

            this.BeginExtract(() => {  }, () => { btnBrowseMaster.Enabled = true; btnBrowseDestination.Enabled = true; }, undatFilesPath);
        }
       
        private void BtnBrowseMaster_Click(object sender, EventArgs e)
        {
            openFileDialog.FileName = "";
            openFileDialog.Filter = "Fallout 1 DAT (*.DAT)|*.DAT";
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                txtMaster.Text = openFileDialog.FileName;
            }
        }

        private void BtnBrowseDestination_Click(object sender, EventArgs e)
        {
            if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
            {
                if(!Misc.FalloutExeInFolder(folderBrowserDialog.SelectedPath))
                {
                    if (MessageBox.Show("Fallout2.exe was not found in the selected directory, do you want to select it anyway?", "FO1 DAT extractor", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.No)
                        return;
                }
                txtDestination.Text = folderBrowserDialog.SelectedPath;
            }
        }

        private void lnkRotators_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://github.com/rotators");
        }

        private void lnkSource_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://github.com/rotators/Fo1in2/tree/master/Tools/UndatUI/src");
        }

        private void lnkFodev_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://fodev.net");
        }
    }
}
