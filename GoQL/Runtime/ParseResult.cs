namespace Unity.GoQL
{
    /// <summary>
    /// Parsing correct or incorrect GoQL code will result in these different states.
    /// </summary>
    public enum ParseResult
    {
        /// <summary></summary>
        UnexpectedEndOfInput,
        /// <summary></summary>
        Empty,
        /// <summary></summary>
        OK,
        /// <summary></summary>
        ClosingTokenMismatch,
        /// <summary></summary>
        InvalidNumberFormat,
        /// <summary></summary>
        DiscriminatorSyntaxError
    }

}