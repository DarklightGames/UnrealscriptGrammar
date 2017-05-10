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
    public class Type : AstNode
    {
        [Flags]
        enum TypeFlags
        {
            IsPrimitive = 0x1
        };

        public object RawType { get; set; }

        public override void Init(AstContext context, ParseTreeNode treeNode)
        {
            base.Init(context, treeNode);

            foreach (var node in treeNode.ChildNodes)
            {
                if (node.AstNode == null)
                {
                    RawType = node.Token.Terminal.Name;
                }
                else
                {
                    RawType = node.AstNode;
                }
            }

            AsString = RawType.ToString();
        }
    }
}
