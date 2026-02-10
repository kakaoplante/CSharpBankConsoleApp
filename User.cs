using System;

namespace BankApp;


public class User
{

    public string? Username { get; set; }
    public string? Password { get; set; }
    public int UserId { get; set; }
    public List<int>? Accounts { get; set; }

}
