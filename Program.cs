using System.Runtime.CompilerServices;
using System.Text.Json;

namespace BankApp;

class Program
{

    public delegate void CurrentRender(string windowsTitle);


    static void Main(string[] args)
    {

        //TODO
        // 1. Creating Bank class migrate methods to this class. CHECK
        // 1. Save to file (Settings)(UserDB)(AccountDB) CHECK
        // 2. Add event listeners logging transaction and alarming big transaction
        // 3. adding user interface

        //Creating settings

        //Filepaths setup
        Dictionary<string, string> filePaths = new Dictionary<string, string>
        {
            ["pathSettings"] = Path.Combine(AppContext.BaseDirectory, "Settings.json"),
            ["pathAccDB"] = Path.Combine(AppContext.BaseDirectory, "AccountDB.json"),
            ["pathUserDB"] = Path.Combine(AppContext.BaseDirectory, "UserDB.json"),
            ["pathLog"] = Path.Combine(AppContext.BaseDirectory, "log.txt")
        };

        List<User>? userDB;
        Dictionary<int, Account>? accDB;
        Dictionary<string, int>? bankSettings;

        // Loading settings:
        if (!File.Exists(filePaths["pathSettings"]))
        {
            bankSettings = new Dictionary<string, int>
            {
                ["nextUserId"] = 100,
                ["nextAccId"] = 100,
            };

            string json = JsonSerializer.Serialize(bankSettings, new JsonSerializerOptions
            {
                WriteIndented = true
            });

            File.WriteAllText(filePaths["pathSettings"], json);

        }
        else
        {
            bankSettings = JsonSerializer.Deserialize<Dictionary<string, int>>(File.ReadAllText(filePaths["pathSettings"]));
        }

        // READING AND CREATING USERS
        if (!File.Exists(filePaths["pathUserDB"]))
        {
            userDB = new List<User>();

            string json = JsonSerializer.Serialize(userDB, new JsonSerializerOptions
            {
                WriteIndented = true
            });

            File.WriteAllText(filePaths["pathUserDB"], json);

        }
        else
        {
            userDB = JsonSerializer.Deserialize<List<User>>(File.ReadAllText(filePaths["pathUserDB"]));
        }

        // READIGN AND CREATING ACCOUNTS
        if (!File.Exists(filePaths["pathAccDB"]))
        {
            accDB = new Dictionary<int, Account>();

            string json = JsonSerializer.Serialize(accDB, new JsonSerializerOptions
            {
                WriteIndented = true
            });

            File.WriteAllText(filePaths["pathAccDB"], json);

        }
        else
        {
            accDB = JsonSerializer.Deserialize<Dictionary<int, Account>>(File.ReadAllText(filePaths["pathAccDB"]));
        }

        // Creating log:
        if (!File.Exists(filePaths["pathLog"]))
        {
            File.WriteAllText(filePaths["pathLog"], "");
        }


        bool isAuthenticated = false;
        Bank bank = new Bank(accDB, userDB, bankSettings, filePaths);

        string? displayMsg = null;

        //Handle authentication.
        while (!isAuthenticated)
        {
            if (displayMsg != null)
            {
                System.Console.WriteLine(displayMsg);
            }

            System.Console.WriteLine("Menu:\n [1] Login. \n [2] Add new user");
            System.Console.Write("Choice: ");
            var input = Console.ReadLine();
            switch (input.Trim())
            {
                case "1":
                    isAuthenticated = bank.Login();
                    Console.Clear();
                    break;
                case "2":
                    displayMsg = bank.AddUser();
                    Console.Clear();
                    break;
                default:
                    displayMsg = "Please select an option";
                    break;
            }
        }

        System.Console.WriteLine("Current user logged in: " + bank.CurrentUser.Username);

        CurrentRender Render = bank.ShowAccounts;
        string renderTitle = "--------------------SHOWING ACCOUNTS-----------------------";

        while (isAuthenticated)
        {
            Console.Clear();
            Render(renderTitle);
            string input = RenderMenu();
            switch (input)
            {
                case "0":
                    renderTitle = "--------------------SHOWING ACCOUNTS-----------------------";
                    Render = bank.ShowAccounts;
                    break;
                case "1":
                    renderTitle = "----------------------ADD ACCOUNT--------------------------";
                    Render = bank.AddAccount;
                    break;
                case "2":
                    renderTitle = "---------------SHOW TRANSACTION HISTORY-------------------";
                    Render = bank.ShowTransHistory;
                    break;
                case "3":
                    renderTitle = "-----------------------WITHDRAWAL--------------------------";
                    Render = bank.Withdraw;
                    break;
                case "4":
                    renderTitle = "-------------------------DEPOSIT---------------------------";
                    Render = bank.Deposit;
                    break;
                case "5":
                    renderTitle = "------------------------TRANSFER---------------------------";
                    break;
            }



        }

        string RenderMenu()
        {
            string? input = "";
            while (string.IsNullOrWhiteSpace(input))
            {
                System.Console.WriteLine("Menu:\n [0] Show Accounts.  [1] Add account. \n[2] Show account history. [3] Withdrawl. \n[4] Deposit. [5] Transfer");
                System.Console.WriteLine("Choice: ");
                input = Console.ReadLine();
            }

            return input;
        }




    }
}
