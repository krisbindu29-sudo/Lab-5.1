using System;
using System.IO;

namespace Lab5_Task1
{
    // Власний клас виключення для помилок введення даних з клавіатури
    public class InvalidDataInputException : Exception
    {
        public InvalidDataInputException(string message) : base(message) { }
    }

    // Базовий клас "Лікар": містить поля лікаря та ВСІ методи роботи з базою даних
    public class Doctor
    {
        // Властивості базового класу: прізвище лікаря та стаж роботи
        public string LastName { get; set; }
        public int Experience { get; set; }

        // Конструктор без параметрів
        public Doctor() { }

        // Конструктор з параметрами для ініціалізації полів базового класу
        public Doctor(string lastName, int experience)
        {
            LastName = lastName;
            Experience = experience;
        }

        // Зчитування записів 
        public static Appointment[] ReadFromFile(string filePath)
        {
            // Якщо файлу ще немає - створюємо його з початковими записами
            if (!File.Exists(filePath))
            {
                CreateInitialDatabase(filePath);
            }

            string[] lines = File.ReadAllLines(filePath);

            // Перший прохід: рахуємо рядки, що мають 5 полів (щоб знати розмір масиву)
            int validCount = 0;
            for (int i = 0; i < lines.Length; i++)
            {
                if (!string.IsNullOrWhiteSpace(lines[i]))
                {
                    string[] parts = lines[i].Split(';');
                    if (parts.Length == 5)
                    {
                        validCount++;
                    }
                }
            }

            Appointment[] appointments = new Appointment[validCount];
            int index = 0;

            // Другий прохід: розбираємо рядки й створюємо об'єкти похідного класу
            for (int i = 0; i < lines.Length; i++)
            {
                // Порожні рядки пропускаємо мовчки
                if (string.IsNullOrWhiteSpace(lines[i])) continue;

                string[] parts = lines[i].Split(';');

                // Стаж (parts[1]) і кількість відвідувачів (parts[3]) мають бути числами
                if (parts.Length == 5 &&
                    int.TryParse(parts[1], out int exp) &&
                    int.TryParse(parts[3], out int visitors))
                {
                    appointments[index] = new Appointment(
                        parts[0],   // прізвище лікаря
                        exp,        // стаж
                        parts[2],   // день тижня
                        visitors,   // кількість відвідувачів
                        parts[4]    // коментар
                    );
                    index++;
                }
                else
                {
                    // Пошкоджений рядок не ігноруємо мовчки, а повідомляємо про нього
                    Console.WriteLine($"[Рядок {i + 1} файлу пропущено]: некоректний формат.");
                }
            }

            // Якщо частину рядків пропущено - вкорочуємо масив до реальної кількості
            if (index < validCount)
            {
                Array.Resize(ref appointments, index);
            }

            return appointments;
        }

        // Створення початкового файлу бази даних із 5 записами
        private static void CreateInitialDatabase(string filePath)
        {
            string[] initialRecords = new string[]
            {
                "Коваленко;12;Понеділок;15;Первинний огляд пацієнтів",
                "Шевченко;2;Вівторок;8;Консультація після здачі аналізів",
                "Фіщук;10;Середа;22;Плановий огляд та виписка рецептів",
                "Хома;12;Четвер;5;Терміновий позаплановий прийом",
                "Коваленко;1;П'ятниця;19;Профілактичний медогляд працівників"
            };
            File.WriteAllLines(filePath, initialRecords);
        }

        // Збереження масиву записів у текстовий файл (перезапис)
        public static void SaveToFile(string filePath, Appointment[] appointments)
        {
            string[] lines = new string[appointments.Length];
            for (int i = 0; i < appointments.Length; i++)
            {
                Appointment a = appointments[i];
                // Формат рядка: прізвище;стаж;день;відвідувачі;коментар
                lines[i] = $"{a.LastName};{a.Experience};{a.Day};{a.VisitorsCount};{a.Comment}";
            }
            File.WriteAllLines(filePath, lines);
        }

        // ДОДАВАННЯ запису: створюємо новий масив на 1 елемент більший
        public static void AddRecord(string filePath, Appointment newAppointment)
        {
            Appointment[] current = ReadFromFile(filePath);
            Appointment[] updated = new Appointment[current.Length + 1];

            // Копіюємо наявні записи
            for (int i = 0; i < current.Length; i++)
            {
                updated[i] = current[i];
            }
            // Новий запис ставимо в кінець
            updated[current.Length] = newAppointment;

            SaveToFile(filePath, updated);
        }

        // РЕДАГУВАННЯ запису за індексом
        public static void EditRecord(string filePath, int index, Appointment updatedAppointment)
        {
            Appointment[] appointments = ReadFromFile(filePath);

            if (index >= 0 && index < appointments.Length)
            {
                appointments[index] = updatedAppointment;
                SaveToFile(filePath, appointments);
            }
            else
            {
                throw new InvalidDataInputException($"Запису з номером [{index}] не існує!");
            }
        }

        // ВИДАЛЕННЯ запису за індексом: переносимо решту в новий менший масив
        public static void DeleteRecord(string filePath, int index)
        {
            Appointment[] current = ReadFromFile(filePath);

            if (index < 0 || index >= current.Length)
            {
                throw new InvalidDataInputException($"Запису з номером [{index}] не існує!");
            }

            Appointment[] updated = new Appointment[current.Length - 1];
            int newIdx = 0;

            for (int i = 0; i < current.Length; i++)
            {
                if (i == index) continue;   // пропускаємо видалений запис
                updated[newIdx] = current[i];
                newIdx++;
            }

            SaveToFile(filePath, updated);
        }

        // ВИВЕДЕННЯ всіх записів з файла на екран (метод базового класу)
        public static void PrintAll(string filePath)
        {
            Appointment[] appointments = ReadFromFile(filePath);

            Console.WriteLine("\n--- Вміст бази даних (" + filePath + ") ---");
            if (appointments.Length == 0)
            {
                Console.WriteLine("База даних порожня.");
                return;
            }

            for (int i = 0; i < appointments.Length; i++)
            {
                Console.WriteLine($"[{i}] {appointments[i]}");
            }
        }

        // ВІРТУАЛЬНИЙ метод: середня кількість відвідувачів (похідний клас може його перевизначити)
        public virtual double GetAverageVisitors(Appointment[] appointments)
        {
            if (appointments == null || appointments.Length == 0) return 0;

            double sum = 0;
            for (int i = 0; i < appointments.Length; i++)
            {
                sum += appointments[i].VisitorsCount;
            }
            return sum / appointments.Length;
        }

        // Пошук прийому з мінімальною кількістю відвідувачів
        public Appointment GetMinVisitorsAppointment(Appointment[] appointments)
        {
            if (appointments == null || appointments.Length == 0) return null;

            Appointment minApp = appointments[0];   // початково мінімум - перший запис
            for (int i = 1; i < appointments.Length; i++)
            {
                if (appointments[i].VisitorsCount < minApp.VisitorsCount)
                {
                    minApp = appointments[i];
                }
            }
            return minApp;
        }

        // Пошук прийому з найдовшим коментарем
        public Appointment GetLongestCommentAppointment(Appointment[] appointments)
        {
            if (appointments == null || appointments.Length == 0) return null;

            Appointment longestApp = appointments[0];
            for (int i = 1; i < appointments.Length; i++)
            {
                if (appointments[i].Comment.Length > longestApp.Comment.Length)
                {
                    longestApp = appointments[i];
                }
            }
            return longestApp;
        }

        // ВИВЕДЕННЯ результатів обчислень за варіантом 9 (метод базового класу)
        public void PrintResults(string filePath)
        {
            Appointment[] appointments = ReadFromFile(filePath);

            if (appointments.Length == 0)
            {
                Console.WriteLine("Немає даних для обчислення!");
                return;
            }

            Console.WriteLine("\n--- Результати обчислень (Варіант 9) ---");

            // Віртуальний виклик: виконається версія, що відповідає реальному типу об'єкта
            Console.WriteLine($"1) Середня кількість відвідувачів: {GetAverageVisitors(appointments):F2}");

            Appointment minApp = GetMinVisitorsAppointment(appointments);
            Console.WriteLine($"2) Прийом з мінімальною кількістю відвідувачів: {minApp.Day} ({minApp.VisitorsCount} відв.)");

            Appointment longestApp = GetLongestCommentAppointment(appointments);
            Console.WriteLine($"3) Прийом з найдовшим коментарем: {longestApp.Day} -> \"{longestApp.Comment}\"");
        }
    }

    // Похідний клас "Прийом": успадковує Doctor і додає свої поля
    public class Appointment : Doctor
    {
        // Власні властивості похідного класу
        public string Day { get; set; }            // день тижня
        public int VisitorsCount { get; set; }     // кількість відвідувачів
        public string Comment { get; set; }        // коментар

        public Appointment() : base() { }

        // Параметри лікаря передаємо в конструктор базового класу через base(...)
        public Appointment(string lastName, int experience, string day, int visitorsCount, string comment)
            : base(lastName, experience)
        {
            Day = day;
            VisitorsCount = visitorsCount;
            Comment = comment;
        }

        // ПЕРЕВИЗНАЧЕННЯ віртуального методу: власна реалізація обчислення середнього
        // (враховує лише записи з коректною, невід'ємною кількістю відвідувачів)
        public override double GetAverageVisitors(Appointment[] appointments)
        {
            if (appointments == null || appointments.Length == 0) return 0;

            double sum = 0;
            int count = 0;   // кількість записів, що увійшли в розрахунок

            for (int i = 0; i < appointments.Length; i++)
            {
                if (appointments[i].VisitorsCount >= 0)
                {
                    sum += appointments[i].VisitorsCount;
                    count++;
                }
            }

            if (count == 0) return 0;
            return sum / count;
        }

        // Рядкове подання одного запису для виводу в консоль
        public override string ToString()
        {
            return $"Лікар: {LastName} (стаж: {Experience} р.) | День: {Day} | Відвідувачів: {VisitorsCount} | Коментар: {Comment}";
        }
    }

    // Головний клас програми з консольним меню
    class Program
    {
        // Ім'я текстового файлу бази даних
        static string filePath = "doctors.txt";

        // Додавання нового запису: введення з клавіатури та збереження у файл
        static void AddNewRecord()
        {
            Console.WriteLine("\n--- Додавання запису ---");
            Appointment app = ReadAppointmentFromKeyboard();
            Doctor.AddRecord(filePath, app);
            Console.WriteLine("Запис успішно збережено у файл!");
        }

        // Редагування запису: вибір номера та введення нових даних
        static void EditExistingRecord()
        {
            Doctor.PrintAll(filePath);
            Console.Write("\nВведіть номер запису для редагування: ");
            if (!int.TryParse(Console.ReadLine(), out int index))
                throw new InvalidDataInputException("Номер запису повинен бути цілим числом!");

            Console.WriteLine("Введіть оновлені дані:");
            Appointment updated = ReadAppointmentFromKeyboard();
            Doctor.EditRecord(filePath, index, updated);
            Console.WriteLine("Запис успішно відредаговано!");
        }

        // Видалення запису за номером
        static void DeleteExistingRecord()
        {
            Doctor.PrintAll(filePath);
            Console.Write("\nВведіть номер запису для видалення: ");
            if (!int.TryParse(Console.ReadLine(), out int index))
                throw new InvalidDataInputException("Номер запису повинен бути цілим числом!");

            Doctor.DeleteRecord(filePath, index);
            Console.WriteLine("Запис успішно видалено!");
        }

        // Введення та перевірка всіх полів запису з клавіатури
        static Appointment ReadAppointmentFromKeyboard()
        {
            Console.Write("Прізвище лікаря: ");
            string lastName = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(lastName))
                throw new InvalidDataInputException("Прізвище не може бути порожнім!");
            // ';' - роздільник полів у файлі, тому замінюємо його на ','
            lastName = lastName.Replace(';', ',');

            Console.Write("Стаж (років): ");
            if (!int.TryParse(Console.ReadLine(), out int exp) || exp < 0)
                throw new InvalidDataInputException("Стаж має бути цілим невід'ємним числом!");

            Console.Write("День тижня: ");
            string day = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(day))
                throw new InvalidDataInputException("День прийому не може бути порожнім!");
            day = day.Replace(';', ',');

            Console.Write("Кількість відвідувачів: ");
            if (!int.TryParse(Console.ReadLine(), out int visitors) || visitors < 0)
                throw new InvalidDataInputException("Кількість відвідувачів має бути цілим невід'ємним числом!");

            Console.Write("Коментар: ");
            string comment = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(comment))
                throw new InvalidDataInputException("Коментар не може бути порожнім!");
            comment = comment.Replace(';', ',');

            return new Appointment(lastName, exp, day, visitors, comment);
        }

        // Точка входу - в кінці файлу
        static void Main(string[] args)
        {
            // UTF-8 для коректного відображення кирилиці
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Console.InputEncoding = System.Text.Encoding.UTF8;

            // Об'єкт для виклику методів обчислення
            Doctor doctorService = new Doctor();

            // Головний цикл меню
            while (true)
            {
                Console.WriteLine("\n======= МЕНЮ ЗАВДАННЯ 1 (Базовий клас Doctor) =======");
                Console.WriteLine("[1] - Вивести інформацію з файла на екран");
                Console.WriteLine("[2] - Додати новий запис");
                Console.WriteLine("[3] - Редагувати запис");
                Console.WriteLine("[4] - Видалити запис");
                Console.WriteLine("[5] - Обчислити та вивести результати");
                Console.WriteLine("[0] або [Enter] - Вихід з програми");
                Console.Write("Натисніть клавішу: ");

                // Читаємо клавішу без натискання Enter; цифри не залежать від розкладки
                ConsoleKeyInfo key = Console.ReadKey(true);
                Console.WriteLine(key.KeyChar);

                // Вихід: Enter або 0
                if (key.Key == ConsoleKey.Enter || key.KeyChar == '0')
                {
                    Console.WriteLine("Роботу завершено.");
                    break;
                }

                try
                {
                    switch (key.KeyChar)
                    {
                        case '1':
                            Doctor.PrintAll(filePath);
                            break;
                        case '2':
                            AddNewRecord();
                            break;
                        case '3':
                            EditExistingRecord();
                            break;
                        case '4':
                            DeleteExistingRecord();
                            break;
                        case '5':
                            doctorService.PrintResults(filePath);
                            break;
                        default:
                            Console.WriteLine("Невідома клавіша! Оберіть цифру з меню.");
                            break;
                    }
                }
                catch (InvalidDataInputException ex)
                {
                    // Наше власне виключення: некоректне введення
                    Console.WriteLine($"\n[ПОМИЛКА ВВЕДЕННЯ]: {ex.Message}");
                }
                catch (Exception ex)
                {
                    // Системні помилки (наприклад, доступ до файлу)
                    Console.WriteLine($"\n[ПОМИЛКА]: {ex.Message}");
                }
            }
        }
    }
}