using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace Cryptool.Plugins.Webcam.Libaries
{
    /// <summary>
    /// Class defining all commands to capture images
    /// </summary>
    public static class CaptureImageCommands
    {
        #region Constructor & destructor
        static CaptureImageCommands()
        {
            // Capture Image
            CaptureImage = new RoutedUICommand("Capture image", "CaptureImage", typeof(CaptureImageCommands));

            // Remove Image
            RemoveImage = new RoutedUICommand("Remove image", "RemoveImage", typeof(CaptureImageCommands));

            // Clear All Images
            ClearAllImages = new RoutedUICommand("Clear all images", "ClearAllImages", typeof(CaptureImageCommands));
        }
        #endregion

        /// <summary>
        /// Captures an image
        /// </summary>
        public static RoutedUICommand CaptureImage { get; private set; }

        /// <summary>
        /// Removes an image
        /// </summary>
        public static RoutedUICommand RemoveImage { get; private set; }

        /// <summary>
        /// Clears all images
        /// </summary>
        public static RoutedUICommand ClearAllImages { get; private set; }
    }
}
