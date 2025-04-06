using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

// Класс для представления задачи
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

    // Настройка логирования
    public static void SetupLogging()
    {
        Trace.Listeners.Clear();

        // Консольный слушатель
        Trace.Listeners.Add(new ConsoleTraceListener());

        // Файловый слушатель
        string logFilePath = "tasks.log";
        Trace.Listeners.Add(new TextWriterTraceListener(logFilePath));

        Trace.AutoFlush = true;

        Trace.TraceInformation($"Логирование инициализировано: {DateTime.Now}");
    }

    // Добавление задачи
    public void AddTask(string title, string description)
    {
        Trace.TraceInformation($"Попытка добавить задачу: {title}");

        if (string.IsNullOrWhiteSpace(title))
        {
            Trace.TraceWarning("Пустой заголовок задачи");
            throw new ArgumentException("Заголовок задачи не может быть пустым");
        }

        var task = new TaskItem(nextId++, title, description);
        tasks.Add(task);

        Trace.TraceInformation($"Задача добавлена: {task}");
        Trace.WriteLine($"Детали задачи: ID={task.Id}, CreatedAt={task.CreatedAt}");
    }

    // Просмотр всех задач
    public void ViewAllTasks()
    {
        Trace.TraceInformation("Запрос списка всех задач");

        if (tasks.Count == 0)
        {
            Trace.TraceWarning("Список задач пуст");
            Console.WriteLine("Список задач пуст");
            return;
        }

        Console.WriteLine("\nСписок задач:");
        foreach (var task in tasks)
        {
            Console.WriteLine(task);
            Trace.WriteLine($"Отображена задача: {task}");
        }
    }

    // Удаление задачи
    public void DeleteTask(int id)
    {
        Trace.TraceInformation($"Попытка удаления задачи с ID: {id}");

        var task = tasks.Find(t => t.Id == id);
        if (task == null)
        {
            Trace.TraceError($"Задача с ID {id} не найдена");
            throw new ArgumentException($"Задача с ID {id} не найдена");
        }

        tasks.Remove(task);
        Trace.TraceInformation($"Задача удалена: {task}");
    }

    // Отметка задачи как выполненной
    public void MarkAsCompleted(int id)
    {
        Trace.TraceInformation($"Попытка отметить задачу с ID {id} как выполненную");

        var task = tasks.Find(t => t.Id == id);
        if (task == null)
        {
            Trace.TraceError($"Задача с ID {id} не найдена");
            throw new ArgumentException($"Задача с ID {id} не найдена");
        }

        if (task.IsCompleted)
        {
            Trace.TraceWarning($"Задача с ID {id} уже была выполнена");
            Console.WriteLine("Эта задача уже была выполнена");
            return;
        }

        task.IsCompleted = true;
        task.CompletedAt = DateTime.Now;
        Trace.TraceInformation($"Задача отмечена как выполненная: {task}");
    }
}

class Program
{
    static void Main()
    {
        // Инициализация логирования
        TaskManager.SetupLogging();

        Trace.TraceInformation("Приложение запущено");

        var manager = new TaskManager();

        Console.WriteLine("Менеджер задач");
        Console.WriteLine("--------------");

        while (true)
        {
            try
            {
                Console.WriteLine("\nВыберите действие:");
                Console.WriteLine("1. Добавить задачу");
                Console.WriteLine("2. Просмотреть все задачи");
                Console.WriteLine("3. Удалить задачу");
                Console.WriteLine("4. Отметить задачу как выполненную");
                Console.WriteLine("5. Выйти");

                Console.Write("Ваш выбор: ");
                string choice = Console.ReadLine();
                Trace.WriteLine($"Пользователь выбрал: {choice}");

                switch (choice)
                {
                    case "1":
                        Console.Write("Введите заголовок задачи: ");
                        string title = Console.ReadLine();
                        Console.Write("Введите описание задачи: ");
                        string description = Console.ReadLine();
                        manager.AddTask(title, description);
                        Console.WriteLine("Задача успешно добавлена");
                        break;

                    case "2":
                        manager.ViewAllTasks();
                        break;

                    case "3":
                        Console.Write("Введите ID задачи для удаления: ");
                        if (int.TryParse(Console.ReadLine(), out int deleteId))
                        {
                            manager.DeleteTask(deleteId);
                            Console.WriteLine("Задача успешно удалена");
                        }
                        else
                        {
                            Trace.TraceWarning("Некорректный ввод ID для удаления");
                            Console.WriteLine("Некорректный ID");
                        }
                        break;

                    case "4":
                        Console.Write("Введите ID задачи для отметки как выполненной: ");
                        if (int.TryParse(Console.ReadLine(), out int completeId))
                        {
                            manager.MarkAsCompleted(completeId);
                            Console.WriteLine("Задача отмечена как выполненная");
                        }
                        else
                        {
                            Trace.TraceWarning("Некорректный ввод ID для отметки");
                            Console.WriteLine("Некорректный ID");
                        }
                        break;

                    case "5":
                        Trace.TraceInformation("Приложение завершает работу");
                        Console.WriteLine("До свидания!");
                        return;

                    default:
                        Trace.TraceWarning($"Неизвестная команда: {choice}");
                        Console.WriteLine("Неизвестная команда");
                        break;
                }
            }
            catch (Exception ex)
            {
                Trace.TraceError($"Ошибка: {ex.Message}");
                Console.WriteLine($"Ошибка: {ex.Message}");
            }
        }
    }
}
