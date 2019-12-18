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
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using CameraControl.Core;
using CameraControl.Core.Classes;
using CameraControl.Core.Interfaces;
using CameraControl.Devices;
using CameraControl.Devices.Classes;
using MessageBox = System.Windows.MessageBox;

#endregion

namespace CameraControl.Plugins.MainWindowPlugins
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class SimpleMainWindow : IMainWindowPlugin, INotifyPropertyChanged
    {
        public string DisplayName { get; set; }

        private string _saveFolder;

        public string SaveFolder
        {
            get { return _saveFolder; }
            set
            {
                _saveFolder = value;
                NotifyPropertyChanged("SaveFolder");
            }
        }

        public SimpleMainWindow()
        {
            DisplayName = "Simple Capture";
            InitializeComponent();
        }

        private void MetroWindow_Loaded(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(
                "This is a demostration of digiCamControl plugin usage.\nFor normal usage please restart the application and select Default window!");
            SaveFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
            ServiceProvider.DeviceManager.PhotoCaptured += DeviceManager_PhotoCaptured;
        }


        private void DeviceManager_PhotoCaptured(object sender, PhotoCapturedEventArgs eventArgs)
        {
            PhotoCaptured(eventArgs);
            //Thread thread = new Thread(PhotoCaptured);
            //thread.Start(eventArgs);
        }

        private void MetroWindow_Closed(object sender, EventArgs e)
        {
            ServiceProvider.WindowsManager.ExecuteCommand(CmdConsts.All_Close);
        }

        #region Implementation of INotifyPropertyChanged

        public virtual event PropertyChangedEventHandler PropertyChanged;

        public virtual void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }

        #endregion

        private void btn_capture_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ServiceProvider.DeviceManager.SelectedCameraDevice.CapturePhoto();
            }
            catch (Exception exception)
            {
                MessageBox.Show("Error occurred :" + exception.Message);
            }
        }

        private void btn_Click(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog dialog = new FolderBrowserDialog();
            dialog.SelectedPath = SaveFolder;
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                SaveFolder = dialog.SelectedPath;
        }

        private void PhotoCaptured(object o)
        {
            PhotoCapturedEventArgs eventArgs = o as PhotoCapturedEventArgs;
            if (eventArgs == null)
                return;
            try
            {
                string fileName = Path.Combine(SaveFolder, eventArgs.FileName);
                // if file exist try to generate a new filename to prevent file lost. 
                // This useful when camera is set to record in ram the the all file names are same.
                if (File.Exists(fileName))
                    fileName =
                        StaticHelper.GetUniqueFilename(
                            Path.GetDirectoryName(fileName) + "\\" + Path.GetFileNameWithoutExtension(fileName) + "_", 0,
                            Path.GetExtension(fileName));

                // check the folder of filename, if not found create it
                if (!Directory.Exists(Path.GetDirectoryName(fileName)))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(fileName));
                }
                eventArgs.CameraDevice.TransferFile(eventArgs.Handle, fileName);
                // the IsBusy may used internally, if file transfer is done should set to false  
                eventArgs.CameraDevice.IsBusy = false;
                //img_photo.ImageLocation = fileName;
            }
            catch (Exception exception)
            {
                eventArgs.CameraDevice.IsBusy = false;
                MessageBox.Show("Error download photo from camera :\n" + exception.Message);
            }
        }

        private void Btn_showfolder_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ProcessStartInfo processStartInfo = new ProcessStartInfo();
                processStartInfo.FileName = "explorer";
                processStartInfo.UseShellExecute = true;
                processStartInfo.WindowStyle = ProcessWindowStyle.Normal;
                processStartInfo.Arguments =
                    string.Format("/e,/select,\"{0}\"", SaveFolder);
                Process.Start(processStartInfo);
            }
            catch (Exception exception)
            {
                Log.Error("Error to show file in explorer", exception);
            }
        }

        private void btn_top_Click(object sender, RoutedEventArgs e)
        {
            Topmost = !Topmost;
        }
    }
}