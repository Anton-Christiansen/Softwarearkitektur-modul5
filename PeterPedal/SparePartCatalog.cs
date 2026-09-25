using System;
using System.Collections.Generic;

namespace PeterPedal;

/// <summary>
/// The catalog for spare parts
/// </summary>
public class SparePartCatalog
{
    private const decimal Discount = 0.2m;
    
    private readonly Dictionary<string, decimal> _prices = new()
    {
        { "Gear cable", 150m },
        { "Sprocket", 300m },
        { "Brake pads", 120m }
    };

    /// <summary>
    /// Finds the price for the part
    /// </summary>
    /// <param name="partName">The part name</param>
    /// <param name="customer">The customer</param>
    /// <returns>The price of the part with a discount if availablbe to the customer</returns>
    /// <exception cref="ArgumentNullException">If there was no matching part for the partName parameter</exception>
    public decimal GetPrice(string partName, Customer customer)
    {
        if (_prices.TryGetValue(partName, out var price))
        {
            return customer.Discount
                ? price * (1 - Discount)
                : price;
        }
        
        throw new ArgumentNullException(partName);
    }

    
    /// <summary>
    /// Resolves the partname from a finding with a try-pattern
    /// </summary>
    /// <param name="query">The finding query</param>
    /// <param name="name">The outgoing variable that contains the part name</param>
    /// <returns>true if found and false if not</returns>
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