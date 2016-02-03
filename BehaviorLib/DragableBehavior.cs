using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace BehaviorLib
{
    public static partial class Behavior
    {
        // Dragable Behavior for FrameworkElements with a parent Canvas
        #region Attached DragableProperty
        public static readonly DependencyProperty DragableProperty = DependencyProperty.RegisterAttached
            ("Dragable", typeof(bool), typeof(Behavior), new PropertyMetadata(dragableChanged));

        public static bool GetDragable(FrameworkElement element)
        {
            return (bool)element.GetValue(DragableProperty);
        }

        public static void SetDragable(FrameworkElement element, bool dragable)
        {
            element.SetValue(DragableProperty, dragable);
        }

        // The FrameworkElement being dragged
        static FrameworkElement element;
        // Drag position relative to the element
        static Point dragPos;

        static void dragableChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            FrameworkElement element = o as FrameworkElement;
            if (element != null)
            {
                if (e.NewValue.Equals(true) && e.OldValue.Equals(false))
                    element.PreviewMouseDown += element_PreviewMouseDown;
                else if (e.NewValue.Equals(false) && e.OldValue.Equals(true))
                    element.PreviewMouseDown -= element_PreviewMouseDown;
            }
        }

        static void element_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            element = (FrameworkElement)sender;
            Canvas canvas = element.Parent as Canvas;
            if (canvas == null)
                return;
            canvas.PreviewMouseMove += canvas_PreviewMouseMove;
            canvas.PreviewMouseUp += canvas_PreviewMouseUp;
            dragPos = e.GetPosition(element);
            canvas.CaptureMouse();
        }

        static void canvas_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            Canvas canvas = (Canvas)sender;
            Point newMousePosition = e.GetPosition(canvas);
            Canvas.SetLeft(element, newMousePosition.X - dragPos.X);
            Canvas.SetTop(element, newMousePosition.Y - dragPos.Y);
        }

        static void canvas_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            Canvas canvas = (Canvas)sender;
            canvas.PreviewMouseMove -= canvas_PreviewMouseMove;
            canvas.PreviewMouseUp -= canvas_PreviewMouseUp;
            canvas.ReleaseMouseCapture();
        }
        #endregion
    }
}
