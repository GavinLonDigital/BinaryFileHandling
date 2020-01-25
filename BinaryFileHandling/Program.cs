using System;
using System.Globalization;
using System.IO;

namespace BinaryFileHandling
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                int recSize = sizeof(int) + ((Employee.NameSize + 1) * 2) + sizeof(decimal) + (sizeof(char) - 1) + sizeof(bool);

                string rootPath = AppDomain.CurrentDomain.BaseDirectory;

                string binaryFile = Path.Combine(rootPath, "Employees.dat");

                SeedData(binaryFile);

                do
                {
                    ShowMainScreen(binaryFile, recSize);
                    Console.WriteLine();
                    Console.WriteLine("Please press the 'y' key if you'd like to update a particular record \nor press any other key to end the application");

                    ConsoleKey key = Console.ReadKey().Key;

                    if (key == ConsoleKey.Y)
                    {
                        Console.WriteLine();
                        Console.WriteLine("Please enter the Id of the record you wish to update");

                        int inputId = Convert.ToInt32(Console.ReadLine());

                        UpdateEmployeeRecord(inputId, binaryFile, recSize);
                    }
                    else
                    {
                        break;
                    }

                }
                while (true);

                Console.Clear();
                Console.WriteLine("Thank you!");

                Console.ReadKey();
            }
            catch (Exception ex)
            {
                Console.Clear();
                Console.BackgroundColor = ConsoleColor.DarkRed;
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine(ex.Message);
                Console.ResetColor();
            }

        }
        private static void SeedData(string binaryFile)
        {
            if (!File.Exists(binaryFile))
            {
                Employee employee1 = new Employee { Id = 1001, FirstName = "Andy", LastName = "Thompson", Salary = 50000, Gender = 'm', IsManager = true };
                Employee employee2 = new Employee { Id = 1002, FirstName = "Sarah", LastName = "Smith", Salary = 60000, Gender = 'f', IsManager = true };
                Employee employee3 = new Employee { Id = 1003, FirstName = "Bob", LastName = "Harris", Salary = 40000, Gender = 'm', IsManager = false };

                using (BinaryWriter writer = new BinaryWriter(new FileStream(binaryFile, FileMode.Create)))
                {
                    AddEmployeeRecord(writer, employee1);
                    AddEmployeeRecord(writer, employee2);
                    AddEmployeeRecord(writer, employee3);
                }

            }
        }
        private static void AddEmployeeRecord(BinaryWriter writer, Employee employee)
        {
            writer.Write(employee.Id);
            writer.Write(employee.FirstName);
            writer.Write(employee.LastName);
            writer.Write(employee.Salary);
            writer.Write(employee.Gender);
            writer.Write(employee.IsManager);

        }
        private static void UpdateEmployeeRecord(int inputId, string binaryFile, int recSize)
        {
            int totalRecords = GetNumberOfRecords(GetFileSize(binaryFile), recSize);

            int pos = FindRecordById(binaryFile, inputId, recSize, totalRecords);

            if (pos != -1)
            {
                using (FileStream fileStream = new FileStream(binaryFile, FileMode.Open))
                {
                    fileStream.Seek(pos + sizeof(int), SeekOrigin.Begin);

                    using (BinaryWriter writer = new BinaryWriter(fileStream))
                    {
                        UpdateName(fileStream, writer, "first name");
                        UpdateName(fileStream, writer, "last name");
                        UpdateSalary(fileStream, writer, "salary");
                        UpdateGender(fileStream, writer, "gender");
                        UpdateIsManager(fileStream, writer);

                    }
                }
            }
            else
            {
                Console.WriteLine();
                Console.WriteLine("Unable to find record. Please press any key to navigate to the main screen");
                Console.ReadKey();
            }
            
        }

        private static void UpdateName(FileStream fileStream, BinaryWriter writer, string fieldLabel)
        {
            Console.WriteLine($"Please enter a value for {fieldLabel}");

            string name = Console.ReadLine();

            if (String.IsNullOrWhiteSpace(name))
            {
                fileStream.Seek(Employee.NameSize + 1, SeekOrigin.Current);
            }
            else
            {
                writer.Write(name.PadRight(Employee.NameSize));
            }
        }
        private static void UpdateSalary(FileStream fileStream, BinaryWriter writer, string fieldLabel)
        {
            Console.WriteLine($"Please enter a value for the employee's {fieldLabel}");

            string salaryInput = Console.ReadLine();

            if (String.IsNullOrEmpty(salaryInput))
            {
                fileStream.Seek(sizeof(decimal), SeekOrigin.Current);
            }
            else
            {
                decimal salary = Decimal.Parse(salaryInput, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture);

                writer.Write(salary);
            }
        }

        private static void UpdateGender(FileStream fileStream, BinaryWriter writer, string fieldLabel)
        {
            Console.WriteLine($"Please enter a value for the employee's {fieldLabel} ('m'/'f')");

            string genderInput = Console.ReadLine();

            if (String.IsNullOrEmpty(genderInput))
            {
                fileStream.Seek(sizeof(char) - 1, SeekOrigin.Current);
            }
            else
            {
                char gender = Convert.ToChar(genderInput);
                writer.Write(gender);
            }

        }
        private static void UpdateIsManager(FileStream fileStream, BinaryWriter writer)
        {
            Console.WriteLine("The employee is a manager (true/false)");

            string isManagerInput = Console.ReadLine();

            if (!String.IsNullOrWhiteSpace(isManagerInput))
            {
                bool isManager = Convert.ToBoolean(isManagerInput);
                writer.Write(isManager);
            }

        }

        private static int FindRecordById(string binaryFile, int inputId, int recSize, int totalRecords)
        {
            int recPosition = -1;
            int readId = -1;

            using (FileStream fileStream = new FileStream(binaryFile, FileMode.Open))
            {
                using (BinaryReader reader = new BinaryReader(fileStream))
                {
                    for (int count = 0; count < totalRecords; count++)
                    {
                        recPosition = GetRecordPosition(recSize, count);

                        fileStream.Seek(recPosition,SeekOrigin.Begin);

                        readId = reader.ReadInt32();

                        if (readId == inputId)
                        {
                            return recPosition;
                        }
                        else
                        {
                            recPosition = -1;
                        }
                    }
                }
            }
            return recPosition;
        }
        private static int GetRecordPosition(int recSize, int position)
        {
            return recSize * position;
        }

        private static void ShowMainScreen(string binaryFile, int recSize)
        {
            Console.Clear();
            DisplayHeadings();
            
            int totalRecords = GetNumberOfRecords(GetFileSize(binaryFile), recSize);

            DisplayAllRecordsOnScreen(binaryFile, totalRecords);

        }

        private static int GetFileSize(string binaryFile)
        {
            FileInfo f = new FileInfo(binaryFile);

            return Convert.ToInt32(f.Length);
        }

        private static int GetNumberOfRecords(int fileLength, int recSize)
        {
            return fileLength / recSize;
        }

        private static void DisplayAllRecordsOnScreen(string binaryFile, int totalRecords)
        {
            using (BinaryReader reader = new BinaryReader(new FileStream(binaryFile, FileMode.Open)))
            {
                for (int count = 0; count < totalRecords; count++)
                {
                    Console.WriteLine($"{reader.ReadInt32().ToString().PadRight(7)} {reader.ReadString().PadRight(Employee.NameSize)} {reader.ReadString().PadRight(Employee.NameSize)} {reader.ReadDecimal().ToString().PadRight(8)} {reader.ReadChar().ToString().PadRight(7)} {reader.ReadBoolean().ToString().PadRight(8)}");
                }
            }
        }

        private static void DisplayHeadings()
        {
            string mainHeading = GetMainHeading();

            Console.WriteLine(mainHeading);
            Console.WriteLine(GetUnderLine(mainHeading));
            Console.WriteLine();

            string fieldHeadings = GetFieldHeadings();
            Console.WriteLine(fieldHeadings);
            Console.WriteLine(GetUnderLine(fieldHeadings));
            Console.WriteLine();

        }

        private static string GetMainHeading()
        {
            return "Employee Records binary Application";
        }

        private static string GetFieldHeadings()
        {
            return $"{"Id".PadRight(7)} {"First Name".PadRight(Employee.NameSize)} {"Last Name".PadRight(Employee.NameSize)} {"Salary".PadRight(8)} {"Gender".PadRight(7)} {"Manager".PadRight(8)}";

        }

        private static string GetUnderLine(string heading)
        {
            return new string('-', heading.Length);
        }
}
    
    public class Employee
    {
        public const int NameSize = 20;
        private string _firstName = String.Empty;
        private string _lastName = String.Empty;

        public int Id { get; set; }

        public string FirstName
        {
            get
            {
                return (_firstName.Length > NameSize) ? _firstName.Substring(0, NameSize) : _firstName.PadRight(NameSize);
            }
            set
            {
                _firstName = value;
            }

        }
        public string LastName
        {
            get
            {
                return (_lastName.Length > NameSize) ? _lastName.Substring(0, NameSize) : _lastName.PadRight(NameSize);
            }
            set
            {
                _lastName = value;
            }
        }
        public decimal Salary { get; set; }
        public char Gender { get; set; }
        public bool IsManager { get; set; }
    
    }

}
