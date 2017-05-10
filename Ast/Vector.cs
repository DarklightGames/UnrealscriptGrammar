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
    public struct Vector
    {
        public float X;
        public float Y;
        public float Z;

        public override string ToString()
        {
            return "vect(" + string.Join(",", X, Y, Z) + ")";
        }
    }

    public class VectorNode : LiteralValueNode
    {
        public override void Init(AstContext context, ParseTreeNode treeNode)
        {
            var vector = new Vector();
            vector.X = Convert.ToSingle((treeNode.ChildNodes[1].AstNode as LiteralValueNode).Value);
            vector.Y = Convert.ToSingle((treeNode.ChildNodes[2].AstNode as LiteralValueNode).Value);
            vector.Z = Convert.ToSingle((treeNode.ChildNodes[3].AstNode as LiteralValueNode).Value);

            Value = vector;
            AsString = Value.ToString();

            base.Init(context, treeNode);
        }
    }
}
