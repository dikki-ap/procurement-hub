namespace ProcureHub.SharedKernel.Notifications;

public static class EmailTemplates
{
    private static string Wrap(string title, string body) => $$"""
        <!DOCTYPE html>
        <html lang="en">
        <head>
          <meta charset="UTF-8" />
          <meta name="viewport" content="width=device-width, initial-scale=1.0"/>
          <title>{{title}}</title>
          <style>
            body { font-family: 'Segoe UI', Arial, sans-serif; background: #f4f6f9; margin: 0; padding: 0; }
            .container { max-width: 600px; margin: 32px auto; background: #fff; border-radius: 8px; overflow: hidden; box-shadow: 0 2px 8px rgba(0,0,0,.08); }
            .header { background: #2563eb; padding: 28px 32px; color: #fff; }
            .header h1 { margin: 0; font-size: 22px; font-weight: 600; }
            .header p { margin: 4px 0 0; font-size: 13px; opacity: .8; }
            .body { padding: 32px; color: #374151; font-size: 15px; line-height: 1.6; }
            .body h2 { margin: 0 0 16px; font-size: 18px; color: #111827; }
            .info { background: #f9fafb; border: 1px solid #e5e7eb; border-radius: 6px; padding: 16px; margin: 20px 0; }
            .info p { margin: 4px 0; font-size: 14px; }
            .info strong { color: #111827; }
            .footer { background: #f9fafb; border-top: 1px solid #e5e7eb; padding: 18px 32px; color: #9ca3af; font-size: 12px; text-align: center; }
          </style>
        </head>
        <body>
          <div class="container">
            <div class="header">
              <h1>ProcureHub</h1>
              <p>Procurement Management System</p>
            </div>
            <div class="body">
              {{body}}
            </div>
            <div class="footer">This is an automated message. Please do not reply directly to this email.</div>
          </div>
        </body>
        </html>
        """;

    public static string VendorApproved(string vendorName) => Wrap(
        "Vendor Account Approved",
        $$"""
        <h2>Your vendor account has been approved</h2>
        <p>Congratulations! Your vendor registration for <strong>{{vendorName}}</strong> has been reviewed and approved.</p>
        <p>You can now log in to the ProcureHub vendor portal to:</p>
        <ul>
          <li>View and acknowledge purchase orders</li>
          <li>Submit invoices</li>
          <li>Track your performance score</li>
        </ul>
        <p>If you have any questions, please contact your assigned procurement officer.</p>
        """);

    public static string VendorBlacklisted(string vendorName, string reason) => Wrap(
        "Vendor Account Status Update",
        $$"""
        <h2>Account status update</h2>
        <p>We regret to inform you that the vendor account for <strong>{{vendorName}}</strong> has been suspended.</p>
        <div class="info">
          <p><strong>Reason:</strong> {{reason}}</p>
        </div>
        <p>Please contact your procurement officer for more information.</p>
        """);

    public static string RFQInvitation(
        string vendorName, string rfqNumber, string title, DateTime deadline) => Wrap(
        $"RFQ Invitation — {rfqNumber}",
        $$"""
        <h2>You have been invited to submit a quotation</h2>
        <p>Dear <strong>{{vendorName}}</strong>,</p>
        <p>You are invited to participate in the following Request for Quotation:</p>
        <div class="info">
          <p><strong>RFQ Number:</strong> {{rfqNumber}}</p>
          <p><strong>Title:</strong> {{title}}</p>
          <p><strong>Bid Deadline:</strong> {{deadline:dddd, dd MMMM yyyy HH:mm}} UTC</p>
        </div>
        <p>Please log in to the ProcureHub vendor portal to review the requirements and submit your quotation before the deadline.</p>
        """);

    public static string BidDeadlineReminder(
        string vendorName, string rfqNumber, string title,
        DateTime deadline, int hoursLeft) => Wrap(
        $"Bid Deadline Reminder — {rfqNumber}",
        $$"""
        <h2>Reminder: {{hoursLeft}}-hour bid deadline</h2>
        <p>Dear <strong>{{vendorName}}</strong>,</p>
        <p>This is a reminder that the bid deadline for the following RFQ is approaching:</p>
        <div class="info">
          <p><strong>RFQ Number:</strong> {{rfqNumber}}</p>
          <p><strong>Title:</strong> {{title}}</p>
          <p><strong>Bid Deadline:</strong> {{deadline:dddd, dd MMMM yyyy HH:mm}} UTC</p>
          <p><strong>Time Remaining:</strong> approximately {{hoursLeft}} hours</p>
        </div>
        <p>If you have not yet submitted your quotation, please do so before the deadline.</p>
        """);

    public static string ApprovalRequired(
        string approverName, string refNumber,
        string description, string requester) => Wrap(
        $"Approval Required — {refNumber}",
        $$"""
        <h2>Your approval is required</h2>
        <p>Dear <strong>{{approverName}}</strong>,</p>
        <p>A document is awaiting your approval:</p>
        <div class="info">
          <p><strong>Reference:</strong> {{refNumber}}</p>
          <p><strong>Description:</strong> {{description}}</p>
          <p><strong>Requested by:</strong> {{requester}}</p>
        </div>
        <p>Please log in to ProcureHub to review and take action.</p>
        """);

    public static string ApprovalEscalation(
        string approverName, string refNumber, int pendingHours) => Wrap(
        $"Escalation Notice — {refNumber}",
        $$"""
        <h2>Pending approval escalation</h2>
        <p>Dear <strong>{{approverName}}</strong>,</p>
        <p>The following approval has been pending for more than {{pendingHours}} hours and requires urgent attention:</p>
        <div class="info">
          <p><strong>Reference:</strong> {{refNumber}}</p>
          <p><strong>Pending Since:</strong> {{pendingHours}} hours ago</p>
        </div>
        <p>Please log in to ProcureHub immediately to review and approve or reject this request.</p>
        """);

    public static string ContractActivated(
        string vendorName, string contractNumber, string title,
        DateTime? startDate, DateTime? endDate) => Wrap(
        $"Contract Activated — {contractNumber}",
        $$"""
        <h2>Your contract is now active</h2>
        <p>Dear <strong>{{vendorName}}</strong>,</p>
        <p>We are pleased to inform you that the following contract has been activated:</p>
        <div class="info">
          <p><strong>Contract Number:</strong> {{contractNumber}}</p>
          <p><strong>Title:</strong> {{title}}</p>
          <p><strong>Start Date:</strong> {{(startDate.HasValue ? startDate.Value.ToString("dd MMMM yyyy") : "—")}}</p>
          <p><strong>End Date:</strong> {{(endDate.HasValue ? endDate.Value.ToString("dd MMMM yyyy") : "—")}}</p>
        </div>
        <p>Please review the contract terms and keep a copy for your records. Contact your procurement team if you have any questions.</p>
        """);

    public static string ContractExpiring(
        string vendorName, string contractNumber, string title,
        DateTime endDate, int daysLeft) => Wrap(
        $"Contract Expiry Reminder — {contractNumber}",
        $$"""
        <h2>Contract expiring in {{daysLeft}} day{{(daysLeft == 1 ? "" : "s")}}</h2>
        <p>Dear <strong>{{vendorName}}</strong>,</p>
        <p>This is a reminder that the following contract will expire soon:</p>
        <div class="info">
          <p><strong>Contract Number:</strong> {{contractNumber}}</p>
          <p><strong>Title:</strong> {{title}}</p>
          <p><strong>Expiry Date:</strong> {{endDate:dddd, dd MMMM yyyy}}</p>
          <p><strong>Days Remaining:</strong> {{daysLeft}}</p>
        </div>
        <p>Please contact your procurement team to discuss renewal or replacement before the expiry date.</p>
        """);
}
