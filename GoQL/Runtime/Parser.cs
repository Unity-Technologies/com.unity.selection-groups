using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Unity.GoQL
{
    /// <summary>
    /// The parser for GoQL code.
    /// </summary>
    public class Parser
    {
        /// <summary>
        /// Parse GoQL code represented as a string, and append instructions to the instructions list.
        /// </summary>
        /// <param name="code"></param>
        /// <param name="instructions"></param>
        /// <param name="parseResult"></param>
        public static void Parse(string code, List<object> instructions, out ParseResult parseResult)
        {
            var tokenizer = new Tokenizer();
            var tokens = tokenizer.Tokenize(code);
            parseResult = ParseResult.Empty;
            while (tokens.Count > 0)
            {
                parseResult = _Parse(tokens, instructions);
                if (parseResult != ParseResult.OK)
                {
                    return;
                }
            }
        }

        /// <summary>
        /// Parse GoQL code represented as a string.
        /// </summary>
        /// <param name="code"></param>
        /// <param name="parseResult"></param>
        /// <returns>List of instructions.</returns>
        public static List<object> Parse(string code, out ParseResult parseResult)
        {
            var tokenizer = new Tokenizer();
            var tokens = tokenizer.Tokenize(code);
            var instructions = new List<object>();
            parseResult = ParseResult.Empty;
            while (tokens.Count > 0)
            {
                parseResult = _Parse(tokens, instructions);
                if (parseResult != ParseResult.OK)
                {
                    return instructions;
                }
            }
            return instructions;
        }

        static ParseResult _Parse(List<Token> tokens, List<object> instructions)
        {
            if (tokens.Count == 0)
            {
                return ParseResult.UnexpectedEndOfInput;
                //throw new System.Exception("Syntax Error: unexpected EOF");
            }

            var token = tokens[0];
            tokens.RemoveAt(0);
            switch (token.type)
            {
                //a string in this context is a name discrimator.
                case TokenType.String:
                    instructions.Add((string)(token.value));
                    instructions.Add(GoQLCode.FilterName);
                    return ParseResult.OK;
                case TokenType.OpenSquare:
                    return _ParseIndexes(tokens, instructions);
                case TokenType.OpenAngle:
                    return _ParseDiscriminators(tokens, instructions);
                case TokenType.Slash:
                    instructions.Add(GoQLCode.EnterChildren);
                    return ParseResult.OK;
            }
            return ParseResult.OK;
        }

        static ParseResult _ParseDiscriminators(List<Token> tokens, List<object> instructions)
        {
            var elements = new List<object>();
            while (true)
            {
                if (tokens.Count == 0)
                {
                    return ParseResult.UnexpectedEndOfInput;
                }

                switch (tokens[0].type)
                {
                    //end of discriminator
                    case TokenType.CloseAngle:
                        tokens.RemoveAt(0);
                        if (elements.Count > 0)
                        {
                            instructions.AddRange(elements);
                            instructions.Add(elements.Count);
                            instructions.Add(GoQLCode.FilterByDiscriminators);
                        }
                        return ParseResult.OK;
                    case TokenType.Comma:
                        tokens.RemoveAt(0);
                        break;
                    case TokenType.Colon:
                        //Make a discriminator object
                        //if last element was a string, and next element is a string, create a discriminator.
                        if (elements.Count > 0 && elements.Last() is string && tokens.Count > 1 && tokens[1].type == TokenType.String)
                        {
                            var lastElement = elements[elements.Count - 1];
                            elements.RemoveAt(elements.Count - 1);
                            elements.Add(new Discrimator() { type = (string)lastElement, value = (string)tokens[1].value });
                            tokens.RemoveRange(0, 2);
                        }
                        else
                        {
                            return ParseResult.DiscriminatorSyntaxError;
                        }
                        break;
                    case TokenType.String:
                        //default discriminator is "type"
                        elements.Add((string)tokens[0].value);
                        tokens.RemoveAt(0);
                        break;

                    default:
                        // ignore everything else, it is a syntax error.
                        break;
                }
            }
        }

        static ParseResult _ParseIndexes(List<Token> tokens, List<object> instructions)
        {
            var elements = new List<object>();
            while (true)
            {
                if (tokens.Count == 0)
                {
                    return ParseResult.UnexpectedEndOfInput;
                }

                switch (tokens[0].type)
                {
                    //end of discriminator
                    case TokenType.CloseSquare:
                        tokens.RemoveAt(0);
                        if (elements.Count > 0)
                        {
                            instructions.AddRange(elements);
                            instructions.Add(elements.Count);
                            instructions.Add(GoQLCode.FilterIndex);
                        }
                        return ParseResult.OK;
                    case TokenType.Comma:
                        tokens.RemoveAt(0);
                        break;
                    case TokenType.Colon:
                        //Make a range object
                        int start, end;
                        tokens.RemoveAt(0);
                        start = 0;
                        if (elements.Count > 0)
                        {
                            var last = elements.Last();
                            //start range was specified, so grab it then remove from discriminator instructions.
                            if (last is int)
                            {
                                start = (int)last;
                                elements.RemoveAt(elements.Count - 1);
                            }

                        }
                        //if next token exists and is a number, it is the end range value.
                        if (tokens.Count > 0 && tokens[0].type == TokenType.Number)
                        {
                            if (!int.TryParse(tokens[0].value, out end))
                                end = -1;
                            tokens.RemoveAt(0);
                        }
                        else
                        //no end range specified, default to end index.
                        {
                            end = -1;
                        }
                        elements.Add(new Range(start, end));
                        break;
                    case TokenType.Number:
                        if (int.TryParse(tokens[0].value, out int n))
                            elements.Add(n);
                        else
                            return ParseResult.InvalidNumberFormat;
                        tokens.RemoveAt(0);
                        break;
                    default:
                        // ignore everything else, it is a syntax error.
                        break;
                }
            }
        }

    }

}