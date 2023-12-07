using System;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using SRHWiscMano.Core.ViewModels;
using Timer = System.Timers.Timer;

namespace SRHWiscMano.App.Views;

/// <summary>
/// Interaction logic for ManoDataView.xaml
/// </summary>
public partial class ViewerView : UserControl
{
    private bool isResizing;
    private DateTime eventTime;
    private Timer eventTimer;
    private IViewerViewModel viewModel;

    public ViewerView()
    {
        InitializeComponent();

        // // SizeChanged 가 발생한 경우에 Subscribe 함수가 Throttle 값에 따라 call 된다.
        // var resizeObservable = Observable.FromEventPattern<SizeChangedEventHandler, SizeChangedEventArgs>(
        //         h => SizeChanged += h,
        //         h => SizeChanged -= h)
        //     .TimeInterval(Sch);
        //     
        //
        // // Subscribe to the debounced event stream
        // resizeObservable.Subscribe(_ => OnResizeEnd());
    }


    /// <summary>
    /// ZoomLevel 가 선택되었을 때 기존에 있는 숫자가 아닌 문자열은 삭제하고 표시하돌고 한다
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void Zoomlevel_OnGotFocus(object sender, RoutedEventArgs e)
    {
        var textBox = sender as TextBox;
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
        var textBox = sender as TextBox;
        if (textBox != null && !textBox.IsKeyboardFocusWithin)
        {
            // Focus the TextBox and select all text
            textBox.Focus();
            e.Handled = true;
        }
    }
}