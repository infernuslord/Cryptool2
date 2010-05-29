using System;
using System.Windows;
using System.Windows.Controls;
using Cryptool.P2PEditor.Distributed;

namespace Cryptool.P2PEditor.GUI
{
    public class P2PUserControl : UserControl
    {
        public JobListManager JobListManager
        {
            get { return (JobListManager) GetValue(JobListManagerProperty); }
            set { SetValue(JobListManagerProperty, value); }
        }

        public static readonly DependencyProperty JobListManagerProperty =
            DependencyProperty.RegisterAttached("JobListManagerProperty", typeof (JobListManager),
                                                typeof(P2PUserControl),
                                                new FrameworkPropertyMetadata(null,
                                                                              FrameworkPropertyMetadataOptions.Inherits));

        public P2PEditor P2PEditor
        {
            get { return (P2PEditor) GetValue(P2PEditorProperty); }
            set { SetValue(P2PEditorProperty, value); }
        }

        public static readonly DependencyProperty P2PEditorProperty =
            DependencyProperty.RegisterAttached("P2PEditorProperty", typeof (P2PEditor),
                                                typeof(P2PUserControl),
                                                new FrameworkPropertyMetadata(null,
                                                                              FrameworkPropertyMetadataOptions.Inherits));

        public P2PEditorPresentation P2PEditorPresentation
        {
            get { return (P2PEditorPresentation)GetValue(P2PEditorPresentationProperty); }
            set { SetValue(P2PEditorPresentationProperty, value); }
        }

        public static readonly DependencyProperty P2PEditorPresentationProperty =
            DependencyProperty.RegisterAttached("P2PEditorPresentationProperty", typeof(P2PEditorPresentation),
                                                typeof(P2PUserControl),
                                                new FrameworkPropertyMetadata(null,
                                                                              FrameworkPropertyMetadataOptions.Inherits));
    }
}