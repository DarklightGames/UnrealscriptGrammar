using Irony.Interpreter;
using Irony.Parsing;
using Unrealscript.Ast;
using System;

namespace Unrealscript
{
    public class UnrealscriptGrammar : InterpretedLanguageGrammar
    {
        public UnrealscriptGrammar() : base(false)
        {
            var Identifier = new IdentifierTerminal("Identifier");

            #region Literals
            // Number
            var Number = new NumberLiteral("Number")
            {
                DefaultIntTypes = new[] { TypeCode.Int32 },
                DefaultFloatType = TypeCode.Single
            };
            Number.AddPrefix("0x", NumberOptions.Hex);
            // Integer
            var Integer = new NumberLiteral("Integer", NumberOptions.IntOnly);
            // String
            var String = new StringLiteral("String", "\"", StringOptions.None);
            // Name
            var Name = new StringLiteral("Name", "'");
            // Vector
            var VectKeyTerm = ToTerm("vect");
            var Vector = new NonTerminal("Vector", typeof(VectorNode));
            Vector.Rule = VectKeyTerm + "(" + Number + "," + Number + "," + Number + ")";

            var Literal = new NonTerminal("Literal");
            Literal.Rule = Number | String | Name | "true" | "false" | "none" | Vector;
            #endregion
            
            var VarEdit = new NonTerminal("VarEdit", typeof(AuxiliaryNode));
            VarEdit.Rule = "(" + Identifier + ")" |
                           "(" + ")";
            var VarEditOpt = new NonTerminal("VarEdit?", typeof(AuxiliaryNode));
            VarEditOpt.Rule = Empty | VarEdit;

            var Extends = new NonTerminal("Extends", typeof(AuxiliaryNode));
            Extends.Rule = ToTerm("extends") + Identifier;

            #region Variables
            var VarModifier = new NonTerminal("VarModifier", typeof(Modifier));
            VarModifier.Rule = ToTerm("config") |
                                ToTerm("const") |
                                ToTerm("editconst") |
                                ToTerm("editconstarray") |
                                ToTerm("editinline") |
                                ToTerm("export") |
                                ToTerm("noexport") |
                                ToTerm("globalconfig") |
                                ToTerm("input") |
                                ToTerm("localized") |
                                ToTerm("native") |
                                ToTerm("private") |
                                ToTerm("protected") |
                                ToTerm("transient") |
                                ToTerm("travel");

            var VarModifiers = new NonTerminal("VarModifier*", typeof(AuxiliaryNode));
            VarModifiers.Rule = MakeStarRule(VarModifiers, VarModifier);
            #endregion

            #region Comments
            var SingleLineComment = new CommentTerminal("SingleLineComment", "//", "\n");
            var MultiLineComment = new CommentTerminal("MultiLineComment", "/*", "*/");
            NonGrammarTerminals.Add(SingleLineComment);
            NonGrammarTerminals.Add(MultiLineComment);
            #endregion

            var Identifiers = new NonTerminal("Identifiers", typeof(AuxiliaryNode));
            Identifiers.Rule = MakePlusRule(Identifiers, ToTerm(","), Identifier);

            var Const = new NonTerminal("Const", typeof(Const));
            Const.Rule = ToTerm("const") + Identifier + "=" + Literal + ";";

            var PreClassDeclaration = new NonTerminal("PreClassDeclaration");
            PreClassDeclaration.Rule = Const;

            var Declarations = new NonTerminal("PreClassDeclaration*", typeof(AuxiliaryNode));
            Declarations.Rule = MakeStarRule(Declarations, PreClassDeclaration);

            #region Types
            var ByteTerm = ToTerm("byte");
            var IntTerm = ToTerm("int");
            var BoolTerm = ToTerm("bool");
            var FloatTerm = ToTerm("float");
            var StringTerm = ToTerm("string");
            var NameTerm = ToTerm("name");
            var Type = new NonTerminal("Type", typeof(Ast.Type));

            var Array = new NonTerminal("Array", typeof(Ast.Array));
            Array.Rule = ToTerm("array") + "<" + Type + ">";

            var Class = new NonTerminal("Class", typeof(Ast.Class));
            Class.Rule = ToTerm("class") + "<" + Identifier + ">";

            Type.Rule = ByteTerm | IntTerm | BoolTerm | FloatTerm | StringTerm | NameTerm | Array | Class | Identifier;
            #endregion

            var ArraySize = new NonTerminal("ArraySize", typeof(AuxiliaryNode));
            ArraySize.Rule = Empty |
                             "[" + Integer + "]" |
                             "[" + Identifier + "]";


            #region Struct
            var StructVarEdit = new NonTerminal("StructVarEdit");
            StructVarEdit.Rule = Empty | "(" + ")";

            var StructVar = new NonTerminal("StructVar", typeof(Variable));
            StructVar.Rule = ToTerm("var") + StructVarEdit + Identifier + ArraySize + ";";

            var StructVars = new NonTerminal("StructVar+");
            StructVars.Rule = MakePlusRule(StructVars, StructVar);

            var Struct = new NonTerminal("Struct", typeof(Struct));
            Struct.Rule = ToTerm("struct") + Identifier + "{" + StructVars + "}";

            var StructStatement = new NonTerminal("StructStatement", typeof(AuxiliaryNode));
            StructStatement.Rule = Struct + ";";
            #endregion

            // Enum
            var EnumTerm = ToTerm("enum");
            var Enum = new NonTerminal("Enum", typeof(Ast.Enum));
            Enum.Rule = EnumTerm + Identifier + "{" + Identifiers + "}";

            var EnumStatement = new NonTerminal("EnumStatement", typeof(AuxiliaryNode));
            EnumStatement.Rule = Enum + ";";

            #region Variables
            // Variables
            var VarType = new NonTerminal("VarType");
            var VarName = new NonTerminal("VarName", typeof(VariableName));
            var VarNames = new NonTerminal("VarName+", typeof(AuxiliaryNode));
            var Var = new NonTerminal("Var", typeof(Variable));
            var Vars = new NonTerminal("Var+", typeof(AuxiliaryNode));

            VarType.Rule = Type | Struct | Enum;
            VarName.Rule = Identifier + ArraySize;
            VarNames.Rule = MakePlusRule(VarNames, ToTerm(","), VarName);
            Var.Rule = ToTerm("var") + VarEditOpt + VarModifiers + VarType + VarNames + ";";
            Vars.Rule = MakeStarRule(Vars, Var);
            #endregion

            var VarStructEnumConst = new NonTerminal("VarStructEnumConst");
            VarStructEnumConst.Rule = Var | StructStatement | EnumStatement | Const;

            var PreFunctionDeclarations = new NonTerminal("VarStructEnumConst*", typeof(AuxiliaryNode));
            PreFunctionDeclarations.Rule = MakeStarRule(PreFunctionDeclarations, VarStructEnumConst);

            // MEAT OF THE LOGIC!
            var Reference = new NonTerminal("Reference");   // TODO: probably won't work, my guess
            Reference.Rule = Identifier + Name |
                             ToTerm("class") + Name;

            var Allocation = new NonTerminal("Allocation");
            Allocation.Rule = ToTerm("new") + Reference;

            var Atom = new NonTerminal("Atom");
            Atom.Rule = Identifier | Literal | Reference | Class | Allocation;

            var Primary = new NonTerminal("Primary");

            var ExpressionParen = new NonTerminal("ExpressionParen");
            var Expression = new NonTerminal("Expression");
            Expression.Rule = Primary | ExpressionParen;
            ExpressionParen.Rule = "(" + Expression + ")";

            var Default = new NonTerminal("Default");
            Default.Rule = ToTerm("default") + "." + Identifier;

            var Subscription = new NonTerminal("Subscription");
            Subscription.Rule = Primary + "[" + Expression + "]";

            var Arguments = new NonTerminal("Argument*");
            Arguments.Rule = MakeStarRule(Arguments, ToTerm(","), Expression.Q());

            var Call = new NonTerminal("Call");
            Call.Rule = Identifier + "(" + Arguments + ")" |
                        Class + "(" + Arguments + ")";  // TODO: what's this one for?

            var SuperCall = new NonTerminal("SuperCall");
            SuperCall.Rule = ToTerm("super") + "." + Call |
                             ToTerm("super") + "(" + Identifier + ")" + "." + Call;

            var StaticCall = new NonTerminal("StaticCall");
            StaticCall.Rule = Primary + "." + ToTerm("static") + "." + Call |
                              ToTerm("static") + "." + Call;

            var GlobalCall = new NonTerminal("GlobalCall");
            GlobalCall.Rule = ToTerm("global") + "." + Call;

            var PreUnaryOperator = new NonTerminal("PreUnaryOperator", ToTerm("-") | ToTerm("!") | ToTerm("++") | ToTerm("--") | ToTerm("^"));
            var PostUnaryOperator = new NonTerminal("PostUnaryOperator", ToTerm("++") | ToTerm("--"));

            var UnaryOp = new NonTerminal("UnaryOp");
            UnaryOp.Rule = PreUnaryOperator + Expression |
                           Expression + PostUnaryOperator;

            var BinaryOperator = new NonTerminal("BinaryOperator");
            BinaryOperator.Rule = ToTerm("==") |
                                  ToTerm("!=") |
                                  ToTerm("<=") |
                                  ToTerm(">=") |
                                  ToTerm("~=") |
                                  ToTerm("%") |
                                  ToTerm("*") |
                                  ToTerm("/") |
                                  ToTerm("+") |
                                  ToTerm("-") |
                                  ToTerm("<") |
                                  ToTerm(">") |
                                  ToTerm("$") |
                                  ToTerm("@") |
                                  ToTerm("||") |
                                  ToTerm("+=") |
                                  ToTerm("-=") |
                                  ToTerm("*=") |
                                  ToTerm("/=") |
                                  ToTerm("dot") |
                                  ToTerm("&&") |
                                  ToTerm(">>") |
                                  ToTerm("<<") |
                                  ToTerm("<<<") |
                                  ToTerm(">>>") |
                                  ToTerm("**") |
                                  ToTerm("cross") |
                                  ToTerm("&") |
                                  ToTerm("|") |
                                  ToTerm("^") |
                                  ToTerm("^^");

            var AssignmentOperator = new NonTerminal("AssignmentOperator");
            AssignmentOperator.Rule = ToTerm("=") | ToTerm("$=") | ToTerm("@=");

            var Target = new NonTerminal("Target");
            var Attribute = new NonTerminal("Attribute");
            Target.Rule = Identifier | Attribute | Default | Subscription;

            var Assignment = new NonTerminal("Assignment");
            Assignment.Rule = Target + AssignmentOperator + Expression;

            var BinaryOp = new NonTerminal("BinaryOp", Expression + BinaryOperator + Expression);

            Primary.Rule = Atom | Attribute | Default | Subscription | SuperCall | StaticCall | GlobalCall | Call | UnaryOp | BinaryOp | ExpressionParen;

            Attribute.Rule = Primary + "." + Primary;

            var Return = new NonTerminal("Return");
            Return.Rule = ToTerm("return") + Expression.Q();

            var SimpleStatement = new NonTerminal("SimpleStatement");
            SimpleStatement.Rule = Return | ToTerm("break") | ToTerm("continue") | Assignment /*| Goto*/ | Expression;

            var SimpleStatements = new NonTerminal("SimpleStatements");
            SimpleStatements.Rule = MakeStarRule(SimpleStatements, ToTerm(","), SimpleStatement);

            var Codeline = new NonTerminal("Codeline");
            Codeline.Rule = SimpleStatement + ";";

            var Statement = new NonTerminal("Statement");
            var CompoundStatement = new NonTerminal("CompoundStatement");
            Statement.Rule = Codeline | Const | CompoundStatement/* | Label*/;

            var Statements = new NonTerminal("Statements");
            Statements.Rule = MakeStarRule(Statements, Statement);

            var For = new NonTerminal("For");
            For.Rule = ToTerm("for") + "(" + SimpleStatements + ";" + Expression.Q() + ";" + SimpleStatement + ")" + "{" + Statements + "}" |
                       ToTerm("for") + "(" + SimpleStatements + ";" + Expression.Q() + ";" + SimpleStatement + ")" + Statement;

            var ForEach = new NonTerminal("ForEeach");
            ForEach.Rule = ToTerm("foreach") + Primary + Statement |
                           ToTerm("foreach") + Primary + "{" + Statements + "}";

            var Until = new NonTerminal("Until");
            Until.Rule = ToTerm("until") + "(" + Expression + ")" + ToTerm(";").Q();

            var Do = new NonTerminal("Do");
            Do.Rule = ToTerm("do") + "{" + Statements + "}" + Until.Q();

            var While = new NonTerminal("While");
            While.Rule = ToTerm("while") + "(" + Expression + ")" + Statement |
                         ToTerm("while") + "(" + Expression + ")" + "{" + Statements + "}";

            var ElseIf = new NonTerminal("ElseIf");
            ElseIf.Rule = ToTerm("else") + ToTerm("if") + "(" + Expression + ")" + Statement |
                          ToTerm("else") + ToTerm("if") + "(" + Expression + ")" + "{" + Statements + "}";

            var ElseIfs = new NonTerminal("ElseIf*");
            ElseIf.Rule = MakeStarRule(ElseIfs, ElseIf);

            var Else = new NonTerminal("Else");
            Else.Rule = ToTerm("else") + Statement |
                        ToTerm("else") + "{" + Statements + "}";

            var If = new NonTerminal("If");
            If.Rule = ToTerm("if") + "(" + Expression + ")" + Statement + ElseIfs + Else.Q() |
                      ToTerm("if") + "(" + Expression + ")" + "{" + Statements + "}" + ElseIfs + Else.Q();

            var DefaultCaseLabel = new RegexBasedTerminal("default\\s*:");

            var SwitchCase = new NonTerminal("SwitchCase");
            SwitchCase.Rule = ToTerm("case") + Atom + ":" + Statements |
                              DefaultCaseLabel + Statements;

            var SwitchCases = new NonTerminal("SwitchCase*");
            SwitchCases.Rule = MakeStarRule(SwitchCases, SwitchCase);

            var Switch = new NonTerminal("Switch");
            Switch.Rule = ToTerm("switch") + "(" + Expression + ")" + "{" + SwitchCases + "}";

            CompoundStatement.Rule = For | ForEach | While | If | Switch | Do;

            #region Functions
            var Local = new NonTerminal("Local", typeof(Local));
            Local.Rule = ToTerm("local") + Type + VarNames + ";";

            var Locals = new NonTerminal("Local*", typeof(AuxiliaryNode));
            Locals.Rule = MakeStarRule(Locals, Local);

            var Ignores = new NonTerminal("Ignores", typeof(AuxiliaryNode));
            Ignores.Rule = ToTerm("ignores") + Identifiers;

            var FunctionModifier = new NonTerminal("FunctionModifier");
            FunctionModifier.Rule = ToTerm("exec") |
                                    ToTerm("final") |
                                    ToTerm("iterator") |
                                    ToTerm("latent") |
                                    ToTerm("native") |
                                    ToTerm("simulated") |
                                    ToTerm("singular") |
                                    ToTerm("static") |
                                    ToTerm("private") |
                                    ToTerm("protected");
            var FunctionModifiers = new NonTerminal("FunctionModifier*", typeof(AuxiliaryNode));
            FunctionModifiers.Rule = MakeStarRule(FunctionModifiers, FunctionModifier);

            var FunctionType = new NonTerminal("FunctionType");
            FunctionType.Rule = ToTerm("function") | ToTerm("event");

            var FunctionArgumentModifer = new NonTerminal("FunctionArgumentModifier");
            FunctionArgumentModifer.Rule = ToTerm("coerce") | ToTerm("optional") | ToTerm("out");

            var FunctionArgumentModifers = new NonTerminal("FunctionArgumentModifer*", typeof(AuxiliaryNode));
            FunctionArgumentModifers.Rule = MakeStarRule(FunctionArgumentModifers, FunctionArgumentModifer);

            var FunctionArgument = new NonTerminal("FunctionArgument", typeof(FunctionArgument));
            FunctionArgument.Rule = FunctionArgumentModifers + Type + Identifier + ArraySize;

            var FunctionArguments = new NonTerminal("FunctionArgument*", typeof(AuxiliaryNode));
            FunctionArguments.Rule = MakeStarRule(FunctionArguments, ToTerm(","), FunctionArgument);

            var FunctionDefinition = new NonTerminal("FunctionDefinition", typeof(Function));
            FunctionDefinition.Rule = FunctionModifiers + FunctionType + FunctionModifiers + Type + Identifier + "(" + FunctionArguments + ")" |
                                      FunctionModifiers + FunctionType + FunctionModifiers + Identifier + "(" + FunctionArguments + ")";

            var FunctionDeclaration = new NonTerminal("FunctionDeclaration", typeof(FunctionDeclaration));
            FunctionDeclaration.Rule = FunctionDefinition + ";" |
                                       FunctionDefinition + "{" + Locals + Statements + "}";
            #endregion

            #region States
            var StateDeclaration = new NonTerminal("StateDeclaration", typeof(AuxiliaryNode));
            StateDeclaration.Rule = Const | FunctionDeclaration;

            var StateDeclarations = new NonTerminal("StateDeclaration*", typeof(AuxiliaryNode));
            StateDeclarations.Rule = MakeStarRule(StateDeclarations, StateDeclaration);

            var State = new NonTerminal("State", typeof(AuxiliaryNode));
            State.Rule = ToTerm("state") + Identifier + "{" + Ignores.Q() + StateDeclarations + "}";
            #endregion

            // funcstateconst
            var FunctionStateConst = new NonTerminal("FunctionStateConst");
            FunctionStateConst.Rule = FunctionDeclaration | Const | State; // TODO: add state

            var FuncStateConstDeclarations = new NonTerminal("FuncStateConstDeclarations", typeof(AuxiliaryNode));
            FuncStateConstDeclarations.Rule = MakeStarRule(Declarations, FunctionStateConst);

            var ClassModifier = new NonTerminal("ClassModifier", typeof(ClassDeclaration.Modifier));
            ClassModifier.Rule = ToTerm("abstract") |
                ToTerm("cacheexempt") |
                ToTerm("config") + "(" + Identifier + ")" |
                ToTerm("dependson") + "(" + Identifier + ")" |
                ToTerm("instanced") |
                ToTerm("parseconfig") |
                ToTerm("perobjectconfig") |
                ToTerm("safereplace") |
                ToTerm("transient") |
                ToTerm("collapsecategories") + "(" + Identifiers + ")" |
                ToTerm("dontcollapsecategories") + "(" + Identifiers + ")" |
                ToTerm("editinline") |
                ToTerm("noteditinline") |
                ToTerm("hidecategories") + "(" + Identifiers + ")" |
                ToTerm("showcategories") + "(" + Identifiers + ")" |
                ToTerm("hidedropdown") |
                ToTerm("placeable") |
                ToTerm("notplaceable") |
                ToTerm("exportstructs") |
                ToTerm("native") |
                ToTerm("nativereplication") |
                ToTerm("noexport");

            #region DefaultProperties
            var DefaultPropertiesKey = new NonTerminal("DefaultPropertiesKey", typeof(DefaultPropertiesKey));
            var DefaultPropertiesAssignmentValue = new NonTerminal("DefaultPropertiesAssignmentValue");
            var DefaultPropertiesAssignment = new NonTerminal("DefaultPropertiesAssignment", typeof(DefaultPropertiesAssignment));
            var DefaultPropertiesAssignments = new NonTerminal("DefaultPropertiesAssignments", typeof(AuxiliaryNode));
            var DefaultPropertiesObjectArguments = new NonTerminal("DefaultPropertiesObjectArguments", typeof(AuxiliaryNode));
            var DefaultPropertiesArrayArgument = new NonTerminal("DefaultPropertiesArrayArgument");
            var DefaultPropertiesArrayArguments = new NonTerminal("DefaultPropertiesArrayArguments", typeof(AuxiliaryNode));
            var DefaultPropertiesArray = new NonTerminal("DefaultPropertiesArray");
            var DefaultPropertiesDeclaration = new NonTerminal("DefaultPropertiesDeclaration");
            var DefaultPropertiesDeclarations = new NonTerminal("DefaultPropertiesDeclarations", typeof(AuxiliaryNode));
            var DefaultPropertiesObject = new NonTerminal("DefaultPropertiesObject");
            var DefaultPropertiesObjectAssignment = new NonTerminal("DefaultPropertiesObjectAssignment");
            var DefaultPropertiesObjectAssignments = new NonTerminal("DefaultPropertiesObjectAssignments", typeof(AuxiliaryNode));
            var DefaultProperties = new NonTerminal("DefaultProperties", typeof(DefaultProperties));

            DefaultPropertiesKey.Rule = Identifier | Identifier + "(" + Integer + ")";
            DefaultPropertiesObjectAssignment.Rule = DefaultPropertiesKey + "=" + DefaultPropertiesAssignmentValue;
            DefaultPropertiesObjectAssignments.Rule = MakeStarRule(DefaultPropertiesObjectAssignments, DefaultPropertiesObjectAssignment);
            DefaultPropertiesObject.Rule = ToTerm("Begin") + ToTerm("Object") + DefaultPropertiesObjectAssignments + ToTerm("End") + ToTerm("Object");
            DefaultPropertiesDeclaration.Rule = DefaultPropertiesAssignment | DefaultPropertiesObject;
            DefaultPropertiesDeclarations.Rule = MakeStarRule(DefaultPropertiesDeclarations, DefaultPropertiesDeclaration);
            DefaultPropertiesAssignmentValue.Rule = Literal | Reference | Identifier | DefaultPropertiesObjectArguments | DefaultPropertiesArray;
            DefaultPropertiesAssignment.Rule = DefaultPropertiesKey + "=" + DefaultPropertiesAssignmentValue;
            DefaultPropertiesAssignments.Rule = MakeStarRule(DefaultPropertiesAssignments, ToTerm(","), DefaultPropertiesAssignment);
            DefaultPropertiesObjectArguments.Rule = "(" + DefaultPropertiesAssignments + ")";
            DefaultPropertiesArrayArgument.Rule = DefaultPropertiesObjectArguments |
                                                  DefaultPropertiesArrayArguments + "," + DefaultPropertiesObjectArguments;
            DefaultPropertiesArrayArguments.Rule = MakeStarRule(DefaultPropertiesArrayArguments, ToTerm(","), DefaultPropertiesArrayArgument);
            DefaultPropertiesArray.Rule = "(" + DefaultPropertiesArrayArguments + ")";
            DefaultProperties.Rule = Empty | ToTerm("defaultproperties") + "{" + DefaultPropertiesDeclarations + "}";
            #endregion

            var ClassModifiers = new NonTerminal("ClassModifiers", typeof(AuxiliaryNode));
            ClassModifiers.Rule = MakeStarRule(ClassModifiers, ClassModifier);

            var ClassDeclaration = new NonTerminal("ClassDeclaration", typeof(ClassDeclaration));
            ClassDeclaration.Rule = ToTerm("class") + Identifier + Extends + ClassModifiers + ";";

            var Program = new NonTerminal("Program", typeof(Program));
            Program.Rule = Declarations + ClassDeclaration + PreFunctionDeclarations + FuncStateConstDeclarations + DefaultProperties;

            Root = Program;

            RegisterBracePair("(", ")");
            RegisterBracePair("[", "]");
            //RegisterBracePair("<", ">");  // TODO: complains about matching brace

            MarkPunctuation(".", ":", ";", ",", "(", ")", "<", ">", "=", "[", "]", "{", "}");
            MarkTransient(Literal, PreClassDeclaration, VarType, VarStructEnumConst, FunctionModifier, FunctionArgumentModifer, Primary, Atom, DefaultPropertiesAssignmentValue, DefaultPropertiesDeclaration);

            LanguageFlags = LanguageFlags.CreateAst;
        }
    }
}
