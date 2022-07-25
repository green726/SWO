namespace AST;

using System.Collections.Generic;

public class Prototype : AST.Node
{
    public string name = "";
    public Dictionary<Type, string> arguments = new Dictionary<Type, string>();
    private bool typePredicted = true;
    private Type prevType;

    public Type returnType;

    public Prototype(Util.Token token, List<Util.Token> arguments = null, bool startWithRet = false) : base(token)
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
                // switch (item.value)
                // {
                //     case "double":
                //         prevType = new TypeAST("double");
                //         break;
                //     case "int":
                //         prevType = new TypeAST("int");
                //         break;
                //     case "string":
                //         prevType = new TypeAST("string");
                //         break;
                //     default:
                //         throw new ArgumentException($"expected type declaration but got something else at {item.line}:{item.column}");
                // }
            }
            else
            {
                this.arguments.Add(prevType, item.value);
            }

            //swap typePredicted
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

        if (item.type != Util.TokenType.DelimiterOpen && item.type != Util.TokenType.DelimiterClose)
        {
            addArgs(new List<Util.Token>() { item });
        }

    }
}
