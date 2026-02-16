using System;

namespace BankApp;




public class Transaction
{
    public decimal Amount { get; set; }
    public int? fromAccId { get; set; }
    public int? toAccId { get; set; }
    public string? Message { get; set; }
    public string? TypeOfTrans { get; set; }

    public DateTime? TimeOfTransaction { get; set; }

}
