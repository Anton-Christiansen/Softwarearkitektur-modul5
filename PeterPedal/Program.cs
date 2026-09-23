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

    public decimal? GetPrice(string partName)
    {
        if (_prices.Keys.Any(p => p.Contains(partName)))
        {
            return _prices[partName];
        }

        return null;
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
            var price = _catalog.GetPrice(finding);
            if (price is null) continue;
            
            c.Parts.Add(finding);
            Console.WriteLine($"Found part for case {c.FrameNumber}: {finding} ({price} kr)");
        }

        Console.WriteLine($"Found {c.Parts.Count} part(s) for case {frameNumber}.");
    }
    
    private decimal CalculatePriceWithMarkup(string part)
    {
        var price = _catalog.GetPrice(part);
        
        if (price is null) throw new ArgumentNullException(nameof(price));
        
        var markup = price.Value * 0.1m;
        return price.Value + markup;
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

    // Sofia repairs the bike.
    public void PimpMyBike(string frameNumber) {
        var c = FindCase(frameNumber);
        Console.WriteLine($"Sofia is repairing the bike, frame number {c.FrameNumber}...");
    }

    // Calculates the final total price for the receipt.
    private decimal CalculateTotal(RepairCase c)
    {
        var partsPrice = CalculatePriceWithMarkup("Gear cable") + CalculatePriceWithMarkup("Sprocket") + CalculatePriceWithMarkup("Brake pads");
        const decimal labor = HourlyRate * 2;
        var subtotal = partsPrice + labor;
        var vat = subtotal * 0.25m;
        return subtotal + vat;
    }

    public void FinishRepair(string frameNumber)
    {
        var c = FindCase(frameNumber);

        if (c.Status == 2 && c.Parts.Count > 0 && c.Approved)
        {
            var total = CalculateTotal(c);

            if (total < 0)
            {
                Console.WriteLine("Error: negative price");
            }

            c.TotalPrice = total;
            c.Status = 3;

            _notifier.SendSms(c.CustomerInfo.Phone, $"Hi {c.CustomerInfo.FirstName}, your bike is ready for pickup!");

            Console.WriteLine("--- Receipt ---");
            Console.WriteLine("Frame number: " + c.FrameNumber);
            Console.WriteLine("Total: " + Math.Round(total, 2) + " kr");
        }
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
        service.PimpMyBike("STL-4471");
        service.FinishRepair("STL-4471");
        service.PayCase("STL-4471");
    }
}