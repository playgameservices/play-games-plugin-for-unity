/// <summary>
/// Xcode PBX support library.  This is from the Unity open source.
/// https://bitbucket.org/Unity-Technologies/xcodeapi/overview
/// </summary>
///
/// The MIT License (MIT)

/// Copyright (c) 2014 Unity Technologies
///
/// Permission is hereby granted, free of charge, to any person obtaining a copy
/// of this software and associated documentation files (the "Software"), to deal
/// in the Software without restriction, including without limitation the rights
/// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
/// copies of the Software, and to permit persons to whom the Software is
/// furnished to do so, subject to the following conditions:
///
/// The above copyright notice and this permission notice shall be included in
/// all copies or substantial portions of the Software.
///
/// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
/// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
/// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
/// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
/// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
/// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
/// THE SOFTWARE.
///
#if !UNITY_5
namespace GooglePlayGames.xcode
{
    using System.Collections.Generic;
    using System.Text.RegularExpressions;
    using System.IO;
    using System.Linq;
    using System;

    class ValueAST {}

    // IdentifierAST := <quoted string> \ <string>
    class IdentifierAST : ValueAST
    {
        public int value = 0; // token id
    }

    // TreeAST := '{' KeyValuePairList '}'
    // KeyValuePairList := KeyValuePair ',' KeyValuePairList
    //                     KeyValuePair ','
    //                     (empty)
    class TreeAST : ValueAST
    {
        public List<KeyValueAST> values = new List<KeyValueAST>();
    }

    // ListAST := '(' ValueList ')'
    // ValueList := ValueAST ',' ValueList
    //              ValueAST ','
    //              (empty)
    class ArrayAST : ValueAST
    {
        public List<ValueAST> values = new List<ValueAST>();
    }

    // KeyValueAST := IdentifierAST '=' ValueAST ';'
    // ValueAST := IdentifierAST | TreeAST | ListAST
    class KeyValueAST
    {
        public IdentifierAST key = null;
        public ValueAST value = null; // either IdentifierAST, TreeAST or ListAST
    }

    class Parser
    {
        TokenList tokens;
        int currPos;

        public Parser(TokenList tokens)
        {
            this.tokens = tokens;
            currPos = SkipComments(0);
        }

        int SkipComments(int pos)
        {
            while (pos < tokens.Count && tokens[pos].type == TokenType.Comment)
            {
                pos++;
            }
            return pos;
        }

        // returns new position
        int IncInternal(int pos)
        {
            if (pos >= tokens.Count)
                return pos;
            pos++;

            return SkipComments(pos);
        }

        // Increments current pointer if not past the end, returns previous pos
        int Inc()
        {
            int prev = currPos;
            currPos = IncInternal(currPos);
            return prev;
        }

        // Returns the token type of the current token
        TokenType Tok()
        {
            if (currPos >= tokens.Count)
                return TokenType.EOF;
            return tokens[currPos].type;
        }

        void SkipIf(TokenType type)
        {
            if (Tok() == type)
                Inc();
        }

        string GetErrorMsg()
        {
            return "Invalid PBX project (parsing line " + tokens[currPos].line + ")";
        }

        public IdentifierAST ParseIdentifier()
        {
            if (Tok() != TokenType.String && Tok() != TokenType.QuotedString)
                throw new Exception(GetErrorMsg());
            var ast = new IdentifierAST();
            ast.value = Inc();
            return ast;
        }

        public TreeAST ParseTree()
        {
            if (Tok() != TokenType.LBrace)
                throw new Exception(GetErrorMsg());
            Inc();

            var ast = new TreeAST();
            while (Tok() != TokenType.RBrace && Tok() != TokenType.EOF)
            {
                ast.values.Add(ParseKeyValue());
            }
            SkipIf(TokenType.RBrace);
            return ast;
        }

        public ArrayAST ParseList()
        {
            if (Tok() != TokenType.LParen)
                throw new Exception(GetErrorMsg());
            Inc();

            var ast = new ArrayAST();
            while (Tok() != TokenType.RParen && Tok() != TokenType.EOF)
            {
                ast.values.Add(ParseValue());
                SkipIf(TokenType.Comma);
            }
            SkipIf(TokenType.RParen);
            return ast;
        }

        // throws on error
        public KeyValueAST ParseKeyValue()
        {
            var ast = new KeyValueAST();
            ast.key = ParseIdentifier();

            if (Tok() != TokenType.Eq)
                throw new Exception(GetErrorMsg());
            Inc(); // skip '='

            ast.value = ParseValue();
            SkipIf(TokenType.Semicolon);

            return ast;
        }

        // throws on error
        public ValueAST ParseValue()
        {
            if (Tok() == TokenType.String || Tok() == TokenType.QuotedString)
                return ParseIdentifier();
            else if (Tok() == TokenType.LBrace)
                return ParseTree();
            else if (Tok() == TokenType.LParen)
                return ParseList();
            throw new Exception(GetErrorMsg());
        }
    }

}
#endif
