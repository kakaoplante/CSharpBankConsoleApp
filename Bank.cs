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

    public void Log(object sender, string msg)
    {
        DateTime now = DateTime.Now;
        string message = $"{now}: from {sender}. Msg:{msg}. \n";
        File.AppendAllText(_filePathLog, message);
    }

    public bool Login()
    {
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

    public void AddAccount(string name, bool isSavingsAccount)
    {
        string newAccName = name.Trim().ToLower();
        bool exists = AccountDB.Any(a => a.Value.Name == newAccName);
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
            SaveUsers(_filePathUserDB, UserDB);
            SaveAccounts(_filePathAccDB, AccountDB);
            Log(this, $"{CurrentUser.Username} created new account(id: {newAcc.AccountId})");

        }

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

    public void ShowAccounts()
    {
        if (CurrentUser != null && _isLoggedIn)
        {

            List<Account> userAccounts = AccountDB.Values
                            .Where(a => a.OwnerId == CurrentUser.UserId)
                            .ToList();

            foreach (Account acc in userAccounts)
            {
                System.Console.WriteLine($"Id: {acc.AccountId}, Name: {acc.Name}. Type:{acc.Rate}. Balance:{acc.Balance}");
            }
        }
    }

    public void ShowTransHistory(int id)
    {
        if (CurrentUser != null && _isLoggedIn)
        {

            AccountDB.TryGetValue(id, out Account? currentAcc);
            if (currentAcc != null && currentAcc.OwnerId == CurrentUser.UserId)
            {
                System.Console.WriteLine("");
                System.Console.WriteLine($"Showing transferhistory of account {currentAcc.AccountId}:");
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

    public void Withdraw(decimal amount, int id, string msg)
    {
        if (CurrentUser != null && _isLoggedIn)
        {

            AccountDB.TryGetValue(id, out Account? currentAcc);
            if (currentAcc != null && currentAcc.OwnerId == CurrentUser.UserId)
            {
                amount = Math.Round(amount, 2);
                if (amount <= currentAcc.Balance)
                {
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
                    //System.Console.WriteLine($"Balance after withdrawel: {currentAcc.Balance}$");
                }
                else
                {
                    System.Console.WriteLine("Not enough money");
                }

            }
            else
            {
                System.Console.WriteLine("No account match");
            }
        }
    }

    public void Deposit(decimal amount, int id, string msg)
    {
        if (CurrentUser != null && _isLoggedIn)
        {

            AccountDB.TryGetValue(id, out Account? currentAcc);
            if (currentAcc != null && currentAcc.OwnerId == CurrentUser.UserId)
            {

                amount = Math.Round(amount, 2);
                int accId = currentAcc.AccountId;
                Transaction newTransaction = new Transaction()
                {
                    Amount = amount,
                    Message = msg,
                    toAccId = accId,
                    TypeOfTrans = "deposit"
                };

                currentAcc.TransactionsHistory.Add(newTransaction);
                currentAcc.Balance += amount;
                SaveAccounts(_filePathAccDB, AccountDB);
                Log(this, $"{CurrentUser.Username} deposited money");

                //System.Console.WriteLine($"Balance after deposit: {currentAcc.Balance}$");

            }
            else
            {
                System.Console.WriteLine("No account match");
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
}

