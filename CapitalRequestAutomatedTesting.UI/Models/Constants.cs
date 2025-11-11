namespace CapitalRequestAutomatedTesting.UI.Models
{
    public static class Constants
    {
        //Email Templates
        public const string EMAIL_NOTIFICATION = "Notification Email";
        public const string EMAIL_VERIFICATION = "Verification";

        public const string EMAIL_INITIAL_EMAIL = "Initial Email";
        public const string EMAIL_REQUEST_MORE_INFORMATION = "Request More Information Email";
        public const string EMAIL_PROVIDE_MORE_INFORMATION = "Return of Requested Information";
        public const string EMAIL_PURCHASING_FINANCE_AUTHOR = "Purchasing, Finance, and Author Email";


        
        //Email Types
        public const string EMAIL_TYPE_NOTIFY = "Notify";
        public const string EMAIL_TYPE_VERITY = "Verification";

        //Option Types
        public const string OPTION_TYPE_REQUEST = "Request";
        public const string OPTION_TYPE_ADD_INFO = "AddInfo";
        public const string OPTION_TYPE_REPLY = "Reply";
        public const string OPTION_TYPE_VERIFY = "Verify";
        public const string OPTION_TYPE_NOTIFY = "Notify";
        public const string OPTION_TYPE_VERIFY_WBS = "VerifyWBS";

        public const string BUTTON_CAPTION_VERIFY = "Verify";
        public const string BUTTON_CAPTION_REPLY = "Reply";

        //Response Types
        public const string RESPONSE_VERIFIED = "Verified";
        public const string RESPONSE_REQUEST_MORE_INFORMATION = "Requested More Information";
        public const string RESPONSE_RETURN_MORE_INFORMATION = "Returned Requested Information";
        public const string RESPONSE_WORFLOW_OVERRIDDEN = "Overrode Workflow";
        public const string RESPONSE_RESENT = "Email sent to all active reviewers.";
        public const string RESPONSE_VERIFY_WBS = "Verified WBS";

        //Response Messages
        public const string RESPONSE_ACTION_VERIFIED = "Thank you for verifying this project!";
        public const string RESPONSE_ACTION_TAKEN = "Thank you for trying to take action. Someone has already taken action on this request.";
        public const string RESPONSE_CANCELLED = "This project is no longer moving forward. No further action is required.";
        public const string RESPONSE_ACTION_NO_LONGER_AVAILABLE = "This Request is no longer available.";
        public const string RESPONSE_NOT_AVAILABLE = "This project is not available. No further action can be taken.";
        public const string RESPONSE_REQUEST_FOR_MORE_INFORMATION_SENT = "Your request for more information has been sent.";
        public const string RESPONSE_ADDED_MORE_INFORMATION_SENT = "Your added information has been successfully submitted.";
        public const string RESPONSE_ADDED_MORE_INFORMATION_FAILED_TO_SEND = "Your added information failed to send.";

        public const string ACTION_TYPE_REQUEST = "Request";
        public const string ACTION_TYPE_ADD_INFO = "AddInfo";
        public const string ACTION_TYPE_REPLY = "Reply";
        public const string ACTION_TYPE_VERIFY = "Verify";
        public const string ACTION_TYPE_NOTIFY = "Notify";
        public const string ACTION_TYPE_VERIFY_WBS = "VerifyWBS";
        public const string ACTION_TYPE_APPROVE_WBS = "ApproveWBS";
        public const string ACTION_TYPE_VIEW_WBS = "ViewWBS";

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

        public const string EMAIL_TEMPLATE_REQUEST_MORE_INFORMATION = "{{ fullName }} from {{ requestingGroupName }} requested more information from {{ requestedGroup }} on {{ requestDate }}";
        public const string EMAIL_TEMPLATE_RETURN_OF_REQUESTED_INFORMATION = "{{ fullName }} from {{ reviewerGroupName }} returned requested information to {{ requestingUser }} in {{requestingGroupName}} on {{ requestDate }}";
        public const string EMAIL_TEMPLATE_INITIAL_EMAIL = "Email notification and verification to all reviewers";

        //Dashboard
        public const string DASHBOARD_STATUS_INFORMATION_REQUESTED = "I";
        public const string DASHBOARD_STATUS_VERIFIED = "V";
        public const string DASHBOARD_STATUS_CANCELLED = "X";
        public const string DASHBOARD_STATUS_SKIPPED = "-";
        public const string DASHBOARD_STATUS_CLEAR = " ";
        public const string DASHBOARD_STATUS_SUBMITTED = " ";

        //Access Maintenance
        public const string APPLICATION_ROLE_NAME_ADMIN = "Admin";
        public const string APPLICATION_ROLE_NAME_SYSTEM = "System";
        public const string APPLICATION_ROLE_NAME_AUTHOR = "Author";
        public const string APPLICATION_ROLE_NAME_REGIONAL = "Regional";
        public const string APPLICATION_ROLE_NAME_REVIEWER = "Reviewer";
        public const string APPLICATION_ROLE_NAME_REPORT = "Report";

        public const int APPLICATION_ROLE_ID_ADMIN = 1;
        public const int APPLICATION_ROLE_ID_REVIEWER = 4;
        public const int APPLICATION_ROLE_ID_AUTHOR = 5;

        public const string UPLOAD_DIRECTORY_ATTACHMENTS = "UploadDirectoryAttachments";



    }
}
