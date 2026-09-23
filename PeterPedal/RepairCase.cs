using System.Collections.Generic;

namespace PeterPedal;

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