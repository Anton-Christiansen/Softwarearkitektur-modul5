using System;
using System.Collections.Generic;

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
        if (_prices.TryGetValue(partName, out var price))
        {
            return price;
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
            string stlnr = c.FrameNumber;
            if (finding.Contains("Gear cable"))
            {
                c.Parts.Add("Gear cable");
                Console.WriteLine($"Found part for case {stlnr}: Gear cable ({_catalog.GetPrice("Gear cable")} kr)");
            }
            else if (finding.Contains("Sprocket"))
            {
                c.Parts.Add("Sprocket");
                Console.WriteLine($"Found part for case {stlnr}: Sprocket ({_catalog.GetPrice("Sprocket")} kr)");
            }
            else if (finding.Contains("Brake pads"))
            {
                c.Parts.Add("Brake pads");
                Console.WriteLine($"Found part for case {stlnr}: Brake pads ({_catalog.GetPrice("Brake pads")} kr)");
            }
        }

        Console.WriteLine($"Found {c.Parts.Count} part(s) for case {frameNumber}.");
    }

    // Calculates the price of a gear cable including markup.
    private decimal CalculatePriceForGearCable()
    {
        const decimal price = 150m;
        const decimal markup = price * 0.1m;
        return price + markup;
    }

    // Calculates the price of a sprocket including markup.
    private decimal CalculatePriceForSprocket()
    {
        const decimal price = 300m;
        const decimal markup = price * 0.1m;
        return price + markup;
    }

    // Calculates the price of brake pads including markup.
    private decimal CalculatePriceForBrakePad()
    {
        const decimal price = 120m;
        const decimal markup = price * 0.1m;
        return price + markup;
    }

    public void CalculateOffer(string frameNumber)
    {
        var c = FindCase(frameNumber);
        var price = CalculateTotal(c);
        c.TotalPrice = price;
        c.Status = 1;

        const int d = 3;
        string cstTlf = c.CustomerInfo.Phone;
        Console.WriteLine($"Offer for case {frameNumber}: {price:F2} kr, delivery in {d} days.");
        Console.WriteLine($"Calling {cstTlf}...");
        _notifier.LeaveVoicemail(cstTlf);
    }

    public void ApproveCase(string frameNumber)
    {
        var c = FindCase(frameNumber);
        c.Approved = true;
        c.Status = 2;
        Console.WriteLine($"{c.CustomerInfo.FirstName} accepted the offer.");
    }

    // Sofia repairs the bike.
    public void PimpMyBike(string frameNumber) {
        var c = FindCase(frameNumber);
        Console.WriteLine($"Sofia is repairing the bike, frame number {c.FrameNumber}...");
    }

    // Calculates the final total price for the receipt.
    private decimal CalculateTotal(RepairCase c)
    {
        var partsPrice = CalculatePriceForGearCable() + CalculatePriceForSprocket() + CalculatePriceForBrakePad();
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