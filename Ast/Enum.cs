using Irony.Ast;
using Irony.Interpreter.Ast;
using Irony.Parsing;
using System.Collections.Generic;
using System.Linq;

namespace Unrealscript.Ast
{
    public class Enum : AstNode
    {
        public IdentifierNode Name;
        public IList<IdentifierNode> Values;

        public Enum()
        {
            Values = new List<IdentifierNode>();
        }

        public override void Init(AstContext context, ParseTreeNode treeNode)
        {
            base.Init(context, treeNode);

            foreach (var node in treeNode.ChildNodes)
            {
                if (node.AstNode is IdentifierNode)
                {
                    Name = node.AstNode as IdentifierNode;
                }
                else if (node.AstNode is AuxiliaryNode)
                {
                    var values = (node.AstNode as AuxiliaryNode).ChildNodes.OfType<IdentifierNode>();

                    foreach (var value in values)
                    {
                        Values.Add(value);
                        value.Parent = this;
                    }
                }
            }
        }
    }
}