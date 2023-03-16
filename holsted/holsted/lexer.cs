using System.Diagnostics;
using System.Drawing;

namespace holsted
{
    using System;
    using System.Collections.Generic;
    using System.Text.RegularExpressions;

    namespace Metr1
    {
        public enum TokenType
        {
            KEYWORD,
            IDENTIFIER,
            OPERATOR,
            NUMBER,
            STRING,
            METHOD,
            NEW_METHOD,
            SKIP_WORDS,
            END
        };

        public class Token
        {
            public TokenType type;
            public string lexeme;

            public Token(TokenType t, string l)
            {
                this.type = t;
                this.lexeme = l;
            }
        }

        public class Lexer
        {
            public string SoursCode;

            private Regex ident,
                keyword,
                skip,
                number,
                punctuator,
                metod,
                str,
                oper,
                newMetod,
                komment1,
                komment2,
                entryPoin;

            public Lexer(string code)
            {
                SoursCode = code;
                
                ident = new Regex("([a-zA-Z_][a-zA-Z0-9_]*)");
                keyword = new Regex("(?<![a-zA-Z0-9_])(match|try|while|for|foreach|if|goto( )+([a-zA-Z_][a-zA-Z0-9_]*);)\\s+");
                skip = new Regex(
                    "((?<![a-zA-Z0-9_])(abstract|bool|byte|char|checked|const|decimal|do|to|then|catch|finally|else|elif|default|delegate|double|enum|event|explicit|extern|fixed|float|implicit|interface|internal|int|in|lock|long|let|var|object|override|private|protected|public|readonly|sbyte|short|static|class|struct|string|uint|ulong|ushort|using( )*[^;]*;|virtual|void|volatile|namespace([^{]*))(\\s+|\\[))|:|;");

                number = new Regex("\\d+");
                str = new Regex("(\"[^\"]*\")");
                oper = new Regex(
                    "(\\(|\\)|\\{|\\}|\\[|\\]|==|\\+\\+|--|&&|\\|\\||!=|[<>]=|->|\\+\\=|-\\=|\\*\\=|/\\=|%\\=|<<=|>>=|&=|\\|\\=|\\^\\=|\\*|\\/|\\%|\\+|\\-|\\!|\\~|\\^|\\&|\\||\\?|\\.|\\=|\\.\\.|,|<<|<<|<|>)");
                newMetod = new Regex("[a-zA-Z][a-zA-Z0-9]*\\s*(\\((.*?)\\))+\\s*=");
                metod = new Regex("([a-zA-Z_][a-zA-Z0-9_]*\\([^\\)]*\\)|[a-zA-Z_][a-zA-Z0-9_]\\s*[a-zA-Z_][a-zA-Z0-9_]*|[a-zA-Z_][a-zA-Z0-9_]*\\s*(\"[^\"]*\"))");
                komment1 = new Regex("(/\\*+[^(*/)]*\\*/)");
                komment2 = new Regex("\\/\\/[^\r\n]*");
                entryPoin = new Regex("\\[<EntryPoint>\\]");

            }

            public List<Token> FillTokensArr()
            {
                List<Token> toks = new List<Token>();
                Match match = komment1.Match(SoursCode);
                SoursCode = SoursCode.Remove(SoursCode.IndexOf(match.Value), match.Length);
                match = komment2.Match(SoursCode);
                SoursCode = SoursCode.Remove(SoursCode.IndexOf(match.Value), match.Length);
                match = entryPoin.Match(SoursCode);
                SoursCode = SoursCode.Remove(SoursCode.IndexOf(match.Value), match.Length);

                Token token = new Token(TokenType.STRING, "");

                while (token.type != TokenType.END)
                {
                    token = GetToken();
                    if (token.type != TokenType.SKIP_WORDS &&
                        token.type != TokenType.END && token.lexeme != "}" &&
                        token.lexeme != "]" && token.lexeme != ")")
                    {
                        toks.Add(token);
                    }
                }

                return toks;
            }

            private Token GetToken()
            {
                string code = SoursCode;
                Match match = null;
                Token tok = new Token(TokenType.STRING, "");
                match = str.Match(code);
                if (match.Value != "")
                {
                    tok.type = TokenType.STRING;
                    tok.lexeme = match.Value;
                }
                else
                {
                    match = skip.Match(code);
                    if (match.Value != "")
                    {
                        tok.type = TokenType.SKIP_WORDS;
                        tok.lexeme = match.Value.Trim();
                    }

                    else
                    {
                        match = keyword.Match(code);
                        if (match.Value != "")
                        {
                            tok.type = TokenType.KEYWORD;
                            tok.lexeme = match.Value.Trim();
                            switch (tok.lexeme)
                            {
                                case "if":
                                {
                                    tok.lexeme = "if..then..else";
                                    break;
                                }
                                case "try":
                                {
                                    tok.lexeme = "try..catch..finally";
                                    break;
                                }
                                case "for":
                                {
                                    tok.lexeme = "for..to..do";
                                    break;
                                }
                            }
                        }
                        
                        else
                        {
                            match = newMetod.Match(code);
                            if (match.Value != "")
                            {
                                tok.type = TokenType.NEW_METHOD;
                                tok.lexeme = match.Value;
                                match = ident.Match(tok.lexeme);
                                tok.lexeme = match.Value;
                            }

                            else
                            {
                                match = metod.Match(code);
                                if (match.Value != "")
                                {
                                    tok.type = TokenType.METHOD;
                                    tok.lexeme = match.Value;
                                    match = ident.Match(tok.lexeme);
                                    tok.lexeme = match.Value;
                                }

                                else
                                {
                                    match = oper.Match(code);
                                    if (match.Value != "")
                                    {
                                        tok.type = TokenType.OPERATOR;
                                        tok.lexeme = match.Value;
                                        switch (tok.lexeme)
                                        {
                                             case "{":
                                            {
                                                tok.lexeme = "{}";
                                                break;
                                            }
                                            case "(":
                                            {
                                                tok.lexeme = "()";
                                                break;
                                            }
                                            case "[":
                                            {
                                                tok.lexeme = "[]";
                                                break;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        match = ident.Match(code);
                                        if (match.Value != "")
                                        {
                                            tok.type = TokenType.IDENTIFIER;
                                            tok.lexeme = match.Value;
                                        }
                                        else
                                        {
                                            match = number.Match(code);
                                            if (match.Value != "")
                                            {
                                                tok.type = TokenType.NUMBER;
                                                tok.lexeme = match.Value;
                                            }
                                            else
                                            {
                                                tok.type = TokenType.END;
                                                tok.lexeme = "";
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                SoursCode = SoursCode.Substring(0, code.IndexOf(match.Value)) + " " + SoursCode.Substring(code.IndexOf(match.Value) + match.Length);
                return tok;
            }
        }
    }
}