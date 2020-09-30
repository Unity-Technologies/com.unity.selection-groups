namespace Unity.GoQL
{
    struct Token
    {
        public readonly TokenType type;
        public readonly string value;

        public Token (TokenType type, string value)
        {
            this.type = type;
            this.value = value;
        }

        public Token (TokenType type, char value)
        {
            this.type = type;
            this.value = "" + value;
        }

		public override string ToString ()
		{
			return string.Format ("[Token type: {0} value: {1}]", type, value);
		}
    }
}

