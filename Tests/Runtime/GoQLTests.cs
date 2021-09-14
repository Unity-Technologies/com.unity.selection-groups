using NUnit.Framework;
using Unity.GoQL;
using UnityEngine;

namespace Unity.SelectionGroups.Tests 
{
    public class GoQLTests
    {
        
        [Test]
        public void TestTokenizerSimple()
        {
            var tokenizer = new Tokenizer();
            var tokens = tokenizer.Tokenize("simon,wittber,123");
            Debug.Log(string.Join(" | ", tokens));
            Assert.AreEqual(TokenType.String, tokens[0].type);
            Assert.AreEqual(TokenType.Comma, tokens[1].type);
            Assert.AreEqual(TokenType.String, tokens[2].type);
            Assert.AreEqual(TokenType.Comma, tokens[3].type);
            Assert.AreEqual(TokenType.Number, tokens[4].type);
        }

        [Test]
        public void TestTokenizerNotSoSimple()
        {
            var tokenizer = new Tokenizer();
            var tokens = tokenizer.Tokenize("/simon*[wittber],-123, 3.14, <:> **");
            Assert.AreEqual(TokenType.Slash, tokens[0].type);
            Assert.AreEqual(TokenType.String, tokens[1].type);
            Assert.AreEqual(TokenType.OpenSquare, tokens[2].type);
            Assert.AreEqual(TokenType.String, tokens[3].type);
            Assert.AreEqual(TokenType.CloseSquare, tokens[4].type);
            Assert.AreEqual(TokenType.Comma, tokens[5].type);
            Assert.AreEqual(TokenType.Number, tokens[6].type);
            Assert.AreEqual(TokenType.Comma, tokens[7].type);
            Assert.AreEqual(TokenType.Number, tokens[8].type);
            Assert.AreEqual(TokenType.Comma, tokens[9].type);
            Assert.AreEqual(TokenType.OpenAngle, tokens[10].type);
            Assert.AreEqual(TokenType.Colon, tokens[11].type);
            Assert.AreEqual(TokenType.CloseAngle, tokens[12].type);
            Assert.AreEqual(TokenType.Operator, tokens[13].type);
        }
    }
}