namespace AST;

public class VariableExpression : Expression
{
    public VariableExpression(Util.Token token, AST.Node parent, bool parentRequired = true) : base(token)
    {
        this.nodeType = NodeType.VariableExpression;
        this.generator = new Generator.VariableExpression(this);

        this.value = token.value;
        this.parent = parent;

        //TODO: allow this to handle structs
        DebugConsole.Write(this.value);
        if (this.parent.nodeType == NodeType.VariableExpression)
        {
            VariableExpression varExprPar = (VariableExpression)parent;
            VariableDeclaration varDec = parser.declaredStructs[varExprPar.type.value].getProperty(this.value, this);
            AST.Type originalType = varDec.type;
            this.type = originalType;
            DebugConsole.Write(this.type.value);
            DebugConsole.Write("set varExpr type based on struct");
        }
        else if (this.parent.nodeType == NodeType.Reference)
        {
            Reference refPar = (Reference)parent;
            if (refPar.parent.nodeType == NodeType.VariableExpression)
            {
                VariableExpression varExprPar = (VariableExpression)refPar.parent;
                VariableDeclaration varDec = parser.declaredStructs[varExprPar.type.value].getProperty(this.value, this);
                AST.Type originalType = varDec.type;
                this.type = originalType;
                DebugConsole.Write("set varExpr type based on ref");
            }
        }
        else if (this.parent.nodeType == NodeType.Dereference)
        {
            Dereference derefPar = (Dereference)parent;
            if (derefPar.parent.nodeType == NodeType.VariableExpression)
            {
                VariableExpression varExprPar = (VariableExpression)derefPar.parent;
                VariableDeclaration varDec = parser.declaredStructs[varExprPar.type.value].getProperty(this.value, this);
                AST.Type originalType = varDec.type;
                this.type = originalType;
                DebugConsole.Write("set varExpr type based on deref");
            }
        }
        if (this.type == null)
        {
            AST.Type originalType = parser.getNamedValueInScope(this.value, this);
            this.type = originalType;
            DebugConsole.Write("set varExpr type in else (based on original dec)");
        }
        DebugConsole.WriteAnsi($"[yellow]original type + {this.type.value}[/]");
        // this.type = new Type("int", this);

        this.newLineReset = true;

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
        if (child.nodeType != NodeType.VariableExpression && child.nodeType != NodeType.IndexReference && child.nodeType != NodeType.Reference && child.nodeType != NodeType.Dereference)
        {
            throw ParserException.FactoryMethod("An illegal child was added to a variable expression", "remove it", child);
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
