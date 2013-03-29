using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;

/*
    Copyright (C) 2011-2013 Dmitry Brant <me@dmitrybrant.com>
  
    This software is free software; you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation; either version 2 of the License, or
    (at your option) any later version.
  
    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.
  
    You should have received a copy of the GNU General Public License along
    with this program; if not, write the Free Software Foundation, Inc., 51
    Franklin Street, Fifth Floor, Boston, MA 02110-1301 USA.

 */
namespace ExifRename
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            this.Text = Application.ProductName;
            FixDialogFont(this);
        }

        public static void FixDialogFont(Control c0)
        {
            Font old = c0.Font;
            c0.Font = new Font(SystemFonts.MessageBoxFont.FontFamily.Name, old.Size, old.Style);
            if (c0.Controls.Count > 0)
                foreach (Control c in c0.Controls)
                    FixDialogFont(c);
        }



        private void RenameDir(string directory, bool recurse)
        {
            try
            {

                var files = Directory.GetFiles(directory, "*.*");
                string pattern = txtPattern.Text;
                string tempStr;
                char[] splitChars = { ':', ' ' };

                for (int i = 0; i < files.Length; i++)
                {
                    try
                    {
                        lblStatus.Text = "Processing " + (i + 1).ToString() + " of " + files.Length.ToString();
                        Application.DoEvents();

                        if (!files[i].ToLower().Contains(".jp") && !files[i].ToLower().Contains(".mpo"))
                            continue;

                        string extension = Path.GetExtension(files[i]);
                        string newName = "";
                        {
                            Image img = new Bitmap(files[i]);
                            foreach (var p in img.PropertyItems)
                            {
                                if (p.Id == 0x9003)
                                {
                                    string[] poo = Encoding.ASCII.GetString(p.Value).Replace("\0", "").Split(splitChars);
                                    if (poo.Length < 6) break;

                                    tempStr = pattern;
                                    tempStr = tempStr.Replace("[YYYY]", poo[0]);
                                    tempStr = tempStr.Replace("[MM]", poo[1]);
                                    tempStr = tempStr.Replace("[DD]", poo[2]);
                                    tempStr = tempStr.Replace("[hh]", poo[3]);
                                    tempStr = tempStr.Replace("[mm]", poo[4]);
                                    tempStr = tempStr.Replace("[ss]", poo[5]);

                                    newName = Path.GetDirectoryName(files[i]);
                                    newName += "\\" + tempStr;
                                    break;
                                }
                            }
                            img.Dispose();
                        }
                        if (newName != "")
                        {
                            if (files[i] != newName)
                            {
                                for (int k = 1; k < 10; k++)
                                {
                                    if (File.Exists(newName + extension))
                                        newName += "_";
                                    else break;
                                }
                                newName += extension;
                                File.Move(files[i], newName);
                            }
                        }

                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(this, "Failed to process image " + files[i] + ":\n" + ex.Message, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    }
                }

                if (recurse)
                {
                    var dirs = Directory.GetDirectories(directory);
                    foreach (var dir in dirs)
                    {
                        RenameDir(dir, recurse);
                    }
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }


        private void button1_Click(object sender, EventArgs e)
        {
            btnDo.Enabled = false;
            RenameDir(txtPhotoDir.Text, chkRecurse.Checked);
            btnDo.Enabled = true;

            lblStatus.Text = "Ready...";
        }

        private void txtPhotoDir_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop, false) == true)
                e.Effect = DragDropEffects.All;
        }

        private void txtPhotoDir_DragDrop(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (files.Length == 0) return;
            txtPhotoDir.Text = files[0];
        }

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            var dlg = new FolderBrowserDialog();
            dlg.Description = "Select folder with photos to be renamed:";
            if (dlg.ShowDialog(this) != DialogResult.OK) return;
            txtPhotoDir.Text = dlg.SelectedPath;
        }

        private void lnkAbout_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            MessageBox.Show(this, "ExifRename - a simple photo renaming tool that uses EXIF timestamps.\n\nA Dmitry Brant joint.\nhttp://dmitrybrant.com", "About...", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }


    }
}
