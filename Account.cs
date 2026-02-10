using System;

namespace BankApp;

public class Account
{
    public int AccountId { get; set; }
    public int OwnerId { get; set; }
    public decimal Rate { get; set; }
    public decimal Balance { get; set; }
    public string? Name { get; set; }
    public List<Transaction>? TransactionsHistory { get; set; }
    public bool SavingsAccount { get; set; }

}

