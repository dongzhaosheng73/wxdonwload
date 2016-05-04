using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System;

namespace wxdownload
{
    public class MoveThumb : Thumb
    {
        public MoveThumb()
        {
            DragDelta += new DragDeltaEventHandler(this.MoveThumb_DragDelta);
        }
        private T GetParentObject<T>(DependencyObject obj, string name) where T : FrameworkElement
        {
            DependencyObject parent = VisualTreeHelper.GetParent(obj);

            while (parent != null)
            {
                if (parent is T && (((T)parent).Name == name | string.IsNullOrEmpty(name)))
                {
                    return (T)parent;
                }

                parent = VisualTreeHelper.GetParent(parent);
            }

            return null;
        }
        private void MoveThumb_DragDelta(object sender, DragDeltaEventArgs e)
        {
            ContentControl designerItem = DataContext as ContentControl;

            if (designerItem != null)
            {
                Point dragDelta = new Point(e.HorizontalChange, e.VerticalChange);

                RotateTransform rotateTransform = designerItem.RenderTransform as RotateTransform;

                if (rotateTransform != null)
                {
                    dragDelta = rotateTransform.Transform(dragDelta);
                }
                Canvas c = GetParentObject<Canvas>(designerItem, designerItem.DataContext.ToString());

                double dx, dy = 0;

                dx = Canvas.GetLeft(designerItem);
                dy = Canvas.GetTop(designerItem);

                Point dragPoint = new Point();

                dragPoint = designerItem.TranslatePoint(new Point(0, 0), c);

                dragPoint.X = dragDelta.X + dx + designerItem.ActualWidth;
                dragPoint.Y = dragDelta.Y + dy + designerItem.ActualHeight;

                if (dragPoint.X - designerItem.ActualWidth > 0 && dragPoint.X < c.ActualWidth)
                {
                    Canvas.SetLeft(designerItem, Canvas.GetLeft(designerItem) + dragDelta.X);
                }
                if (dragPoint.Y - designerItem.ActualHeight > 0 && dragPoint.Y < c.ActualHeight)
                {
                    Canvas.SetTop(designerItem, Canvas.GetTop(designerItem) + dragDelta.Y);
                }
            }
        }
    }
}
