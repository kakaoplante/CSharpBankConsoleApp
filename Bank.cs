using System;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json;

namespace BankApp;

public class Bank
{
    private Dictionary<string, int> settings = new Dictionary<string, int>();
    private string _filePathSettings;
    private string _filePathAccDB;
    private string _filePathUserDB;
    private string _filePathLog;
    public Dictionary<int, Account> AccountDB;
    public List<User> UserDB;
    public User? CurrentUser;
    private bool _isLoggedIn;


    public Bank(Dictionary<int, Account> accountDB, List<User> userDB, Dictionary<string, int> settings, Dictionary<string, string> paths)
    {
        AccountDB = accountDB;
        UserDB = userDB;
        CurrentUser = null;
        this.settings = settings;
        _filePathSettings = paths["pathSettings"];
        _filePathAccDB = paths["pathAccDB"];
        _filePathUserDB = paths["pathUserDB"];
        _filePathLog = paths["pathLog"];


    }

    

    private void renderWindowsTitle(string title) {
        int windowsTitleLength = 58;
        int inputTitleLenght = title.Length;
        string seperatorString = "----------------------------------------";
        int minusTitleLength = windowsTitleLength-inputTitleLenght;
        int startSeperatorLength = (int)Math.Floor(minusTitleLength/2.00);
        int endSeperatorLength = (int)Math.Ceiling(minusTitleLength/2.00);
        string windowsTitleRendered = $"{seperatorString[..startSeperatorLength]}{title}{seperatorString[..endSeperatorLength]}";
        System.Console.WriteLine(windowsTitleRendered);
    }
    public bool Login()
    {
        Console.Clear();
        renderWindowsTitle("Login Screen");
        System.Console.Write("Username: ");
        string? username = Console.ReadLine().Trim().ToLower();

        User? UserToCheck = UserDB.Find(x => x.Username == username);
        if (UserToCheck != null)
        {
            System.Console.Write("Password: ");
            string? password = Console.ReadLine();

            if (UserToCheck.Password == password)
            {
                CurrentUser = UserToCheck;
                _isLoggedIn = true;
                System.Console.WriteLine("Login Succesfull");
                Log(this, $"{CurrentUser.Username} logged in");
                return true;
            }
            else
            {
                System.Console.WriteLine("Wrong password");
                Log(this, $"{UserToCheck.Username} failed log in. Wrong password");
                return false;
            }
        }
        System.Console.WriteLine("No such user");
        return false;
    }

    public string AddUser()
    {
        string username;
        while (true)
        {
            Console.Write("Enter username: ");
            var input = Console.ReadLine();

            if (!string.IsNullOrWhiteSpace(input))
            {
                username = input;
                break;
            }
            else
            {
                System.Console.WriteLine("Please try again");
            }
        }


        string newUsername = username.Trim().ToLower();
        bool exists = UserDB.Any(u => u.Username == newUsername);
        if (exists == false)
        {
            string password;
            while (true)
            {
                System.Console.WriteLine("Please enter a password: ");
                var input = Console.ReadLine();

                if (!string.IsNullOrWhiteSpace(input))
                {
                    password = input;
                    break;
                }
                else
                {
                    System.Console.WriteLine("Please try again");
                }
            }

            User newUser = new User()
            {
                Username = username.Trim().ToLower(),
                Password = password,
                UserId = settings["nextUserId"]++,
                Accounts = []
            };
            UserDB.Add(newUser);
            SaveSettings(_filePathSettings, settings);
            SaveUsers(_filePathUserDB, UserDB);
            Log(this, $"New user has been created: {newUser.Username} ");
            return "User succesfully created. Please login.";
        }
        else
        {
            return "User already in database";
        }
    }

    public void ShowAccounts(string windowsTitle)
    {
        if (CurrentUser != null && _isLoggedIn)
        {
            // Overly complicated way to render a table instead of using built-in features.
            // Done just for practice.

            
            string idString = "ID    ";
            string nameString = "Name               ";
            string rateString = "Rate  ";
            string balanceString = "                Balance";
            string whitespaceString = "                                     ";
            renderWindowsTitle(windowsTitle);
            System.Console.WriteLine($"{idString}|{nameString}|{rateString}|{balanceString}|");
            List<Account> userAccounts = AccountDB.Values
                            .Where(a => a.OwnerId == CurrentUser.UserId)
                            .ToList();

            foreach (Account acc in userAccounts)
            {
                string accNameStringFormated = acc.Name.Length > 12 ? $"{acc.Name[0..12]}..." : acc.Name ;
                System.Console.WriteLine($"{acc.AccountId}{whitespaceString[..(idString.Length - acc.AccountId.ToString().Length)]}┆" + 
                                        $"{accNameStringFormated}{whitespaceString[..(nameString.Length - accNameStringFormated.Length)]}┆"+ 
                                        $"{acc.Rate}{whitespaceString[..(rateString.Length - acc.Rate.ToString().Length)]}┆"+
                                        $"{whitespaceString[..(balanceString.Length - acc.Balance.ToString().Length)]}{acc.Balance}┆");
            }
        }
    }

    public void AddAccount(string windowsTitle)
    {
        Console.Clear();
        ShowAccounts(windowsTitle);
        System.Console.WriteLine("Please enter new account name: ");
        string? name = Console.ReadLine();
        bool exists = true;
        string newAccName = "";
        bool isSavingsAccount = false;

        while (exists)
        {
            while (string.IsNullOrWhiteSpace(name))
            {
                System.Console.WriteLine("Please try again:");
                name = Console.ReadLine();
            }

            newAccName = name.Trim().ToLower();
            exists = AccountDB.Any(a => a.Value.Name == newAccName);

            if (exists)
            {
                System.Console.WriteLine("Account name already taken. Please try a new one.");
                name = Console.ReadLine();
            }
        }

        string? input;
        do
        {
            System.Console.WriteLine("Is it a savings account Y/N?");
            input = Console.ReadLine();
        } while (input.ToLower() != "y" && input.ToLower() != "n");

        isSavingsAccount = input == "y";
        System.Console.WriteLine("Succes");

        if (CurrentUser != null && _isLoggedIn && exists == false)
        {

            Account newAcc = new Account()
            {
                Name = newAccName,
                Balance = 0,
                TransactionsHistory = [],
                SavingsAccount = isSavingsAccount,
                Rate = isSavingsAccount ? 0.75m : 0.5m,
                AccountId = settings["nextAccId"]++,
                OwnerId = CurrentUser.UserId
            };

            AccountDB[newAcc.AccountId] = newAcc;
            CurrentUser.Accounts.Add(newAcc.AccountId);
            SaveSettings(_filePathSettings, settings);
            SaveUsers(_filePathUserDB, UserDB);
            SaveAccounts(_filePathAccDB, AccountDB);
            Log(this, $"{CurrentUser.Username} created new account(id: {newAcc.AccountId})");

        }
        Console.Clear();
        ShowAccounts("ACCOUNT ADDED");

    }

    public void ShowTransHistory(string windowsTitle)
    {
        ShowAccounts(windowsTitle);
        string input;
        int id;
        do
        {
            System.Console.WriteLine("Please select account id:");
            input = Console.ReadLine().Trim();

        } while (!int.TryParse(input, out id));
        Console.Clear();
        if (CurrentUser != null && _isLoggedIn)
        {

            AccountDB.TryGetValue(id, out Account? currentAcc);
            if (currentAcc != null && currentAcc.OwnerId == CurrentUser.UserId)
            {
                renderWindowsTitle(windowsTitle);
                System.Console.WriteLine($"Showing transfaction history of account {currentAcc.AccountId}:");
                foreach (var item in currentAcc.TransactionsHistory)
                {
                    System.Console.WriteLine($"Message: {item.Message}. {item.TypeOfTrans}: {item.Amount}$");
                }
                System.Console.WriteLine($"TransHist: Current balance: {currentAcc.Balance}$");
                System.Console.WriteLine("");
            }
            else
            {
                System.Console.WriteLine("No acc by that id.");
            }
        }
    }

    public void Withdraw(string windowsTitle)
    {
        if (CurrentUser != null && _isLoggedIn)
        {
            Console.Clear();
            int id;

            string? input;

            Account? currentAcc = null;
            string? displayMsg = null;
            do
            {
                Console.Clear();
                ShowAccounts(windowsTitle);
                if (displayMsg != null)
                {
                    System.Console.WriteLine(displayMsg);
                }
                System.Console.WriteLine("Please enter the ID of the account to withdraw from.");
                input = Console.ReadLine();
                if (int.TryParse(input, out id))
                {
                    if (AccountDB.TryGetValue(id, out currentAcc) && currentAcc.OwnerId == CurrentUser.UserId)
                    {
                        break;
                    }
                    else
                    {
                        currentAcc = null;
                        displayMsg = "No account by that id.";
                        input = "Fail";
                    }
                }
            } while (!int.TryParse(input, out id));


            Console.Clear();

            decimal amount;
            System.Console.WriteLine($"{currentAcc.AccountId}: {currentAcc.Name}. Balance: {currentAcc.Balance}");
            System.Console.WriteLine("Please enter the amount:");
            input = Console.ReadLine();

            if (decimal.TryParse(input, out amount))
            {
                amount = Math.Round(amount, 2);
                if (amount <= currentAcc.Balance)
                {
                    System.Console.WriteLine("Please enter a message.");
                    string? msg = Console.ReadLine();
                    if (string.IsNullOrEmpty(msg)) msg = "No message given.";

                    int accId = currentAcc.AccountId;
                    Transaction newTransaction = new Transaction()
                    {
                        Amount = amount,
                        fromAccId = accId,
                        Message = msg,
                        TypeOfTrans = "withdrawel"
                    };

                    currentAcc.TransactionsHistory.Add(newTransaction);
                    currentAcc.Balance -= amount;
                    SaveAccounts(_filePathAccDB, AccountDB);
                    Log(this, $"{CurrentUser.Username} withdrawel");
                    Console.Clear();
                    ShowAccounts("SHOWING ACCOUNTS");

                }
                else
                {
                    Console.Clear();
                    ShowAccounts("SHOWING ACCOUNTS");
                    System.Console.WriteLine("DENIAL: Not enough money");
                }

            }

        }
    }

    public void Deposit(string windowsTitle)
    {
        if (CurrentUser != null && _isLoggedIn)
        {
            Console.Clear();
            int id;

            string? input;

            Account? currentAcc = null;
            string? displayMsg = null;
            do
            {
                Console.Clear();
                ShowAccounts(windowsTitle);
                if (displayMsg != null)
                {
                    System.Console.WriteLine(displayMsg);
                }
                System.Console.WriteLine("Please enter the ID of the account to withdraw from.");
                input = Console.ReadLine();
                if (int.TryParse(input, out id))
                {
                    if (AccountDB.TryGetValue(id, out currentAcc) && currentAcc.OwnerId == CurrentUser.UserId)
                    {
                        break;
                    }
                    else
                    {
                        currentAcc = null;
                        displayMsg = "No account by that id.";
                        input = "Fail";
                    }
                }
            } while (!int.TryParse(input, out id));


            Console.Clear();

            decimal amount;
            System.Console.WriteLine($"{currentAcc.AccountId}: {currentAcc.Name}. Balance: {currentAcc.Balance}");
            System.Console.WriteLine("Please enter the amount:");
            input = Console.ReadLine();

            if (decimal.TryParse(input, out amount))
            {
                amount = Math.Round(amount, 2);


                System.Console.WriteLine("Please enter a message.");
                string? msg = Console.ReadLine();
                if (string.IsNullOrEmpty(msg)) msg = "No message given.";

                int accId = currentAcc.AccountId;
                Transaction newTransaction = new Transaction()
                {
                    Amount = amount,
                    fromAccId = accId,
                    Message = msg,
                    TypeOfTrans = "deposit"
                };

                currentAcc.TransactionsHistory.Add(newTransaction);
                currentAcc.Balance += amount;
                SaveAccounts(_filePathAccDB, AccountDB);
                Log(this, $"{CurrentUser.Username} deposit");
                Console.Clear();
                ShowAccounts("SHOWING ACCOUNTS");




            }

        }
    }

    public void Transfer(decimal amount, int currentAccId, int recieverAccId, string msgToSelf, string msgToReciever)
    {
        if (CurrentUser != null && _isLoggedIn)
        {

            AccountDB.TryGetValue(currentAccId, out Account? currentAcc);

            AccountDB.TryGetValue(recieverAccId, out Account? recieverAcc);

            if (currentAcc != null && currentAcc.OwnerId == CurrentUser.UserId && recieverAcc != null)
            {
                amount = Math.Round(amount, 2);
                if (amount < currentAcc.Balance)
                {

                    Transaction currentUserTransaction = new Transaction()
                    {
                        Amount = -1 * amount,
                        fromAccId = currentAcc.AccountId,
                        toAccId = recieverAcc.AccountId,
                        Message = msgToSelf,
                        TypeOfTrans = "transfer"
                    };

                    Transaction recieverTransaction = new Transaction()
                    {
                        Amount = amount,
                        fromAccId = currentAcc.AccountId,
                        toAccId = recieverAcc.AccountId,
                        Message = msgToReciever,
                        TypeOfTrans = "transfer"
                    };
                    currentAcc.TransactionsHistory.Add(currentUserTransaction);
                    recieverAcc.TransactionsHistory.Add(recieverTransaction);
                    currentAcc.Balance -= amount;
                    recieverAcc.Balance += amount;
                    SaveAccounts(_filePathAccDB, AccountDB);
                    Log(this, $"{CurrentUser.Username} new transfer");

                    System.Console.WriteLine($"Balance after transfer: {currentAcc.Balance}$");

                }
                else
                {
                    System.Console.WriteLine("Not enough money for transfer");
                }
            }
            else
            {
                System.Console.WriteLine("No account match");
            }

        }
    }

    void SaveSettings(string filePath, Dictionary<string, int> settings)
    {
        string json = JsonSerializer.Serialize(settings, new JsonSerializerOptions
        {
            WriteIndented = true
        });

        File.WriteAllText(filePath, json);
    }

    void SaveUsers(string filePath, List<User> userdb)
    {


        string json = JsonSerializer.Serialize(userdb, new JsonSerializerOptions
        {
            WriteIndented = true
        });

        File.WriteAllText(filePath, json);
    }

    void SaveAccounts(string filePath, Dictionary<int, Account> accountdb)
    {
        string json = JsonSerializer.Serialize(accountdb, new JsonSerializerOptions
        {
            WriteIndented = true
        });

        File.WriteAllText(filePath, json);
    }

    public void Log(object sender, string msg)
    {
        DateTime now = DateTime.Now;
        string message = $"{now}: from {sender}. Msg:{msg}. \n";
        File.AppendAllText(_filePathLog, message);
    }
}

