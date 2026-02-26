using System.Text.Json;

namespace Console_Task_Tracker;

public class TaskItem //инструкция для json
{
    public int ID { get; set; }
    public string Name { get; set; }
}

class Program
{
    static void Main(string[] args)
    {
        List<TaskItem> myTasks = Calljson();

        while (true)
        {
            Console.Clear();
            PrintTasks(myTasks);
        
            //Редактирование заметок
            Console.Write("Введите цифру которая будет означать ваше действия для заметок \n" +
                          "1. Добавить \n" +
                          "2. Изменить \n" +
                          "3. Удалить \n" +
                          "Введите любой другой символ или нажмите Enter чтобы закрыть программу: ");
            string number = Console.ReadLine();
            Console.WriteLine("---------------------------------");
            
            if (number == "1")
                AddTask(myTasks);
        
            else if (number == "2")
                ActionSelection(myTasks);
            
            else if (number == "3")
                Delete(myTasks);
            
            else
            {
                Console.WriteLine("Программа завершила работу.");
                break;
            }
            Console.WriteLine("\nНажмите любую клавишу чтобы продолжить...");
            Console.ReadKey();
        }
    }


    static void PrintTasks(List<TaskItem> list) //вывод
    {
        Console.WriteLine("---------------------------------");
        Console.WriteLine("Вот список ваших задач: ");
        foreach (var task in list)
            Console.WriteLine($"[{task.ID}] {task.Name}");
        Console.WriteLine("---------------------------------");
    }

    
    static List<TaskItem> Calljson()//Сериализация/Десириализация
    {
        List<TaskItem> NameNote = new List<TaskItem>();
        
        if (File.Exists("name.json"))
        {
            string json = File.ReadAllText("name.json");
            NameNote = JsonSerializer.Deserialize<List<TaskItem>>(json);
            return NameNote;
        }
        else
        {
            string json = JsonSerializer.Serialize(NameNote);
            File.WriteAllText("name.json", json);
            return NameNote;
        }
        
    }


    static void ActionSelection(List<TaskItem> list)//Изменение имени
    {
        Console.Write("Введите номер заметки которую хотите изменить: ");
        int id = Convert.ToInt32(Console.ReadLine());
        bool found = false;
        Console.WriteLine("---------------------------------");

        for (int n = 0; n < list.Count; n++)
        {
            if (id == list[n].ID)
            {
                Console.Write($"Вы ввели заметку: {list[n].Name} \nВведите новое название: ");
                list[n].Name = Console.ReadLine();
                Console.WriteLine("---------------------------------");
                
                Console.Write($"Заметка обновлена: {list[n].ID} теперь называется {list[n].Name}\n");

                string updatedJson = JsonSerializer.Serialize(list);
                File.WriteAllText("name.json", updatedJson);
                
                found = true;
                break;
            }
        }
        if (!found)
            Console.WriteLine("Вы ввели некорретный номер заметки");
    }

    static void AddTask(List<TaskItem> list)//Добавление имени
    {
        Console.Write("Введите имя заметки: ");
        string? title = Console.ReadLine();
 
        //новая задача
        TaskItem newTask = new TaskItem();
        newTask.Name = title;
        newTask.ID = list.Any() ? list.Max(t => t.ID) + 1 : 1;
        
        //добавить в список
        list.Add(newTask);
        
        //Сохранить полученый список 
        string updatedJson = JsonSerializer.Serialize(list);
        File.WriteAllText("name.json", updatedJson);
    }

    static void Delete(List<TaskItem> list) //Удаление
    {
        while (true)
        {
            Console.Write("Напишите какую строку вы хотите удалить: ");
            string input = Console.ReadLine();
            
            if (input == "Exit") break;

            if (int.TryParse(input, out int id))
            {
                if (list.Any(t => t.ID == id))
                {
                    list.RemoveAll(t => t.ID == id);
                    Console.WriteLine("---------------------------------");

                    string updateJson = JsonSerializer.Serialize(list);
                    File.WriteAllText("name.json", updateJson);

                    Console.WriteLine($"Заметка {id} была успешна удалена!");
                    break;
                }
                else
                {
                    Console.WriteLine("Вы ввели некорретный ID, \n" +
                                    "Напишите Exit, если хотите выйти с метода удаления заметки, или \n" +
                                    "Нажмите Enter(либо любой другой символ) чтобы заново запустить метод удаления: ");
                    if (Console.ReadLine() == "Exit")
                        break;
                }
            }
        }
    }
}