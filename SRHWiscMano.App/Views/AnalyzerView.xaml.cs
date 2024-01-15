﻿using CommunityToolkit.Mvvm.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using SRHWiscMano.App.ViewModels;
using SRHWiscMano.Core.ViewModels;
using System.ComponentModel;

namespace SRHWiscMano.App.Views
{
    /// <summary>
    /// Interaction logic for ExplorerView.xaml
    /// </summary>
    public partial class AnalyzerView : UserControl
    {
        public AnalyzerView()
        {
            InitializeComponent();
            if (!DesignerProperties.GetIsInDesignMode(new DependencyObject()))
                this.DataContext = Ioc.Default.GetService<IAnalyzerViewModel>();
        }
        private void ListBox_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            ScrollViewer scrollViewer = GetScrollViewer(sender as ListBox);
            if (scrollViewer != null)
            {
                if (e.Delta > 0)
                {
                    scrollViewer.LineLeft();
                    scrollViewer.LineLeft();
                    scrollViewer.LineLeft();
                    scrollViewer.LineLeft();
                    scrollViewer.LineLeft();
                }
                else
                {
                    scrollViewer.LineRight();
                    scrollViewer.LineRight();
                    scrollViewer.LineRight();
                    scrollViewer.LineRight();
                    scrollViewer.LineRight();
                }
                e.Handled = true;
            }
        }

        private ScrollViewer GetScrollViewer(DependencyObject o)
        {
            if (o is ScrollViewer)
                return o as ScrollViewer;

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(o); i++)
            {
                var child = VisualTreeHelper.GetChild(o, i);
                var result = GetScrollViewer(child);
                if (result == null)
                    continue;
                else
                    return result;
            }

            return null;
        }

    }
}
