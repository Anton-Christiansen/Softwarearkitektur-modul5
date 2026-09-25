using System.Collections.Generic;

namespace PeterPedal;

/// <summary>
/// Where the program starts 
/// </summary>
public class Program
{
    static void Main(string[] args)
    {
        var service = new RepairService();
        var customer = new Customer
        {
            FirstName = "Egon",
            LastName = "Cykelmyggen",
            Phone = "20123456",
            Discount = true
        };
        
        service.CreateCase(customer, "STL-4471", "The gears are not shifting properly and the bike is almost impossible to ride.");
        service.RegisterFindings("STL-4471", new List<string> { "Gear cable needs replacement", "Sprocket is worn", "Brake pads are worn" });
        service.LookUpParts("STL-4471", customer);
        service.CalculateOffer("STL-4471");
        service.ApproveCase("STL-4471");
        service.RepairBike("STL-4471");
        service.FinishRepair("STL-4471");
        service.PayCase("STL-4471");
    }
}