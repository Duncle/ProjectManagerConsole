using AuthConsoleApp.Models;
using AuthConsoleApp.Persistence;
using AuthConsoleApp.Services;
using Microsoft.Extensions.DependencyInjection;
using TaskStatus = AuthConsoleApp.Models.TaskStatus;

var services = new ServiceCollection()
    .AddSingleton<IStorage, JsonFileStorage>()
    .AddSingleton<AuthService>()
    .AddSingleton<UserService>()
    .AddSingleton<TaskService>()
    .BuildServiceProvider();

var auth   = services.GetRequiredService<AuthService>();
var users  = services.GetRequiredService<UserService>();
var tasks  = services.GetRequiredService<TaskService>();

await auth.InitAsync();   // не забудьте инициализировать!
await users.InitAsync();
await tasks.InitAsync();

Console.OutputEncoding = System.Text.Encoding.UTF8;
Console.InputEncoding = System.Text.Encoding.UTF8;
if (!users.GetAllUsers().Any())
{
    Console.WriteLine("=== Первый запуск: создаём управляющего ===");
    Console.Write("Логин: "); var admLogin = Console.ReadLine() ?? "";
    Console.Write("Пароль: "); var admPwd   = ReadPassword();
    await users.RegisterManagerAsync(admLogin, admPwd);
    Console.WriteLine("✓ Менеджер создан. Перезапустите программу.");
    return;
}

// ---------- UI-цикл ----------
while (true)
{
    Console.Clear();
    Console.Write("Login: ");    var login = Console.ReadLine()?.Trim() ?? "";
    Console.Write("Password: "); var pass  = ReadPassword();

    var user = auth.Login(login, pass);
    if (user is null)
    {
        Console.WriteLine("\nНеверные учётные данные. Нажмите любую клавишу…");
        Console.ReadKey();
        continue;
    }

    if (user.Role == Role.Manager)   
        await ShowManagerMenu(user, users, tasks);
    else                             
        await ShowEmployeeMenu(user, tasks);
}

// ================== локальные функции ==================

static string ReadPassword()
{
    var pwd = new System.Text.StringBuilder();
    ConsoleKeyInfo key;
    while ((key = Console.ReadKey(intercept: true)).Key != ConsoleKey.Enter)
    {
        if (key.Key == ConsoleKey.Backspace && pwd.Length > 0)
        {
            pwd.Length--;                                   // убираем символ
            Console.Write("\b \b");                         // стираем в консоли
        }
        else if (!char.IsControl(key.KeyChar))
        {
            pwd.Append(key.KeyChar);
            Console.Write('*');
        }
    }
    Console.WriteLine();
    return pwd.ToString();
}

static async Task ShowManagerMenu(User manager,
                                  UserService userSvc,
                                  TaskService taskSvc)
{
    while (true)
    {
        Console.Clear();
        Console.WriteLine($"*** Менеджер: {manager.Login} ***");
        Console.WriteLine("1. Создать задачу");
        Console.WriteLine("2. Зарегистрировать сотрудника");
        Console.WriteLine("0. Выход");
        Console.Write("→ ");

        if (!int.TryParse(Console.ReadLine(), out var choice)) continue;

        switch (choice)
        {
            // ───────── 1. СОЗДАТЬ ЗАДАЧУ ─────────
            case 1:
                Console.Write("Заголовок: ");
                var title = Console.ReadLine() ?? "";
                Console.Write("Описание: ");
                var desc  = Console.ReadLine() ?? "";

                var employees = userSvc.GetAllEmployees().ToList();
                if (employees.Count == 0)
                {
                    Console.WriteLine("Нет сотрудников");
                    break;
                }

                Console.WriteLine("Выберите исполнителя:");
                for (int i = 0; i < employees.Count; i++)
                    Console.WriteLine($"{i + 1}. {employees[i].Login}");

                if (!int.TryParse(Console.ReadLine(), out var idx) ||
                    idx < 1 || idx > employees.Count) break;

                var task = taskSvc.Create(title, desc, employees[idx - 1].Id);
                await taskSvc.PersistAsync();
                Console.WriteLine($"Задача создана (ID: {task.Id})");
                break;

            // ───────── 2. РЕГИСТРАЦИЯ СОТРУДНИКА ─────────
            case 2:
                Console.Write("Логин: ");
                var login = Console.ReadLine() ?? "";
                Console.Write("Пароль: ");
                var pwd   = ReadPassword();

                await userSvc.RegisterEmployeeAsync(login, pwd);
                Console.WriteLine("Сотрудник зарегистрирован");
                break;

            // ───────── ВЫХОД ─────────
            case 0:
                return;
        }

        Console.WriteLine("Нажмите любую клавишу…");
        Console.ReadKey();
    }
}

static async Task ShowEmployeeMenu(User emp, TaskService taskSvc)
{
    while (true)
    {
        Console.Clear();
        Console.WriteLine($"*** Сотрудник: {emp.Login} ***");
        Console.WriteLine("1. Мои задачи");
        Console.WriteLine("2. Изменить статус задачи");
        Console.WriteLine("0. Выход");
        Console.Write("→ ");

        if (!int.TryParse(Console.ReadLine(), out var choice)) continue;
        switch (choice)
        {
            case 1:
                PrintTasks(taskSvc.GetTasksForList(emp.Id));
                break;

            case 2:
                var list = taskSvc.GetTasksForList(emp.Id);
                if (list.Count == 0) { Console.WriteLine("У вас нет задач"); break; }

                PrintTasks(list);
                Console.Write("№ задачи: ");
                if (!int.TryParse(Console.ReadLine(), out var num)
                    || num < 1 || num > list.Count) break;

                var task = list[num - 1];

                Console.WriteLine("Новый статус: 1-ToDo  2-InProgress  3-Done");
                if (!int.TryParse(Console.ReadLine(), out var st) || st is < 1 or > 3) break;

                taskSvc.UpdateStatus(task.Id, (TaskStatus)(st - 1), emp.Id);
                await taskSvc.PersistAsync();
                Console.WriteLine("Статус обновлён");
                break;

            case 0: return;
        }
        Console.WriteLine("Нажмите любую клавишу…"); Console.ReadKey();
    }
}

static void PrintTasks(IReadOnlyList<ProjectTask> tasks)
{
    for (int i = 0; i < tasks.Count; i++)
        Console.WriteLine($"{i + 1}. {tasks[i].Title} | {tasks[i].Status}");
}
