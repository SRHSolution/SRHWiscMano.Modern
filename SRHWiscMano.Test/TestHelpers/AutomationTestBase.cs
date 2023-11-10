// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Diagnostics;
using System.Windows;
using ControlzEx.Theming;
using MahApps.Metro.Controls;

namespace MahApps.Metro.Tests.TestHelpers
{
    /// <summary>
    /// This is the base class for all of our UI tests.
    /// </summary>
    public class AutomationTestBase : IDisposable
    {
        public AutomationTestBase()
        {
            var message = $"Create test class '{this.GetType().Name}' with Thread.CurrentThread: {Thread.CurrentThread.ManagedThreadId}" +
                          $" and Current.Dispatcher.Thread: {Application.Current.Dispatcher.Thread.ManagedThreadId}";
            Debug.WriteLine(message);

            // Reset the application as good as we can
            Application.Current.Invoke(() =>
                {
                    var windows = Application.Current.Windows.OfType<Window>().ToList();
                    foreach (Window window in windows)
                    {
                        window.Close();
                    }
                });

            Application.Current.Invoke(() => { ThemeManager.Current.ChangeTheme(Application.Current, "Light.Blue"); });
        }

        /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
        public virtual void Dispose()
        {
            var message = $"Dispose test class '{this.GetType().Name}' with Thread.CurrentThread: {Thread.CurrentThread.ManagedThreadId}" +
                          $" and Current.Dispatcher.Thread: {Application.Current.Dispatcher.Thread.ManagedThreadId}";
            Debug.WriteLine(message);
        }
    }
}