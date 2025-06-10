namespace CapitalRequestAutomatedTesting.UI.Models
{
    public static class Constants
    {
        //Email Templates
        public const string EMAIL_REQUEST_MORE_INFORMATION = "Request More Information Email";
        public const string EMAIL_PROVIDE_MORE_INFORMATION = "Return of Requested Information";
        public const string EMAIL_PURCHASING_FINANCE_AUTHOR = "Purchasing, Finance, and Author Email";

        //Email Types
        public const string EMAIL_TYPE_NOTIFY = "Notify";

        //Response Messages
        public const string RESPONSE_ACTION_TAKEN = "Thank you for trying to take action. Someone has already taken action on this request.";

        //Option Types
        public const string OPTION_TYPE_REQUEST = "Request";
        public const string OPTION_TYPE_ADD_INFO = "AddInfo";
        public const string OPTION_TYPE_REPLY = "Reply";
        public const string OPTION_TYPE_VERIFY = "Verify";
        public const string OPTION_TYPE_VERIFY_WBS = "VerifyWBS";

        //Response Types
        public const string RESPONSE_VERIFIED = "Verified";
        public const string RESPONSE_REQUEST_MORE_INFORMATION = "Requested More Information";
        public const string RESPONSE_RETURN_MORE_INFORMATION = "Returned Requested Information";
        public const string RESPONSE_WORFLOW_OVERRIDDEN = "Overrode Workflow";
        public const string RESPONSE_RESENT = "Email sent to all active reviewers.";
        public const string RESPONSE_VERIFY_WBS = "Verified WBS";

        //Responder Types
        public const string RESPONDER_REQUEST = "Request";
        public const string RESPONDER_ADD_INFO = "AddInfo";
        public const string RESPONDER_REPLY = "Reply";
        public const string RESPONDER_VERIFY = "Verify";

        //Review Types 
        public const string REVIEW_TYPE_REVIEW = "Review";

        //Reviewer Groups
        public const string REVIEWER_GROUP_AUTHOR = "Author";
        public const string REVIEWER_GROUP_CORPORATE = "Corporate";
        public const string REVIEWER_GROUP_VPOps = "VP Ops";

        //WorkFlow
        public const int STEP_ONE = 1;
        public const int STEP_SIX = 6;
        public const string STAKE_HOLDER_NOTIFICATION_TYPE = "Email";
        public const string COMPLETE_MESSAGE = "Verified";
        public const string CANCELLED_MESSAGE = "Cancelled";
        public const string EPMO_GROUP = "EPMO";
        public const string ADMIN_GROUP = "Admin";
        public const string PURCHASING_GROUP = "Purchasing";
        public const string AUTHOR_GROUP = "Author";

        public const string EMAIL_ACTION_REQUEST_MORE_INFORMATION = "{{ fullName }} from {{ requestingGroupName }} requested more information from {{ requestedGroup }} on {{ requestDate }}.";
    }
}
