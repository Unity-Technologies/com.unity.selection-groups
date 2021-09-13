
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Unity.GoQL
{
    class Tokenizer
    {
        System.Action _TokenizeFunction;
        string code = "";
        int index = 0;
        string valueAccumulator = "";
        List<Token> _tokens = new List<Token>();
        HashSet<char> specialChars = new HashSet<char>("<>,:/[]".ToCharArray());

        HashSet<string> operators = new HashSet<string>(new[]
        {
            "**"
        });

        public List<Token> Tokenize(string code)
        {
            this.index = 0;
            this.code = code;
            WhatIsMyNextState();
            while (DataAvailable)
                _TokenizeFunction();
            this.code = null;
            return _tokens;
        }

        char PeekChar(int offset=0) => code[index+offset];

        void ConsumeChar() => index++;

        bool DataAvailable => index < code.Length;

        void WhatIsMyNextState()
        {
            Log($"What is my next state: {DataAvailable}");
            if (DataAvailable)
            {
                var c = PeekChar();
                if (IsStringChar(c))
                    ChangeState(CollectString);
                else if (char.IsNumber(c) || c == '-')
                    ChangeState(CollectNumber);
                else if (char.IsWhiteSpace(c))
                    ChangeState(CollectWhitespace);
                else
                    ChangeState(CollectToken);
            }
        }

        private static bool IsStringChar(char c)
        {
            return char.IsLetter(c) || c == '*' || c == '_';
        }

        void CollectWhitespace()
        {
            var c = PeekChar();
            if (char.IsWhiteSpace(c))
                ConsumeChar();
            WhatIsMyNextState();
        }

        void CollectString()
        {
            var c = PeekChar();
            if (valueAccumulator.Length == 0) //This is the first char, so collect it.
            {
                valueAccumulator += c;
                ConsumeChar();
                if (!DataAvailable) 
                    AddToken(TokenType.String);
            }
            else if (specialChars.Contains(c)) //Special chars will end and collect the string
            {
                AddToken(TokenType.String);
                WhatIsMyNextState();
            }
            else //This is part of the string being collected.
            {
                valueAccumulator += c;
                ConsumeChar();
                if (!DataAvailable) 
                    AddToken(TokenType.String);
            }
        }

        void CollectNumber()
        {
            var c = PeekChar();
            if (valueAccumulator.Length == 0) //This is the first char, so collect it.
            {
                valueAccumulator += c;
                ConsumeChar();
                if (!DataAvailable) 
                    AddToken(TokenType.Number);
            }
            else if (specialChars.Contains(c)) //Special chars will end and collect the number
            {
                AddToken(TokenType.Number);
                WhatIsMyNextState();
            }
            else //This is part of the string being collected.
            {
                valueAccumulator += c;
                ConsumeChar();
                if (!DataAvailable) 
                    AddToken(TokenType.Number);
            }
        }

        void CollectToken()
        {
            var c = PeekChar();
            switch (c)
            {
                case '[':
                    valueAccumulator += c;
                    ConsumeChar();
                    AddToken(TokenType.OpenSquare);
                    WhatIsMyNextState();
                    break;
                case ']':
                    valueAccumulator += c;
                    ConsumeChar();
                    AddToken(TokenType.CloseSquare);
                    WhatIsMyNextState();
                    break;
                case '<':
                    valueAccumulator += c;
                    ConsumeChar();
                    AddToken(TokenType.OpenAngle);
                    WhatIsMyNextState();
                    break;
                case '>':
                    valueAccumulator += c;
                    ConsumeChar();
                    AddToken(TokenType.CloseAngle);
                    WhatIsMyNextState();
                    break;
                case ':':
                    valueAccumulator += c;
                    ConsumeChar();
                    AddToken(TokenType.Colon);
                    WhatIsMyNextState();
                    break;
                case ',':
                    valueAccumulator += c;
                    ConsumeChar();
                    AddToken(TokenType.Comma);
                    WhatIsMyNextState();
                    break;
                case '/':
                    valueAccumulator += c;
                    ConsumeChar();
                    AddToken(TokenType.Slash);
                    WhatIsMyNextState();
                    break;
                default:
                    Debug.Log($"Unhandled char: {c}");
                    ConsumeChar();
                    WhatIsMyNextState();
                    break;
            }

        }

        void AddToken(TokenType type)
        {
            if (type == TokenType.String && operators.Contains(valueAccumulator))
                type = TokenType.Operator;
            Log("Adding token: " + type + " " + valueAccumulator);
            _tokens.Add(new Token(type, valueAccumulator));
            valueAccumulator = "";
        }

        void ChangeState(System.Action fn)
        {
            Log("Changing state: ", fn.Method);
            _TokenizeFunction = fn;
        }

        void Log(string msg, params object[] args)
        {
            if (debug)
            {
                foreach (var o in args)
                {
                    msg = msg + " " + o.ToString();
                }
                Debug.Log(msg);
            }
        }

        bool debug = false;
    }
}



