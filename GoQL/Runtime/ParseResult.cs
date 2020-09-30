namespace Unity.GoQL
{
    /// <summary>
    /// Parsing correct or incorrect GoQL code will result in these different states.
    /// </summary>
    public enum ParseResult
    {
        UnexpectedEndOfInput,
        Empty,
        OK,
        ClosingTokenMismatch,
        InvalidNumberFormat,
        DiscriminatorSyntaxError
    }

}