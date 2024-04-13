using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
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
using static TodoList.ImageManager;


namespace TodoList
{
    /// <summary>
    /// Interaction logic for TaskPanel.xaml
    /// </summary>
    public partial class TaskPanel : UserControl, INotifyPropertyChanged
    {
        public MyTask Task { get; set; }
        public string? Title
        {
            get => Task.Title;
            set
            {
                Task.Title = value;
                OnPropertyChanged();
            }
        }

        public string? Description
        {
            get => Task.Description;
            set
            {
                Task.Description = value;
                OnPropertyChanged();
            }
        }

        public DateTime DueDate
        {
            get => Task.DueDate;
            set
            {
                Task.DueDate = value;

                UpdateBackground();

                OnPropertyChanged();
            }
        }

        public TaskStatus Status
        {
            get => Task.Status;
            set
            {
                Task.ChangeStatus(value);

                UpdateIcon();

                UpdateBackground();

                OnPropertyChanged();
            }
        }

        private BitmapImage _icon;
        public BitmapImage Icon
        {
            get => _icon;
            set
            {
                _icon = value;
                OnPropertyChanged();
            }
        }

        private ICommand? _removeMeCommand;
        public ICommand? RemoveMeCommand
        {
            get => _removeMeCommand;
            set => SetField(ref _removeMeCommand, value);
        }

        public IEnumerable<TaskStatus> Statuses => Enum.GetValues<TaskStatus>();

        private SolidColorBrush _bgColor;
        public SolidColorBrush BgColor
        {
            get => _bgColor;
            set => SetField(ref _bgColor, value);
        }

        #region icons
                public BitmapImage RemoveIcon => MyTaskStyle.RemoveIcon;
                public BitmapImage RemoveOpenIcon => MyTaskStyle.RemoveOpenIcon;
                public BitmapImage OverDueIcon => MyTaskStyle.OverDueIcon;
        #endregion

        private void UpdateIcon()
        {
            switch (Status)
            {
                case TaskStatus.InProgress:
                    Icon = MyTaskStyle.InProgressIcon;
                    break;
                case TaskStatus.Completed:
                    Icon = MyTaskStyle.CompletedIcon;
                    break;
            }
        }

        //overdue icon
        public Visibility IsOverdue => Task.IsOverdue ? Visibility.Visible : Visibility.Collapsed;

        private void UpdateBackground()
        {
            if (Task.IsOverdue)
            {
                BgColor = MyTaskStyle.OverdueBrush;
            }
            else
            {
                switch (Status)
                {
                    case TaskStatus.InProgress:
                        BgColor = MyTaskStyle.DefaultBrush;
                        break;
                    case TaskStatus.Completed:
                        BgColor = MyTaskStyle.CompletedBrush;
                        break;
                }
            }

            OnPropertyChanged(nameof(IsOverdue));
        }

        public TaskPanel(MyTask task, ICommand removeMeCommand)
        {
            InitializeComponent();

            Task = task;

            RemoveMeCommand = removeMeCommand;

            _icon = MyTaskStyle.InProgressIcon;
            _bgColor = MyTaskStyle.DefaultBrush;

            UpdateIcon();

            UpdateBackground();

            DataContext = this;
        }


        #region propertyChanged
        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
        #endregion
    }

    //Converters (for buttons)
    #region conv
    public class BooleanToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is true ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is Visibility.Visible;
        }
    }

    // Converts a boolean value to Visibility (Collapsed if true, Visible if false)
    public class InverseBooleanToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is true ? Visibility.Collapsed : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is Visibility.Collapsed;
        }
    }
#endregion
}
