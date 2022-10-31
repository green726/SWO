namespace AST;

public class VariableDeclaration : Node
{
    //NOTE: The name of the variable
    public string name = "";
    //NOTE: The type of the variable
    public Type type;
    //NOTE: the default value
    public Expression defaultValue;
    public bool mutable = false;

    //NOTE: a number used internally to tell where in the parsing we are
    private int childLoop = 0;

    public bool isArray = false;
    private bool parsingArraySize = false;

    public bool generated = false;

    public bool keyword = true;

    public bool local = false;

    public VariableDeclaration(Util.Token token, bool mutable, AST.Node parent) : base(token)
    {
        this.nodeType = NodeType.VariableDeclaration;
        this.generator = new Generator.VariableDeclaration(this);

        this.newLineReset = true;

        this.mutable = mutable;
        parser.globalVarAss.Add(this);

        this.parent = parent;

        this.defaultValue = new NullExpression(new Util.Token(Util.TokenType.Keyword, Config.settings.general.nulls.keyword, token.line, token.column));

        if (parent.nodeType == AST.Node.NodeType.Empty)
        {
            DebugConsole.Write("adding var dec to parser nodes");
            parser.nodes.Add(this);
            local = false;
        }
        else
        {
            local = true;
            parent?.addChild(this);
        }
    }

    public VariableDeclaration(Util.Token token, bool mutable) : base(token)
    {
        this.nodeType = NodeType.VariableDeclaration;
        this.generator = new Generator.VariableDeclaration(this);

        this.newLineReset = true;

        this.mutable = mutable;
        parser.globalVarAss.Add(this);

        this.defaultValue = new NullExpression(new Util.Token(Util.TokenType.Keyword, Config.settings.general.nulls.keyword, token.line, token.column));

        if (parent.nodeType == AST.Node.NodeType.Empty)
        {
            DebugConsole.Write("adding var dec to parser nodes");
            parser.nodes.Add(this);
            local = false;
        }
        else
        {
            local = true;
            parent?.addChild(this);
        }
    }

    public VariableDeclaration(Util.Token typeTok, Node parent = null) : base(typeTok)
    {
        this.nodeType = NodeType.VariableDeclaration;
        this.generator = new Generator.VariableDeclaration(this);
        this.newLineReset = true;

        this.type = new Type(typeTok);

        if (Config.settings.variable.declaration.keyword.forced)
        {
            throw ParserException.FactoryMethod("A variable declaration with no keyword was found while this options was disabled", $"Enable the option or declare the variable with your keywords (mutable: {Config.settings.variable.declaration.keyword.mutable}, constant: {Config.settings.variable.declaration.keyword.constant})", typeTok);
        }

        this.mutable = true;
        this.keyword = false;

        this.parent = parent;

        this.defaultValue = new NullExpression(new Util.Token(Util.TokenType.Keyword, Config.settings.general.nulls.keyword, typeTok.line, typeTok.column));

        if (parent.nodeType == AST.Node.NodeType.Empty)
        {
            DebugConsole.Write("adding var dec to parser nodes");
            parser.nodes.Add(this);
            this.local = false;
        }
        else
        {
            this.local = true;
            parent?.addChild(this);
        }
    }

    public override void addChild(Util.Token child)
    {
        if (parsingArraySize)
        {
            if (child.value == "]")
            {
                this.parsingArraySize = false;
                return;
            }
        }
        if (keyword)
        {
            switch (childLoop)
            {
                case 0:
                    this.type = new Type(child); parser.addNamedValueInScope(this.name, this.type);
                    DebugConsole.WriteAnsi("[green]adding vardec to stack[/]");
                    break;
                case 1:
                    //TODO: replace this with config delimiter
                    if (child.value == "[")
                    {
                        DebugConsole.Write("array detected");
                        this.isArray = true;
                        this.parsingArraySize = true;
                        this.type.isArray = true;
                        return;
                    }
                    this.name = child.value;
                    if (Config.settings.general.typo.enabled)
                    {
                        Typo.addToLittle(this);
                    }
                    parser.declaredGlobalsDict.Add(this.name, this);
                    break;
                case 2:
                    if (child.type != Util.TokenType.AssignmentOp) throw new ParserException($"expected assignment op but got {child.type} in a variable assignment", child);
                    break;
                // case 3:
                //     this.defaultValue = child;
                //     break;
                default:
                    throw new ParserException($"Illegal extra items after variable declaration", this);

            }
        }
        else
        {
            switch (childLoop)
            {
                case 0:
                    this.name = child.value;
                    parser.addNamedValueInScope(this.name, this.type);
                    DebugConsole.WriteAnsi($"[green]adding vardec to stack with name: {this.name}[/]");
                    break;
                case 1:
                    if (Config.settings.variable.declaration.keyword.mutableIsSymbol)
                    {
                        if (child.value == Config.settings.variable.declaration.keyword.mutable)
                        {
                            this.mutable = true;
                        }
                        else
                        {
                            this.mutable = false;
                        }
                    }
                    break;
                // case 2:
                //     this.strValue = child.value;
                //     break;
                default:
                    throw new ParserException($"Illegal extra items after variable assignment", this);
            }
        }
        childLoop++;
        // base.addChild(child);
    }


    public override void addChild(Node child)
    {
        base.addChild(child);
        DebugConsole.Write("adding child of node type " + child.nodeType + " to vardec");

        if (parsingArraySize)
        {
            if (child.nodeType != AST.Node.NodeType.NumberExpression)
            {
                throw ParserException.FactoryMethod($"Expected a number for array size but got {child.nodeType}", $"Replace the {child.nodeType} with a number", child);
            }
            NumberExpression numExpr = (NumberExpression)child;
            if (numExpr.type.value != "int")
            {
                throw ParserException.FactoryMethod($"Expected an int but got {numExpr.type.value} in an array declaration", $"Replace the {numExpr.type.value} with \"int\"", child);
            }
            this.type.size = numExpr.value;
        }

        if (!child.isExpression)
        {
            throw ParserException.FactoryMethod("Value that was not an expression was added to variable declaration", "remove the non-expression", child);
        }
        switch (childLoop)
        {
            case 2:
                if (!keyword)
                {
                    if (isArray)
                    {
                        ArrayExpression expr = (ArrayExpression)child;
                        if (this.type.size != 0 && expr.length != this.type.size)
                        {
                            throw ParserException.FactoryMethod("An array declaration received a default value that did not correspond with its declared size", "Make the initial expression size the same as in the assignment", child);
                        }
                        this.type.size = expr.length;
                        DebugConsole.WriteAnsi($"[green]array size of {expr.length} and type size of {this.type.size} [/]");
                    }
                    this.defaultValue = (Expression)child;
                }
                else
                {
                    throw ParserException.FactoryMethod("An illegal expression was added as the second word of a keywordless variable assignment", "Remove the illegal expression | you may have forgetten an equal sign", child);
                }
                break;
            case 3:
                if (keyword)
                {
                    if (isArray)
                    {
                        ArrayExpression expr = (ArrayExpression)child;
                        if (this.type.size != 0 && expr.length != this.type.size)
                        {
                            throw ParserException.FactoryMethod("An array declaration received a default value that did not correspond with its declared size", "Make the initial expression size the same as in the assignment", child);
                        }
                        this.type.size = expr.length;
                        DebugConsole.WriteAnsi($"[purple]array size of {expr.length} and type size of {this.type.size} [/]");
                    }
                    this.defaultValue = (Expression)child;
                }
                else
                {
                    throw ParserException.FactoryMethod("An illegal expression was added as the third word of a keyword variable assignment", "Remove the illegal expression | you may have forgetten an equal sign", child);
                }
                break;
        }

    }

}
