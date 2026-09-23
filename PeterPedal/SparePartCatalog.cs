using System;
using System.Collections.Generic;

namespace PeterPedal;

class SparePartCatalog
{
    private const decimal Discount = 0.2m;
    
    private readonly Dictionary<string, decimal> _prices = new()
    {
        { "Gear cable", 150m },
        { "Sprocket", 300m },
        { "Brake pads", 120m }
    };

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