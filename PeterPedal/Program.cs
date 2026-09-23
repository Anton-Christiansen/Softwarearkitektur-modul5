using System;
using System.Collections.Generic;
using System.Linq;

namespace PeterPedal;

// Customer contact details for a repair case.
class Customer
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Phone { get; set; }
}

class RepairCase
{
    public string FrameNumber { get; set; }
    public string Problem { get; set; }
    public Customer CustomerInfo { get; set; }
    public List<string> Findings { get; } = [];
    public List<string> Parts { get; } = [];
    public int Status { get; set; } // 0 = created, 1 = awaiting approval, 2 = approved, 3 = finished
    public bool Approved { get; set; }
    public decimal TotalPrice { get; set; }
}

class SparePartCatalog
{
    private readonly Dictionary<string, decimal> _prices = new()
    {
        { "Gear cable", 150m },
        { "Sprocket", 300m },
        { "Brake pads", 120m }
    };

    public decimal GetPrice(string partName)
    {
        if (_prices.TryGetValue(partName, out var price))
        {
            return price;
        }
        
        throw new ArgumentNullException(partName);
    }

    public bool Resolve(string query, out string name)
    {
        name = string.Empty;
        foreach (var key in _prices.Keys)
        {
            if (query.Contains(key,  StringComparison.InvariantCultureIgnoreCase))
            {
                name = key;
                return true;
            }
        }

        return false;
    }
}

class Notifier
{
    public void SendSms(string phone, String message)
    {
        Console.WriteLine("SMS to " + phone + ": " + message);
    }

    public void LeaveVoicemail(string phone)
    {
        Console.WriteLine($"Voicemail left for {phone}: please call us back regarding your bike.");
    }
}

class RepairService
{
    private readonly List<RepairCase> _cases = [];
    private readonly SparePartCatalog _catalog = new();
    private readonly Notifier _notifier = new();

    private const decimal HourlyRate = 450;

    public void CreateCase(string firstName, string lastName, string phone, string frameNumber, string problem)
    {
        var customer = new Customer
        {
            FirstName = firstName,
            LastName = lastName,
            Phone = phone
        };

        var c = new RepairCase
        {
            FrameNumber = frameNumber,
            Problem = problem,
            CustomerInfo = customer,
            Status = 0
        };

        _cases.Add(c);

        Console.WriteLine($"Case created for {customer.FirstName} {customer.LastName}, frame number {frameNumber}.");
        Console.WriteLine($"Problem: {problem}");
    }

    public void RegisterFindings(string frameNumber, List<string> findings)
    {
        var @case = FindCase(frameNumber);
        
        if (@case == null) return;
        if (findings == null) return;
        if (findings.Count <= 0) return;
        
        foreach (var finding in findings)
        {
            if (string.IsNullOrWhiteSpace(finding)) continue;
            
            @case.Findings.Add(finding);
            Console.WriteLine("Finding registered: " + finding);
        }
    }

    public void LookUpParts(string frameNumber)
    {
        var c = FindCase(frameNumber);

        foreach (var finding in c.Findings)
        {
            
            if (_catalog.Resolve(finding, out var part) is false) continue;
            
            
            c.Parts.Add(part);
            Console.WriteLine($"Found part for case {c.FrameNumber}: {part} ({_catalog.GetPrice(part)} kr)");
        }

        Console.WriteLine($"Found {c.Parts.Count} part(s) for case {frameNumber}.");
    }
    
    private decimal CalculatePriceWithMarkup(string part)
    {
        var price = _catalog.GetPrice(part);
        var markup = price * 0.1m;
        return price + markup;
    }
    

    public void CalculateOffer(string frameNumber)
    {
        var @case = FindCase(frameNumber);
        var price = CalculateTotal(@case);
        @case.TotalPrice = price;
        @case.Status = 1;

        const int days = 3;
        var customerPhone = @case.CustomerInfo.Phone;
        Console.WriteLine($"Offer for case {frameNumber}: {price:F2} kr, delivery in {days} days.");
        Console.WriteLine($"Calling {customerPhone}...");
        _notifier.LeaveVoicemail(customerPhone);
    }

    public void ApproveCase(string frameNumber)
    {
        var @case = FindCase(frameNumber);
        @case.Approved = true;
        @case.Status = 2;
        Console.WriteLine($"{@case.CustomerInfo.FirstName} accepted the offer.");
    }

    public void RepairBike(string frameNumber) {
        Console.WriteLine($"Sofia is repairing the bike, frame number {FindCase(frameNumber).FrameNumber}...");
    }

    // Calculates the final total price for the receipt.
    private decimal CalculateTotal(RepairCase @case)
    {
        var partsPrice = @case.Parts.Sum(CalculatePriceWithMarkup);
        const decimal vatRate = 0.25m;
        
        const decimal labor = HourlyRate * 2;
        var subtotal = partsPrice + labor;
        var vat = subtotal * vatRate;
        return subtotal + vat;
    }

    public void FinishRepair(string frameNumber)
    {
        var c = FindCase(frameNumber);

        var approved = c.Status == 2 && c.Approved;
        var containsParts = c.Parts.Count > 0;

        if (approved is false || containsParts is false) return;
        
        var total = CalculateTotal(c);

        if (total < 0)
        {
            Console.WriteLine("Error: negative price");
            return;
        }

        c.TotalPrice = total;
        c.Status = 3;

        _notifier.SendSms(c.CustomerInfo.Phone, $"Hi {c.CustomerInfo.FirstName}, your bike is ready for pickup!");

        Console.WriteLine("--- Receipt ---");
        Console.WriteLine($"Frame number: {c.FrameNumber}");
        Console.WriteLine($"Total: {Math.Round(total, 2)} kr");
    }

    public void PayCase(string frameNumber)
    {
        var result = FindCase(frameNumber);
        Console.WriteLine($"{result.CustomerInfo.FirstName} has paid {result.TotalPrice:F2} kr. The bike is ready to ride!");
    }

    private RepairCase FindCase(string frameNumber)
    {
        foreach (var c in _cases)
        {
            if (c.FrameNumber == frameNumber)
            {
                return c;
            }
        }
        return null;
    }
}

class Program
{
    static void Main(string[] args)
    {
        var service = new RepairService();

        service.CreateCase("Egon", "Cykelmyggen", "20123456", "STL-4471", "The gears are not shifting properly and the bike is almost impossible to ride.");
        service.RegisterFindings("STL-4471", new List<string> { "Gear cable needs replacement", "Sprocket is worn", "Brake pads are worn" });
        service.LookUpParts("STL-4471");
        service.CalculateOffer("STL-4471");
        service.ApproveCase("STL-4471");
        service.RepairBike("STL-4471");
        service.FinishRepair("STL-4471");
        service.PayCase("STL-4471");
    }
}