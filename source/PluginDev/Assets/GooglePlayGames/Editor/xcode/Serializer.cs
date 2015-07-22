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
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.IO;
    using System.Linq;

    class PropertyCommentChecker
    {
        private int m_Level;
        private bool m_All;
        private List<List<string>> m_Props;

        /*  The argument is an array of matcher strings each of which determine
                whether a property with a certain path needs to be decorated with a
                comment.

                A path is a number of path elements concatenated by '/'. The last path
                element is either:
                    the key if we're referring to a dict key
                    the value if we're referring to a value in an array
                    the value if we're referring to a value in a dict
                All other path elements are either:
                    the key if the container is dict
                    '*' if the container is array

                Path matcher has the same structure as a path, except that any of the
                elements may be '*'. Matcher matches a path if both have the same number
                of elements and for each pair matcher element is the same as path element
                or is '*'.

                a/b/c matches a/b/c but not a/b nor a/b/c/d
                a/* /c matches a/d/c but not a/b nor a/b/c/d
                * /* /* matches any path from three elements
            */
        protected PropertyCommentChecker(int level, List<List<string>> props)
        {
            m_Level = level;
            m_All = false;
            m_Props = props;
        }

        public PropertyCommentChecker()
        {
            m_Level = 0;
            m_All = false;
            m_Props = new List<List<string>>();
        }

        public PropertyCommentChecker(IEnumerable<string> props)
        {
            m_Level = 0;
            m_All = false;
            m_Props = new List<List<string>>();
            foreach (var prop in props)
            {
                m_Props.Add(new List<string>(prop.Split('/')));
            }
        }

        bool CheckContained(string prop)
        {
            if (m_All)
                return true;
            foreach (var list in m_Props)
            {
                if (list.Count == m_Level+1)
                {
                    if (list[m_Level] == prop)
                        return true;
                    if (list[m_Level] == "*")
                    {
                        m_All = true; // short-circuit all at this level
                        return true;
                    }
                }
            }
            return false;
        }

        public bool CheckStringValueInArray(string value) { return CheckContained(value); }
        public bool CheckKeyInDict(string key) { return CheckContained(key); }

        public bool CheckStringValueInDict(string key, string value)
        {
            foreach (var list in m_Props)
            {
                if (list.Count == m_Level + 2)
                {
                    if ((list[m_Level] == "*" || list[m_Level] == key) &&
                        list[m_Level+1] == "*" || list[m_Level+1] == value)
                        return true;
                }
            }
            return false;
        }

        public PropertyCommentChecker NextLevel(string prop)
        {
            var newList = new List<List<string>>();
            foreach (var list in m_Props)
            {
                if (list.Count <= m_Level+1)
                    continue;
                if (list[m_Level] == "*" || list[m_Level] == prop)
                    newList.Add(list);
            }
            return new PropertyCommentChecker(m_Level + 1, newList);
        }
    }

    class Serializer
    {
        public static PBXElementDict ParseTreeAST(TreeAST ast, TokenList tokens, string text)
        {
            var el = new PBXElementDict();
            foreach (var kv in ast.values)
            {
                PBXElementString key = ParseIdentifierAST(kv.key, tokens, text);
                PBXElement value = ParseValueAST(kv.value, tokens, text);
                el[key.value] = value;
            }
            return el;
        }

        public static PBXElementArray ParseArrayAST(ArrayAST ast, TokenList tokens, string text)
        {
            var el = new PBXElementArray();
            foreach (var v in ast.values)
            {
                el.values.Add(ParseValueAST(v, tokens, text));
            }
            return el;
        }

        public static PBXElement ParseValueAST(ValueAST ast, TokenList tokens, string text)
        {
            if (ast is TreeAST)
                return ParseTreeAST((TreeAST)ast, tokens, text);
            if (ast is ArrayAST)
                return ParseArrayAST((ArrayAST)ast, tokens, text);
            if (ast is IdentifierAST)
                return ParseIdentifierAST((IdentifierAST)ast, tokens, text);
            return null;
        }

        public static PBXElementString ParseIdentifierAST(IdentifierAST ast, TokenList tokens, string text)
        {
            Token tok = tokens[ast.value];
            string value;
            switch (tok.type)
            {
                case TokenType.String:
                    value = text.Substring(tok.begin, tok.end - tok.begin);
                    return new PBXElementString(value);
                case TokenType.QuotedString:
                    value = text.Substring(tok.begin, tok.end - tok.begin);
                    value = PBXStream.UnquoteString(value);
                    return new PBXElementString(value);
                default:
                    throw new Exception("Internal parser error");
            }
        }

        static string k_Indent = "\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t";

        static string GetIndent(int indent)
        {
            return k_Indent.Substring(0, indent);
        }

        static void WriteStringImpl(StringBuilder sb, string s, bool comment, GUIDToCommentMap comments)
        {
            if (comment)
                comments.WriteStringBuilder(sb, s);
            else
                sb.Append(PBXStream.QuoteStringIfNeeded(s));
        }

        public static void WriteDictKeyValue(StringBuilder sb, string key, PBXElement value, int indent, bool compact,
                                             PropertyCommentChecker checker, GUIDToCommentMap comments)
        {
            if (!compact)
            {
                sb.Append("\n");
                sb.Append(GetIndent(indent));
            }
            WriteStringImpl(sb, key, checker.CheckKeyInDict(key), comments);
            sb.Append(" = ");

            if (value is PBXElementString)
                WriteStringImpl(sb, value.AsString(), checker.CheckStringValueInDict(key, value.AsString()), comments);
            else if (value is PBXElementDict)
                WriteDict(sb, value.AsDict(), indent, compact, checker.NextLevel(key), comments);
            else if (value is PBXElementArray)
                WriteArray(sb, value.AsArray(), indent, compact, checker.NextLevel(key), comments);
            sb.Append(";");
            if (compact)
                sb.Append(" ");
        }

        public static void WriteDict(StringBuilder sb, PBXElementDict el, int indent, bool compact,
                                     PropertyCommentChecker checker, GUIDToCommentMap comments)
        {
            sb.Append("{");

            if (el.Contains("isa"))
                WriteDictKeyValue(sb, "isa", el["isa"], indent+1, compact, checker, comments);
            var keys = new List<string>(el.values.Keys);
            keys.Sort(StringComparer.Ordinal);
            foreach (var key in keys)
            {
                if (key != "isa")
                    WriteDictKeyValue(sb, key, el[key], indent+1, compact, checker, comments);
            }
            if (!compact)
            {
                sb.Append("\n");
                sb.Append(GetIndent(indent));
            }
            sb.Append("}");
        }

        public static void WriteArray(StringBuilder sb, PBXElementArray el, int indent, bool compact,
                                      PropertyCommentChecker checker, GUIDToCommentMap comments)
        {
            sb.Append("(");
            foreach (var value in el.values)
            {
                if (!compact)
                {
                    sb.Append("\n");
                    sb.Append(GetIndent(indent+1));
                }

                if (value is PBXElementString)
                    WriteStringImpl(sb, value.AsString(), checker.CheckStringValueInArray(value.AsString()), comments);
                else if (value is PBXElementDict)
                    WriteDict(sb, value.AsDict(), indent+1, compact, checker.NextLevel("*"), comments);
                else if (value is PBXElementArray)
                    WriteArray(sb, value.AsArray(), indent+1, compact, checker.NextLevel("*"), comments);
                sb.Append(",");
                if (compact)
                    sb.Append(" ");
            }

            if (!compact)
            {
                sb.Append("\n");
                sb.Append(GetIndent(indent));
            }
            sb.Append(")");
        }
    }
}
#endif
