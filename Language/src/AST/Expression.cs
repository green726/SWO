namespace AST;

public abstract class Expression : Node
{
    public dynamic value { get; set; } = 0;

    public bool isReference = false;
    public bool isDereference = false;

    public ParserTypeInformation type { get; set; }

    public Generator.ImplicitCast implicitCast { get; set; }

    public Expression(bool dontDoPrevExpr) : base()
    {
        this.isExpression = true;
        this.newLineReset = true;
    }

    public Expression() : base()
    {
        this.isExpression = true;
        this.newLineReset = true;
        DebugConsole.Write("resetting prev expression from within expr constructor");
        this.parser.previousExpression = this;
    }

    public Expression(Util.Token token) : base(token)
    {
        this.isExpression = true;

        this.newLineReset = true;

        DebugConsole.Write("resetting prev expression from within expr constructor");
        this.parser.previousExpression = this;
    }

    public Expression(Util.Token token, Node parent) : base(token)
    {
        this.isExpression = true;

        // if (Parser.tokenList[Parser.currentTokenNum - 1].value == "&")
        // {
        //     DebugConsole.WriteAnsi("[yellow]reference detected[/]");
        //     this.isReference = true;
        //     this.value = token.value.Substring(1, token.value.Length - 1);
        // }
        // else if (Parser.tokenList[Parser.currentTokenNum - 1].value == "*")
        // {
        //     DebugConsole.WriteAnsi("[blue]dereference detected[/]");
        //     this.isDereference = true;
        //     this.value = token.value.Substring(1, token.value.Length - 1);
        //     DebugConsole.WriteAnsi($"[blue]post deref val {this.value}[/]");
        // }
        this.newLineReset = true;
        this.parser.previousExpression = this;
        DebugConsole.Write("resetting prev expression from within expr constructor");
    }

    public Expression(Node node, bool useNodeParser) : base(node, useNodeParser)
    {
        this.isExpression = true;

        this.newLineReset = true;
        DebugConsole.Write("resetting prev expression from within expr constructor");
        this.parser.previousExpression = this;
    }

    public Expression(Node node) : base(node)
    {
        this.isExpression = true;

        this.newLineReset = true;
        DebugConsole.Write("resetting prev expression from within expr constructor");
        this.parser.previousExpression = this;
    }


    public Expression(Node node, Node parent) : base(node)
    {
        this.isExpression = true;

        this.newLineReset = true;
        DebugConsole.Write("resetting prev expression from within expr constructor");
        this.parser.previousExpression = this;
    }

}
