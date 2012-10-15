// -*- mode: csharp; encoding: utf-8; tab-width: 4; c-basic-offset: 4; indent-tabs-mode: nil; -*-
// $Id$
/* YACQ <http://yacq.net/>
 *   Yet Another Compilable Query Language, based on Expression Trees API
 * Copyright © 2011-2012 Takeshi KIRIYA (aka takeshik) <takeshik@yacq.net>
 * All rights reserved.
 * 
 * This file is part of YACQ.
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL
 * THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
 * THE SOFTWARE.
 */

using System;
using System.Linq.Expressions;
using System.Runtime.Serialization;
using XSpect.Yacq.Expressions;

namespace XSpect.Yacq.Serialization
{
    [DataContract(IsReference = true)]
    [KnownType(typeof(BinaryNode))]
    [KnownType(typeof(NewArrayNode))]
    [KnownType(typeof(TypeBinaryNode))]
    [KnownType(typeof(UnaryNode))]
    [KnownType(typeof(YacqNode))]
    [KnownType(typeof(ArrayIndex))]
    [KnownType(typeof(Block))]
    [KnownType(typeof(Call))]
    [KnownType(typeof(Condition))]
    [KnownType(typeof(Constant))]
    [KnownType(typeof(DebugInfo))]
    [KnownType(typeof(Default))]
    [KnownType(typeof(Goto))]
    [KnownType(typeof(Index))]
    [KnownType(typeof(Invoke))]
    [KnownType(typeof(Label))]
    [KnownType(typeof(Lambda))]
    [KnownType(typeof(ListInit))]
    [KnownType(typeof(Loop))]
    [KnownType(typeof(MemberAccess))]
    [KnownType(typeof(MemberInit))]
    [KnownType(typeof(New))]
    [KnownType(typeof(Parameter))]
    [KnownType(typeof(RuntimeVariables))]
    [KnownType(typeof(Switch))]
    [KnownType(typeof(Try))]
    [KnownType(typeof(AssemblyRef))]
    [KnownType(typeof(TypeRef))]
    [KnownType(typeof(MemberRef))]
    [KnownType(typeof(ElementInit))]
    [KnownType(typeof(LabelTarget))]
    [KnownType(typeof(MemberBinding))]
    [KnownType(typeof(SymbolDocumentInfo))]
    internal abstract partial class Node
    {
        public const String Namespace = "http://yacq.net/schema";

        [DataMember(Order = 0, EmitDefaultValue = false)]
        public TypeRef Type
        {
            get;
            set;
        }

        internal static Node Serialize(Expression expression)
        {
            switch (expression.NodeType)
            {
                case ExpressionType.Add:
                    return Add((BinaryExpression) expression);
                case ExpressionType.AddChecked:
                    return AddChecked((BinaryExpression) expression);
                case ExpressionType.And:
                    return And((BinaryExpression) expression);
                case ExpressionType.AndAlso:
                    return AndAlso((BinaryExpression) expression);
                case ExpressionType.ArrayLength:
                    return ArrayLength((UnaryExpression) expression);
                case ExpressionType.ArrayIndex:
                    return ArrayIndex((BinaryExpression) expression);
                case ExpressionType.Call:
                    return Call((MethodCallExpression) expression);
                case ExpressionType.Coalesce:
                    return Coalesce((BinaryExpression) expression);
                case ExpressionType.Conditional:
                    return Condition((ConditionalExpression) expression);
                case ExpressionType.Constant:
                    return Constant((ConstantExpression) expression);
                case ExpressionType.Convert:
                    return Convert((UnaryExpression) expression);
                case ExpressionType.ConvertChecked:
                    return ConvertChecked((UnaryExpression) expression);
                case ExpressionType.Divide:
                    return Divide((BinaryExpression) expression);
                case ExpressionType.Equal:
                    return Equal((BinaryExpression) expression);
                case ExpressionType.ExclusiveOr:
                    return ExclusiveOr((BinaryExpression) expression);
                case ExpressionType.GreaterThan:
                    return GreaterThan((BinaryExpression) expression);
                case ExpressionType.GreaterThanOrEqual:
                    return GreaterThanOrEqual((BinaryExpression) expression);
                case ExpressionType.Invoke:
                    return Invoke((InvocationExpression) expression);
                case ExpressionType.Lambda:
                    return Lambda((LambdaExpression) expression);
                case ExpressionType.LeftShift:
                    return LeftShift((BinaryExpression) expression);
                case ExpressionType.LessThan:
                    return LessThan((BinaryExpression) expression);
                case ExpressionType.LessThanOrEqual:
                    return LessThanOrEqual((BinaryExpression) expression);
                case ExpressionType.ListInit:
                    return ListInit((ListInitExpression) expression);
                case ExpressionType.MemberAccess:
                    return MemberAccess((MemberExpression) expression);
                case ExpressionType.MemberInit:
                    return MemberInit((MemberInitExpression) expression);
                case ExpressionType.Modulo:
                    return Modulo((BinaryExpression) expression);
                case ExpressionType.Multiply:
                    return Multiply((BinaryExpression) expression);
                case ExpressionType.MultiplyChecked:
                    return MultiplyChecked((BinaryExpression) expression);
                case ExpressionType.Negate:
                    return Negate((UnaryExpression) expression);
                case ExpressionType.UnaryPlus:
                    return UnaryPlus((UnaryExpression) expression);
                case ExpressionType.NegateChecked:
                    return NegateChecked((UnaryExpression) expression);
                case ExpressionType.New:
                    return New((NewExpression) expression);
                case ExpressionType.NewArrayInit:
                    return NewArrayInit((NewArrayExpression) expression);
                case ExpressionType.NewArrayBounds:
                    return NewArrayBounds((NewArrayExpression) expression);
                case ExpressionType.Not:
                    return Not((UnaryExpression) expression);
                case ExpressionType.NotEqual:
                    return NotEqual((BinaryExpression) expression);
                case ExpressionType.Or:
                    return Or((BinaryExpression) expression);
                case ExpressionType.OrElse:
                    return OrElse((BinaryExpression) expression);
                case ExpressionType.Parameter:
                    return Parameter((ParameterExpression) expression);
                case ExpressionType.Power:
                    return Power((BinaryExpression) expression);
                case ExpressionType.Quote:
                    return Quote((UnaryExpression) expression);
                case ExpressionType.RightShift:
                    return RightShift((BinaryExpression) expression);
                case ExpressionType.Subtract:
                    return Subtract((BinaryExpression) expression);
                case ExpressionType.SubtractChecked:
                    return SubtractChecked((BinaryExpression) expression);
                case ExpressionType.TypeAs:
                    return TypeAs((UnaryExpression) expression);
                case ExpressionType.TypeIs:
                    return TypeIs((TypeBinaryExpression) expression);
                case ExpressionType.Assign:
                    return Assign((BinaryExpression) expression);
                case ExpressionType.Block:
                    return Block((BlockExpression) expression);
                case ExpressionType.DebugInfo:
                    return DebugInfo((DebugInfoExpression) expression);
                case ExpressionType.Decrement:
                    return Decrement((UnaryExpression) expression);
                case ExpressionType.Dynamic:
                    break;
                case ExpressionType.Default:
                    return Default((DefaultExpression) expression);
                case ExpressionType.Extension:
                    if (expression is YacqExpression)
                    {
                        switch (expression.GetType().Name)
                        {
                            case "AmbiguousLambdaExpression":
                                return AmbiguousLambda((AmbiguousLambdaExpression) expression);
                            case "AmbiguousParameterExpression":
                                return AmbiguousParameter((AmbiguousParameterExpression) expression);
                            case "ContextfulExpression":
                                return Contextful((ContextfulExpression) expression);
                            case "DispatchExpression":
                                return Dispatch((DispatchExpression) expression);
                            case "IdentifierExpression":
                                return Identifier((IdentifierExpression) expression);
                            case "IgnoredExpression":
                                return Ignored((IgnoredExpression) expression);
                            case "LambdaListExpression":
                                return LambdaList((LambdaListExpression) expression);
                            case "ListExpression":
                                return List((ListExpression) expression);
                            case "MacroExpression":
                                return Macro((MacroExpression) expression);
                            case "NumberExpression":
                                return Number((NumberExpression) expression);
                            case "QuotedExpression":
                                return Quoted((QuotedExpression) expression);
                            case "SerializedExpression":
                                return Serialized((SerializedExpression) expression);
                            case "SymbolTableExpression":
                                break;
                            case "TextExpression":
                                return Text((TextExpression) expression);
                            case "TypeCandidateExpression":
                                return TypeCandidate((TypeCandidateExpression) expression);
                            case "VectorExpression":
                                return Vector((VectorExpression) expression);
                        }
                    }
                    break;
                case ExpressionType.Goto:
                    return Goto((GotoExpression) expression);
                case ExpressionType.Increment:
                    return Increment((UnaryExpression) expression);
                case ExpressionType.Index:
                    return Index((IndexExpression) expression);
                case ExpressionType.Label:
                    return Label((LabelExpression) expression);
                case ExpressionType.RuntimeVariables:
                    return RuntimeVariables((RuntimeVariablesExpression) expression);
                case ExpressionType.Loop:
                    return Loop((LoopExpression) expression);
                case ExpressionType.Switch:
                    return Switch((SwitchExpression) expression);
                case ExpressionType.Throw:
                    return Throw((UnaryExpression) expression);
                case ExpressionType.Try:
                    return Try((TryExpression) expression);
                case ExpressionType.Unbox:
                    return Unbox((UnaryExpression) expression);
                case ExpressionType.AddAssign:
                    return AddAssign((BinaryExpression) expression);
                case ExpressionType.AndAssign:
                    return AndAssign((BinaryExpression) expression);
                case ExpressionType.DivideAssign:
                    return DivideAssign((BinaryExpression) expression);
                case ExpressionType.ExclusiveOrAssign:
                    return ExclusiveOrAssign((BinaryExpression) expression);
                case ExpressionType.LeftShiftAssign:
                    return LeftShiftAssign((BinaryExpression) expression);
                case ExpressionType.ModuloAssign:
                    return ModuloAssign((BinaryExpression) expression);
                case ExpressionType.MultiplyAssign:
                    return MultiplyAssign((BinaryExpression) expression);
                case ExpressionType.OrAssign:
                    return OrAssign((BinaryExpression) expression);
                case ExpressionType.PowerAssign:
                    return PowerAssign((BinaryExpression) expression);
                case ExpressionType.RightShiftAssign:
                    return RightShiftAssign((BinaryExpression) expression);
                case ExpressionType.SubtractAssign:
                    return SubtractAssign((BinaryExpression) expression);
                case ExpressionType.AddAssignChecked:
                    return AddAssignChecked((BinaryExpression) expression);
                case ExpressionType.MultiplyAssignChecked:
                    return MultiplyAssignChecked((BinaryExpression) expression);
                case ExpressionType.SubtractAssignChecked:
                    return SubtractAssignChecked((BinaryExpression) expression);
                case ExpressionType.PreIncrementAssign:
                    return PreIncrementAssign((UnaryExpression) expression);
                case ExpressionType.PreDecrementAssign:
                    return PreDecrementAssign((UnaryExpression) expression);
                case ExpressionType.PostIncrementAssign:
                    return PostIncrementAssign((UnaryExpression) expression);
                case ExpressionType.PostDecrementAssign:
                    return PostDecrementAssign((UnaryExpression) expression);
                case ExpressionType.TypeEqual:
                    return TypeEqual((TypeBinaryExpression) expression);
                case ExpressionType.OnesComplement:
                    return OnesComplement((UnaryExpression) expression);
                case ExpressionType.IsTrue:
                    return IsTrue((UnaryExpression) expression);
                case ExpressionType.IsFalse:
                    return IsFalse((UnaryExpression) expression);
                default:
                    throw new ArgumentOutOfRangeException("expression.NodeType");
            }
            throw new NotSupportedException(String.Format("Expression node type '{0}' is not supported.", expression.NodeType));
        }

        public abstract Expression Deserialize();

        public TExpression Deserialize<TExpression>()
            where TExpression : Expression
        {
            return (TExpression) this.Deserialize();
        }
    }
}
// vim:set ft=cs fenc=utf-8 ts=4 sw=4 sts=4 et:
