namespace AST;

public class VariableExpression : Expression
{
    public string unmodifiedVal = "";

    public TypeInformation desiredType;

    public VariableExpression(Util.Token token, AST.Node parent, bool parentRequired = true) : base(token)
    {
        this.nodeType = NodeType.VariableExpression;
        this.generator = new Generator.VariableExpression(this);
        this.parent = parent;
        this.newLineReset = true;

        // if (Parser.isAnArrayRef(token))
        // {
        //     DebugConsole.Write($"isArrayRef (from var expr constructor) {token.value}");
        //     this.value = token.value.Remove(token.value.IndexOf("["));
        //     this.isArrayRef = true;
        //     DebugConsole.Write(token.value.Substring(token.value.IndexOf("[") + 1, token.value.IndexOf("]") - token.value.IndexOf("[") - 1));
        //     // this.arrayIndex = int.Parse(token.value.Substring(token.value.IndexOf("[") + 1, token.value.IndexOf("]") - token.value.IndexOf("[") - 1));
        //     DebugConsole.Write(this.arrayIndex);
        //     this.unmodifiedVal = token.value;
        //     handleArrayRefConstruction(token, parentRequired);
        //     return;
        // }
        // else
        // {
        this.value = token.value;
        // }

        //NOTE:  below is the absolute BS for structs
        DebugConsole.Write(this.value);
        discernType();
        checkForCasts();

        DebugConsole.WriteAnsi($"[yellow]original type:[/]");
        DebugConsole.Write(this.type.value);
        // this.type = new Type("int", this);


        // if (token.value.Contains("[") && token.value.Contains("]"))
        // {
        //     String[] splitStr = token.value.Split("[");
        //
        //     this.value = splitStr[0];
        //     this.addChild(new NumberExpression(new Util.Token(Util.TokenType.Int, splitStr[1], token.line, token.column + 1), this));
        // }


        if (parent != null)
        {
            parent.addChild(this);
        }
        else if (parentRequired)
        {
            throw new ParserException($"Illegal variable expression {this.value}", token);
        }
    }

    public void checkForCasts()
    {
        //TODO: ensure that the cast is possible and implicit - if not throw an error
        if (this.desiredType != null && this.type.value != this.desiredType.value && this.desiredType.value != "void")
        {
            DebugConsole.Write("creating implicit cast for variable expression to type " + this.desiredType.value);
            Generator.ImplicitCast implCast = new Generator.ImplicitCast(this, (Generator.Expression)this.generator, this.desiredType);
            this.generator = implCast;
        }
    }

    private void discernType()
    {
        switch (this.parent.nodeType)
        {
            case NodeType.VariableExpression:
                {
                    VariableExpression varExprPar = (VariableExpression)parent;
                    VariableDeclaration varDec = parser.declaredStructs[varExprPar.type.value].getProperty(value, this);
                    Type originalType = varDec.type;
                    this.type = (ParserTypeInformation)originalType;
                    DebugConsole.Write(type.value);
                    DebugConsole.Write("set varExpr type based on struct");
                    return;
                }
            case NodeType.Reference:
                {
                    Reference refPar = (Reference)parent;
                    if (refPar.parent.nodeType == NodeType.VariableExpression)
                    {
                        VariableExpression varExprPar = (VariableExpression)refPar.parent;
                        VariableDeclaration varDec = parser.declaredStructs[varExprPar.type.value].getProperty(value, this);
                        Type originalType = varDec.type;
                        this.type = (ParserTypeInformation)originalType;
                        DebugConsole.Write("set varExpr type based on ref");
                        return;
                    }
                    break;
                }
            case NodeType.Dereference:
                {
                    Dereference derefPar = (Dereference)parent;
                    if (derefPar.parent.nodeType == NodeType.VariableExpression)
                    {
                        VariableExpression varExprPar = (VariableExpression)derefPar.parent;
                        VariableDeclaration varDec = parser.declaredStructs[varExprPar.type.value].getProperty(value, this);
                        Type originalType = varDec.type;
                        this.type = (ParserTypeInformation)originalType;
                        DebugConsole.Write("set varExpr type based on deref");
                        return;
                    }
                    break;
                }
        }
        DebugConsole.Write(this.value);
        ParserTypeInformation originalTypeDefault = parser.getNamedValueInScope(this.value, this);
        this.type = originalTypeDefault;
        DebugConsole.Write("set varExpr type in else (based on original dec)");
    }

    private void handleArrayRefConstruction(Util.Token token, bool parentRequired = true)
    {
        discernType();
        this.type = (ParserTypeInformation)this.type.getContainedType(this);
        DebugConsole.Write(this.type.value);
        checkForCasts();

        if (parent != null)
        {
            parent.addChild(this);
        }
        else if (parentRequired)
        {
            throw new ParserException($"Illegal variable expression {this.value}", token);
        }
    }

    public override void addChild(Util.Token child)
    {
        if (this.children.Count() > 0)
        {
            this.children.Last().addChild(child);
            return;
        }
        base.addChild(child);
    }

    public override void addChild(AST.Node child)
    {
        if (child.nodeType != NodeType.VariableExpression && child.nodeType != NodeType.IndexReference && child.nodeType != NodeType.Reference && child.nodeType != NodeType.Dereference && child.nodeType != NodeType.FunctionCall)
        {
            throw ParserException.FactoryMethod($"An illegal child of type {child.nodeType} was added to a variable expression", "remove it", child, this);
        }

        // if (child.nodeType == NodeType.VariableExpression)
        // {
        //     this.isPointer = true;
        // }

        DebugConsole.Write($"adding child to varExpr with type of " + child.nodeType);
        // if (child.nodeType == NodeType.NumberExpression)
        // {
        //     this.numExpr = (NumberExpression)child;
        //     return;
        // }

        if (children.Count() > 0)
        {
            AST.Node lastChild = children.Last();
            child.parent = lastChild;
            lastChild.addChild(child);
            return;
        }

        base.addChild(child);
    }
}
