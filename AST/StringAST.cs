public class StringAST : ASTNode
    {
        public string value;

        public StringAST(Util.Token token)
        {
            checkToken(token, expectedType: Util.TokenType.Keyword);

            this.value = token.value;

            this.line = token.line;
            this.column = token.column;
        }

    }