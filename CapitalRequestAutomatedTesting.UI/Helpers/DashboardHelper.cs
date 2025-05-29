using CapitalRequestAutomatedTesting.UI.Models;

namespace CapitalRequestAutomatedTesting.UI.Helpers
{
    public static class DashboardHelper
    {
        public static bool HasGroupTakenAction(Request request, string groupName)
        {
            string status = groupName switch
            {
                "EPMO" => request.EPMOReviewStatus,
                "Facilities" => request.FacilitiesReviewStatus,
                "Finance" => request.FinanceReviewStatus,
                "IT" => request.ITReviewStatus,
                "Purchasing" => request.PurchasingReviewStatus,
                "Supply Chain" => request.SupplyChainReviewStatus,
                "VPFinance" => request.VPFinanceReviewStatus,
                "VPOps" => request.VPOpsReviewStatus,
                _ => throw new ArgumentException("Invalid group name")
            };

            return status == "I" || status == "V";
        }
    }

}
