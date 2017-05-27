using Irony.Interpreter.Ast;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Irony.Ast;
using Irony.Parsing;
using System.Collections;

namespace Unrealscript.Ast
{
    public class Program : AstNode
    {
        public IList<Const> Constants { get; }
        public IList<Variable> Variables { get; }
        public DefaultProperties DefaultProperties { get; private set; }

        public Program()
        {
            Constants = new List<Const>();
            Variables = new List<Variable>();
        }

        public override void Init(AstContext context, ParseTreeNode treeNode)
        {
            base.Init(context, treeNode);

            foreach (var node in treeNode.ChildNodes)
            {
                if (node.AstNode is AuxiliaryNode)
                {
                    var constants = (node.AstNode as AuxiliaryNode).ChildNodes.OfType<Const>();
                    var variables = (node.AstNode as AuxiliaryNode).ChildNodes.OfType<Variable>();

                    foreach (var constant in constants)
                    {
                        Constants.Add(constant);
                        constant.Parent = this;
                    }

                    foreach (var variable in variables)
                    {
                        Variables.Add(variable);
                        variable.Parent = this;
                    }
                }
                else if (node.AstNode is DefaultProperties)
                {
                    DefaultProperties = (node.AstNode as DefaultProperties);
                    DefaultProperties.Parent = this;
                }
            }
        }

        public override IEnumerable GetChildNodes()
        {
            foreach (var constant in Constants)
            {
                yield return constant;
            }

            foreach (var variable in Variables)
            {
                yield return variable;
            }

            yield return DefaultProperties;
        }
    }
}
