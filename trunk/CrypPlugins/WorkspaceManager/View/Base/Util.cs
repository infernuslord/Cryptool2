using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using WorkspaceManager.View.BinVisual;
using System.Windows;
using System.Windows.Media;
using System.Runtime.InteropServices;
using WorkspaceManager.View.VisualComponents;
using System.Windows.Controls.Primitives;
using Cryptool.PluginBase.Control;
using Cryptool.PluginBase;
using System.Reflection;
using System.Numerics;
using WorkspaceManager.Model;

namespace WorkspaceManager.View.Base
{
    public static class Util
    {

        public static T TryFindParent<T>(this DependencyObject child) where T : DependencyObject
        {
            //get parent item
            DependencyObject parentObject = GetParentObject(child);

            //we've reached the end of the tree
            if (parentObject == null) return null;

            //check if the parent matches the type we're looking for
            T parent = parentObject as T;
            if (parent != null)
            {
                return parent;
            }
            else
            {
                //use recursion to proceed with next level
                return TryFindParent<T>(parentObject);
            }
        }

        public static DependencyObject GetParentObject(this DependencyObject child)
        {
            if (child == null) return null;

            //handle content elements separately
            ContentElement contentElement = child as ContentElement;
            if (contentElement != null)
            {
                DependencyObject parent = ContentOperations.GetParent(contentElement);
                if (parent != null) return parent;

                FrameworkContentElement fce = contentElement as FrameworkContentElement;
                return fce != null ? fce.Parent : null;
            }

            //also try searching for parent in framework elements (such as DockPanel, etc)
            FrameworkElement frameworkElement = child as FrameworkElement;
            if (frameworkElement != null)
            {
                DependencyObject parent = frameworkElement.Parent;
                if (parent != null) return parent;
            }

            //if it's not a ContentElement/FrameworkElement, rely on VisualTreeHelper
            return VisualTreeHelper.GetParent(child);
        }

        public static MultiBinding CreateConnectorBinding(BinConnectorVisual connectable, CryptoLineView link)
        {
            MultiBinding multiBinding = new MultiBinding();
            multiBinding.Converter = new BinConnectorVisualBindingConverter();
            multiBinding.ConverterParameter = connectable;

            Binding binding = new Binding();
            binding.Source = connectable.WindowParent;
            binding.Path = new PropertyPath(BinComponentVisual.PositionProperty);
            multiBinding.Bindings.Add(binding);

            binding = new Binding();
            binding.Source = link;
            binding.Path = new PropertyPath(CryptoLineView.StrokeThicknessProperty);
            multiBinding.Bindings.Add(binding);

            binding = new Binding();
            binding.Source = connectable.WindowParent.West;
            binding.Path = new PropertyPath(FrameworkElement.ActualHeightProperty);
            multiBinding.Bindings.Add(binding);

            binding = new Binding();
            binding.Source = connectable.WindowParent.East;
            binding.Path = new PropertyPath(FrameworkElement.ActualHeightProperty);
            multiBinding.Bindings.Add(binding);

            binding = new Binding();
            binding.Source = connectable.WindowParent.North;
            binding.Path = new PropertyPath(FrameworkElement.ActualWidthProperty);
            multiBinding.Bindings.Add(binding);

            binding = new Binding();
            binding.Source = connectable.WindowParent.South;
            binding.Path = new PropertyPath(FrameworkElement.ActualWidthProperty);
            multiBinding.Bindings.Add(binding);

            binding = new Binding();
            binding.Source = connectable;
            binding.Path = new PropertyPath(BinConnectorVisual.OrientationProperty);
            multiBinding.Bindings.Add(binding);

            return multiBinding;
        }

        public static MultiBinding CreateIsDraggingBinding(object[] value)
        {
            MultiBinding multiBinding = new MultiBinding();
            multiBinding.Converter = new IsDraggingConverter();

            Binding binding;
            Type valueType = value.GetType();
            if (valueType.IsArray && typeof(Thumb).IsAssignableFrom(valueType.GetElementType()))
            {

                foreach (var t in value.Cast<Thumb>())
                {
                    binding = new Binding();
                    binding.Source = t;
                    binding.Path = new PropertyPath(Thumb.IsDraggingProperty);
                    multiBinding.Bindings.Add(binding);
                }
            }

            if (valueType.IsArray && typeof(BinComponentVisual).IsAssignableFrom(valueType.GetElementType()))
            {

                foreach (var b in value.Cast<BinComponentVisual>())
                {
                    binding = new Binding();
                    binding.Source = b;
                    binding.Path = new PropertyPath(BinComponentVisual.IsDraggingProperty);
                    multiBinding.Bindings.Add(binding);
                }
            }

            return multiBinding;
        }

        public static Binding CreateMultiDraggingBinding(BinComponentVisual value, BinComponentVisual parameter)
        {
            Binding binding = new Binding();


            return binding;
        }

        public static class MouseUtilities
        {
            public static Point CorrectGetPosition(Visual relativeTo)
            {
                Win32Point w32Mouse = new Win32Point();
                GetCursorPos(ref w32Mouse);
                return relativeTo.PointFromScreen(new Point(w32Mouse.X, w32Mouse.Y));
            }

            [StructLayout(LayoutKind.Sequential)]
            internal struct Win32Point
            {
                public Int32 X;
                public Int32 Y;
            };

            [DllImport("user32.dll")]
            [return: MarshalAs(UnmanagedType.Bool)]
            internal static extern bool GetCursorPos(ref Win32Point pt);
        }
    }
}
