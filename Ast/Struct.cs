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
    public class Struct : AstNode
    {
        public IdentifierNode Name;
        public IList<Variable> Variables;

        public override void Init(AstContext context, ParseTreeNode treeNode)
        {
            base.Init(context, treeNode);

            foreach (var node in treeNode.ChildNodes)
            {
                if (node.AstNode is IdentifierNode)
                {
                    Name = node.AstNode as IdentifierNode;
                }
            }
        }
    }
}
