﻿using Irony.Interpreter.Ast;
using System.Collections.Generic;
using System.Linq;
using Irony.Ast;
using Irony.Parsing;

namespace Unrealscript.Ast
{
    public class Variable : AstNode
    {
        public Type Type;
        public bool IsEditable;
        public IdentifierNode EditCategory;
        public IList<VariableName> Names;
        public IList<Modifier> Modifiers;

        public Variable()
        {
            Names = new List<VariableName>();
            Modifiers = new List<Modifier>();
        }

        public override void Init(AstContext context, ParseTreeNode treeNode)
        {
            base.Init(context, treeNode);

            foreach (var node in treeNode.ChildNodes)
            {
                if (node.AstNode is Type)
                {
                    Type = (node.AstNode as Type);
                }
                else if (node.AstNode is AuxiliaryNode)
                {
                    var auxNode = node.AstNode as AuxiliaryNode;
                    var modifiers = auxNode.ChildNodes.OfType<Modifier>();
                    var names = auxNode.ChildNodes.OfType<VariableName>();

                    foreach (var modifier in modifiers)
                    {
                        Modifiers.Add(modifier);
                        modifier.Parent = this;
                    }

                    foreach (var name in names)
                    {
                        Names.Add(name);
                        name.Parent = this;
                    }
                }
            }

            AsString = string.Join(" ", "var", string.Join(" ", Modifiers), Type, string.Join(", ", Names));
        }
    }
}
