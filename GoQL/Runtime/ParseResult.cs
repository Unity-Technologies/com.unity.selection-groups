namespace Unity.GoQL
{
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