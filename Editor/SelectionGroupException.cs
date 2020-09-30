namespace Unity.SelectionGroups
{
    public class SelectionGroupException : System.Exception
    {
        /// <summary>
        /// This exception is thrown when user intervention is required during a selection group operation. Eg, adding invalid objects.
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        public SelectionGroupException(string msg) : base(msg)
        {

        }

    }
}