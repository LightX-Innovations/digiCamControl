﻿#region Licence

// Distributed under MIT License
// ===========================================================
// 
// digiCamControl - DSLR camera remote control open source software
// Copyright (C) 2014 Duka Istvan
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, 
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF 
// MERCHANTABILITY,FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. 
// IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY 
// CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT,
// TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH 
// THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

#endregion

#region

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using CameraControl.Core;
using CameraControl.Core.Classes;
using CameraControl.Core.Interfaces;
using CameraControl.Core.Wpf;
using CameraControl.Devices.Classes;
using Ionic.Zip;

#endregion

namespace CameraControl.Plugins.ExportPlugins
{
    public class ExportToZip : IExportPlugin
    {
        private ProgressWindow dlg = new ProgressWindow();
        private string destfile = "";

        #region Implementation of IExportPlugin

        private string _title;

        public bool Execute()
        {
            if (dlg.IsVisible)
                return true;
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1.Filter = "Zip file|*.zip";
            saveFileDialog1.Title = "Save zip file";
            saveFileDialog1.ShowDialog();

            // If the file name is not an empty string open it for saving.
            if (saveFileDialog1.FileName != "")
            {
                destfile = saveFileDialog1.FileName;
                dlg.Show();
                Thread thread = new Thread(ZipFiles);
                thread.Start(ServiceProvider.Settings.DefaultSession.Files);
            }
            return true;
        }

        public string Title
        {
            get { return "Export to zip"; }
            set { _title = value; }
        }

        #endregion

        private void ZipFiles(object o)
        {
            AsyncObservableCollection<FileItem> items = o as AsyncObservableCollection<FileItem>;
            items = new AsyncObservableCollection<FileItem>(items.Where(file => file.IsChecked));
            dlg.MaxValue = items.Count;
            int i = 0;
            using (ZipFile zip = new ZipFile(destfile))
            {
                foreach (var fileItem in items)
                {
                    dlg.Progress = i;
                    dlg.ImageSource = fileItem.Thumbnail;
                    dlg.Label = Path.GetFileName(fileItem.FileName);
                    zip.AddFile(fileItem.FileName, "");
                    i++;
                    zip.Save(destfile);
                }
            }
            dlg.Hide();
        }
    }
}