using System;
using System.Collections.Generic;

namespace warehouseManagement.Models;

public partial class Approval
{
    public int Id { get; set; }

    public string? RefType { get; set; }

    public int? RefId { get; set; }

    public string? Status { get; set; }

    public int? CurrentStep { get; set; }

    public virtual ICollection<ApprovalLog> ApprovalLogs { get; set; } = new List<ApprovalLog>();
}
