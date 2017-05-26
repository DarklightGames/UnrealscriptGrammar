using Irony.Interpreter;
using Irony.Parsing;
using Unrealscript.Ast;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

            var ExtendsOpt = new NonTerminal("ExtendsOpt", typeof(AuxiliaryNode));
            ExtendsOpt.Rule = Empty | ToTerm("extends") + Identifier;

            // Key terms
            var ClassKeyTerm = ToTerm("class");
            var ExtendsKeyTerm = ToTerm("extends");
            var AbstractKeyTerm = ToTerm("abstract");
            var ConfigKeyTerm = ToTerm("config");
            var NativeKeyTerm = ToTerm("native");
            var NativeReplicationKeyTerm = ToTerm("nativereplication");
            var NoNativeReplicationKeyTerm = ToTerm("nonativereplication");
            var SafeReplaceKeyTerm = ToTerm("safereplace");
            var WithinKeyTerm = ToTerm("within");
            var PerObjectConfigKeyTerm = ToTerm("perobjectconfig");
            var TransientKeyTerm = ToTerm("transient");
            var NoExportKeyTerm = ToTerm("noexport");
            var DependsOnKeyTerm = ToTerm("dependson");
            var ExportStructsKeyTerm = ToTerm("exportstructs");
            var CacheExemptKeyTerm = ToTerm("cacheexempt");
            var HideDropDownKeyTerm = ToTerm("hidedropdown");
            var ParseConfigKeyTerm = ToTerm("parseconfig");
            var CollapseCategoriesKeyTerm = ToTerm("collapsecategories");
            var DontCollapseCategoriesKeyTerm = ToTerm("dontcollapsecategories");
            var HideCategoriesKeyTerm = ToTerm("hidecategories");
            var ShowCategoriesKeyTerm = ToTerm("showcategories");
            var PlaceableKeyTerm = ToTerm("placeable");
            var NotPlaceableKeyTerm = ToTerm("notplaceable");
            var EditInlineNewKeyTerm = ToTerm("editinlinenew");
            var NotEditInlineNewKeyTerm = ToTerm("noteditinlinenew");
            var InstancedKeyTerm = ToTerm("instanced");
            
            // Variables
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

            var SingleLineComment = new CommentTerminal("SingleLineComment", "//", "\n");
            var MultiLineComment = new CommentTerminal("MultiLineComment", "/*", "*/");
            NonGrammarTerminals.Add(SingleLineComment);
            NonGrammarTerminals.Add(MultiLineComment);

            var Identifiers = new NonTerminal("Identifiers", typeof(AuxiliaryNode));
            Identifiers.Rule = MakePlusRule(Identifiers, ToTerm(","), Identifier);

            var ConstTerm = ToTerm("const");
            var Const = new NonTerminal("Const", typeof(Const));
            Const.Rule = ConstTerm + Identifier + "=" + Literal + ";";

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
            var TypeOpt = new NonTerminal("Type?", Empty | Type);
            #endregion

            // Struct
            var StructTerm = ToTerm("struct");
            var Struct = new NonTerminal("Struct", typeof(Struct));
            Struct.Rule = StructTerm + Identifier + "{" + "}";

            // Enum
            var EnumTerm = ToTerm("enum");
            var Enum = new NonTerminal("Enum", typeof(Ast.Enum));
            Enum.Rule = EnumTerm + Identifier + "{" + Identifiers + "}";

            #region Variables
            // Variables
            var ArraySize = new NonTerminal("ArraySize", typeof(AuxiliaryNode));
            ArraySize.Rule = "[" + Integer + "]" |
                             "[" + Identifier + "]";

            var ArraySizeOpt = new NonTerminal("ArraySize?", typeof(AuxiliaryNode));
            ArraySizeOpt.Rule = Empty | ArraySize;

            var VarEditOpt = new NonTerminal("VarEdit", typeof(AuxiliaryNode));
            VarEditOpt.Rule = Empty |
                            "(" + Identifier + ")" |
                            "(" + ")";

            var VarType = new NonTerminal("VarType");
            VarType.Rule = Type | Struct | Enum;

            var VarName = new NonTerminal("VarName");
            VarName.Rule = Identifier + ArraySizeOpt;

            var VarNames = new NonTerminal("VarName+");
            VarNames.Rule = MakePlusRule(VarNames, ToTerm(","), VarName);

            var Var = new NonTerminal("Var", typeof(Variable));
            Var.Rule = ToTerm("var") + VarEditOpt + VarModifiers + VarType + VarNames + ";";

            var Vars = new NonTerminal("Var+", typeof(AuxiliaryNode));
            Vars.Rule = MakeStarRule(Vars, Var);
            #endregion

            var VarStructEnumConst = new NonTerminal("VarStructEnumConst");
            VarStructEnumConst.Rule = Var | Struct | Enum | Const;

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
            Arguments.Rule = MakeStarRule(Arguments, ToTerm(","), Expression);

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

            var ExpressionOpt = new NonTerminal("ExpressionOpt");
            ExpressionOpt.Rule = Expression | Empty;

            var Return = new NonTerminal("Return");
            Return.Rule = ToTerm("return") + ExpressionOpt;

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
            For.Rule = ToTerm("for") + "(" + SimpleStatements + ";" + ExpressionOpt + ";" + SimpleStatement + ")" + Statement |
                       ToTerm("for") + "(" + SimpleStatements + ";" + ExpressionOpt + ";" + SimpleStatement + ")" + "{" + Statements + "}";

            var ForEach = new NonTerminal("ForEeach");
            ForEach.Rule = ToTerm("foreach") + Primary + Statement |
                           ToTerm("foreach") + Primary + "{" + Statements + "}";

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
                        ToTerm("else") + "{" + Statements + "}" |
                        Empty;

            var If = new NonTerminal("If");
            If.Rule = ToTerm("if") + "(" + Expression + ")" + Statement + ElseIfs + Else |
                      ToTerm("if") + "(" + Expression + ")" + "{" + Statements + "}" + ElseIfs + Else;

            var DefaultCaseLabel = new RegexBasedTerminal("default\\s*:");

            var SwitchCase = new NonTerminal("SwitchCase");
            SwitchCase.Rule = ToTerm("case") + Atom + ":" + Statements |
                              DefaultCaseLabel + Statements;

            var SwitchCases = new NonTerminal("SwitchCase*");
            SwitchCases.Rule = MakeStarRule(SwitchCases, SwitchCase);

            var Switch = new NonTerminal("Switch");
            Switch.Rule = ToTerm("switch") + "(" + Expression + ")" + "{" + SwitchCases + "}";

            CompoundStatement.Rule = For | ForEach | While | If | Switch/* | Do*/;

            #region Functions
            var Local = new NonTerminal("Local", typeof(Local));
            Local.Rule = ToTerm("local") + Type + VarNames + ";";

            var Locals = new NonTerminal("Local*", typeof(AuxiliaryNode));
            Locals.Rule = MakeStarRule(Locals, Local);

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
            FunctionArgument.Rule = FunctionArgumentModifers + Type + Identifier;

            var FunctionArguments = new NonTerminal("FunctionArgument*", typeof(AuxiliaryNode));
            FunctionArguments.Rule = MakeStarRule(FunctionArguments, ToTerm(","), FunctionArgument);

            var FunctionDefinition = new NonTerminal("FunctionDefinition", typeof(Function));
            FunctionDefinition.Rule = FunctionModifiers + FunctionType + FunctionModifiers + Type + Identifier + "(" + FunctionArguments + ")" |
                                      FunctionModifiers + FunctionType + FunctionModifiers + Identifier + "(" + FunctionArguments + ")";

            var FunctionDeclaration = new NonTerminal("FunctionDeclaration", typeof(FunctionDeclaration));
            FunctionDeclaration.Rule = FunctionDefinition + ";" |
                                       FunctionDefinition + "{" + Locals + Statements + "}";
            #endregion

            // END MEAT

            // funcstateconst
            var FunctionStateConst = new NonTerminal("FunctionStateConst");
            FunctionStateConst.Rule = FunctionDeclaration | Const; // TODO: add state

            var FuncStateConstDeclarations = new NonTerminal("FuncStateConstDeclarations", typeof(AuxiliaryNode));
            FuncStateConstDeclarations.Rule = MakeStarRule(Declarations, FunctionStateConst);

            var ClassModifier = new NonTerminal("ClassModifier", typeof(ClassDeclaration.Modifier));
            ClassModifier.Rule = AbstractKeyTerm |
                CacheExemptKeyTerm |
                ConfigKeyTerm + "(" + Identifier + ")" |
                DependsOnKeyTerm + "(" + Identifier + ")" |
                InstancedKeyTerm |
                ParseConfigKeyTerm |
                PerObjectConfigKeyTerm |
                SafeReplaceKeyTerm |
                TransientKeyTerm |
                CollapseCategoriesKeyTerm + "(" + Identifiers + ")" |
                DontCollapseCategoriesKeyTerm + "(" + Identifiers + ")" |
                EditInlineNewKeyTerm |
                NotEditInlineNewKeyTerm |
                HideCategoriesKeyTerm + "(" + Identifiers + ")" |
                ShowCategoriesKeyTerm + "(" + Identifiers + ")" |
                HideDropDownKeyTerm |
                PlaceableKeyTerm |
                NotPlaceableKeyTerm |
                ExportStructsKeyTerm |
                NativeKeyTerm |
                NativeReplicationKeyTerm |
                NoExportKeyTerm;

            #region DefaultProperties

            var DefaultPropertiesKey = new NonTerminal("DefaultPropertiesKey");
            var DefaultPropertiesAssignmentValue = new NonTerminal("DefaultPropertiesAssignmentValue");
            var DefaultPropertiesAssignment = new NonTerminal("DefaultPropertiesAssignment");
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
            ClassDeclaration.Rule = ClassKeyTerm + Identifier + ExtendsOpt + ClassModifiers + ";";

            var Program = new NonTerminal("Program", typeof(Program));
            Program.Rule = Declarations + ClassDeclaration + PreFunctionDeclarations + FuncStateConstDeclarations + DefaultProperties;

            Root = Program;

            RegisterBracePair("(", ")");
            RegisterBracePair("[", "]");
            //RegisterBracePair("<", ">");  // TODO: complains about matching brace

            MarkPunctuation(":", ";", ",", "(", ")", "<", ">", "=", "[", "]", "{", "}");
            MarkTransient(Literal, PreClassDeclaration, VarType, VarStructEnumConst, FunctionModifier, FunctionArgumentModifer, Primary, Atom);

            LanguageFlags = LanguageFlags.CreateAst;
        }
    }
}
