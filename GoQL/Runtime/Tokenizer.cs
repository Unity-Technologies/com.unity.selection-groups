
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
        string acc = "";
        List<Token> _tokens = new List<Token>();
        HashSet<char> specialChars = new HashSet<char>("<>,:/[]".ToCharArray());

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

        char PeekChar() => code[index];

        void ConsumeChar() => index++;

        bool DataAvailable => index < code.Length;

        void WhatIsMyNextState()
        {
            if (DataAvailable)
            {
                var c = PeekChar();
                if (char.IsLetter(c) || _TokenizeFunction == CollectString && char.IsDigit(c) || c == '*')
                    ChangeState(CollectString);
                else if (char.IsNumber(c) || c == '-')
                    ChangeState(CollectNumber);
                else if (char.IsWhiteSpace(c))
                    ChangeState(CollectWhitespace);
                else
                    ChangeState(CollectToken);
            }
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
            if (acc.Length == 0)
            {
                acc += c;
                ConsumeChar();
            }
            else if (specialChars.Contains(c))
            {
                AddToken(TokenType.String);
                WhatIsMyNextState();
            }
            else
            {
                acc += c;
                ConsumeChar();
                if (!DataAvailable) 
                    AddToken(TokenType.String);
            }
        }

        void CollectNumber()
        {
            var c = PeekChar();
            if (char.IsNumber(c) || c == '.' || c == '-')
            {
                acc += c;
                ConsumeChar();
            }
            else
            {
                AddToken(TokenType.Number);
                WhatIsMyNextState();
            }
        }

        void CollectToken()
        {
            var c = PeekChar();
            switch (c)
            {
                case '[':
                    acc += c;
                    ConsumeChar();
                    AddToken(TokenType.OpenSquare);
                    WhatIsMyNextState();
                    break;
                case ']':
                    acc += c;
                    ConsumeChar();
                    AddToken(TokenType.CloseSquare);
                    WhatIsMyNextState();
                    break;
                case '<':
                    acc += c;
                    ConsumeChar();
                    AddToken(TokenType.OpenAngle);
                    WhatIsMyNextState();
                    break;
                case '>':
                    acc += c;
                    ConsumeChar();
                    AddToken(TokenType.CloseAngle);
                    WhatIsMyNextState();
                    break;
                case ':':
                    acc += c;
                    ConsumeChar();
                    AddToken(TokenType.Colon);
                    WhatIsMyNextState();
                    break;
                case ',':
                    acc += c;
                    ConsumeChar();
                    AddToken(TokenType.Comma);
                    WhatIsMyNextState();
                    break;
                case '*':
                    acc += c;
                    ConsumeChar();
                    AddToken(TokenType.Wildcard);
                    WhatIsMyNextState();
                    break;
                case '/':
                    acc += c;
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
            Log("Adding token: " + type + " " + acc);
            _tokens.Add(new Token(type, acc));
            acc = "";
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



