using static IRGen;
using LLVMSharp;
using Newtonsoft.Json;

public abstract class TypeInformation
{
    public string value { get; set; } = "";

    public bool isPointer { get; set; } = false;
    public bool isArray { get; set; } = false;

    public int size { get; set; } = 0;
    [JsonIgnore]
    public Parser parser { get; set; }

    public TypeInformation(Parser parser)
    {
        this.parser = parser;
    }

    public string getTypePointedTo()
    {
        if (this.isPointer)
        {
            return (this.value.Remove(this.value.Length - 1));
        }
        else
        {
            throw new ParserException("Attempted to get referenced of non pointer");
        }
    }

    public string getContainedType(AST.Node caller)
    {
        if (!this.isArray)
        {
            throw ParserException.FactoryMethod("Attempted to get the contained type of a non-array", "Internal compiler error - make an issue on GitHub", caller, 179);
        }
        string ret = this.value.Remove(this.value.IndexOf("["));
        return (ret);
    }

    public string getContainedType()
    {
        if (!this.isArray)
        {
            // throw ParserException.FactoryMethod("Attempted to get the contained of a non-array", "Internal compiler error - make an issue on GitHub", caller, this);
            throw new ParserException("Attempted to get the contained type of a non-array");
        }
        string ret = this.value.Remove(this.value.IndexOf("["));
        return (ret);
    }
}

public class GeneratorTypeInformation : TypeInformation
{
    private IRGen gen { get; set; }

    public GeneratorTypeInformation(Parser parser) : base(parser)
    {
        gen = IRGen.getInstance();
    }

    public static explicit operator GeneratorTypeInformation(ParserTypeInformation infoIn)
    {
        return new GeneratorTypeInformation(infoIn.parser) { value = infoIn.value, isPointer = infoIn.isPointer, isArray = infoIn.isArray, size = infoIn.size };
        // return new GeneratorTypeInformation() { infoIn };
    }

    private LLVMTypeRef getBasicArrayType()
    {
        string containedType = getContainedType();
        DebugConsole.Write(containedType);
        if (gen.namedTypesLLVM.ContainsKey(containedType))
        {
            return LLVM.ArrayType(gen.namedTypesLLVM[containedType], (uint)size);
        }
        (bool isInt, int bits) = parser.checkInt(containedType);
        if (isInt)
        {
            return LLVM.ArrayType(LLVM.IntType((uint)bits), (uint)size);
        }
        switch (containedType)
        {
            case "double":
                return LLVM.ArrayType(LLVM.DoubleType(), (uint)size);
            case "string":
                //TODO: implement strings as stdlib so they can have a sane type
                return LLVM.ArrayType(LLVM.Int8Type(), (uint)size);
            case "null":
                return LLVM.ArrayType(LLVM.VoidType(), (uint)size);
            default:
                throw new GenException("An unknown type was referenced");
        }
    }

    private LLVMTypeRef getBasicType()
    {
        if (isArray)
        {
            return getBasicArrayType();
        }
        if (gen.namedTypesLLVM.ContainsKey(value))
        {
            return gen.namedTypesLLVM[value];
        }
        (bool isInt, int bits) = parser.checkInt(value);
        if (isInt)
        {
            return LLVM.IntType((uint)bits);
        }
        switch (value)
        {
            case "double":
                return LLVM.DoubleType();
            case "string":
                //TODO: implement strings as stdlib so they can have a sane type
                return LLVM.ArrayType(LLVM.Int8Type(), 0);
            case "null":
                return LLVM.VoidType();
            default:
                throw new GenException("An unknown type was referenced");
        }

    }

    private LLVMTypeRef genPointer()
    {
        return (LLVM.PointerType(getBasicType(), 0));
    }

    private LLVMTypeRef genNonArray()
    {
        return (getBasicType());
    }

    public LLVMTypeRef getLLVMType()
    {
        if (!isArray)
        {
            if (isPointer)
            {
                return genPointer();
            }
            return genNonArray();
        }
        else
        {
            if (size == 0)
            {
                return genPointer();
            }
            else
            {
                uint count = (uint)size;
                return LLVM.ArrayType(getBasicType(), count);
            }
        }
    }
}

public class ParserTypeInformation : TypeInformation
{
    public ParserTypeInformation(string value) : base(Parser.getInstance())
    {
        if (value.EndsWith("*"))
        {
            this.isPointer = true;
            value = value.Substring(0, value.Length - 1);
        }

        if (value.Contains("[") && value.IndexOf("]") > value.IndexOf("["))
        {
            this.isArray = true;
            //handles array 
            int idxFirstBrack = value.IndexOf("[");
            int idxSecondBrack = value.IndexOf("]");

            if (idxFirstBrack + 1 == idxSecondBrack)
            {
                // TODO: implement auto-array sizing (gonna need to do it based on future values somehow)
            }
            else
            {
                string arrSizeStr = "";
                foreach (char ch in value.Substring(idxFirstBrack + 1, idxSecondBrack - (idxFirstBrack + 1)))
                {
                    if (!Char.IsDigit(ch))
                    {
                        throw new ParserException($"Illegal non-integer in array size declaration({ch})");
                    }
                    arrSizeStr += ch;
                }
                this.size = int.Parse(arrSizeStr);
            }
        }

        this.value = value;

        this.value = value;
    }

    public static implicit operator ParserTypeInformation(string strIn)
    {
        return new ParserTypeInformation(strIn);
    }

    public static explicit operator ParserTypeInformation(AST.Type typeIn)
    {
        return new ParserTypeInformation(typeIn.value) { isArray = typeIn.isArray, isPointer = typeIn.isPointer, size = typeIn.size, parser = typeIn.parser };
    }
}
