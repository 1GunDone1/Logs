using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Serilog;
using Serilog.Context;
using Serilog.Events;

public class TaskItem
{
    public int Id { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public bool IsCompleted { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }

    public TaskItem(int id, string title, string description)
    {
        Id = id;
        Title = title;
        Description = description;
        IsCompleted = false;
        CreatedAt = DateTime.Now;
        CompletedAt = null;
    }

    public override string ToString()
    {
        string status = IsCompleted ? $"Выполнена ({CompletedAt})" : "В процессе";
        return $"[{Id}] {Title} - {Description} ({status})";
    }
}

public class TaskManager
{
    private List<TaskItem> tasks = new List<TaskItem>();
    private int nextId = 1;

    public void AddTask(string title, string description)
    {
        using (LogContext.PushProperty("Operation", "AddTask"))
        using (var timer = new OperationTimer("Добавление задачи"))
        {
            try
            {
                Log.Information("Начало добавления задачи: {Title}", title);

                if (string.IsNullOrWhiteSpace(title))
                {
                    Log.Warning("Пустой заголовок задачи");
                    throw new ArgumentException("Заголовок задачи не может быть пустым");
                }

                var task = new TaskItem(nextId++, title, description);
                tasks.Add(task);

                Log.Information("Задача успешно добавлена: {@Task}", task);
                Trace.WriteLine($"Детали задачи: ID={task.Id}, CreatedAt={task.CreatedAt}");

                timer.Complete();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Ошибка при добавлении задачи");
                throw;
            }
        }
    }

    public void ViewAllTasks()
    {
        using (LogContext.PushProperty("Operation", "ViewAllTasks"))
        using (var timer = new OperationTimer("Просмотр всех задач"))
        {
            try
            {
                Log.Information("Начало получения списка задач");

                if (tasks.Count == 0)
                {
                    Log.Information("Список задач пуст");
                    Console.WriteLine("Список задач пуст");
                    return;
                }

                Console.WriteLine("\nСписок задач:");
                foreach (var task in tasks)
                {
                    Console.WriteLine(task);
                    Trace.WriteLine($"Отображена задача: {task}");
                }

                Log.Information("Успешно получено {TaskCount} задач", tasks.Count);
                timer.Complete();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Ошибка при получении списка задач");
                throw;
            }
        }
    }

    public void DeleteTask(int id)
    {
        using (LogContext.PushProperty("Operation", "DeleteTask"))
        using (var timer = new OperationTimer("Удаление задачи"))
        {
            try
            {
                Log.Information("Начало удаления задачи с ID: {Id}", id);

                var task = tasks.Find(t => t.Id == id);
                if (task == null)
                {
                    Log.Error("Задача с ID {Id} не найдена", id);
                    throw new ArgumentException($"Задача с ID {id} не найдена");
                }

                tasks.Remove(task);
                Log.Information("Задача успешно удалена: {@Task}", task);

                timer.Complete();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Ошибка при удалении задачи");
                throw;
            }
        }
    }

    public void MarkAsCompleted(int id)
    {
        using (LogContext.PushProperty("Operation", "MarkAsCompleted"))
        using (var timer = new OperationTimer("Отметка задачи как выполненной"))
        {
            try
            {
                Log.Information("Начало отметки задачи с ID {Id} как выполненной", id);

                var task = tasks.Find(t => t.Id == id);
                if (task == null)
                {
                    Log.Error("Задача с ID {Id} не найдена", id);
                    throw new ArgumentException($"Задача с ID {id} не найдена");
                }

                if (task.IsCompleted)
                {
                    Log.Warning("Задача с ID {Id} уже была выполнена", id);
                    Console.WriteLine("Эта задача уже была выполнена");
                    return;
                }

                task.IsCompleted = true;
                task.CompletedAt = DateTime.Now;
                Log.Information("Задача успешно отмечена как выполненная: {@Task}", task);

                timer.Complete();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Ошибка при отметке задачи как выполненной");
                throw;
            }
        }
    }
}

public class OperationTimer : IDisposable
{
    private readonly Stopwatch _stopwatch;
    private readonly string _operationName;
    private bool _completed;

    public OperationTimer(string operationName)
    {
        _operationName = operationName;
        _stopwatch = Stopwatch.StartNew();
        Log.Information("Начало операции: {OperationName}", _operationName);
    }

    public void Complete()
    {
        _completed = true;
    }

    public void Dispose()
    {
        _stopwatch.Stop();
        var status = _completed ? "Успешно" : "Не завершено";
        Log.Information("Окончание операции {OperationName}. Статус: {Status}. Время выполнения: {ElapsedMs} мс",
            _operationName, status, _stopwatch.ElapsedMilliseconds);
    }
}

class Program
{
    static string[] menuOptions = {
        "1 - Добавить задачу",
        "2 - Просмотреть все задачи",
        "3 - Удалить задачу",
        "4 - Отметить задачу как выполненную",
        "5 - Выйти"
    };

    static void Main()
    {
        // Настройка логирования без отображения AppSession
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .Enrich.FromLogContext()
            .WriteTo.Console(
                outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
            .WriteTo.File("task-manager-.log",
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}",
                rollingInterval: RollingInterval.Day)
            .CreateLogger();

        Trace.Listeners.Add(new TextWriterTraceListener(Console.Out));
        Trace.AutoFlush = true;

        Log.Information("Приложение запущено");
        Trace.WriteLine("Запуск менеджера задач");

        var manager = new TaskManager();

        while (true)
        {
            try
            {
                Console.Clear();
                ShowMemoryUsage();

                Console.WriteLine("\nМенеджер задач");
                Console.WriteLine("--------------\n");

                Console.WriteLine("Выберите действие:\n");
                foreach (var option in menuOptions)
                {
                    Console.WriteLine(option);
                }

                Console.Write("\nВаш выбор: ");
                string choice = Console.ReadLine();
                Log.Information("Пользователь выбрал: {Choice}", choice);

                switch (choice)
                {
                    case "1":
                        Console.Write("\nВведите заголовок задачи: ");
                        string title = Console.ReadLine();
                        Console.Write("Введите описание задачи: ");
                        string description = Console.ReadLine();
                        manager.AddTask(title, description);
                        Console.WriteLine("\nЗадача успешно добавлена");
                        Pause();
                        break;

                    case "2":
                        manager.ViewAllTasks();
                        Pause();
                        break;

                    case "3":
                        Console.Write("\nВведите ID задачи для удаления: ");
                        if (int.TryParse(Console.ReadLine(), out int deleteId))
                        {
                            manager.DeleteTask(deleteId);
                            Console.WriteLine("\nЗадача успешно удалена");
                        }
                        else
                        {
                            Log.Warning("Некорректный ввод ID для удаления");
                            Console.WriteLine("\nНекорректный ID");
                        }
                        Pause();
                        break;

                    case "4":
                        Console.Write("\nВведите ID задачи для отметки как выполненной: ");
                        if (int.TryParse(Console.ReadLine(), out int completeId))
                        {
                            manager.MarkAsCompleted(completeId);
                            Console.WriteLine("\nЗадача отмечена как выполненная");
                        }
                        else
                        {
                            Log.Warning("Некорректный ввод ID для отметки");
                            Console.WriteLine("\nНекорректный ID");
                        }
                        Pause();
                        break;

                    case "5":
                        Log.Information("Приложение завершает работу");
                        Trace.WriteLine("Выход из программы");
                        Console.WriteLine("\nДо свидания!");
                        Environment.Exit(0);
                        break;

                    default:
                        Log.Warning("Неизвестная команда: {Choice}", choice);
                        Console.WriteLine("\nНеизвестная команда");
                        Pause();
                        break;
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Ошибка в главном цикле: {Message}", ex.Message);
                Console.WriteLine($"\nОшибка: {ex.Message}");
                Pause();
            }
        }
    }

    static void Pause()
    {
        Console.WriteLine("\nНажмите любую клавишу, чтобы продолжить...");
        Console.ReadKey(true);
    }

    public static void ShowMemoryUsage()
    {
        var process = Process.GetCurrentProcess();
        long memoryMB = process.WorkingSet64 / 1024 / 1024;
        double cpuLoad = GetCpuUsageForProcess();

        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine($"Память: {memoryMB} MB | CPU: {cpuLoad:F2}%");
        Console.ResetColor();

        Log.Information("Использование ресурсов: Память: {Memory} MB | CPU: {Cpu}%", memoryMB, cpuLoad);
        Trace.WriteLine($"Память: {memoryMB} MB | CPU: {cpuLoad:F2}%");
    }

    static double GetCpuUsageForProcess()
    {
        var proc = Process.GetCurrentProcess();
        TimeSpan startCpu = proc.TotalProcessorTime;
        Thread.Sleep(100);
        TimeSpan endCpu = proc.TotalProcessorTime;

        return (endCpu - startCpu).TotalMilliseconds / (Environment.ProcessorCount * 100.0);
    }
}
