namespace System
{
    public static class ExceptionExtensions
    {
        public static string GetSafeMessage(this Exception ex)
        {
            if (ex == null)
                return string.Empty;

            if (ex.InnerException != null)
                return ex.Message + " ---> " + ex.InnerException.GetSafeMessage();

            return ex.Message;
        }
    }
}
