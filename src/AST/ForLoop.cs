
public class ForLoop : ASTNode
{
    public PhiVariable index; // current index of loop
    public PhiVariable value; // current value of loop (if applicable)
    public List<ASTNode> body;
    public bool isBody = false;
    public int parseIteration;
    public bool complex = false;
    public bool valueLoop = false;
    public dynamic iterationObject; // object to be iterated over 
    public dynamic stepValue; // amount to iterate by 

    private Util.Token indexName;
    private Util.Token valueName;

    public ForLoop(Util.Token token, ASTNode parent) : base(token)
    {
        this.nodeType = NodeType.ForLoop;
        this.body = new List<ASTNode>();

        if (parent != null)
        {
            this.parent = parent;
            parent.addChild(this);
        }
        else
        {
            throw ParserException.FactoryMethod("An illegal parentless (top level) for loop was created", "Place the for loop within a function", this);
        }
        parseIteration = 1;
    }

    public override void addChild(Util.Token child)
    {

        base.addChild(child);
        if (isBody)
        {
            throw ParserException.FactoryMethod("An unknown value was added to the body of a for loop", "Remove the unknown value from the for loop", child);
        }
        else
        {
            if (child.type == Util.TokenType.BrackDelimiterOpen && !isBody)
            {
                isBody = true;
                return;
            }

            switch (parseIteration)
            {
                case 1:
                    if (child.type != Util.TokenType.ParenDelimiterOpen)
                    {
                        throw ParserException.FactoryMethod("An \"(\" character was expected to begin the for loop but was not found", "add the \"(\" character or remove an illegal character", child);
                    }
                    break;
                case 2:
                    if (child.type != Util.TokenType.Keyword)
                    {
                        throw new ParserException($"expected keyword but got {child.type}", child);
                    }
                    this.index = new PhiVariable(this);
                    this.indexName = child;
                    break;
                case 3:
                    if (child.value == "=")
                    {
                        complex = true;
                    }
                    else
                    {
                        stepValue = new NumberExpression(new Util.Token(Util.TokenType.Int, "1", this.line, this.column), this);
                        if (child.value == "in")
                        {

                        }
                        else if (child.value == "of")
                        {
                            valueLoop = true;
                        }
                        else
                        {
                            // return;
                            throw ParserException.FactoryMethod("An illegal token was received in a for loop", "replace the illegal token with either \"for\" or \"of\"", child);
                        }
                    }
                    break;
                case 4:
                    if (complex)
                    {

                    }
                    else
                    {
                        switch (child.type)
                        {
                            // case Util.TokenType.Number:
                            // Console.WriteLine($"adding iteration object with value of {child.value}");
                            // iterationObject = Int32.Parse(child.value);
                            // this.index.addChild(new Util.Token(Util.TokenType.Keyword, "double", child.line, child.column));
                            // this.index.addChild(child);
                            // this.index.addChild(new Util.Token(Util.TokenType.AssignmentOp, "=", child.line, child.column));
                            // this.index.addChild(new Util.Token(Util.TokenType.Number, "0", child.line, child.column));
                            // this.index.setType("double");
                            // this.index.setValue()
                            // break;
                        }
                    }
                    break;
                case 5:
                    if (complex)
                    {

                    }
                    else
                    {
                        if (child.type == Util.TokenType.ParenDelimiterClose)
                        {
                            isBody = true;
                        }
                        else
                        {
                            throw ParserException.FactoryMethod("Expected closing parentheses in for loop but recieved an illegal character", "Add a closing parentheses to the end of the for loop or remove the illegal character", child);
                        }
                    }
                    break;
                case 6:
                    if (complex)
                    {

                    }
                    else
                    {
                        throw ParserException.FactoryMethod("extra values in declaration of a simple for loop (a for loop declared with \"in\" or \"of\")", "remove the extra values or turn the for loop into a complex loop (a c for loop)", child);
                    }
                    break;
            }
            parseIteration++;
        }
    }

    public override void addChild(ASTNode child)
    {
        base.addChild(child);
        if (isBody)
        {
            body.Add(child);
        }
        else if (parseIteration == 4 && child.nodeType == NodeType.NumberExpression && !isBody)
        {
            NumberExpression numExpr = (NumberExpression)child;
            iterationObject = numExpr;
            this.index.setName(this.indexName.value);
            this.index.setValue("0");
            this.index.setType("double");
            parseIteration++;
        }
        // else
        // {
        //     throw new ParserException($"illegal addition of ASTNode child of type ({child.nodeType}) to for loop", child);
        // }

    }

    public override void removeChild(ASTNode child)
    {
        base.removeChild(child);
        body.Remove(child);
    }
}

public class PhiVariable : ASTNode
{
    public string name;
    public TypeAST type;
    public string value;
    public NumberExpression numExpr;

    public PhiVariable(ASTNode node) : base(node)
    {
        this.nodeType = NodeType.PhiVariable;
        this.parent = node;
    }

    public void setValue(string value)
    {
        this.value = value;

        Util.Token numExprToken = new Util.Token(Util.TokenType.Int, value, this.line, this.column);
        this.numExpr = new NumberExpression(numExprToken, this);
    }

    public void setType(string type)
    {
        Util.Token typeToken = new Util.Token(Util.TokenType.Keyword, type, this.line, this.column);
        this.type = new TypeAST(typeToken);
    }

    public void setName(string name)
    {
        Console.WriteLine($"setting phi var name to {name}");
        this.name = name;
    }
}
