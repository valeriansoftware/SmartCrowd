using SmartCrowd.Core.Scheduler;

namespace SmartCrowd.Core;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("=== SmartCrowd - Демонстрация системы расписаний и сценариев ===");
        Console.WriteLine();
        
        try
        {
            IntegratedSchedulerDemo.RunDemo();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка при выполнении демонстрации: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
        }
        
        Console.WriteLine("\nНажмите любую клавишу для выхода...");
        Console.ReadKey();
    }
} 