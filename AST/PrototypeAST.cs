public class PrototypeAST : ASTNode
{
    public string name;
    public Dictionary<TypeAST, string> arguments = new Dictionary<TypeAST, string>();
    private bool typePredicted = true;
    private TypeAST prevType;

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
                            prevType = new TypeAST("double");
                            break;
                        case "int":
                            prevType = new TypeAST("int");
                            break;
                        case "string":
                            prevType = new TypeAST("string");
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
            this.arguments = new Dictionary<TypeAST, string>();
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
                        prevType = new TypeAST("double");
                        break;
                    case "int":
                        prevType = new TypeAST("int");
                        break;
                    case "string":
                        prevType = new TypeAST("string");
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
