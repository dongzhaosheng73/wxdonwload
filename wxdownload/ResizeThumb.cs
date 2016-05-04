using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace wxdownload
{
    public class ResizeThumb : Thumb
    {
        private double angle;
        private Point transformOrigin;
        private ContentControl designerItem;

        public ResizeThumb()
        {
            DragStarted += new DragStartedEventHandler(this.ResizeThumb_DragStarted);
            DragDelta += new DragDeltaEventHandler(this.ResizeThumb_DragDelta);
        }

        private void ResizeThumb_DragStarted(object sender, DragStartedEventArgs e)
        {
            this.designerItem = DataContext as ContentControl;

            if (this.designerItem != null)
            {
                this.transformOrigin = this.designerItem.RenderTransformOrigin;
                RotateTransform rotateTransform = this.designerItem.RenderTransform as RotateTransform;

                if (rotateTransform != null)
                {
                    this.angle = rotateTransform.Angle * Math.PI / 180.0;
                }
                else
                {
                    this.angle = 0;
                }
            }
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
        private void ResizeThumb_DragDelta(object sender, DragDeltaEventArgs e)
        {
            if (this.designerItem != null)
            {
                double deltaVertical, deltaHorizontal;
                Canvas c = GetParentObject<Canvas>(designerItem, designerItem.DataContext.ToString());
                double Vgety;
                double Vgetx;
                switch (VerticalAlignment)
                {
                    case System.Windows.VerticalAlignment.Bottom:
                        deltaVertical = Math.Min(-e.VerticalChange, this.designerItem.ActualHeight - this.designerItem.MinHeight);
                        Vgety = Canvas.GetTop(this.designerItem) + (this.transformOrigin.Y * deltaVertical * (1 - Math.Cos(-this.angle)));
                        Vgetx = Canvas.GetLeft(this.designerItem) - deltaVertical * this.transformOrigin.Y * Math.Sin(-this.angle);
                        if (Vgety + this.designerItem.Height - deltaVertical <= c.Height)
                        {
                            Canvas.SetTop(this.designerItem,
                                Vgety);
                            Canvas.SetLeft(this.designerItem,
                               Vgetx);
                            this.designerItem.Height -= deltaVertical;
                        }
                        break;
                    case System.Windows.VerticalAlignment.Top:
                        deltaVertical = Math.Min(e.VerticalChange, this.designerItem.ActualHeight - this.designerItem.MinHeight);
                        Vgety = Canvas.GetTop(this.designerItem) + deltaVertical * Math.Cos(-this.angle) + (this.transformOrigin.Y * deltaVertical * (1 - Math.Cos(-this.angle)));
                        Vgetx = Canvas.GetLeft(this.designerItem) + deltaVertical * Math.Sin(-this.angle) - (this.transformOrigin.Y * deltaVertical * Math.Sin(-this.angle));
                        if (Vgetx >= 0 && Vgety >= 0)
                        {
                            Canvas.SetTop(this.designerItem,
                                Vgety);
                            Canvas.SetLeft(this.designerItem,
                                Vgetx);
                            this.designerItem.Height -= deltaVertical;
                        }
                        break;
                    default:
                        break;
                }

                switch (HorizontalAlignment)
                {
                    case System.Windows.HorizontalAlignment.Left:
                        double Hgety;
                        double Hgetx;
                        deltaHorizontal = Math.Min(e.HorizontalChange, this.designerItem.ActualWidth - this.designerItem.MinWidth);
                        Hgety = Canvas.GetTop(this.designerItem) + deltaHorizontal * Math.Sin(this.angle) - this.transformOrigin.X * deltaHorizontal * Math.Sin(this.angle);
                        Hgetx = Canvas.GetLeft(this.designerItem) + deltaHorizontal * Math.Cos(this.angle) + (this.transformOrigin.X * deltaHorizontal * (1 - Math.Cos(this.angle)));
                        if (Hgetx >= 0 && Hgety >= 0)
                        {
                            Canvas.SetTop(this.designerItem, Hgety);
                            Canvas.SetLeft(this.designerItem, Hgetx);
                            this.designerItem.Width -= deltaHorizontal;
                        }
                        break;
                    case System.Windows.HorizontalAlignment.Right:
                        deltaHorizontal = Math.Min(-e.HorizontalChange, this.designerItem.ActualWidth - this.designerItem.MinWidth);
                        Hgety = Canvas.GetTop(this.designerItem) - this.transformOrigin.X * deltaHorizontal * Math.Sin(this.angle);
                        Hgetx = Canvas.GetLeft(this.designerItem) + (deltaHorizontal * this.transformOrigin.X * (1 - Math.Cos(this.angle)));
                        if (Hgetx + designerItem.Width - deltaHorizontal <= c.Width)
                        {
                            Canvas.SetTop(this.designerItem, Hgety);
                            Canvas.SetLeft(this.designerItem, Hgetx);
                            this.designerItem.Width -= deltaHorizontal;
                        }
                        break;
                    default:
                        break;
                }
            }
            e.Handled = true;
        }
    }
}