// http://blogs.msdn.com/b/drew/archive/2009/12/31/a-simple-lexer-in-c-that-uses-regular-expressions.aspx
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;

namespace SimpleLexer
{
    public interface ILexer
    {
        void AddDefinition(TokenDefinition tokenDefinition);
        IEnumerable<Token> Tokenize(string source);
    }

    public class Lexer : ILexer
    {
        Regex endOfLineRegex = new Regex(@"\r\n|\r|\n", RegexOptions.Compiled);
        IList<TokenDefinition> tokenDefinitions_ = new List<TokenDefinition>();

        public void AddDefinition(TokenDefinition tokenDefinition)
        {
            tokenDefinitions_.Add(tokenDefinition);
        }

        public IEnumerable<Token> Tokenize(string source)
        {
            int currentIndex = 0;
            int currentLine = 1;
            int currentColumn = 0;

            TokenDefinition[] singleCharacterDefinitions = new TokenDefinition[128];
            IList<TokenDefinition> tokenDefinitions = new List<TokenDefinition>();

            TokenDefinition literalDefinition = tokenDefinitions_.Single(t => t.Type == Token.LiteralType);

            foreach (var rule in tokenDefinitions_)
            {
                if (rule.HasCharacter)
                {
                    if (rule.Character >= 0 && rule.Character < singleCharacterDefinitions.Length)
                    {
                        singleCharacterDefinitions[rule.Character] = rule;
                    }
                }
                else
                {
                    tokenDefinitions.Add(rule);
                }
            }

            while (currentIndex < source.Length)
            {
                TokenDefinition matchedDefinition = null;
                int matchLength = 0;

                if (matchedDefinition == null)
                {
                    char c = source[currentIndex];
                    if (c >= 0 && c < singleCharacterDefinitions.Length)
                    {
                        var rule = singleCharacterDefinitions[c];
                        if (rule != null)
                        {
                            matchedDefinition = rule;
                            matchLength = 1;
                        }
                    }
                }

                if (matchedDefinition == null)
                {
                    char c = source[currentIndex];
                    if (c == '"')
                    {
                        int lastIndex = source.IndexOf('"', currentIndex + 1);
                        if (lastIndex >= 0)
                        {
                            matchedDefinition = literalDefinition;
                            matchLength = lastIndex + 1 - currentIndex;
                        }
                    }
                }

                if (matchedDefinition == null)
                {
                    foreach (var rule in tokenDefinitions)
                    {
                        var match = rule.MatchRegex(source, currentIndex);

                        if (match != null && match.Success && (match.Index - currentIndex) == 0)
                        {
                            matchedDefinition = rule;
                            matchLength = match.Length;
                            break;
                        }
                    }
                }

                if (matchedDefinition == null)
                {
                    throw new Exception(string.Format("Unrecognized symbol '{0}' at index {1} (line {2}, column {3}).", source[currentIndex], currentIndex, currentLine, currentColumn));
                }
                else
                {
                    var value = source.Substring(currentIndex, matchLength);

                    if (!matchedDefinition.IsIgnored)
                        yield return new Token(matchedDefinition.Type, value, new TokenPosition(currentIndex, currentLine, currentColumn));

                    var endOfLineMatch = endOfLineRegex.Match(value);
                    if (endOfLineMatch.Success)
                    {
                        currentLine += 1;
                        currentColumn = value.Length - (endOfLineMatch.Index + endOfLineMatch.Length);
                    }
                    else
                    {
                        currentColumn += matchLength;
                    }

                    currentIndex += matchLength;
                }
            }

            // yield return new Token("(end)", null, new TokenPosition(currentIndex, currentLine, currentColumn));
            yield return new Token(Token.EndType, null, new TokenPosition(currentIndex, currentLine, currentColumn));
        }
    }

    public class Token
    {
        public Token(int type, string value, TokenPosition position)
        {
            Type = type;
            Value = value;
            Position = position;
        }

        static public readonly int NullType = 0;
        static public readonly int EndType = -1;
        static public readonly int LiteralType = -2;

        public TokenPosition Position { get; set; }
        public int Type { get; set; }
        public string Value { get; set; }

        public bool Is<T>(T type) where T : struct, IConvertible
        {
            int iType = Convert.ToInt32(type);
            return Type == iType;
        }

        public bool Is<T>(T type, string value) where T : struct, IConvertible
        {
            return Is(type) && Value == value;
        }

        public override string ToString()
        {
            return string.Format("Token: {{ Type: \"{0}\", Value: \"{1}\", Position: {{ Index: \"{2}\", Line: \"{3}\", Column: \"{4}\" }} }}", Type, Value, Position.Index, Position.Line, Position.Column);
        }
    }

    public class TokenDefinition
    {
        public TokenDefinition(
            int type,
            Regex regex)
            : this(type, regex, false)
        {
        }

        public TokenDefinition(
            int type,
            Regex regex,
            bool isIgnored)
        {
            Type = type;
            Regex = regex;
            Character = (char)0;
            IsIgnored = isIgnored;
        }

        public TokenDefinition(
            int type,
            char c)
            : this(type, c, false)
        {
        }

        public TokenDefinition(
            int type,
            char c,
            bool isIgnored)
        {
            Type = type;
            Character = c;
            IsIgnored = isIgnored;
        }

        public bool MatchChar(char c)
        {
            return c == Character;
        }

        public Match MatchRegex(string source, int currentIndex)
        {
            if (HasCharacter)
            {
                return null;
            }
            return Regex.Match(source, currentIndex);
        }

        public bool IsIgnored { get; private set; }
        public Regex Regex { get; private set; }
        public int Type { get; private set; }

        public char Character { get; private set; }
        public bool HasCharacter { get { return Character != 0; } }
    }

    public class TokenPosition
    {
        public TokenPosition(int index, int line, int column)
        {
            Index = index;
            Line = line;
            Column = column;
        }

        public int Column { get; private set; }
        public int Index { get; private set; }
        public int Line { get; private set; }
    }
}
