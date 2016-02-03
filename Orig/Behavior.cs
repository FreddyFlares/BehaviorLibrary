using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace BehaviorLib
{
    #region EventToCommand
    static class EventToCommand
    {
        #region Command

        public static ICommand GetCommand(DependencyObject obj)
        {
            return (ICommand)obj.GetValue(CommandProperty);
        }

        public static void SetCommand(DependencyObject obj, ICommand value)
        {
            obj.SetValue(CommandProperty, value);
        }

        public static readonly DependencyProperty CommandProperty =
            DependencyProperty.RegisterAttached("Command", typeof(ICommand), typeof(EventToCommand), new UIPropertyMetadata(null));

        #endregion

        #region CommandParameter

        public static object GetCommandParameter(DependencyObject obj)
        {
            return (object)obj.GetValue(CommandParameterProperty);
        }

        public static void SetCommandParameter(DependencyObject obj, object value)
        {
            obj.SetValue(CommandParameterProperty, value);
        }

        public static readonly DependencyProperty CommandParameterProperty =
    DependencyProperty.RegisterAttached("CommandParameter", typeof(object), typeof(EventToCommand), new UIPropertyMetadata(null));

        #endregion

        #region Event

        public static RoutedEvent GetEvent(DependencyObject obj)
        {
            return (RoutedEvent)obj.GetValue(EventProperty);
        }

        public static void SetEvent(DependencyObject obj, RoutedEvent value)
        {
            obj.SetValue(EventProperty, value);
        }

        public static readonly DependencyProperty EventProperty =
            DependencyProperty.RegisterAttached("Event", typeof(RoutedEvent), typeof(EventToCommand), new UIPropertyMetadata(null, EventChanged));

        #endregion

        static void EventChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var ele = sender as UIElement;
            if (ele != null)
                ele.AddHandler((RoutedEvent)e.NewValue, new RoutedEventHandler(DoCommand));
        }

        static void DoCommand(object sender, RoutedEventArgs e)
        {
            var ele = sender as FrameworkElement;
            if (ele != null)
            {
                var command = (ICommand)ele.GetValue(EventToCommand.CommandProperty);
                if (command != null)
                {
                    var parameter = ele.GetValue(EventToCommand.CommandParameterProperty);
                    parameter = parameter == null ? e : parameter;
                    command.Execute(parameter);
                }
            }
        }

    }
    #endregion

    #region ItemsSourceBehavior
    // http://stackoverflow.com/questions/6747491/wpf-showing-and-hiding-items-in-an-itemscontrol-with-effects
    public class ItemsSourceBehavior
    {
        #region Attached ItemsSourceProperty
        public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.RegisterAttached("ItemsSource",
            typeof(IList), typeof(ItemsSourceBehavior), new UIPropertyMetadata(null, ItemsSourcePropertyChanged));

        public static void SetItemsSource(DependencyObject element, IList value)
        {
            element.SetValue(ItemsSourceProperty, value);
        }

        public static IList GetItemsSource(DependencyObject element)
        {
            return (IList)element.GetValue(ItemsSourceProperty);
        }
        #endregion

        #region Attached FadeInAnimationProperty
        public static readonly DependencyProperty FadeInAnimationProperty = DependencyProperty.RegisterAttached("FadeInAnimation",
            typeof(Storyboard), typeof(ItemsSourceBehavior), new UIPropertyMetadata(null));

        public static void SetFadeInAnimation(DependencyObject element, Storyboard value)
        {
            element.SetValue(FadeInAnimationProperty, value);
        }

        public static Storyboard GetFadeInAnimation(DependencyObject element)
        {
            return (Storyboard)element.GetValue(FadeInAnimationProperty);
        }
        #endregion

        #region Attached FadeOutAnimationProperty
        public static readonly DependencyProperty FadeOutAnimationProperty = DependencyProperty.RegisterAttached("FadeOutAnimation",
            typeof(Storyboard), typeof(ItemsSourceBehavior), new UIPropertyMetadata(null));

        public static void SetFadeOutAnimation(DependencyObject element, Storyboard value)
        {
            element.SetValue(FadeOutAnimationProperty, value);
        }

        public static Storyboard GetFadeOutAnimation(DependencyObject element)
        {
            return (Storyboard)element.GetValue(FadeOutAnimationProperty);
        }
        #endregion

        static object isRemovingObj = new object();

        private static void ItemsSourcePropertyChanged(DependencyObject source, DependencyPropertyChangedEventArgs e)
        {
            var itemsControl = source as ItemsControl;
            var itemsSource = e.NewValue as IList;
            if (itemsControl == null)
            {
                return;
            }
            if (itemsSource == null)
            {
                // Set the real ItemsSource of the ItemsControl to null and return
                itemsControl.ItemsSource = null;
                return;
            }

            // Now we want to create an instance of an ObservableCollection with the same generic parameter as the itemsSource IList
            var itemsSourceType = itemsSource.GetType();
            var listType = typeof(ObservableCollection<>).MakeGenericType(itemsSourceType.GetGenericArguments()[0]);
            var mirrorItemsSource = (IList)Activator.CreateInstance(listType);
            // The real ItemsSource becomes the mirror instance we just created
            itemsControl.ItemsSource = mirrorItemsSource;           // The following binding also worked
                                                                    //itemsControl.SetBinding(ItemsControl.ItemsSourceProperty, new Binding { Source = mirrorItemsSource });

            // Populate the mirror ObservableCollection
            foreach (var item in itemsSource)
            {
                mirrorItemsSource.Add(item);
            }

            (itemsSource as INotifyCollectionChanged).CollectionChanged += (object sender, NotifyCollectionChangedEventArgs ne) =>
            {
                if (ne.Action == NotifyCollectionChangedAction.Add)
                {
                    int insertAt = 0;
                    for (int i = 0; i < ne.NewStartingIndex; i++)
                    {
                        // Skip the ones that are being remove animated
                        ContentPresenter con = (ContentPresenter)itemsControl.ItemContainerGenerator.ContainerFromIndex(insertAt);
                        while (con.Tag == isRemovingObj)
                        {
                            insertAt++;
                            con = (ContentPresenter)itemsControl.ItemContainerGenerator.ContainerFromIndex(insertAt);
                        }
                        insertAt++;
                    }

                    foreach (var newItem in ne.NewItems)
                    {
                        //insert the items instead of just adding them
                        //this brings support for sorted collections

                        mirrorItemsSource.Insert(insertAt++, newItem);

                        var container = itemsControl.ItemContainerGenerator.ContainerFromItem(newItem) as ContentPresenter;
                        var fadeInAnimation = GetFadeInAnimation(itemsControl);
                        if (container != null && fadeInAnimation != null)
                        {
                            // Defer to Loaded so we can access the Visual Child
                            container.Loaded += (senderLoaded, le) =>
                            {
                                FrameworkElement subContainer;
                                if (VisualTreeHelper.GetChildrenCount(container) == 1 && (subContainer = VisualTreeHelper.GetChild(container, 0) as FrameworkElement) != null)
                                {
                                    fadeInAnimation.Begin(subContainer);
                                }
                            };
                        }
                    }
                }
                if (ne.Action == NotifyCollectionChangedAction.Remove)
                {
                    foreach (var oldItem in ne.OldItems)
                    {
                        var container = itemsControl.ItemContainerGenerator.ContainerFromItem(oldItem) as ContentPresenter;
                        var fadeOutAnimation = GetFadeOutAnimation(itemsControl).Clone();
                        if (container != null && fadeOutAnimation != null)
                        {
                            FrameworkElement subContainer;
                            if (VisualTreeHelper.GetChildrenCount(container) == 1 && (subContainer = VisualTreeHelper.GetChild(container, 0) as FrameworkElement) != null)
                            {
                                container.Tag = isRemovingObj;
                                EventHandler onAnimationCompleted = null;                   // Necessary so we can reference the anonymous method inside the lambda
                                onAnimationCompleted = ((sender2, e2) =>
                                {
                                    fadeOutAnimation.Completed -= onAnimationCompleted;
                                    mirrorItemsSource.Remove(oldItem);
                                });

                                fadeOutAnimation.Completed += onAnimationCompleted;
                                fadeOutAnimation.Begin(subContainer);
                            }
                            else
                                mirrorItemsSource.Remove(oldItem);
                        }
                        else
                        {
                            mirrorItemsSource.Remove(oldItem);
                        }
                    }
                }
            };
        }

        private static void Container_Loaded(object sender, RoutedEventArgs e)
        {
            ContentPresenter container = (ContentPresenter)sender;
            var fadeInAnimation = GetFadeInAnimation(container);
            FrameworkElement subContainer;
            if (VisualTreeHelper.GetChildrenCount(container) == 1 && (subContainer = VisualTreeHelper.GetChild(container, 0) as FrameworkElement) != null)
            {
                fadeInAnimation.Begin(subContainer);
            }
        }
        #endregion
    }
}

