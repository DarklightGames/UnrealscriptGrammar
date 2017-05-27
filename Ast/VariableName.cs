using Irony.Interpreter.Ast;

namespace Unrealscript.Ast
{
    public class VariableName : AstNode
    {
        public bool IsArray
        {
            get
            {
                return ArraySize > 0;
            }
        }

        public string Name;
        public int ArraySize;

        public override string ToString()
        {
            var s = Name;
            if (IsArray)
            {
                s += "[" + ArraySize + "]";
            }
            return s;
        }
    }
}