using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Controls;
using System.IO;
using System.Windows.Media.Imaging;

namespace TodoList
{
    //RELAYCOMMAND
    public class RelayCommand : ICommand
    {
        private readonly Action<object> _execute;
        private readonly Predicate<object> _canExecute;

        public RelayCommand(Action<object> execute, Predicate<object> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public bool CanExecute(object parameter)
        {
            return _canExecute == null || _canExecute(parameter);
        }

        public void Execute(object parameter)
        {
            _execute(parameter);
        }
    }

    public static class MyTaskStyle
    {
        #region icons
        public static BitmapImage InProgressIcon => ImageManager.LoadImageFromByteArray(MyResources.to_do_list)!;
        public static BitmapImage CompletedIcon => ImageManager.LoadImageFromByteArray(MyResources._checked)!;
        public static BitmapImage RemoveIcon => ImageManager.LoadImageFromByteArray(MyResources.delete)!;
        public static BitmapImage RemoveOpenIcon => ImageManager.LoadImageFromByteArray(MyResources.deleteOpen)!;
        public static BitmapImage OverDueIcon => ImageManager.LoadImageFromByteArray(MyResources.overdue)!;

        public static BitmapImage AllIcon => ImageManager.LoadImageFromByteArray(MyResources.checklist)!;
        #endregion

        #region brushes

        public static SolidColorBrush DefaultBrush
        {
            get
            {
                var hexColor = "#bbc7ed";
                var color = (Color)ColorConverter.ConvertFromString(hexColor);
                var solidColorBrush = new SolidColorBrush(color);

                return solidColorBrush;
            }
        }

        public static SolidColorBrush CompletedBrush
        {
            get
            {
                var hexColor = "#deffe2";
                var color = (Color)ColorConverter.ConvertFromString(hexColor);
                var solidColorBrush = new SolidColorBrush(color);

                return solidColorBrush;
            }
        }

        public static SolidColorBrush OverdueBrush
        {
            get
            {
                var hexColor = "#fcb8b3";
                var color = (Color)ColorConverter.ConvertFromString(hexColor);
                var solidColorBrush = new SolidColorBrush(color);

                return solidColorBrush;
            }
        }


        #endregion
    }

    public static class ImageManager
    {
        public static BitmapImage? LoadImageFromByteArray(byte[]? imageData)
        {
            if (imageData == null || imageData.Length == 0) return null;

            var image = new BitmapImage();
            using var mem = new MemoryStream(imageData);

            mem.Position = 0;
            image.BeginInit();

            image.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
            image.CacheOption = BitmapCacheOption.OnLoad;
            image.UriSource = null;
            image.StreamSource = mem;

            image.EndInit();

            //image.Freeze();
            return image;
        }
    }

    /// <summary>
    /// Class that allows to change label into status bar
    /// </summary>
    public class StatusBarManager
    {
        public static Brush GoodBg => Brushes.LightGreen;
        public static Brush BadBg => Brushes.OrangeRed;
        public static Brush InfoBg => Brushes.Orange;
        public static Brush NormalBg => Brushes.CornflowerBlue;

        private CancellationTokenSource? _cancellationTokenSource;
        private Label _label;
        public StatusBarManager(Label lb)
        {
            _label = lb;
        }

        public void ChangeStatusBar_Bad(string message, string messageAfter)
            => ChangeStatusBarSync(message, BadBg, messageAfter, NormalBg);

        public void ChangeStatusBar_Good(string message, string messageAfter) =>
            ChangeStatusBarSync(message, GoodBg, messageAfter, NormalBg);

        public void ChangeStatusBar_Info(string message, string messageAfter) => ChangeStatusBarSync(message, InfoBg, messageAfter, NormalBg);


        private async Task ChangeStatusBar(string message, Brush brush, Label lb, string messageAfter, Brush brushAfter, CancellationToken cc)
        {
            cc.ThrowIfCancellationRequested();

            lb.Background = brush;
            lb.Content = message;

            cc.ThrowIfCancellationRequested();

            await Task.Delay(4000, cc);

            cc.ThrowIfCancellationRequested();

            UpdateStatusBar(messageAfter, brushAfter, lb);
        }

        private static void UpdateStatusBar(string message, Brush brush, Label lb)
        {
            lb.Background = brush;
            lb.Content = message;
        }

        private async void ChangeStatusBarSync(string message, Brush brush, string messageAfter, Brush brushAfter)
        {
            try
            {
                CancelToken();
                _cancellationTokenSource = new();
                CancellationToken cc = _cancellationTokenSource.Token;

                try
                {
                    await ChangeStatusBar(message, brush, _label, messageAfter, brushAfter, cc);
                }
                catch (OperationCanceledException)
                {
                }
            }
            catch
            {
            }
        }

        private void CancelToken()
        {
            if (_cancellationTokenSource is not null && !_cancellationTokenSource.Token.IsCancellationRequested)
            {
                _cancellationTokenSource.Cancel();
            }
            _cancellationTokenSource = new CancellationTokenSource();
        }
    }
}
