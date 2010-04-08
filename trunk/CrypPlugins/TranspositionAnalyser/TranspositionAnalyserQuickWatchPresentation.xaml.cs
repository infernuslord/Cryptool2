﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Collections.ObjectModel;

namespace TranspositionAnalyser
{
    /// <summary>
    /// Interaktionslogik für TranspositionAnalyserQuickWatchPresentation.xaml
    /// </summary>
    public partial class TranspositionAnalyserQuickWatchPresentation : UserControl
    {
        public ObservableCollection<ResultEntry> entries = new ObservableCollection<ResultEntry>();
        public event EventHandler doppelClick;

        public TranspositionAnalyserQuickWatchPresentation()
        {
            InitializeComponent();
            SizeChanged += sizeChanged;
            this.DataContext = entries;
         
        }

        public void sizeChanged(Object sender, EventArgs eventArgs)
        {
            double height = this.ActualHeight - this.Grid.ActualHeight;
            if (height < 0)
            {
                height = 0;
            }
            this.ListView.Height = height;
            this.ListView.Width = this.ActualWidth;

            double heightTransform = (this.ActualHeight - height) / this.Grid.ActualHeight;
            double widthTransform = this.ActualWidth / this.Grid.ActualWidth;

            if (widthTransform > heightTransform)
            {
                widthTransform = heightTransform;
            }


            this.Grid.RenderTransform = new ScaleTransform(widthTransform, heightTransform);
        }

        public void HandleDoubleClick(Object sender, EventArgs eventArgs)
        {
               doppelClick(sender,eventArgs);
        }
    }
}
