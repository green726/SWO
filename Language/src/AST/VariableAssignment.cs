namespace AST;

public class VariableAssignment : AST.Node
{
    public string name = "";
    public Type type;
    public string assignmentOp = "";
    public Expression defaultValue;
    public bool mutable = false;
    private int childLoop = 0;

    public bool reassignment = false;
    public bool binReassignment = false;
    public BinaryExpression? bin = null;
    public AST.Node? targetValue = null;

    public bool isArray = false;
    private bool parsingArraySize = false;

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
                    this.name = prevVarExpr.value;
                    prevVarExpr.addParent(this);
                    this.children.Add(prevVarExpr);
                }
                this.parent = parent;
                this.parent.addChild(this);
                this.childLoop = 1;
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
        if (parsingArraySize)
        {
            if (child.value == "]")
            {
                this.parsingArraySize = false;
                return;
            }
        }

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
                        //TODO: replace this with config delimiter
                        if (child.value == "[")
                        {
                            Console.WriteLine("array detected");
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
                        Parser.declaredGlobalsDict.Add(this.name, this);
                        break;
                    case 2:
                        if (child.type != Util.TokenType.AssignmentOp) throw new ParserException($"expected assignment op but got {child.type} in a variable assignment", child);
                        this.assignmentOp = child.value;
                        break;
                    // case 3:
                    //     this.defaultValue = child;
                    //     break;
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
                    // case 2:
                    //     this.strValue = child.value;
                    //     break;
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

        if (parsingArraySize)
        {
            if (node.nodeType != AST.Node.NodeType.NumberExpression)
            {
                throw ParserException.FactoryMethod($"Expected a number for array size but got {node.nodeType}", $"Replace the {node.nodeType} with a number", node);
            }
            NumberExpression numExpr = (NumberExpression)node;
            if (numExpr.type.value != "int")
            {
                throw ParserException.FactoryMethod($"Expected an int but got {numExpr.type.value} in an array declaration", $"Replace the {numExpr.type.value} with \"int\"", node);
            }
            this.type.size = numExpr.value;
        }

        if (!reassignment)
        {
            if (!node.isExpression)
            {
                throw ParserException.FactoryMethod("Value that was not an expression was added to variable assignment", "remove the non-expression", node);
            }
            switch (childLoop)
            {
                case 2:
                    if (!keyword)
                    {
                        if (isArray)
                        {
                            ArrayExpression expr = (ArrayExpression)node;
                            if (this.type.size != null && expr.length != this.type.size)
                            {
                                throw ParserException.FactoryMethod("An array declaration received a default value that did not correspond with its declared size", "Make the initial expression size the same as in the assignment", node);
                            }
                            this.type.size = expr.length;
                            Spectre.Console.AnsiConsole.MarkupLine($"[green]array size of {expr.length} and type size of {this.type.size} [/]");
                        }
                        this.defaultValue = (Expression)node;
                    }
                    else
                    {
                        throw ParserException.FactoryMethod("An illegal expression was added as the second word of a keywordless variable assignment", "Remove the illegal expression | you may have forgetten an equal sign", node);
                    }
                    break;
                case 3:
                    if (keyword)
                    {
                        if (isArray)
                        {
                            ArrayExpression expr = (ArrayExpression)node;
                            if (this.type.size != null && expr.length != this.type.size)
                            {
                                throw ParserException.FactoryMethod("An array declaration received a default value that did not correspond with its declared size", "Make the initial expression size the same as in the assignment", node);
                            }
                            this.type.size = expr.length;
                            Spectre.Console.AnsiConsole.MarkupLine($"[purple]array size of {expr.length} and type size of {this.type.size} [/]");
                        }
                        this.defaultValue = (Expression)node;
                    }
                    else
                    {
                        throw ParserException.FactoryMethod("An illegal expression was added as the third word of a keyword variable assignment", "Remove the illegal expression | you may have forgetten an equal sign", node);
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
                        this.targetValue = node;
                    }
                    break;
            }
            // this.targetValue = node;
            childLoop++;
        }
    }


}
