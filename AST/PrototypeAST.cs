public class PrototypeAST : ASTNode
    {
        public string name;
        public Dictionary<Util.ClassType, string> arguments = new Dictionary<Util.ClassType, string>();

        public PrototypeAST(int line, int column, string name = "", List<Util.Token> arguments = null)
        {
            this.nodeType = NodeType.Prototype;
            this.name = name;

            bool typePredicted = true;
            Util.ClassType prevType = Util.ClassType.Double;
            if (arguments != null)
            {
                foreach (Util.Token item in arguments)
                {
                    // Console.WriteLine("funcArgs " + item.value);
                    if (typePredicted)
                    {

                        checkToken(item, expectedType: Util.TokenType.Keyword);
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
    }