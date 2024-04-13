using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace TodoList
{
    public class MyTaskContext : DbContext
    {
        public DbSet<MyTask> Tasks { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            try
            {
                optionsBuilder.UseSqlite($"Data Source={UserSettings.Instance.Data!.DatabaseFilePath}");
            }
            catch
            {
                optionsBuilder.UseSqlite("Data Source=myTasks.db");
            }
        }
    }

    /// <summary>
    /// Sqlite database for MyTasks
    /// </summary>
    public class MyTasksDbSqlite : IDisposable
    {
        private readonly MyTaskContext MyDb = new();

        public MyTasksDbSqlite()
        {
            MyDb.Database.EnsureCreated();
        }

        public IEnumerable<MyTask> GetTasks()
        {
            //return MyDb.Tasks.ToList();
            return [.. MyDb.Tasks];
        }

        public void UpsertTask(MyTask task)
        {
            if (MyDb.Tasks.Contains(task))
                MyDb.Tasks.Update(task);
            else
                MyDb.Tasks.Add(task);

            MyDb.SaveChanges();
        }

        public void RefreshAll()
        {
            var entitiesList = MyDb.ChangeTracker.Entries().ToList();
            foreach (var entity in entitiesList)
            {
                entity.Reload();
            }
        }

        public void SaveAll()
        {
            MyDb.SaveChanges();
        }

        public void RemoveTaskById(int id)
        {
            var task = MyDb.Tasks.Find(id);

            if (task != null)
                MyDb.Tasks.Remove(task);
        }

        public void RemoveTask(MyTask task)
        {
            MyDb.Tasks.Remove(task);

            MyDb.SaveChanges();
        }

        public void Dispose()
        {
            MyDb.Dispose();
        }


        public static bool IsSqliteDb(string pathToFile)
        {
            var result = false;

            if (!File.Exists(pathToFile))
                return result;

            using var stream = new FileStream(pathToFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

            var header = new byte[16];

            for (var i = 0; i < 16; i++)
                header[i] = (byte)stream.ReadByte();

            result = Encoding.UTF8.GetString(header).Contains("SQLite format 3");

            stream.Close();

            return result;
        }
    }
}
