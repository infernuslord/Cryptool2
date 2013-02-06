///////////////////////////////////////////////////////////////////////////////
// CapPlayer v1.1
//
// This software is released into the public domain.  You are free to use it
// in any way you like, except that you may not sell this source code.
//
// This software is provided "as is" with no expressed or implied warranty.
// I accept no liability for any damage or loss of business that this software
// may cause.
// 
// This source code is originally written by Tamir Khason (see http://blogs.microsoft.co.il/blogs/tamir
// or http://www.codeplex.com/wpfcap).
// 
// Modifications are made by Geert van Horrik (CatenaLogic, see http://blog.catenalogic.com) 
//
///////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Runtime.InteropServices;
using System.Windows.Media.Animation;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Data;
using System.Diagnostics;

namespace Cryptool.Plugins.WebCam.Libaries
{
    public class CapPlayer : Image, IDisposable
    {
        #region Variables
        #endregion

        #region Constructor & destructor
        public CapPlayer()
        {
            this.LayoutTransform = new ScaleTransform(1, -1); //somehow the cam gives us a fliped picture, so we flip it back
        }

        public void Dispose()
        {
            // Check whether we have a valid device
            if (Device != null)
            {
                // Yes, dispose it
                Device.Dispose();

                // Clear device
                Device = null;
            }
        }
        #endregion

        #region Properties
        /// <summary>
        /// Wrapper for the Device dependency property
        /// </summary>
        public CapDevice Device
        {
            get { return (CapDevice)GetValue(DeviceProperty); }
            set { SetValue(DeviceProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Device.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DeviceProperty = DependencyProperty.Register("Device", typeof(CapDevice),
            typeof(CapPlayer), new UIPropertyMetadata(null, new PropertyChangedCallback(DeviceProperty_Changed)));
    
        /// <summary>
        /// Wrapper for the framerate dependency property
        /// </summary>
        public float Framerate
        {
            get { return (float)GetValue(FramerateProperty); }
            set { SetValue(FramerateProperty, value); }
        }

        public static readonly DependencyProperty FramerateProperty =
            DependencyProperty.Register("Framerate", typeof(float), typeof(CapPlayer), new UIPropertyMetadata(default(float)));

        /// <summary>
        /// Gets the current bitmap
        /// </summary>
        public BitmapSource CurrentBitmap
        {
            get
            {
                // Return right value
                return (Device != null) ? new TransformedBitmap(Device.BitmapSource.Clone(), new ScaleTransform(1, -1) ): null;
            }
        }
        #endregion

        #region Methods
        static void DeviceProperty_Changed(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            // Get the sender
            CapPlayer typedSender = sender as CapPlayer;
            if ((typedSender != null) && (e.NewValue != null))
            {
                // Make sure that we are not in design mode
                if (System.ComponentModel.DesignerProperties.GetIsInDesignMode(typedSender)) return;

                // Unsubscribe from previous device
                CapDevice oldDevice = e.OldValue as CapDevice;
                if (oldDevice != null)
                {
                    // Clean up
                    typedSender.CleanUpDevice(oldDevice);
                }

                // Subscribe to new one
                CapDevice newDevice = e.NewValue as CapDevice;
                if (newDevice != null)
                {
                    // Subscribe
                    newDevice.NewBitmapReady += typedSender.device_OnNewBitmapReady;
                }
            }
        }

       
        /// <summary>
        /// Cleans up a specific device
        /// </summary>
        /// <param name="device">Device to clean up</param>
        void CleanUpDevice(CapDevice device)
        {
            // Check if there even is a device
            if (device == null) return;

            // Stop
            device.Stop();

            // Unsubscribe
            device.NewBitmapReady -= device_OnNewBitmapReady;
        }

        /// <summary>
        /// Invoked when a new bitmap is ready
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">EventArgs</param>
        void device_OnNewBitmapReady(object sender, EventArgs e)
        {
            // Create new binding for the framerate
            Binding b = new Binding();
            b.Source = Device;
            b.Path = new PropertyPath(CapDevice.FramerateProperty);
            SetBinding(CapPlayer.FramerateProperty, b);

            // Get the sender
            CapDevice typedSender = sender as CapDevice;
            if (typedSender != null)
            {
                // Set the source of the image
                Source = typedSender.BitmapSource;
            }
        }
        #endregion
    }
}
