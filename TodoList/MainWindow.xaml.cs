using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.EntityFrameworkCore.Diagnostics;
using static TodoList.ImageManager;
using Windows.UI.Notifications;
using Microsoft.Win32;

//-----------------------------------------------------------------------
// 
// Author: Jakub Iwicki
//
//-----------------------------------------------------------------------


namespace TodoList
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private TodoListViewModel ViewModel { get; set; }

        public MainWindow()
        {
            InitializeComponent();

            ViewModel = new TodoListViewModel(StatusLb);

            Application.Current.Exit += (s, a) => SaveUserSettings();

            this.Icon = LoadImageFromByteArray(MyResources.to_do_list)!;

            this.Language = XmlLanguage.GetLanguage(CultureInfo.CurrentCulture.IetfLanguageTag);

            DataContext = ViewModel;
        }

        private void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            var notes = ViewModel.TaskPanels ?? Enumerable.Empty<TaskPanel>();
            var welcomeWindow = new EntryTasksWindow(notes.Select(x => x.Task));

            welcomeWindow.Focus();
            welcomeWindow.Owner = this;
            welcomeWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner;

            welcomeWindow.ShowDialog();
        }

        private void MainWindow_OnClosing(object? sender, CancelEventArgs e)
        {
            ViewModel.OnClosing();
        }

        private static void SaveUserSettings()
        {
            UserSettings.Instance.Save();
        }
    }


    public class TodoListViewModel : INotifyPropertyChanged
    {
        private MyTasksDbSqlite _db;

        private StatusBarManager _statusBarManager;
        private static string _statusDefMessage = "Status - Todo List";

        private static UserSettings _userSettings;
        public TodoListViewModel(Label statusLb)
        {
            //init
            _db = new MyTasksDbSqlite();

            statusLb.Content = _statusDefMessage;
            _statusBarManager = new StatusBarManager(statusLb);

            _taskPanels = GetTaskPanelsFromDb();

            //settings:
            _userSettings = UserSettings.Instance;

            //init settings
            _selectedTaskFilter = (TaskFilter)_userSettings.Data!.LastSelectedFilterIndex;
            _selectedFilterDate = _userSettings.Data.LastSelectedOverdueToDate;
            IsFilterDateEnabled = _selectedFilterDate != null;
            //
        }

        public static string Title => "Todo List";

        public enum TaskFilter
        {
            All = 0,
            InProgress,
            Completed,
            OverDue
        }

        public IEnumerable<TaskFilter> TaskFilters => Enum.GetValues<TaskFilter>();

        private TaskFilter _selectedTaskFilter;
        public TaskFilter SelectedTaskFilter
        {
            get => _selectedTaskFilter;
            set
            {
                if (SetField(ref _selectedTaskFilter, value))
                {
                    _userSettings.Data!.LastSelectedFilterIndex = (int)value;

                    TaskPanels = GetTaskPanelsFromDb();
                }
            }
        }

        private DateTime? _selectedFilterDate;
        public DateTime? SelectedFilterDate
        {
            get => _selectedFilterDate;
            set
            {
                if (SetField(ref _selectedFilterDate, value))
                {
                    _userSettings.Data!.LastSelectedOverdueToDate = value;

                    TaskPanels = GetTaskPanelsFromDb();
                }
            }
        }

        private bool _isFilterDateEnabled;

        public bool IsFilterDateEnabled
        {
            get => _isFilterDateEnabled;
            set
            {
                SetField(ref _isFilterDateEnabled, value);

                if (!value)
                {
                    SelectedFilterDate = null;

                    _userSettings.Data!.LastSelectedOverdueToDate = null;

                    OnPropertyChanged(nameof(SelectedFilterDate));
                }
            }
        }


        private List<TaskPanel>? _taskPanels;
        public List<TaskPanel>? TaskPanels
        {
            get => _taskPanels;
            set => SetField(ref _taskPanels, value);
        }

        private List<TaskPanel> GetTaskPanelsFromDb()
        {
            var tasks = _db.GetTasks().Select(x => new TaskPanel(x, RemoveTaskCommand)).ToList();

            switch (SelectedTaskFilter)
            {
                case TaskFilter.All:
                    break;
                case TaskFilter.InProgress:
                    tasks = tasks.Where(x => x.Task.Status == TaskStatus.InProgress).ToList();
                    break;
                case TaskFilter.Completed:
                    tasks = tasks.Where(x => x.Task.Status == TaskStatus.Completed).ToList();
                    break;
                case TaskFilter.OverDue:
                    tasks = tasks.Where(x => x.Task.IsOverdue).ToList();
                    break;
            }

            if (SelectedFilterDate != null)
            {
                tasks = tasks
                    .Where(x => x.Task.DueDate <= SelectedFilterDate.Value.AddDays(1))
                    .ToList();
            }

            return tasks;
        }

        public void OnClosing()
        {
            _db.SaveAll();
        }

        #region Commands
        private ICommand? _addTaskCommand;
        public ICommand AddTaskCommand => _addTaskCommand ??= new RelayCommand(x =>
        {
            try
            {
                var task = new MyTask();
                _db.UpsertTask(task);

                TaskPanels = GetTaskPanelsFromDb();

                _statusBarManager.ChangeStatusBar_Good("Task added", _statusDefMessage);
            }
            catch
            {
                _statusBarManager.ChangeStatusBar_Bad("Error adding task", _statusDefMessage);
            }
        });

        private ICommand? _refreshTasksCommand;
        public ICommand RefreshTasksCommand => _refreshTasksCommand ??= new RelayCommand(x =>
        {
            try
            {
                _db.RefreshAll();

                TaskPanels = GetTaskPanelsFromDb();

                _statusBarManager.ChangeStatusBar_Good("Tasks refreshed", _statusDefMessage);
            }
            catch
            {
                _statusBarManager.ChangeStatusBar_Bad("Error refreshing tasks", _statusDefMessage);
            }
        });

        private ICommand? _saveCommand;
        public ICommand SaveCommand => _saveCommand ??= new RelayCommand(x =>
        {
            try
            {
                _db.SaveAll();

                _statusBarManager.ChangeStatusBar_Good("Tasks saved", _statusDefMessage);
            }
            catch
            {
                _statusBarManager.ChangeStatusBar_Bad("Error saving tasks", _statusDefMessage);
            }
        });

        private ICommand? _sortTasksCommand;
        public ICommand SortTasksCommand => _sortTasksCommand ??= new RelayCommand(x =>
        {
            try
            {
                TaskPanels = TaskPanels?.OrderBy(y => y.Task.DueDate).ToList();

                _statusBarManager.ChangeStatusBar_Info("Tasks sorted", _statusDefMessage);
            }
            catch
            {
                _statusBarManager.ChangeStatusBar_Bad("Error sorting tasks", _statusDefMessage);
            }
        });

        private ICommand? _changeDatabaseFile;
        public ICommand ChangeDatabaseFile => _changeDatabaseFile ??= new RelayCommand(x =>
        {
            try
            {
                var dialog = new OpenFileDialog
                {
                    DefaultExt = ".db",
                    Filter = "Database Files (*.db)|*.db"
                };

                if (dialog.ShowDialog() == true)
                {

                    var file = dialog.FileName;

                    if (System.IO.Path.GetExtension(file) != ".db")
                    {
                        _statusBarManager.ChangeStatusBar_Bad("Chosen file is incorrect - must be .db", _statusDefMessage);
                        return;
                    }

                    if (!MyTasksDbSqlite.IsSqliteDb(file))
                    {
                        _statusBarManager.ChangeStatusBar_Bad("Chosen file is not a sqlite database", _statusDefMessage);
                    }

                    try
                    {
                        _userSettings.Data!.DatabaseFilePath = file;
                        _db = new MyTasksDbSqlite();

                        _db.RefreshAll();

                        var properties = GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);

                        foreach (var property in properties)
                            OnPropertyChanged(nameof(property));
                    }
                    catch
                    {
                        _statusBarManager.ChangeStatusBar_Bad("Error connecting to database", _statusDefMessage);
                    }

                    TaskPanels = GetTaskPanelsFromDb();

                    _statusBarManager.ChangeStatusBar_Good("Database file changed", _statusDefMessage);
                }
            }
            catch
            {
                _statusBarManager.ChangeStatusBar_Bad("Error changing database file", _statusDefMessage);
            }
        });


        public ICommand RemoveTaskCommand => new RelayCommand(x =>
        {
            try
            {
                if (x is MyTask task)
                {
                    /*var result = MessageBox.Show($"Are you sure you want to permamently remove task => {task}?", "Remove Task", MessageBoxButton.YesNo);

                    if (result != MessageBoxResult.Yes)
                    {
                        _statusBarManager.ChangeStatusBar_Info("Canceled removing task", _statusDefMessage);
                        return;
                    }*/

                    _db.RemoveTask(task);

                    TaskPanels = GetTaskPanelsFromDb();

                    _statusBarManager.ChangeStatusBar_Good($"Task removed => {task}", _statusDefMessage);
                }
                else
                {
                    _statusBarManager.ChangeStatusBar_Bad("Error removing task", _statusDefMessage);
                }
            }
            catch
            {
                _statusBarManager.ChangeStatusBar_Bad("Error removing task", _statusDefMessage);
            }
        });
#endregion

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