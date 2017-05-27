using Irony.Ast;
using Irony.Interpreter.Ast;
using Irony.Parsing;

namespace Unrealscript.Ast
{
    public class DefaultPropertiesAssignment : AstNode
    {
        public DefaultPropertiesKey Target;
        public AstNode Value;

        public override void Init(AstContext context, ParseTreeNode treeNode)
        {
            base.Init(context, treeNode);

            foreach (var node in treeNode.ChildNodes)
            {
                if (node.AstNode is DefaultPropertiesKey)
                {
                    Target = (node.AstNode as DefaultPropertiesKey);
                }
                // TODO: handle value?
            }
        }
    }
}