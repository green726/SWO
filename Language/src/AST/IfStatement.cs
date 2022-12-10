namespace AST;


public class IfStatement : AST.Node
{
    public List<AST.Node> followingBlocks { get; set; } = new List<AST.Node>();

    public Expression condition { get; set; } = new AST.Empty();

    public List<AST.Node> body { get; set; } = new List<AST.Node>();

    private int tokenChildIdx = 0;
    private bool isBody = false;

    public IfStatement(Util.Token token, AST.Node parent) : base(token, parent)
    {
        this.nodeType = NodeType.IfStatement;
        this.generator = new Generator.IfStatement(this);
    }

    public override void addChild(Util.Token child)
    {
        switch (tokenChildIdx)
        {
            case 0:
                if (child.value != "(")
                {
                    throw ParserException.FactoryMethod("Illegal child added to if statement", "Remove it and replace it with the legal delimiter, \"(\"", child, this);
                }
                break;
            case 1:
                if (child.value != ")")
                {
                    throw ParserException.FactoryMethod("Illegal child added to if statement", "Remove it and replace it with the legal delimiter, \")\"", child, this);
                }
                break;
            case 2:
                if (child.value != "{")
                {
                    throw ParserException.FactoryMethod("Illegal child added to if statement", "Remove it and replace it with the legal delimiter, \"{\"", child, this);
                }
                this.isBody = true;
                break;
            case 3:
                if (child.value != "}")
                {
                    throw ParserException.FactoryMethod("Illegal child added to if statement", "Remove it and replace it with the legal delimiter, \"}\"", child, this);
                }
                break;
        }
        tokenChildIdx++;
        base.addChild(child);
    }

    public override void addChild(Node child)
    {
        if (this.condition.nodeType == NodeType.Empty && !this.isBody)
        {
            if (child.nodeType != NodeType.BinaryExpression)
            {
                throw ParserException.FactoryMethod("A non binary expression (conditional) was used as a conditional in an if statement", "Remove it and replace it with a binary expression (ie foo == bar)", child, this);
            }
            this.condition = (BinaryExpression)child;
        }
        else if (this.isBody && this.condition.nodeType == NodeType.Empty)
        {
            //throw ParserException.FactoryMethod about not being able to have a conditonless if statement
            throw ParserException.FactoryMethod("A conditionless if statement was found", "Remove it and replace it with a conditional", child, this);
        }
        else if (this.isBody && this.condition.nodeType != NodeType.Empty)
        {
            this.body.Add(child);
        }
        base.addChild(child);
    }
}


// public class IfStatementDeclaration : AST.Node
// {
//     public IfStatement ifStatementParent;
//     public BinaryExpression expression = null;
//     public bool finsihedParsing = false;
//
//     public IfStatementDeclaration(Util.Token token, AST.Node parent = null) : base(token)
//     {
//         this.nodeType = NodeType.IfStatementDeclaration;
//         this.generator = new Generator.IfStatementDeclaration(this);
//
//         this.ifStatementParent = new IfStatement(token, this, parent);
//         this.parent = this.ifStatementParent;
//         // parent.addChild(this);
//     }
//
//     public override void addChild(AST.Node child)
//     {
//         if (expression == null)
//         {
//             Parser.checkNode(child, new NodeType[] { NodeType.BinaryExpression, NodeType.NumberExpression, NodeType.VariableExpression });
//             if (child.nodeType == NodeType.BinaryExpression)
//             {
//                 expression = (BinaryExpression)child;
//             }
//             else
//             {
//                 base.addChild(child);
//                 return;
//             }
//         }
//         else
//         {
//             throw new ParserException($"Attempted to add multiple conditions to if statement (currently unsupported)", child);
//         }
//         base.addChild(child);
//     }
//
//     public override void addChild(Util.Token child)
//     {
//         // if (child.type == Util.TokenType.ParenDelimiterClose)
//         // {
//         //     isStat = false;
//         // }
//         // if (child.value == Config.settings.ifModel.body.delimeters[0]/* Util.TokenType.BrackDelimiterOpen */)
//         // {
//         //     ifStatementParent.isBody = true;
//         // }
//         if (child.value == "(")
//         {
//             base.addChild(child);
//             return;
//         }
//         else if (child.value == ")")
//         {
//             finsihedParsing = true;
//             base.addChild(child);
//             return;
//         }
//
//         else
//         {
//             throw new ParserException($"illegal child ({child.value}) added to if statement declaration", child);
//         }
//         // base.addChild(child);
//     }
// }
//
// public class IfStatement : AST.Node
// {
//     public List<AST.Node> thenBody = new List<AST.Node>();
//     public ElseStatement elseStat;
//     public bool isBody = false;
//     public IfStatementDeclaration declaration;
//     // private bool isStat = true;
//
//
//     public IfStatement(Util.Token token, IfStatementDeclaration declaration, AST.Node parent) : base(token)
//     {
//         this.nodeType = NodeType.IfStatement;
//         this.generator = new Generator.IfStatement(this);
//
//         parent?.addChild(this);
//         this.parent = parent;
//
//         this.declaration = declaration;
//
//
//         // Util.Token thenProtoTok = new Util.Token(Util.TokenType.Keyword, "@then" + Parser.ifFuncNum, token.line, token.column);
//         // Prototype thenProto = new Prototype(thenProtoTok);
//         // this.thenFunc = new Function(thenProto, new List<AST.Node>(), topLevel: false);
//         // this.thenFunc.utilFunc = true;
//
//
//         // Util.Token thenCallTok = new Util.Token(Util.TokenType.Keyword, "then" + Parser.ifFuncNum, token.line, token.column);
//         // this.thenCall = new FunctionCall(thenCallTok, null, topLevel: false);
//
//         this.elseStat = new ElseStatement(this, token);
//
//     }
//     public override void addChild(AST.Node child)
//     {
//         if (child.nodeType == NodeType.IfStatementDeclaration)
//         {
//             // base.addChild(child);
//             return;
//         }
//         if (isBody == true)
//         {
//             DebugConsole.Write("adding node of type " + child.nodeType + " to if statement then body");
//             thenBody.Add(child);
//         }
//         else
//         {
//             declaration.addChild(child);
//             return;
//             // throw new ParserException($"Attempted to add multiple conditions to if statement (currently unsupported)", child);
//         }
//         base.addChild(child);
//     }
//
//     public override void addChild(Util.Token child)
//     {
//         if (!declaration.finsihedParsing)
//         {
//             declaration.addChild(child);
//             return;
//         }
//         else if (child.value == Config.settings.ifModel.body.delimeters[0]/* Util.TokenType.BrackDelimiterOpen */)
//         {
//             this.isBody = true;
//         }
//         else if (child.value == Config.settings.ifModel.body.delimeters[1])
//         {
//             base.addChild(child);
//             return;
//         }
//         else
//         {
//             throw new ParserException($"illegal child ({child.value}) added to if statement", child);
//         }
//         base.addChild(child);
//     }
//
// }
