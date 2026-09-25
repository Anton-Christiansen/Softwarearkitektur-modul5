using System.Collections.Generic;

namespace PeterPedal;

/// <summary>
/// Containing information about the individual case repair
/// </summary>
public class RepairCase
{
    internal string FrameNumber { get; set; }
    internal string Problem { get; set; }
    internal Customer CustomerInfo { get; set; }
    internal List<string> Findings { get; } = [];
    internal List<string> Parts { get; } = [];
    internal int Status { get; set; } // 0 = created, 1 = awaiting approval, 2 = approved, 3 = finished
    internal bool Approved { get; set; }
    internal decimal TotalPrice { get; set; }
}