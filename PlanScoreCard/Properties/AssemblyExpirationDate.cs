namespace System.Reflection
{
    /// <summary>
    /// Custom assembly attribute class
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly)]
    public class AssemblyExpirationDate : Attribute
    {
        /// <summary>
        /// ExpirationDate. 
        /// The format is always 'mm/dd/yyyy'
        /// </summary>
        public string ExpirationDate { get; private set; }

        /// <summary>
        /// Initializes a new instance of the AssemblyExpirationDate class.
        /// </summary>
        public AssemblyExpirationDate() : this(string.Empty) { }

        /// <summary>
        /// Initializes a new instance of the AssemblyExpirationDate class.
        /// </summary>
        /// <param name="expDate">Exparation date, expected format is 'mm/dd/yyyy'</param>
        public AssemblyExpirationDate(string? expDate)
        {
            ExpirationDate = string.IsNullOrWhiteSpace(expDate) || expDate == "#{AssemblyExpirationDate}#"
                ? DateTime.Now.AddDays(1).ToString("MM/dd/yyyy")
                : expDate!;
        }
    }
}
