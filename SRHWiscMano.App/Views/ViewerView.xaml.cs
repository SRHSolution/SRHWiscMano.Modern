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

namespace SRHWiscMano.App.Views
{
    /// <summary>
    /// Interaction logic for ManoDataView.xaml
    /// </summary>
    public partial class ViewerView : UserControl
    {
        public ViewerView()
        {
            InitializeComponent();
        }

        /// <summary>
        /// ZoomLevel 가 선택되었을 때 기존에 있는 숫자가 아닌 문자열은 삭제하고 표시하돌고 한다
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Zoomlevel_OnGotFocus(object sender, RoutedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            if (textBox != null && textBox.Text.EndsWith("%"))
            {
                textBox.Text = textBox.Text.TrimEnd('%');
                textBox.SelectAll();
            }
        }

        /// <summary>
        /// TextBox가 선택되고 난 뒤에 SelectAll 명령이 풀리는 것을 방지함
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Zoomlevel_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            if (textBox != null && !textBox.IsKeyboardFocusWithin)
            {
                // Focus the TextBox and select all text
                textBox.Focus();
                e.Handled = true;
            }
        }
    }
}
