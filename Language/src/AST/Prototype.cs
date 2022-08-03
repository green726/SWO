namespace AST;

using System.Collections.Generic;

public class Prototype : AST.Node
{
    public string name = "";
    public Dictionary<Type, string> arguments = new Dictionary<Type, string>();
    private bool typePredicted = true;
    private Type prevType;

    public Type returnType;

    public bool external = false;

    public Prototype(Util.Token token, List<Util.Token> arguments = null, bool startWithRet = false, bool external = false) : base(token)
    {
        this.nodeType = NodeType.Prototype;
        this.generator = new Generator.Prototype(this);

        if (startWithRet == false)
        {
            if (!Config.settings.function.declaration.marker.word)
            {
                this.name = token.value.Substring(1);
            }
            else
            {
                this.name = "";
            }
            this.returnType = new Type("null", this);
        }
        else
        {
            this.returnType = new Type(token);
        }

        if (arguments != null)
        {
            foreach (Util.Token item in arguments)
            {
                // Console.WriteLine("funcArgs " + item.value);
                if (typePredicted)
                {
                    Parser.checkToken(item, expectedType: Util.TokenType.Keyword);
                    prevType = new Type(new Util.Token(Util.TokenType.Keyword, item.value, this.line, this.column));
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
            this.arguments = new Dictionary<Type, string>();
        }

        //TODO: replace this node type with external section
        if (external/*  || parent.nodeType == NodeType.BinaryExpression */)
        {

            // Parser.nodes.Add(this);
        }
        this.external = external;

    }


    public void addArgs(List<Util.Token> arguments)
    {
        foreach (Util.Token item in arguments)
        {
            // Console.WriteLine("funcArgs " + item.value);
            if (typePredicted)
            {
                Parser.checkToken(item, expectedType: Util.TokenType.Keyword);
                prevType = new Type(new Util.Token(Util.TokenType.Keyword, item.value, this.line, this.column));
            }
            else
            {
                //TODO: replace with config delim
                if (item.value == "[" || item.value == "]")
                {
                    Console.WriteLine("array param detected");
                    prevType.addChild(item);
                    return;
                }
                this.arguments.Add(prevType, item.value);
            }

            //swap typePredicted
            Console.WriteLine("swapping type predicted");
            typePredicted = !typePredicted;
        }
    }

    public override void addChild(Util.Token item)
    {
        if (this.name == "")
        {
            if (!Config.settings.function.declaration.marker.word)
            {
                this.name = item.value.Substring(1);
            }
            else
            {
                this.name = item.value;
            }
            return;
        }

        //TODO: replace this with config delims
        else if (item.value != "(" && item.value != ")")
        {
            addArgs(new List<Util.Token>() { item });
        }
    }
}
