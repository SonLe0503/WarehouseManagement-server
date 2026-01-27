using System;
using System.Collections.Generic;

namespace warehouseManagement.Models;

public partial class ApprovalLog
{
    public int Id { get; set; }

    public int? ApprovalId { get; set; }

    public string? Action { get; set; }

    public int? ActionBy { get; set; }

    public DateTime? ActionAt { get; set; }

    public string? Comment { get; set; }

    public virtual User? ActionByNavigation { get; set; }

    public virtual Approval? Approval { get; set; }
}
