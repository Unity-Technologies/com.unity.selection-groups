namespace Unity.SelectionGroups
{
    /// <summary>
    /// This exception is thrown when user intervention is required during a selection group operation. Eg, adding invalid objects.
    /// </summary>
    public class SelectionGroupException : System.Exception
    {
        /// <summary>
        /// Construct exception with error message for user.
        /// </summary>
        /// <param name="msg"></param>
        public SelectionGroupException(string msg) : base(msg)
        {

        }

    }
}