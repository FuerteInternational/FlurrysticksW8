using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace Flurrystics
{
    public class VariableSizeGridView : GridView
    {
        private int rowVal;
        private int colVal;

        private ScrollViewer scrollViewer;

        public VariableSizeGridView()
        {
            MouseDevice.GetForCurrentView().MouseMoved += VariableSizeGridView_MouseMoved;
        }

        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            scrollViewer = GetVisualChild<ScrollViewer>(this);
        }

        protected override void PrepareContainerForItemOverride(Windows.UI.Xaml.DependencyObject element, object item)
        {
            base.PrepareContainerForItemOverride(element, item);
            IVariableSizeItem dataItem = item as IVariableSizeItem;
            if (dataItem != null)
            {
                rowVal = dataItem.RowSpan;
                colVal = dataItem.ColSpan;

                VariableSizedWrapGrid.SetRowSpan(element as UIElement, rowVal);
                VariableSizedWrapGrid.SetColumnSpan(element as UIElement, colVal);
            }
        }

        private void VariableSizeGridView_MouseMoved(MouseDevice sender, MouseEventArgs args)
        {
            if (Window.Current.CoreWindow.PointerPosition.X == Window.Current.CoreWindow.Bounds.Left)
            {
                if (args.MouseDelta.X < 0)
                {
                    scrollViewer.ScrollToHorizontalOffset(scrollViewer.HorizontalOffset + (args.MouseDelta.X * 2));
                }
            }

            if (Window.Current.CoreWindow.PointerPosition.X == Window.Current.CoreWindow.Bounds.Right - 1)
            {
                if (args.MouseDelta.X > 0)
                {
                    scrollViewer.ScrollToHorizontalOffset(scrollViewer.HorizontalOffset + (args.MouseDelta.X * 2));
                }
            }
        }

        private T GetVisualChild<T>(DependencyObject parent) where T : DependencyObject
        {
            T child = default(T);
            int numVisuals = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < numVisuals; i++)
            {
                DependencyObject v = (DependencyObject)VisualTreeHelper.GetChild(parent, i);
                child = v as T;
                if (child == null)
                    child = GetVisualChild<T>(v);
                if (child != null)
                    break;
            }
            return child;
        }

        
    }

    public interface IVariableSizeItem
    {
        int RowSpan { get; }

        int ColSpan { get; }
    }
}
