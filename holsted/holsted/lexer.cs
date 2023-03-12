using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace holsted
{

    public enum Type
    {
        KEYWORD,
        IDENT,
        NUMBER,
        STRING,
        OPERATOR,
        OTHER,
        NEW_METHOD,
        METHOD,
        SPACE,
        END
    }

    public class Token
    {
        public Type Type;
        public string Lex;
        
        public Token(Type t, string l)
        {
            this.Type = t;
            this.Lex = l;
        }

    }
    
    
    public class Lexer
    {
        public string ProgramCode;
        private Regex Ident, Keyword, Other,Number,Space,Metod,Str,Operator,NewMetod,Komment,KommentInLine;
        
        public Lexer(string code)
        {
            this.ProgramCode = code;
            Ident = new Regex("([a-zA-Z_][a-zA-Z0-9_]*)"); 
              Keyword = new Regex("(break|switch|try|continue|while|for|foreach|if|goto( )+([a-zA-Z_][a-zA-Z0-9_]*);|return){1}");
              Other = new Regex("((\\$|abstract|bool|byte|char|checked|const|decimal|do|catch|finally|else|case|default|delegate|double|enum|event|explicit|extern|fixed|float|implicit|interface|internal|int|lock|long|let|object|override|private|protected|public|readonly|sbyte|short|static|class|struct|string|uint|ulong|ushort|using( )*[^;]*;|virtual|void|volatile|([a-zA-Z_][a-zA-Z0-9_]*):|:|;|namespace([^{]*)){1})");
            Number = new Regex("\\d+"); 
            Str = new Regex("(\"[^\"]*\")");
              Space = new Regex("  \\s*");
            Operator = new Regex("(\\(|\\)|\\{|\\}|\\[|\\]|==|\\+\\+|--|&&|\\|\\||!=|[<>]=|->|\\+\\=|-\\=|\\*\\=|/\\=|%\\=|<<=|>>=|&=|\\|\\=|\\^\\=|\\*|\\/|\\%|\\+|\\-|\\!|\\~|\\^|\\&|\\||\\?|\\.|\\=|\\.\\.|\\;|,|<<|<<|<|>| in | is | as )");
            NewMetod = new Regex("[a-zA-Z_][a-zA-Z0-9_]*\\([^\\)]*\\)\\s*\\{[^\\}]*\\}");
            Metod = new Regex("([a-zA-Z_][a-zA-Z0-9_]*\\([^\\)]*\\))");
            Komment = new Regex("\\(\\*[^(*/)]*\\*\\)");
            KommentInLine = new Regex("\\/\\/[^\r\n]*");
        }
        
        
        
        public List<Token> FillTokenList()
        {
            List<Token> tokenList = new List<Token>();
            Match match = Komment.Match(ProgramCode);
            ProgramCode = ProgramCode.Remove(ProgramCode.IndexOf(match.Value), match.Length);
            match = KommentInLine.Match(ProgramCode);
            ProgramCode = ProgramCode.Remove(ProgramCode.IndexOf(match.Value), match.Length);
            
            Token token = new Token(Type.STRING,"");

            while (token.Type != Type.END)
            {
                token = getOneToken();
                if (token.Type != Type.OTHER && token.Type != Type.SPACE && token.Type != Type.END && token.Lex != "}" && token.Lex != "]" && token.Lex != ")")
                  tokenList.Add(token);
            }
            return tokenList;
        }
        
        
        
        private Token getOneToken()
        {
            string code = ProgramCode;
            Match match = null;
            Token getToken = new Token(Type.STRING, "");
            
            match = Str.Match(code);
            if (match.Value != "")
            {
                getToken.Type = Type.STRING;
                getToken.Lex = match.Value;
                ProgramCode = ProgramCode.Substring(0, code.IndexOf(match.Value)) + " " + ProgramCode.Substring(code.IndexOf(match.Value) + match.Length);
                return getToken;
            }
                
            match = Other.Match(code);
            if (match.Value != "") {
                getToken.Type = Type.OTHER;
                getToken.Lex = match.Value;
                ProgramCode = ProgramCode.Substring(0, code.IndexOf(match.Value)) + " " + ProgramCode.Substring(code.IndexOf(match.Value) + match.Length);
                return getToken;
            }
               
                    
            match = Keyword.Match(code);
            if (match.Value != "")
            {
                getToken.Type = Type.KEYWORD;
                getToken.Lex = match.Value;
                switch (getToken.Lex)
                {
                    case "if":
                    {
                        getToken.Lex = "if-else";
                        break;
                    }
                    case "try":
                    {
                        getToken.Lex = "try-catch-finally";
                        break;
                    }
                    case "switch":
                    {
                        getToken.Lex = "switch-case";
                        break;
                    }
                }

                ProgramCode = ProgramCode.Substring(0, code.IndexOf(match.Value)) + " " + ProgramCode.Substring(code.IndexOf(match.Value) + match.Length);
                return getToken;
            }
                   
                        
            match = NewMetod.Match(code);
            if (match.Value != "")
            {
                getToken.Type = Type.NEW_METHOD;
                getToken.Lex = match.Value;
                match = Ident.Match(getToken.Lex);
                getToken.Lex = match.Value;
                ProgramCode = ProgramCode.Substring(0, code.IndexOf(match.Value)) + " " + ProgramCode.Substring(code.IndexOf(match.Value) + match.Length);
                return getToken;
            }
                       
                            
            match = Metod.Match(code);
            if (match.Value != "")
            {
                getToken.Type = Type.METHOD;
                getToken.Lex = match.Value+"()";
                match = Ident.Match(getToken.Lex);
                getToken.Lex = match.Value;
                ProgramCode = ProgramCode.Substring(0, code.IndexOf(match.Value)) + " " + ProgramCode.Substring(code.IndexOf(match.Value) + match.Length);
                return getToken;
            }
                                
            match = Operator.Match(code);
            if (match.Value != "")
            {
                getToken.Type = Type.OPERATOR;
                getToken.Lex = match.Value;
                switch (getToken.Lex)
                {
                    case "{":
                    {
                        getToken.Lex = "{}";
                        break;
                    }
                    case "(":
                    {
                        getToken.Lex = "()";
                        break;
                    }
                    case "[":
                    {
                        getToken.Lex = "[]";
                        break;
                    }
                    case "?":
                    {
                        getToken.Lex = "?:";
                        break;
                    }
                }
                ProgramCode = ProgramCode.Substring(0, code.IndexOf(match.Value)) + " " + ProgramCode.Substring(code.IndexOf(match.Value) + match.Length);
                return getToken;
            }
                                    
            match = Ident.Match(code);
            if (match.Value != "")
            {
                getToken.Type = Type.IDENT;
                getToken.Lex = match.Value;
                ProgramCode = ProgramCode.Substring(0, code.IndexOf(match.Value)) + " " + ProgramCode.Substring(code.IndexOf(match.Value) + match.Length);
                return getToken;
            }
                                        
            match = Number.Match(code);
            if (match.Value != "")
            {
                getToken.Type = Type.NUMBER;
                getToken.Lex = match.Value;
                ProgramCode = ProgramCode.Substring(0, code.IndexOf(match.Value)) + " " + ProgramCode.Substring(code.IndexOf(match.Value) + match.Length);
                return getToken;
            }
                                            
            match = Space.Match(code);
            if (match.Value != "")
            {
                getToken.Type = Type.SPACE;
                getToken.Lex = match.Value;
                ProgramCode = ProgramCode.Substring(0, code.IndexOf(match.Value)) + " " + ProgramCode.Substring(code.IndexOf(match.Value) + match.Length);
                return getToken;
            }
            else
            {
                getToken.Type = Type.END;
                getToken.Lex = "";
                ProgramCode = ProgramCode.Substring(0, code.IndexOf(match.Value)) + " " + ProgramCode.Substring(code.IndexOf(match.Value) + match.Length);
                return getToken;
            }
        }
    }
}