using System.Reflection.Metadata.Ecma335;
using System.Text.Json;
using Spectre.Console;

namespace Console_Task_Tracker;

//инструкция для json
public class TaskItem
{
    public int ID { get; set; }
    public string Name { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime DueDate { get; set; }
    public bool IsCompleted { get; set; }
}

class Program
{
    static async Task Main(string[] args)
    {
        List<TaskItem> myTasks = CallJson();
        
        await LoadTerminal(myTasks);
        
        WorkingTable(myTasks);
    }

    //Вывод
    static void PrintTasks(List<TaskItem> list)
    {
        var sortedList = list
            .OrderBy(t => t.IsCompleted ? 3 : (t.DueDate < DateTime.Now ? 2 : 1))
            .ThenBy(t => t.ID)
            .ToList();
        
        Separation();
        if (list.Count == 0)
        {
            AnsiConsole.MarkupLine("[yellow]Список задач пуст[/]");
            return;
        }

        var table = new Table()
            .Border(TableBorder.Rounded)
            .BorderColor(Color.Gray)
            .Title("[yellow]ВАШИ ЗАДАЧИ[/]");

        table.AddColumn("[bold]ID[/]");
        table.AddColumn("[bold]Статус[/]");
        table.AddColumn("[bold]Название[/]");
        table.AddColumn("[bold]Дата создания[/]");
        table.AddColumn("[bold]Дата дедлайна[/]");

        foreach (var task in sortedList)
        {
            string status = task.IsCompleted ? "[green]✓[/]" :
                task.DueDate < DateTime.Now ? "[red]✗[/]" : "[white]✗[/]";

            string color = "white";
            if (task.IsCompleted)
                color = "grey";
            else if (task.DueDate < DateTime.Now)
                color = "red";

            table.AddRow(
                $"[{color}]{task.ID.ToString()}[/]",
                status,
                $"[{color}]{Markup.Escape(task.Name)}[/]",
                $"[{color}]{task.CreatedDate:dd.MM}[/]",
                $"[{color}]{task.DueDate:dd.MM}[/]"
                );
            
            Console.ResetColor();
        }
        AnsiConsole.Write(table);
        
        Separation();
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
    
    //Разделение
    static void Separation()
    {
        Console.WriteLine(new string('-', 72));
    }
    
    //Загрузка консоли
    static async Task LoadTerminal(List<TaskItem> list)
    {
        Console.Clear();

        //Создаем Логотип (Figlet)
        //Используем стандартный шрифт. Цвет Aqua.
        var logo = new FigletText("TASK TRACKER")
            .Color(Color.Aqua)
            .LeftJustified();
        
        AnsiConsole.Write(logo);

        //Линия-разделитель (Rule)
        var rule = new Rule("[yellow]Personal Productivity Tool v1.0[/]")
            .LeftJustified()
            .RuleStyle("grey");
            
        AnsiConsole.Write(rule);

        AnsiConsole.WriteLine();

        //Анимация загрузки (Status)
        await AnsiConsole.Status()
            .Spinner(Spinner.Known.Dots)
            .SpinnerStyle(Style.Parse("cyan"))
            .StartAsync("Синхронизация с базой данных...", async ctx => 
            {
                //Имитируем чтение JSON файла
                await Task.Delay(1200); 
                AnsiConsole.MarkupLine("[bold green]✓[/] Файл [underline]name.json[/] успешно прочитан.");
                
                await Task.Delay(400);
                AnsiConsole.MarkupLine("[bold green]✓[/] Данные десериализованы.");
            });

        AnsiConsole.WriteLine();

        //Подсчет статистики из списка задач
        int total = list.Count;
        int completed = list.Count(t => t.IsCompleted);
        int active = total - completed;

        //Отрисовка таблицы со сводкой
        var table = new Table()
            .Border(TableBorder.Rounded)
            .BorderColor(Color.Grey);

        //Добавление колонки
        table.AddColumn("[bold white]Категория[/]");
        table.AddColumn(new TableColumn("[bold white]Количество[/]").Centered());

        //Заполнение данными
        table.AddRow("Всего задач", $"[blue]{total}[/]");
        table.AddRow("Активные задачи", $"[yellow]{active}[/]");
        table.AddRow("Выполненные", $"[green]{completed}[/]");

        //Если есть задачи, которые просрочены (дедлайн раньше текущего времени)
        int expired = list.Count(t => !t.IsCompleted && t.DueDate < DateTime.Now);
        if (expired > 0)
        {
            table.AddRow("[red]Просрочено[/]", $"[bold white on red] {expired} [/]");
        }

        AnsiConsole.Write(table);
        
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("[italic grey]Нажмите любую клавишу, чтобы открыть список задач...[/]");
    }
    
    //Работа с таблицей
    static void WorkingTable(List<TaskItem> list)
    {
        while (true)
        {
            Console.Clear();
            PrintTasks(list);
        
            //Редактирование задач
            var action = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("[yellow]Что вы хотите сделать?[/]")
                    .PageSize(10)
                    .MoreChoicesText("[grey](Листайте вверх и вниз для просмотра всех вариантов)[/]")
                    .AddChoices(new[] {
                        "Добавить задачу",
                        "Изменить задачу",
                        "Удалить задачу",
                        "Изменить статус (выполнено/не выполнено)",
                        "Выход"
                    }));

            switch (action)
            {
                case "Добавить задачу":
                    AddTask(list);
                    hold();
                    break;
                
                case "Изменить задачу":
                    ActionSelection(list);
                    hold();
                    break;
                
                case "Удалить задачу":
                    Delete(list);
                    hold();
                    break;
                
                case "Изменить статус (выполнено/не выполнено)":
                    IsComplete(list);
                    hold();
                    break;
                
                case "Выход":
                    Console.WriteLine("Программа завершила работу");
                    return;
            }
        }
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

        DateTime dueDate;
            while (true)
            {
                Console.Write("Введите дату дедлайна (например 31.12 или 12-31): ");
                string dateInput = Console.ReadLine()?.Trim();

                if (string.IsNullOrEmpty(dateInput))
                {
                    dueDate = DateTime.Now.AddDays(7);
                    break;
                }

                if (DateTime.TryParse(dateInput, out dueDate))
                    break;
                else
                    Console.WriteLine("[Ошибка] Неверный формат даты. Укажите дату ещё раз или нажмите Enter");
            }
            
        //новая задача
        TaskItem newTask = new TaskItem()
        {
            ID = list.Any() ? list.Max(t => t.ID) + 1 : 1,
            Name = title,
            CreatedDate = DateTime.Now,
            DueDate = dueDate
        };
        
        //добавить в список
        list.Add(newTask);
        
        //Сохранить полученый список 
        SavingChanges(list);
        
        //Сообщение об успешности
        Console.WriteLine($"Задача добавлена: {newTask.Name}");
        Console.WriteLine($"Дедлайн: {dueDate:dd.MM.yyyy}");
        Separation();

    }

    //Изменение задачи
    static void ActionSelection(List<TaskItem> list)
    {
       Console.Write("Введите номер задачи который хотите изменить: ");
       if (!int.TryParse(Console.ReadLine(), out int id))
       {
           Console.WriteLine("[Ошибка] Введите корретное значение. ID состоит из чисел");
           Separation();
           return;
       }

       var TaskToUpdate = list.FirstOrDefault(t => t.ID == id);

       if (TaskToUpdate == null)
       {
           Console.WriteLine($"[Ошибка] Задача с номером {id} не найдена в списке");
           Separation();
           return;
       }
       
       Separation();
       Console.WriteLine($"Текущее название: {TaskToUpdate.Name}");
       Console.Write("Введите новое название: ");
       
       string NewName = Console.ReadLine()?.Trim();

       if (string.IsNullOrEmpty(NewName))
       {
           Console.WriteLine("[Ошибка] Новое название не может быть пустым.");
           Separation();
           return;
       }

       TaskToUpdate.Name = NewName;

       try
       {
           SavingChanges(list);
           Separation();
           Console.WriteLine($"Задача {id} обновленна на: {NewName}");
           Separation();

       }
       catch (Exception e)
       {
           Console.WriteLine($"[Ошибка при сохранении в файл]: {e.Message}");
           Separation();

       }
    }
    
    
    //Удаление задачи
    static void Delete(List<TaskItem> list) 
    {
            Console.Write("Напишите какую задачу вы хотите удалить: ");
            string input = Console.ReadLine();

            if (string.IsNullOrEmpty(input))
            {
                Console.WriteLine("[Ошибка] Номер задачи не может быть пустым");
                Separation();
                return;
            }

            
            if (int.TryParse(input, out int id))
            {
                if (list.Any(t => t.ID == id))
                {
                    list.RemoveAll(t => t.ID == id);
                    Separation();

                    SavingChanges(list);

                    Console.WriteLine($"Задача {id} была успешна удалена!");
                    Separation();
                }
                else
                {
                    Separation();
                    Console.WriteLine("[Ошибка] Вы ввели некорретный ID");
                    Separation();
                }
            }
            else
            {
                Console.WriteLine("[Ошибка] Вы ввели некорретный ID. ID состоит из чисел");
                Separation();
            }
    }

    //Статус выполнено/не выполнено
    static void IsComplete(List<TaskItem> list)
    {
        Console.Write("Введите номер задачи которой нужно присвоить/убрать метку выполнения: ");
        if (!int.TryParse(Console.ReadLine(), out int id))
        {
            Console.WriteLine("[Ошибка] Введите корректное ID, он состоит из чисел");
            Separation();
            return;
        }

        var TaskToUpdate = list.FirstOrDefault(t => t.ID == id);

        if (TaskToUpdate == null)
        {
            Console.WriteLine($"[Ошибка] Задача с номером {id} не найдена в списке");
            Separation();
            return;
        }
        
        TaskToUpdate.IsCompleted = !TaskToUpdate.IsCompleted;

        Separation();
        string status = TaskToUpdate.IsCompleted ? "выполнена" : "не выполнена";
        Console.WriteLine($"Задача успешно помечена как: {status}");
        Separation();
        
        SavingChanges(list);
    }
}