using CommunityToolkit.Mvvm.DependencyInjection;
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
using Microsoft.Extensions.Options;
using SRHWiscMano.App.Data;

namespace SRHWiscMano.App.Views
{
    /// <summary>
    /// Interaction logic for ExplorerView.xaml
    /// </summary>
    public partial class AnalyzerView : UserControl
    {
        private int scrollLineNum = 5;

        public AnalyzerView()
        {
            InitializeComponent();
            if (!DesignerProperties.GetIsInDesignMode(new DependencyObject()))
                this.DataContext = Ioc.Default.GetService<IAnalyzerViewModel>();
        }


        /// <summary>
        /// ListBox Preview Mouse Wheel event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ListBox_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            ScrollViewer scrollViewer = GetScrollViewer(sender as ListBox);
            if (scrollViewer != null)
            {
                if (e.Delta > 0)
                {
                    for(int i=0; i<scrollLineNum; i++)
                        scrollViewer.LineLeft();
                }
                else
                {
                    for (int i = 0; i < scrollLineNum; i++)
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
