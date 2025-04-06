using System.IO;
using Serilog;
using Serilog.Formatting.Json;
using System;
class Program
{
    static void Main(string[] args)
    {
        // Настройка Serilog с JSON-форматированием
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.Console(new JsonFormatter())  // Логи в консоль в формате JSON
            .WriteTo.File(
                new JsonFormatter(),
                "logs/log.json")
            .CreateLogger();

        try
        {
            Log.Information("Приложение запущено");

            Console.WriteLine("Введите ваше имя:");
            string name = Console.ReadLine();

            Console.WriteLine("Введите ваш возраст:");
            if (!int.TryParse(Console.ReadLine(), out int age))
            {
                Log.Warning("Некорректный ввод возраста");
                Console.WriteLine("Возраст должен быть числом!");
                return;
            }

            // Логируем полученные данные
            Log.Information("Пользователь {@UserData}", new { Name = name, Age = age });
            Console.WriteLine($"Данные сохранены. Имя: {name}, Возраст: {age}");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Произошла ошибка");
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }
}