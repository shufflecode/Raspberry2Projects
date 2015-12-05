using System;

namespace libShared
{
    /// <summary>
    /// Klasse für das Exception Handling
    /// </summary>
    public static class ExceptionHandling
    {
        /// <summary>
        /// Durchläuft eine Exception Rekursiv und gibt einen String zurück welcher den Text aller Inner-Exception enthält.
        /// </summary>
        /// <param name="ex">Die Exception welche Rekursiv durchsucht werden soll</param>
        /// <returns>Der Text aller Exception welche in der Exceptin enthalten sind</returns>
        public static string GetExceptionText(Exception ex)
        {
            if (ex == null)
            {
                return string.Empty;
            }
            else if (ex.InnerException == null)
            {
                return ex.Message;
            }
            else
            {
                return ex.Message + "\n----- Inner Exception -----\n" + GetExceptionText(ex.InnerException);
            }
        }
    }
}
