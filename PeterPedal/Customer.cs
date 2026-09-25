namespace PeterPedal;

/// <summary>
/// Containing rudimentary information about the customer and if the customer applies for a discount on spareparts. 
/// </summary>
public class Customer
{
    internal string FirstName { get; init; }
    internal string LastName { get; init; }
    internal string Phone { get; init; }
    internal bool Discount { get; set; }
}