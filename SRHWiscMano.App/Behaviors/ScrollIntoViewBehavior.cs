using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xaml.Behaviors;
using System.Windows;
using System.Windows.Controls;

namespace SRHWiscMano.App.Behaviors
{
    /// <summary>
    /// ListBox가 Scrollviewer에 내장되어 있는 경우 Scrollviewer의 position 이 현재 item 이 항상보일 수 있도록 제어하는 behavior
    /// </summary>
    public class ScrollIntoViewBehavior : Behavior<ListBox>
    {
        protected override void OnAttached()
        {
            base.OnAttached();
            this.AssociatedObject.SelectionChanged += OnSelectionChanged;
        }

        protected override void OnDetaching()
        {
            this.AssociatedObject.SelectionChanged -= OnSelectionChanged;
            base.OnDetaching();
        }

        private void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is ListBox listBox && listBox.SelectedItem != null)
            {
                listBox.ScrollIntoView(listBox.SelectedItem);
            }
        }
    }

}
