public class PrototypeAST : ASTNode
{
    public string name;
    public Dictionary<Util.ClassType, string> arguments = new Dictionary<Util.ClassType, string>();
    private bool typePredicted = true;
    private Util.ClassType prevType = Util.ClassType.Double;

    public PrototypeAST(string name = "", List<Util.Token> arguments = null)
    {
        this.nodeType = NodeType.Prototype;
        this.name = name;


        if (arguments != null)
        {
            foreach (Util.Token item in arguments)
            {
                // Console.WriteLine("funcArgs " + item.value);
                if (typePredicted)
                {

                    Parser.checkToken(item, expectedType: Util.TokenType.Keyword);
                    switch (item.value)
                    {
                        case "double":
                            prevType = Util.ClassType.Double;
                            break;
                        case "int":
                            prevType = Util.ClassType.Int;
                            break;
                        case "string":
                            prevType = Util.ClassType.String;
                            break;
                        default:
                            throw new ArgumentException($"expected type declaration but got something else at {item.line}:{item.column}");
                    }
                }
                else
                {
                    this.arguments.Add(prevType, item.value);
                }

                //swap typePredicted
                typePredicted = !typePredicted;
            }
        }
        else
        {
            this.arguments = new Dictionary<Util.ClassType, string>();
        }
    }

    public void addArgs(List<Util.Token> arguments)
    {

        foreach (Util.Token item in arguments)
        {
            // Console.WriteLine("funcArgs " + item.value);
            if (typePredicted)
            {

                Parser.checkToken(item, expectedType: Util.TokenType.Keyword);
                switch (item.value)
                {
                    case "double":
                        prevType = Util.ClassType.Double;
                        break;
                    case "int":
                        prevType = Util.ClassType.Int;
                        break;
                    case "string":
                        prevType = Util.ClassType.String;
                        break;
                    default:
                        throw new ArgumentException($"expected type declaration but got something else at {item.line}:{item.column}");
                }
            }
            else
            {
                this.arguments.Add(prevType, item.value);
            }

            //swap typePredicted
            typePredicted = !typePredicted;
        }
    }

    public void addItem(Util.Token item)
    {
        if (this.name == "")
        {
            this.name = item.value;
            return;
        }

        addArgs(new List<Util.Token>() { item });
    }
}
