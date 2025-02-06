namespace PersonalBudgetManager.Api.Common
{
    public static class ErrorMessages
    {
        public const string FailedToCreateUser = "Failed to insert new user.";
        public const string InvalidIdValue = "The id must be a positive value greater than 0.";
        public const string InvalidToken = "Not valid token.";
        public const string InvalidUserCredentials = "User name or password are incorrect.";
        public const string NotRegisteredCategory = "The category is not registed yet.";
        public const string EntryNotFound = "Entry not found.";
        public const string OperationCanceled = "Operation canceled by the user.";
        public const string UnexpectedError = "An unexpected server error occurred.";
        public const string UserNotFound = "Operation not valid.";
        public const string ProvideParater = "Missing necesary parameter.";
        public const string UnauthorizedOperation = "User does not have access to this record.";
    }
}
