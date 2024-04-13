using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using System.Windows.Shapes;

namespace TodoList
{
    /// <summary>
    /// Interaction logic for EntryTasksWindow.xaml
    /// </summary>
    public partial class EntryTasksWindow : Window
    {
        public EntryTasksViewModel ViewModel { get; set; }
        public EntryTasksWindow(IEnumerable<MyTask> tasks)
        {
            InitializeComponent();

            ViewModel = new EntryTasksViewModel(tasks, () => this.Close());

            DataContext = ViewModel;
        }
    }



    public class EntryTasksViewModel : INotifyPropertyChanged
    {
        private IEnumerable<MyTask> _currentTasks;
        private readonly Action _closeWindow;

        public EntryTasksViewModel(IEnumerable<MyTask> currentTasks, Action closeWindow)
        {
            _currentTasks = currentTasks;
            _closeWindow = closeWindow;
        }

        public BitmapImage InProgressIcon => MyTaskStyle.InProgressIcon;
        public BitmapImage CompletedIcon => MyTaskStyle.CompletedIcon;
        public BitmapImage OverDueIcon => MyTaskStyle.OverDueIcon;
        public BitmapImage AllIcon => MyTaskStyle.AllIcon;

        public IEnumerable<MyTask> CurrentTasks
        {
            get => _currentTasks;
            set
            {
                _currentTasks = value;
                OnPropertyChanged();
            }
        }


        public int TotalTasks => CurrentTasks.Count();
        public int TotalCompletedTasks => CurrentTasks.Count(t => t.Status == TaskStatus.Completed);
        public int TotalInProgressTasks => CurrentTasks.Count(t => t.Status == TaskStatus.InProgress);
        public int TotalOverDueTasks => CurrentTasks.Count(t => t.IsOverdue);


        private ICommand? _closeWinCmd;
        public ICommand CloseWinCmd => _closeWinCmd ??= new RelayCommand(_ => _closeWindow());


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
}
