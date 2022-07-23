namespace AST;

public class VariableAssignment : AST.Node
{
    public string name = "";
    public Type type;
    public string assignmentOp = "";
    public string strValue = "";
    public bool mutable = false;
    private int childLoop = 0;

    public bool reassignment = false;
    public bool binReassignment = false;
    public BinaryExpression? bin = null;
    public AST.Node targetValue = null;

    public bool generated = false;

    public bool keyword = true;

    public VariableAssignment(Util.Token token, bool mutable, AST.Node? parent = null) : base(token)
    {
        this.nodeType = NodeType.VariableAssignment;
        this.generator = new Generator.VariableAssignment(this);


        this.newLineReset = true;

        this.mutable = mutable;
        Parser.globalVarAss.Add(this);


        if (token.value != Config.settings.variable.declaration.keyword.constant && token.value != Config.settings.variable.declaration.keyword.mutable)
        {
            reassignment = true;
            if (parent != null)
            {

                AST.Node prevNode = parent.children.Last();
                if (prevNode.nodeType == NodeType.VariableExpression)
                {
                    VariableExpression prevVarExpr = (VariableExpression)prevNode;
                    this.name = prevVarExpr.varName;
                    prevVarExpr.addParent(this);
                    this.children.Add(prevVarExpr);
                }
                this.parent = parent;
                this.parent.addChild(this);
            }
            else
            {
                throw new ParserException("illegal top level variable reassignment", token);
                // AST.Node prevNode = Parser.nodes.Last();
                // if (prevNode.nodeType == AST.Node.NodeType.VariableExpression)
                // {
                //     VariableExpression prevVarExpr = (VariableExpression)prevNode;
                //     this.name = prevVarExpr.varName;
                //     prevVarExpr.addParent(this);
                //     this.children.Add(prevVarExpr);
                // }
            }
        }
        else
        {

            Parser.nodes.Add(this);
        }

    }

    public VariableAssignment(Util.Token typeTok, Node parent = null) : base(typeTok)
    {
        this.nodeType = NodeType.VariableAssignment;
        this.generator = new Generator.VariableAssignment(this);
        this.newLineReset = true;

        this.type = new Type(typeTok);

        if (Config.settings.variable.declaration.keyword.forced)
        {
            throw ParserException.FactoryMethod("A variable declaration with no keyword was found while this options was disabled", $"Enable the option or declare the variable with your keywords (mutable: {Config.settings.variable.declaration.keyword.mutable}, constant: {Config.settings.variable.declaration.keyword.constant})", typeTok);
        }

        this.mutable = true;
        this.keyword = false;



        this.childLoop = 0;

        Parser.nodes.Add(this);
    }


    public override void addChild(Util.Token child)
    {
        if (!reassignment)
        {
            if (keyword)
            {
                switch (childLoop)
                {
                    case 0:
                        this.type = new Type(child);
                        break;
                    case 1:
                        this.name = child.value;
                        if (Config.settings.general.typo.enabled)
                        {
                            Typo.addToLittle(this);
                        }
                        Parser.declaredGlobalsDict.Add(this.name, this);
                        break;
                    case 2:
                        if (child.type != Util.TokenType.AssignmentOp) throw new ParserException($"expected assignment op but got {child.type} in a variable assignment", child);
                        this.assignmentOp = child.value;
                        break;
                    case 3:
                        this.strValue = child.value;
                        break;
                    default:
                        throw new ParserException($"Illegal extra items after variable assignment", this);

                }
            }
            else
            {
                switch (childLoop)
                {
                    case 0:
                        this.name = child.value;
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
                    case 2:
                        this.strValue = child.value;
                        break;
                    default:
                        throw new ParserException($"Illegal extra items after variable assignment", this);
                }
            }
        }
        else
        {
            switch (childLoop)
            {
                case 0:
                    break;
            }
        }
        childLoop++;


    }

    public override void addChild(AST.Node node)
    {
        base.addChild(node);
        Console.WriteLine("adding child of node type " + node.nodeType + "to varass");
        if (!reassignment)
        {
            switch (node.nodeType)
            {
                case NodeType.StringExpression:
                    if (childLoop == 3)
                    {
                        StringExpression strExp = (StringExpression)node;
                        this.strValue = strExp.value;
                    }
                    else
                    {
                        throw new ParserException($"Illegal value (type {node.nodeType}) of variable {this.name}", node);
                    }
                    break;
            }
        }
        else
        {
            switch (childLoop)
            {
                case 1:
                    if (node.nodeType == NodeType.BinaryExpression)
                    {
                        BinaryExpression binExpr = (BinaryExpression)node;
                        binExpr.leftHand = this.children.Last();
                        this.binReassignment = true;
                        this.bin = binExpr;
                    }
                    else
                    {
                        this.binReassignment = false;
                        this.targetValue = this.children.Last();
                    }
                    break;
            }
            this.targetValue = node;
            childLoop++;
        }
    }


}
