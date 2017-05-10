using Irony.Ast;
using Irony.Interpreter.Ast;
using Irony.Parsing;
using System.Collections.Generic;
using System.Linq;
using System.Collections;

namespace Unrealscript.Ast
{
    public class ClassDeclaration : AstNode
    {
        public class Modifier : AstNode
        {
            public string Keyword;
            public IList<IdentifierNode> Arguments;

            public Modifier()
            {
                Arguments = new List<IdentifierNode>();
            }

            public override void Init(AstContext context, ParseTreeNode parseNode)
            {
                base.Init(context, parseNode);

                foreach (var childNode in parseNode.ChildNodes)
                {
                    if (childNode.AstNode == null)
                    {
                        Keyword = childNode.Token.Terminal.Name;
                    }
                    else if (childNode.AstNode is IdentifierNode)
                    {
                        Arguments.Add(childNode.AstNode as IdentifierNode);
                    }
                    else if (childNode.AstNode is AuxiliaryNode)
                    {
                        foreach (var identifier in (childNode.AstNode as AuxiliaryNode).ChildNodes.OfType<IdentifierNode>())
                        {
                            Arguments.Add(identifier);
                        }
                    }
                }

                AsString = Keyword;

                if (Arguments.Count > 0)
                {
                    AsString += "(" + string.Join(",", Arguments) + ")";
                }
            }
        }

        public IdentifierNode Name { get; set; }
        public IdentifierNode Superclass { get; set; }
        public IList<Modifier> Modifiers { get; }

        public ClassDeclaration()
        {
            Modifiers = new List<Modifier>();
        }

        public override void Init(AstContext context, ParseTreeNode parseNode)
        {
            base.Init(context, parseNode);

            foreach (var node in parseNode.ChildNodes)
            {
                // copy sentences to block
                if (node.AstNode is AuxiliaryNode)
                {
                    var auxNode = node.AstNode as AuxiliaryNode;

                    foreach (var modifier in auxNode.ChildNodes.OfType<Modifier>())
                    {
                        Modifiers.Add(modifier);
                        modifier.Parent = this;
                    }

                    foreach (var type in auxNode.ChildNodes.OfType<IdentifierNode>())
                    {
                        Superclass = type;
                    }
                }
            }

            AsString = "Unrealscript Class";
        }
    }
}
