using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;

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

        private void EntryTasksWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            ViewModel.GenerateNotification();
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

        public void GenerateNotification()
        {
            WindowsNotifications.SendNotification("Tasks", $"You have {TotalInProgressTasks} tasks to do today");
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
}

