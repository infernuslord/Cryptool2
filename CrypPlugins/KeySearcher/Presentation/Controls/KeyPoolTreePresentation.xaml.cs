using System;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Threading;
using KeySearcher.Helper;
using KeySearcher.KeyPattern;
using KeySearcher.P2P.Presentation;
using KeySearcher.P2P.Storage;
using KeySearcher.P2P.Tree;

namespace KeySearcher.Presentation.Controls
{
    /// <summary>
    /// Interaction logic for KeyPoolTreePresentation.xaml
    /// </summary>
    public partial class KeyPoolTreePresentation : UserControl
    {
        private Node _rootNode;

        public KeyPoolTreePresentation()
        {
            InitializeComponent();
        }

        internal KeyPatternPool PatternPool
        {
            get; 
            set;
        }

        internal KeyQualityHelper KeyQualityHelper
        {
            get;
            set;
        }

        internal StorageKeyGenerator KeyGenerator
        {
            get;
            set;
        }

        public StatusContainer StatusContainer
        {
            get;
            set;
        }

        private void Update()
        {
            _rootNode = null;
            if (PatternPool != null && KeyQualityHelper != null && KeyGenerator != null && StatusContainer != null)
            {
                var identifier = KeyGenerator.Generate();
                var storageHelper = new StorageHelper(null, null, StatusContainer);

                _rootNode = (Node) NodeFactory.CreateNode(storageHelper, KeyQualityHelper, null, 0, PatternPool.Length - 1,
                                                          identifier);
                _rootNode.UpdateAll();
            }
        }

        private void FillTreeItem(Node node, TreeViewItem item)
        {
            item.Header = string.Format("Node: {0} to {1}", node.From, node.To);
            item.ToolTip = string.Format("Node: {0}\n{1}", node.ToString(), node.IsReserved() ? "reserved" : "not reserved");
            item.Tag = node;
            if (node.IsReserved())
                item.Background = Brushes.Yellow;

            TreeViewItem leftChildItem = CreateTreeItem(node.leftChild, node.LeftChildFinished);
            item.Items.Add(leftChildItem);
            TreeViewItem rightChildItem = CreateTreeItem(node.rightChild, node.RightChildFinished);
            item.Items.Add(rightChildItem);
        }

        private TreeViewItem CreateTreeItem(NodeBase child, bool finished)
        {
            TreeViewItem childItem = new TreeViewItem();
            if (child == null)
            {
                childItem.Header = "Not loaded!";
            }
            else
            {
                if (child is Node)
                    FillTreeItem((Node)child, childItem);
                else
                {
                    childItem.ToolTip = string.Format("Leaf: {0}\n{1}", child.ToString(), child.IsCalculated() ? "calculated" : "not calculated");
                    childItem.Header = string.Format("Leaf: {0} to {1}", child.From, child.To);
                    childItem.Tag = child;
                    childItem.Background = Brushes.Green;
                    if (child.IsReserved())
                        childItem.Background = Brushes.YellowGreen;
                }
            }
            if (finished)
            {
                childItem.Background = Brushes.DarkBlue;
                childItem.Foreground = Brushes.White;
                childItem.Header = string.Format("Finished!");
                childItem.ToolTip = string.Format("Finished!");
            }
            
            return childItem;
        }

        private void Button_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            waitLabel.Visibility = System.Windows.Visibility.Visible;
            refreshButton.IsEnabled = false;
            var thread = new Thread(delegate (Object obj)
                                        {
                                            Update();
                                            Dispatcher.Invoke(DispatcherPriority.Normal, (SendOrPostCallback) delegate
                                                                {
                                                                    if (_rootNode != null)
                                                                    {
                                                                        treeView.Items.Clear();
                                                                        var rootItem = new TreeViewItem();
                                                                        treeView.Items.Add(rootItem);
                                                                        FillTreeItem(_rootNode, rootItem);
                                                                    }

                                                                    waitLabel.Visibility = System.Windows.Visibility.Collapsed;
                                                                    refreshButton.IsEnabled = true;
                                                                },
                                                              null);
                                        });
            thread.Start();
        }
    }

    class NodeBaseToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return "No information available...";

            string res = "Results:\n";
            var nodebase = (NodeBase) value;
            foreach (var re in nodebase.Result)
            {
                res += re.ToString() + "\n";
            }
            res += "\nLast Update: " + nodebase.LastUpdate;
            res += "\nActivity:\n";
            foreach (var re in nodebase.Activity)
            {
                res += re.Key;
                res = re.Value.Aggregate(res, (current, re1) => current + string.Format(" {0} -> {1}\n", re1.Key, re1.Value));
            }
            return res;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

}
