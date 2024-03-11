using System;
using System.Linq;

namespace SourceGeneration
{
    public class ImplementationMethod
    {
        public readonly string Name;
        public readonly Param ReturnType;
        public AttributeDecoder AttributeDecoder;
        public bool HasContext;

        public ImplementationMethod(string className, string name, Param returnType, Param[] arguments,
            AttributeDecoder attr)
        {
            AttributeDecoder = attr;
            ClassName = className;
            Name = name;
            ReturnType = returnType;
            if (arguments.Length > 0 && arguments[0].Name == "context")
            {
                HasContext = true;
                AllArguments = arguments.Select(a => a.ShiftIndexLeft()).ToArray();
            }
            else AllArguments = arguments;
        }

        public Param[] AllArguments { get; }

        public Param[] TypedArguments => HasContext ? AllArguments.Skip(1).ToArray() : AllArguments;
        public Param ContextArgument => HasContext ? AllArguments.First() : throw new InvalidOperationException();
        public string ClassName { get; }

        public string OverloadName => AttributeDecoder.OverloadName();
        public bool HasScalar => AttributeDecoder.IsFuncOrOp;
        public bool HasColumnar => AttributeDecoder.IsFuncOrOp;
    }
}