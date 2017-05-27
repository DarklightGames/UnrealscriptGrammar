using Irony.Ast;
using Irony.Interpreter.Ast;
using Irony.Parsing;

namespace Unrealscript.Ast
{
    public class DefaultPropertiesKey : AstNode
    {
        public string Name;
        public int Index = -1;

        public override void Init(AstContext context, ParseTreeNode treeNode)
        {
            base.Init(context, treeNode);

            foreach (var node in treeNode.ChildNodes)
            {
                if (node.AstNode is IdentifierNode)
                {
                    Name = (node.AstNode as IdentifierNode).AsString;
                }
            }

            AsString = ToString();
        }

        public override string ToString()
        {
            return Name + (Index != -1 ? "[" + Index + "]" : "");
        }
    }
}