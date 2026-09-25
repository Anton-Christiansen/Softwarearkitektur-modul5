using System;
using System.Collections.Generic;
using System.Linq;

namespace PeterPedal;

/// <summary>
/// The service encapsulating the use cases pertaining to repair
/// </summary>
public class RepairService
{
    private readonly List<RepairCase> _cases = [];
    private readonly SparePartCatalog _catalog = new();
    private readonly Notifier _notifier = new();

    private const decimal HourlyRate = 450;

    /// <summary>
    /// Creates a case on a specific bike for a customer with a given problem description
    /// </summary>
    /// <param name="customer">The customer</param>
    /// <param name="frameNumber">The bikes unique frame number</param>
    /// <param name="problem">The problem description from the customer</param>
    public void CreateCase(Customer customer, string frameNumber, string problem)
    {
        var @case = new RepairCase
        {
            FrameNumber = frameNumber,
            Problem = problem,
            CustomerInfo = customer,
            Status = 0
        };

        _cases.Add(@case);

        Console.WriteLine($"Case created for {customer.FirstName} {customer.LastName}, frame number {frameNumber}.");
        Console.WriteLine($"Problem: {problem}");
    }

    /// <summary>
    /// Registers the identified problems
    /// </summary>
    /// <param name="frameNumber">The bikes unique frame number</param>
    /// <param name="findings">A list of the identified problems</param>
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

    /// <summary>
    /// Looks up the parts in the spare part catalog
    /// </summary>
    /// <param name="frameNumber">The bikes unique frame number</param>
    /// <param name="customer">The customer</param>
    public void LookUpParts(string frameNumber, Customer customer)
    {
        var c = FindCase(frameNumber);

        foreach (var finding in c.Findings)
        {
            
            if (_catalog.Resolve(finding, out var part) is false) continue;
            
            
            c.Parts.Add(part);
            Console.WriteLine($"Found part for case {c.FrameNumber}: {part} ({_catalog.GetPrice(part, customer)} kr)");
        }

        Console.WriteLine($"Found {c.Parts.Count} part(s) for case {frameNumber}.");
    }
    
    
    private decimal CalculatePriceWithMarkup(string part, Customer customer)
    {
        var price = _catalog.GetPrice(part, customer);
        var markup = price * 0.1m;

        return price + markup;
    }
    

    /// <summary>
    /// Calculates and offer and send it to the customer
    /// </summary>
    /// <param name="frameNumber">The bikes unique frame number</param>
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

    
    /// <summary>
    /// Approves the case on the bike
    /// </summary>
    /// <param name="frameNumber">The bikes unique frame number</param>
    public void ApproveCase(string frameNumber)
    {
        var @case = FindCase(frameNumber);
        @case.Approved = true;
        @case.Status = 2;
        Console.WriteLine($"{@case.CustomerInfo.FirstName} accepted the offer.");
    }

    /// <summary>
    /// Repairs the bike
    /// </summary>
    /// <param name="frameNumber">The bikes unique frame number</param>
    public void RepairBike(string frameNumber) {
        Console.WriteLine($"Sofia is repairing the bike, frame number {FindCase(frameNumber).FrameNumber}...");
    }

    private decimal CalculateTotal(RepairCase @case)
    {
        var partsPrice = @case.Parts.Sum(part => CalculatePriceWithMarkup(part, @case.CustomerInfo));
        const decimal vatRate = 0.25m;
        
        const decimal labor = HourlyRate * 2;
        var subtotal = partsPrice + labor;
        var vat = subtotal * vatRate;
        return subtotal + vat;
    }

    /// <summary>
    /// Finishes the repair
    /// </summary>
    /// <param name="frameNumber">The bikes unique frame number</param>
    public void FinishRepair(string frameNumber)
    {
        var @case = FindCase(frameNumber);

        var approved = @case.Status == 2 && @case.Approved;
        var containsParts = @case.Parts.Count > 0;

        if (approved is false || containsParts is false) return;
        
        var total = CalculateTotal(@case);

        if (total < 0)
        {
            Console.WriteLine("Error: negative price");
            return;
        }

        @case.TotalPrice = total;
        @case.Status = 3;

        _notifier.SendSms(@case.CustomerInfo.Phone, $"Hi {@case.CustomerInfo.FirstName}, your bike is ready for pickup!");

        Console.WriteLine("--- Receipt ---");
        Console.WriteLine($"Frame number: {@case.FrameNumber}");
        Console.WriteLine($"Total: {Math.Round(total, 2)} kr");
    }

    
    /// <summary>
    /// Customer pays for the service provided
    /// </summary>
    /// <param name="frameNumber">The bikes unique frame number</param>
    public void PayCase(string frameNumber)
    {
        var result = FindCase(frameNumber);
        Console.WriteLine($"{result.CustomerInfo.FirstName} has paid {result.TotalPrice:F2} kr. The bike is ready to ride!");
    }

    private RepairCase FindCase(string frameNumber)
    {
        foreach (var @case in _cases)
        {
            if (@case.FrameNumber == frameNumber)
            {
                return @case;
            }
        }
        return null;
    }
}