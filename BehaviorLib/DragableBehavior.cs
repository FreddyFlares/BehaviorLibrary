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
        // Dragable Behavior for UIElements
        #region Attached DragableProperty
        public static readonly DependencyProperty DragableProperty = DependencyProperty.RegisterAttached
            ("Dragable", typeof(bool), typeof(Behavior), new PropertyMetadata(dragableChanged));

        public static bool GetDragable(UIElement element)
        {
            return (bool)element.GetValue(DragableProperty);
        }

        public static void SetDragable(UIElement element, bool dragable)
        {
            element.SetValue(DragableProperty, dragable);
        }

        // The UIElement being dragged
        static UIElement element;
        static TransformCollection transforms;
        // Start drag mouse position relative to the window
        static Point startDragPos;
        // The TranslateTransform to store the transformation
        static TranslateTransform transform = new TranslateTransform(0,0);

        static void dragableChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            UIElement element = o as UIElement;
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
            element = (UIElement)sender;
            if (!(element.RenderTransform is TransformGroup))
                return;
            Window window = FindAncestor<Window>(element);
            if (window == null)
                return;
            transforms = ((TransformGroup)element.RenderTransform).Children;
            window.CaptureMouse();                  // Put this line before wiring the event handlers
            window.PreviewMouseMove += window_PreviewMouseMove;
            window.PreviewMouseUp += window_PreviewMouseUp;
            startDragPos = e.GetPosition(window);
            transform.X = 0;
            transform.Y = 0;
            transforms.Add(transform);
        }

        static void window_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            Window window = (Window)sender;
            Point newMousePosition = e.GetPosition(window);
            transform.X = newMousePosition.X - startDragPos.X;
            transform.Y = newMousePosition.Y - startDragPos.Y;
        }

        static void window_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            Window window = (Window)sender;
            window.PreviewMouseMove -= window_PreviewMouseMove;
            window.PreviewMouseUp -= window_PreviewMouseUp;
            window.ReleaseMouseCapture();           // After releasing the event handlers
            transforms.Remove(transform);
            TranslateTransform last = (TranslateTransform)transforms.LastOrDefault((tr) => tr is TranslateTransform);
            if (last != null)
            {
                last.X += transform.X;
                last.Y += transform.Y;
            }
            else
            {
                transforms.Add(new TranslateTransform(transform.X, transform.Y));
            }
        }
        #endregion

        static T FindAncestor<T>(DependencyObject element) where T : DependencyObject
        {
            do { 
                element = VisualTreeHelper.GetParent(element);
            } while (element != null && !(element is T));
            return element as T;
        }
    }
}
