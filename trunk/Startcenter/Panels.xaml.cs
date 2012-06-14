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
using Cryptool.PluginBase;

namespace Startcenter
{
    /// <summary>
    /// Interaction logic for Panels.xaml
    /// </summary>
    [Cryptool.PluginBase.Attributes.Localization("Startcenter.Properties.Resources")]
    public partial class Panels : UserControl
    {
        private Templates _templatesObj;

        public string TemplatesDir
        {
            set
            {
                ((Templates)templates.Child).TemplatesDir = value;
            }
        }

        public event OpenEditorHandler OnOpenEditor;
        public event OpenTabHandler OnOpenTab;

        public Panels()
        {
            InitializeComponent();
            ((LastOpenedFilesList)lastOpenedFilesList.Child).OnOpenEditor += (content, title, filename) => OnOpenEditor(content, title, filename);
            ((LastOpenedFilesList)lastOpenedFilesList.Child).OnOpenTab += (content, title, parent) => OnOpenTab(content, title, parent);
            _templatesObj = ((Templates) templates.Child);
            _templatesObj.OnOpenEditor += (content, title, filename) => OnOpenEditor(content, title, filename);
            _templatesObj.OnOpenTab += (content, title, parent) => OnOpenTab(content, title, parent);
        }

        public void ShowHelp()
        {
            _templatesObj.ShowHelp();
        }
    }
}
