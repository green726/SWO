
public class ForLoop : ASTNode
{
    public VariableAssignment index; // current index of loop
    public VariableAssignment value; // current value of loop (if applicable)
    public List<ASTNode> body;
    public bool isBody = false;
    public int parseIteration = 0;
    public bool complex = false;
    public bool valueLoop = false;
    public dynamic iterationObject; // object to be iterated over 
    public dynamic iterationValue; // amount to iterate by 

    public ForLoop(Util.Token token, ASTNode parent) : base(token)
    {
        this.nodeType = NodeType.ForLoop;
        this.body = new List<ASTNode>();

        if (parent != null)
        {
            parent.addChild(this);
        }
        else
        {
            throw new ParserException("illegal parentless for loop", this);
        }
        parseIteration++;
    }

    public override void addChild(Util.Token child)
    {

        base.addChild(child);
        if (isBody)
        {
            throw new ParserException("illegal token usage in for loop body", child);
        }
        else
        {
            if (child.type == Util.TokenType.ParenDelimiterClose && !isBody)
            {
                isBody = true;
                return;
            }

            switch (parseIteration)
            {
                case 1:
                    if (child.type != Util.TokenType.ParenDelimiterOpen)
                    {
                        throw new ParserException($"paren delimeter open expected but got {child.value}", child);
                    }
                    break;
                case 2:
                    if (child.type != Util.TokenType.Keyword)
                    {
                        throw new ParserException($"expected keyword but got {child.type}", child);
                    }
                    this.index = new VariableAssignment(new Util.Token(Util.TokenType.Keyword), false);

                    break;
                case 3:
                    if (child.value == "=")
                    {
                        complex = true;
                    }
                    else
                    {
                        iterationValue = 1;
                        if (child.value == "in")
                        {

                        }
                        else if (child.value == "of")
                        {
                            valueLoop = true;
                        }
                        else
                        {
                            throw new ParserException("illegal token in for loop", child);
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
                            case Util.TokenType.Number:
                                Console.WriteLine($"adding iteration object with value of {child.value}");
                                iterationObject = Int32.Parse(child.value);
                                break;
                            default:
                                throw new ParserException("non numerical for loops not yet supported", child);
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
                            throw new ParserException($"closing parenthese expected but got", child);
                        }
                    }
                    break;
                case 6:
                    if (complex)
                    {

                    }
                    else
                    {
                        throw new ParserException("extra tokens in simple for (possibly meant to be complex?)", child);
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
        else
        {
            parseIteration++;

        }

    }
}
