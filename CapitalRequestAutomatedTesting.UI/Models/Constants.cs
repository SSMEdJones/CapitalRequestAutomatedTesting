namespace CapitalRequestAutomatedTesting.UI.Models
{
    public static class Constants
    {
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

    }
}
