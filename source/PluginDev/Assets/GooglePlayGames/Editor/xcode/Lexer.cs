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

    enum TokenType
    {
        EOF,
        Invalid,
        String,
        QuotedString,
        Comment,

        Semicolon,  // ;
        Comma,      // ,
        Eq,         // =
        LParen,     // (
        RParen,     // )
        LBrace,     // {
        RBrace,     // }
    }

    class Token
    {
        public TokenType type;

        // the line of the input stream the token starts in (0-based)
        public int line;

        // start and past-the-end positions of the token in the input stream
        public int begin, end;
    }

    class TokenList : List<Token>
    {
    }

    class Lexer
    {
        string text;
        int pos;
        int length;
        int line;

        public static TokenList Tokenize(string text)
        {
            var lexer = new Lexer();
            lexer.SetText(text);
            return lexer.ScanAll();
        }

        public void SetText(string text)
        {
            this.text = text + "    "; // to prevent out-of-bounds access during look ahead
            pos = 0;
            length = text.Length;
            line = 0;
        }

        public TokenList ScanAll()
        {
            var tokens = new TokenList();

            while (true)
            {
                var tok = new Token();
                ScanOne(tok);
                tokens.Add(tok);
                if (tok.type == TokenType.EOF)
                    break;
            }
            return tokens;
        }

        void UpdateNewlineStats(char ch)
        {
            if (ch == '\n')
                line++;
        }

        // tokens list is modified in the case when we add BrokenLine token and need to remove already
        // added tokens for the current line
        void ScanOne(Token tok)
        {
            while (true)
            {
                while (pos < length && Char.IsWhiteSpace(text[pos]))
                {
                    UpdateNewlineStats(text[pos]);
                    pos++;
                }

                if (pos >= length)
                {
                    tok.type = TokenType.EOF;
                    break;
                }

                char ch = text[pos];
                char ch2 = text[pos+1];

                if (ch == '\"')
                    ScanQuotedString(tok);
                else if (ch == '/' && ch2 == '*')
                    ScanMultilineComment(tok);
                else if (ch == '/' && ch2 == '/')
                    ScanComment(tok);
                else if (IsOperator(ch))
                    ScanOperator(tok);
                else
                    ScanString(tok); // be more robust and accept whatever is left
                return;
            }
        }

        void ScanString(Token tok)
        {
            tok.type = TokenType.String;
            tok.begin = pos;
            while (pos < length)
            {
                char ch = text[pos];
                char ch2 = text[pos+1];

                if (Char.IsWhiteSpace(ch))
                    break;
                else if (ch == '\"')
                    break;
                else if (ch == '/' && ch2 == '*')
                    break;
                else if (ch == '/' && ch2 == '/')
                    break;
                else if (IsOperator(ch))
                    break;
                pos++;
            }
            tok.end = pos;
            tok.line = line;
        }

        void ScanQuotedString(Token tok)
        {
            tok.type = TokenType.QuotedString;
            tok.begin = pos;
            pos++;

            while (pos < length)
            {
                // ignore escaped quotes
                if (text[pos] == '\\' && text[pos+1] == '\"')
                {
                    pos += 2;
                    continue;
                }

                // note that we close unclosed quotes
                if (text[pos] == '\"')
                    break;

                UpdateNewlineStats(text[pos]);
                pos++;
            }
            pos++;
            tok.end = pos;
            tok.line = line;
        }

        void ScanMultilineComment(Token tok)
        {
            tok.type = TokenType.Comment;
            tok.begin = pos;
            pos += 2;

            while (pos < length)
            {
                if (text[pos] == '*' && text[pos+1] == '/')
                    break;

                // we support multiline comments
                UpdateNewlineStats(text[pos]);
                pos++;
            }
            pos += 2;
            tok.end = pos;
            tok.line = line;
        }

        void ScanComment(Token tok)
        {
            tok.type = TokenType.Comment;
            tok.begin = pos;
            pos += 2;

            while (pos < length)
            {
                if (text[pos] == '\n')
                    break;
                pos++;
            }
            UpdateNewlineStats(text[pos]);
            pos++;
            tok.end = pos;
            tok.line = line;
        }

        bool IsOperator(char ch)
        {
            if (ch == ';' || ch == ',' || ch == '=' || ch == '(' || ch == ')' || ch == '{' || ch == '}')
                return true;
            return false;
        }

        void ScanOperator(Token tok)
        {
            switch (text[pos])
            {
                case ';': ScanOperatorSpecific(tok, TokenType.Semicolon); return;
                case ',': ScanOperatorSpecific(tok, TokenType.Comma); return;
                case '=': ScanOperatorSpecific(tok, TokenType.Eq); return;
                case '(': ScanOperatorSpecific(tok, TokenType.LParen); return;
                case ')': ScanOperatorSpecific(tok, TokenType.RParen); return;
                case '{': ScanOperatorSpecific(tok, TokenType.LBrace); return;
                case '}': ScanOperatorSpecific(tok, TokenType.RBrace); return;
                default: return;
            }
        }

        void ScanOperatorSpecific(Token tok, TokenType type)
        {
            tok.type = type;
            tok.begin = pos;
            pos++;
            tok.end = pos;
            tok.line = line;
        }
    }


}
#endif
