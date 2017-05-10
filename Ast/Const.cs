using Irony.Interpreter.Ast;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Irony.Ast;
using Irony.Parsing;

namespace Unrealscript.Ast
{
    public class Const : AstNode
    {
        public IdentifierNode Identifier;
        public LiteralValueNode Value;

        public override void Init(AstContext context, ParseTreeNode treeNode)
        {
            base.Init(context, treeNode);

            foreach (var childNode in treeNode.ChildNodes)
            {
                if (childNode.AstNode is IdentifierNode)
                {
                    Identifier = childNode.AstNode as IdentifierNode;
                }
                else if (childNode.AstNode is LiteralValueNode)
                {
                    Value = childNode.AstNode as LiteralValueNode;
                }
            }

            AsString = string.Join(" ", "const", Identifier, "=", Value);
        }
    }
}
