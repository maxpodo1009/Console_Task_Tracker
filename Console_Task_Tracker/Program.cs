using System.Text.Json;

namespace Console_Task_Tracker;

//инструкция для json
public class TaskItem
{
    public int ID { get; set; }
    public string Name { get; set; }
    public DateTime CreatedDate { get; set; } 
}

class Program
{
    static void Main(string[] args)
    {
        List<TaskItem> myTasks = CallJson();

        while (true)
        {
            Console.Clear();
            PrintTasks(myTasks);
        
            //Редактирование задач
            Console.Write("Введите цифру которая будет означать ваше действия для задач \n" +
                          "1. Добавить \n" +
                          "2. Изменить \n" +
                          "3. Удалить \n" +
                          "4. Выйти из программы \n" +
                          "Введите любой другой символ или нажмите Enter чтобы закрыть программу: ");
            string number = Console.ReadLine();
            Console.WriteLine(new string('-', 45));

            switch (number)
            {
                case "1":
                {
                    AddTask(myTasks);
                    hold();
                    break;
                }
                case "2":
                {
                    ActionSelection(myTasks);
                    hold();
                    break;
                }
                case "3":
                {
                    Delete(myTasks);
                    hold();
                    break;
                }
                case "4":
                {
                    Console.WriteLine("Программа завершила работу");
                    return;
                }
                default:
                {
                    Console.WriteLine("[Ошибка]: Неккоретное значение");
                    hold();
                    break;
                }
            }
        }
    }

    //Вывод
    static void PrintTasks(List<TaskItem> list)
    {
        Console.WriteLine(new string('-', 45));
        Console.WriteLine("{0, -5} | {1, -20} | {2, -15}", "ID", "Название", "Дата создания");
        if (list.Count == 0) Console.WriteLine("Пусто");
        foreach (var task in list)  
        {
            Console.WriteLine("{0, -5} | {1, -20} | {2, -15:dd.MM HH:mm}", task.ID, task.Name,  task.CreatedDate);
        }
        Console.WriteLine(new string('-', 45));
    }

    //Сериализация/Десириализация
    static List<TaskItem> CallJson()
    {
        try
        {
            if (!File.Exists("name.json")) return new List<TaskItem>();

            string json = File.ReadAllText("name.json");
            return JsonSerializer.Deserialize<List<TaskItem>>(json) ?? new List<TaskItem>();
        }
        catch (Exception e)
        {
            Console.WriteLine($"[Ошибка при загрузке]: {e.Message}");
            return new List<TaskItem>();
        }
    }

    //Сохранение изменений
    static void SavingChanges(List<TaskItem> list)
    {
        string updatedJson = JsonSerializer.Serialize(list);
        File.WriteAllText("name.json", updatedJson);
    }
    
    //Удержание консоли
    static void hold()
    {
        Console.WriteLine("Нажмите Enter чтобы продолжить");
        Console.ReadKey();
    }
    
    //Добавление задачи
    static void AddTask(List<TaskItem> list)
    {
        Console.Write("Введите имя задачи: ");
        string title = Console.ReadLine()?.Trim();

        if (string.IsNullOrEmpty(title))
        {
            Console.WriteLine("[Ошибка] Имя задачи не может быть пустым");
            return;
        }
 
        //новая задача
        TaskItem newTask = new TaskItem()
        {
            ID = list.Any() ? list.Max(t => t.ID) + 1 : 1,
            Name = title,
            CreatedDate = DateTime.Now
        };
        
        //добавить в список
        list.Add(newTask);
        
        //Сохранить полученый список 
        SavingChanges(list);
        
        //Сообщение об успешности
        Console.WriteLine($"Задача добавлена: {newTask.Name}");
        Console.WriteLine(new string('-', 45));

    }

    //Изменение задачи
    static void ActionSelection(List<TaskItem> list)
    {
       Console.Write("Введите номер задачи который хотите изменить: ");
       if (!int.TryParse(Console.ReadLine(), out int id))
       {
           Console.WriteLine("[Ошибка] Введите корретное значение. ID состоит из чисел");
           Console.WriteLine(new string('-', 45));
           return;
       }

       var TaskToUpdate = list.FirstOrDefault(t => t.ID == id);

       if (TaskToUpdate == null)
       {
           Console.WriteLine($"[Ошибка] Задача с номером {id} не найдена в списке");
           Console.WriteLine(new string('-', 45));
           return;
       }
       
       Console.WriteLine(new string('-', 45));
       Console.WriteLine($"Текущее название: {TaskToUpdate.Name}");
       Console.Write("Введите новое название: ");
       
       string NewName = Console.ReadLine()?.Trim();

       if (string.IsNullOrEmpty(NewName))
       {
           Console.WriteLine("[Ошибка] Новое название не может быть пустым.");
           Console.WriteLine(new string('-', 45));
           return;
       }

       TaskToUpdate.Name = NewName;

       try
       {
           SavingChanges(list);
           Console.WriteLine(new string('-', 45));
           Console.WriteLine($"Задача {id} обновленна на: {NewName}");
           Console.WriteLine(new string('-', 45));

       }
       catch (Exception e)
       {
           Console.WriteLine($"[Ошибка при сохранении в файл]: {e.Message}");
           Console.WriteLine(new string('-', 45));

       }
    }
    
    
    //Удаление задачи
    static void Delete(List<TaskItem> list) 
    {
        while (true)
        {
            Console.Write("Напишите какую задачу вы хотите удалить: ");
            string input = Console.ReadLine();

            if (string.IsNullOrEmpty(input))
            {
                Console.WriteLine("[Ошибка] Номер задачи не может быть пустым");
                Console.WriteLine(new string('-', 45));
                break;
            }

            
            if (int.TryParse(input, out int id))
            {
                if (list.Any(t => t.ID == id))
                {
                    list.RemoveAll(t => t.ID == id);
                    Console.WriteLine(new string('-', 45));

                    SavingChanges(list);

                    Console.WriteLine($"Задача {id} была успешна удалена!");
                    Console.WriteLine(new string('-', 45));
                    break;
                }
                else
                {
                    Console.WriteLine(new string('-', 45));
                    Console.WriteLine("[Ошибка] Вы ввели некорретный ID");
                    Console.WriteLine(new string('-', 45));

                    break;
                }
            }
            else
            {
                Console.WriteLine("[Ошибка] Вы ввели некорретный ID. ID состоит из чисел");
                Console.WriteLine(new string('-', 45));
                break;
            }
        }
    }
}