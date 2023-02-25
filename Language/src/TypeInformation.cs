using static IRGen;
using LLVMSharp;
using Newtonsoft.Json;

public abstract class TypeInformation
{
    public string value { get; set; } = "";

    public bool isPointer { get; set; } = false;
    public bool isArray { get; set; } = false;
    public bool isStruct { get; set; } = false;
    public bool isTrait { get; set; } = false;

    public int size { get; set; } = 0;

    public TypeInformation containedType = null;

    [JsonIgnore]
    public Parser parser { get; set; }

    public TypeInformation(Parser parser)
    {
        this.parser = parser;
        (this.isStruct, this.isTrait) = (false, false);
    }

    public TypeInformation(string value, Parser parser)
    {
        this.value = value;
        this.parser = parser;
        (this.isStruct, this.isTrait) = checkForCustomType(this.value, parser);
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
        if (!this.isArray && !this.isPointer)
        {
            throw ParserException.FactoryMethod("Attempted to get the contained type of a non-array", "Internal compiler error - make an issue on GitHub", caller, 179);
        }
        return this.containedType.value;
    }

    public string getContainedType()
    {
        if (!this.isArray && !this.isPointer)
        {
            // throw ParserException.FactoryMethod("Attempted to get the contained of a non-array", "Internal compiler error - make an issue on GitHub", caller, this);
            throw new ParserException("Attempted to get the contained type of a non-array");
        }
        return this.containedType.value;
    }

    public static bool isValidType(ParserTypeInformation type)
    {
        if (type.isPointer || type.isArray)
        {
            return isValidType((ParserTypeInformation)type.containedType);
        }

        else
        {
            (bool isInt, int bits) = Parser.checkInt(type.value); if (isInt)
            {
                return true;
            }

            switch (type.value)
            {
                case "double":
                case "string":
                case "null":
                case "bool":
                case "void":
                case "char":
                    return true;
                default:
                    if (type.parser.declaredStructs.ContainsKey(type.value))
                    {
                        return true;
                    }
                    else if (type.parser.declaredStructTraits.ContainsKey(type.value))
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
            }
        }
    }

    public static bool isValidType(string value, Parser parser)
    {
        try
        {
            ParserTypeInformation typeInfo = new ParserTypeInformation(value, parser);
            return isValidType(typeInfo);
        }
        catch
        {
            return false;
        }

    }

    public static (bool, bool) checkForCustomType(string value, Parser parser)
    {
        (bool isInt, int bits) = Parser.checkInt(value);
        if (isInt)
        {
            return (false, false);
        }
        else
        {
            switch (value)
            {
                case "double":
                case "string":
                case "null":
                case "bool":
                case "void":
                case "char":
                    return (false, false);
                default:
                    // if (LLVM.GetTypeByName(parser.module, value).Pointer == IntPtr.Zero)
                    // {
                    //     throw new GenException($"Type ({value}) not found | Remove it or replace it with a declared type");
                    // }

                    if (parser.declaredStructTraits.ContainsKey(value))
                    {
                        return (false, true);
                    }
                    return (true, false);
            }
        }
    }
}

public class GeneratorTypeInformation : TypeInformation
{
    private IRGen gen { get; set; }

    public GeneratorTypeInformation(string value, Parser parser) : base(value, parser)
    {
        gen = IRGen.getInstance();
    }

    public GeneratorTypeInformation(Parser parser) : base(parser)
    {
        gen = IRGen.getInstance();
    }

    public static explicit operator GeneratorTypeInformation(ParserTypeInformation infoIn)
    {
        return new GeneratorTypeInformation(infoIn.parser) { value = infoIn.value, isPointer = infoIn.isPointer, isArray = infoIn.isArray, size = infoIn.size, isStruct = infoIn.isStruct, isTrait = infoIn.isTrait, containedType = infoIn.containedType };
        // return new GeneratorTypeInformation() { infoIn };
    }

    public static explicit operator GeneratorTypeInformation(AST.Type infoIn)
    {
        GeneratorTypeInformation contained = null;
        if (infoIn.containedType != null)
        {
            contained = (GeneratorTypeInformation)infoIn.containedType;
        }
        return new GeneratorTypeInformation(infoIn.parser) { value = infoIn.value, isPointer = infoIn.isPointer, isArray = infoIn.isArray, size = infoIn.size, isStruct = infoIn.isStruct, isTrait = infoIn.isTrait, containedType = contained };
    }

    public LLVMTypeRef genType()
    {
        return genType(this, this.gen);
    }

    public static LLVMTypeRef genType(TypeInformation type, IRGen gen)
    {
        DebugConsole.Write("gen type called with value: " + type.value);
        if (type.containedType != null)
        {
            DebugConsole.Write("detected contained types");
            LLVMTypeRef containedType = genType(type.containedType, gen);

            if (type.isPointer)
            {
                return LLVM.PointerType(containedType, 0);
            }
            else if (type.isArray)
            {
                if (type.size == 0)
                {
                    return LLVM.PointerType(containedType, 0);
                }
                return LLVM.ArrayType(containedType, (uint)type.size);
            }
        }
        else
        {
            DebugConsole.Write("no contained types");
            LLVMTypeRef basicType;
            (bool isInt, int bits) = Parser.checkInt(type.value);
            if (isInt)
            {
                basicType = LLVM.IntType((uint)bits);
            }
            else
            {
                switch (type.value)
                {
                    case "double":
                        basicType = LLVM.DoubleType();
                        break;
                    case "string":
                        //TODO: implement strings as stdlib so they can have a sane type
                        if (type.size != 0)
                        {
                            basicType = LLVM.ArrayType(LLVM.Int8Type(), (uint)type.size);
                        }
                        else
                        {
                            basicType = LLVM.PointerType(LLVM.Int8Type(), 0);
                        }
                        break;
                    case "null":
                        basicType = LLVM.VoidType();
                        break;
                    case "bool":
                        basicType = LLVM.Int1Type();
                        break;
                    case "void":
                        basicType = LLVM.Int8Type();
                        break;
                    case "char":
                        basicType = LLVM.Int8Type();
                        break;
                    default:
                        // basicType = LLVM.PointerType(LLVM.GetTypeByName(gen.module, type.value), 0);
                        basicType = LLVM.GetTypeByName(gen.module, type.value);
                        if (basicType.Pointer == IntPtr.Zero)
                        {
                            throw new GenException($"Type ({type}) not found | Remove it or replace it with a declared type");
                        }
                        break;
                }
            }
            return basicType;
        }
        return new LLVMTypeRef();
    }

    public static LLVMTypeRef getBaseStructType(string type, IRGen gen)
    {
        LLVMTypeRef declaredType = LLVM.GetTypeByName(gen.module, type);
        if (declaredType.Pointer == IntPtr.Zero)
        {
            throw new GenException($"Type ({type}) not found | Remove it or replace it with a declared type");
        }
        return declaredType;
    }
}

public class ParserTypeInformation : TypeInformation
{
    public ParserTypeInformation(string value, TypeInformation typeInfo) : this(value, typeInfo.parser) { }

    public ParserTypeInformation(string value) : this(value, Parser.getInstance()) { }

    public ParserTypeInformation(string value, Parser parser) : base(parser)
    {
        this.value = value;
        if (value.EndsWith("*"))
        {
            this.isPointer = true;
            this.containedType = new ParserTypeInformation(value.Substring(0, value.Length - 1), this);
        }
        else if (value.Contains("[") && value.IndexOf("]") > value.IndexOf("["))
        {
            this.isArray = true;
            //handles array types
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
                        throw new ParserException($"Illegal non-integer in array size declaration({ch}) - Replace it with an integer");
                    }
                    arrSizeStr += ch;
                }
                this.size = int.Parse(arrSizeStr);
            }
            this.containedType = new ParserTypeInformation(value.Substring(0, idxFirstBrack), this);
        }

        if (this.containedType != null)
        {
            this.isStruct = this.containedType.isStruct;
            this.isTrait = this.containedType.isTrait;
        }
        else
        {
            (this.isStruct, this.isTrait) = TypeInformation.checkForCustomType(this.value, parser);
        }
    }

    public static explicit operator ParserTypeInformation(string strIn)
    {
        return new ParserTypeInformation(strIn);
    }

    public static explicit operator ParserTypeInformation(AST.Type typeIn)
    {
        ParserTypeInformation contained = null;
        if (typeIn.containedType != null)
        {
            contained = (ParserTypeInformation)typeIn.containedType;
        }
        return new ParserTypeInformation(typeIn.value) { isArray = typeIn.isArray, isPointer = typeIn.isPointer, size = typeIn.size, parser = typeIn.parser, isStruct = typeIn.isStruct, isTrait = typeIn.isTrait, containedType = contained };
    }
}
