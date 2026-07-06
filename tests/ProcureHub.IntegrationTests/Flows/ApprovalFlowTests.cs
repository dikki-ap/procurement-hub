using ProcureHub.IntegrationTests.Base;
using ProcureHub.IntegrationTests.Infrastructure;

namespace ProcureHub.IntegrationTests.Flows;

/// <summary>
/// Tests multi-level approval workflows: full approve, soft reject (revise), hard reject.
/// </summary>
public class ApprovalFlowTests : IntegrationTestBase
{
    private readonly Guid _l1ApproverId = Guid.NewGuid();
    private readonly Guid _l2ApproverId = Guid.NewGuid();

    public ApprovalFlowTests(TestWebApplicationFactory factory) : base(factory) { }

    private async Task<Guid> CreateAndSubmitPR(HttpClient purchasing)
    {
        var createResp = await purchasing.PostAsync("/api/v1/purchase-requisitions", Json(new
        {
            companyId        = CompanyId,
            title            = "Office Supplies Q3",
            description      = "Integration test PR",
            department       = "IT",
            deliveryLocation = "HQ",
            requiredDate     = DateTime.UtcNow.AddDays(30),
            items = new[]
            {
                new
                {
                    itemDescription      = "Laptop Stand",
                    quantity             = 5m,
                    estimatedUnitPrice   = 150_000m,
                    unitLabel            = "pcs",
                }
            }
        }));

        createResp.StatusCode.Should().Be(HttpStatusCode.Created);
        var prId = await ParseGuidData(createResp);

        var submitResp = await purchasing.PostAsync(
            $"/api/v1/purchase-requisitions/{prId}/submit", null);
        submitResp.StatusCode.Should().Be(HttpStatusCode.OK);

        return prId;
    }

    private async Task<Guid> SubmitWorkflow(HttpClient requester, Guid prId)
    {
        var resp = await requester.PostAsync("/api/v1/approval-workflows", Json(new
        {
            companyId       = CompanyId,
            referenceType   = "PurchaseRequisition",
            referenceId     = prId,
            referenceNumber = "PR-TEST-001",
            referenceTitle  = "Office Supplies Q3",
            totalValue      = 750_000m,
            isStrategicItem = false,
            isSingleSource  = false,
            requestedById   = Guid.NewGuid(),
            approvers = new[]
            {
                new { level = 1, userId = _l1ApproverId, userName = "L1 Approver" },
                new { level = 2, userId = _l2ApproverId, userName = "L2 Approver" },
            }
        }));

        resp.StatusCode.Should().Be(HttpStatusCode.Created);
        return await ParseGuidData(resp);
    }

    [Fact]
    public async Task FullApproval_L1ThenL2_WorkflowBecomesApproved()
    {
        var purchasing = AsPurchasing();
        var prId       = await CreateAndSubmitPR(purchasing);
        var wfId       = await SubmitWorkflow(purchasing, prId);

        // L1 approves
        var l1Client   = AsApprover(_l1ApproverId);
        var l1Resp     = await l1Client.PostAsync($"/api/v1/approval-workflows/{wfId}/approve", Json(new
        {
            approverId   = _l1ApproverId,
            approverName = "L1 Approver",
        }));
        l1Resp.StatusCode.Should().Be(HttpStatusCode.OK);

        // L2 approves
        var l2Client   = AsApprover(_l2ApproverId);
        var l2Resp     = await l2Client.PostAsync($"/api/v1/approval-workflows/{wfId}/approve", Json(new
        {
            approverId   = _l2ApproverId,
            approverName = "L2 Approver",
        }));
        l2Resp.StatusCode.Should().Be(HttpStatusCode.OK);

        // Verify status
        var detailResp = await purchasing.GetAsync($"/api/v1/approval-workflows/{wfId}");
        detailResp.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = await detailResp.Content.ReadAsStringAsync();
        json.Should().Contain("Approved");
    }

    [Fact]
    public async Task SoftReject_Revise_ThenReapprove_WorkflowBecomesApproved()
    {
        var purchasing = AsPurchasing();
        var prId       = await CreateAndSubmitPR(purchasing);
        var wfId       = await SubmitWorkflow(purchasing, prId);

        // L1 approves
        var l1Resp = await AsApprover(_l1ApproverId)
            .PostAsync($"/api/v1/approval-workflows/{wfId}/approve", Json(new
            {
                approverId   = _l1ApproverId,
                approverName = "L1 Approver",
            }));
        l1Resp.StatusCode.Should().Be(HttpStatusCode.OK);

        // L2 revises (sends back to L1)
        var reviseResp = await AsApprover(_l2ApproverId)
            .PostAsync($"/api/v1/approval-workflows/{wfId}/revise", Json(new
            {
                approverId   = _l2ApproverId,
                approverName = "L2 Approver",
                reason       = "Need more justification",
            }));
        reviseResp.StatusCode.Should().Be(HttpStatusCode.OK);

        // L1 re-approves
        var l1ReApproveResp = await AsApprover(_l1ApproverId)
            .PostAsync($"/api/v1/approval-workflows/{wfId}/approve", Json(new
            {
                approverId   = _l1ApproverId,
                approverName = "L1 Approver",
            }));
        l1ReApproveResp.StatusCode.Should().Be(HttpStatusCode.OK);

        // L2 finally approves
        var l2FinalResp = await AsApprover(_l2ApproverId)
            .PostAsync($"/api/v1/approval-workflows/{wfId}/approve", Json(new
            {
                approverId   = _l2ApproverId,
                approverName = "L2 Approver",
            }));
        l2FinalResp.StatusCode.Should().Be(HttpStatusCode.OK);

        var detail = await purchasing.GetAsync($"/api/v1/approval-workflows/{wfId}");
        var json   = await detail.Content.ReadAsStringAsync();
        json.Should().Contain("Approved");
    }

    [Fact]
    public async Task HardReject_WorkflowBecomesRejected()
    {
        var purchasing = AsPurchasing();
        var prId       = await CreateAndSubmitPR(purchasing);
        var wfId       = await SubmitWorkflow(purchasing, prId);

        // L1 approves
        await AsApprover(_l1ApproverId)
            .PostAsync($"/api/v1/approval-workflows/{wfId}/approve", Json(new
            {
                approverId   = _l1ApproverId,
                approverName = "L1 Approver",
            }));

        // L2 hard-rejects
        var rejectResp = await AsApprover(_l2ApproverId)
            .PostAsync($"/api/v1/approval-workflows/{wfId}/reject", Json(new
            {
                approverId   = _l2ApproverId,
                approverName = "L2 Approver",
                reason       = "Budget exceeded",
            }));
        rejectResp.StatusCode.Should().Be(HttpStatusCode.OK);

        var detail = await purchasing.GetAsync($"/api/v1/approval-workflows/{wfId}");
        var json   = await detail.Content.ReadAsStringAsync();
        json.Should().Contain("Rejected");
    }
}
