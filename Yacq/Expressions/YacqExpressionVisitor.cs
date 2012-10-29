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
using System.Linq;
using System.Linq.Expressions;

namespace XSpect.Yacq.Expressions
{
    /// <summary>
    /// Provides easy-to-use <see cref="ExpressionVisitor"/> implementation for <see cref="Expression"/> and also <see cref="YacqExpression"/> nodes.
    /// </summary>
    public class YacqExpressionVisitor
        : ExpressionVisitor
    {
        /// <summary>
        /// Gets the parent <see cref="YacqExpressionVisitor"/> to refer as overriden implementations.
        /// </summary>
        /// <returns>The parent <see cref="YacqExpressionVisitor"/>.</returns>
        public YacqExpressionVisitor Parent
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets and sets the implementation of <see cref="Visit"/> method.
        /// </summary>
        /// <value>The implementation of <see cref="Visit"/> method.</value>
        public Func<YacqExpressionVisitor, Expression, Expression> Expression
        {
            get;
            set;
        }

        /// <summary>
        /// Gets and sets the implementation of <see cref="VisitBinary"/> method.
        /// </summary>
        /// <value>The implementation of <see cref="VisitBinary"/> method.</value>
        public Func<YacqExpressionVisitor, BinaryExpression, Expression> Binary
        {
            get;
            set;
        }

        /// <summary>
        /// Gets and sets the implementation of <see cref="VisitBlock"/> method.
        /// </summary>
        /// <value>The implementation of <see cref="VisitBlock"/> method.</value>
        public Func<YacqExpressionVisitor, BlockExpression, Expression> Block
        {
            get;
            set;
        }

        /// <summary>
        /// Gets and sets the implementation of <see cref="VisitConditional"/> method.
        /// </summary>
        /// <value>The implementation of <see cref="VisitConditional"/> method.</value>
        public Func<YacqExpressionVisitor, ConditionalExpression, Expression> Conditional
        {
            get;
            set;
        }

        /// <summary>
        /// Gets and sets the implementation of <see cref="VisitConstant"/> method.
        /// </summary>
        /// <value>The implementation of <see cref="VisitConstant"/> method.</value>
        public Func<YacqExpressionVisitor, ConstantExpression, Expression> Constant
        {
            get;
            set;
        }

        /// <summary>
        /// Gets and sets the implementation of <see cref="VisitDebugInfo"/> method.
        /// </summary>
        /// <value>The implementation of <see cref="VisitDebugInfo"/> method.</value>
        public Func<YacqExpressionVisitor, DebugInfoExpression, Expression> DebugInfo
        {
            get;
            set;
        }

        /// <summary>
        /// Gets and sets the implementation of <see cref="VisitDynamic"/> method.
        /// </summary>
        /// <value>The implementation of <see cref="VisitDynamic"/> method.</value>
        public Func<YacqExpressionVisitor, DynamicExpression, Expression> Dynamic
        {
            get;
            set;
        }

        /// <summary>
        /// Gets and sets the implementation of <see cref="VisitDefault"/> method.
        /// </summary>
        /// <value>The implementation of <see cref="VisitDefault"/> method.</value>
        public Func<YacqExpressionVisitor, DefaultExpression, Expression> Default
        {
            get;
            set;
        }

        /// <summary>
        /// Gets and sets the implementation of <see cref="VisitExtension"/> method.
        /// </summary>
        /// <value>The implementation of <see cref="VisitExtension"/> method.</value>
        public Func<YacqExpressionVisitor, Expression, Expression> Extension
        {
            get;
            set;
        }

        /// <summary>
        /// Gets and sets the implementation of <see cref="VisitGoto"/> method.
        /// </summary>
        /// <value>The implementation of <see cref="VisitGoto"/> method.</value>
        public Func<YacqExpressionVisitor, GotoExpression, Expression> Goto
        {
            get;
            set;
        }

        /// <summary>
        /// Gets and sets the implementation of <see cref="VisitInvocation"/> method.
        /// </summary>
        /// <value>The implementation of <see cref="VisitInvocation"/> method.</value>
        public Func<YacqExpressionVisitor, InvocationExpression, Expression> Invocation
        {
            get;
            set;
        }

        /// <summary>
        /// Gets and sets the implementation of <see cref="VisitLabelTarget"/> method.
        /// </summary>
        /// <value>The implementation of <see cref="VisitLabelTarget"/> method.</value>
        public Func<YacqExpressionVisitor, LabelTarget, LabelTarget> LabelTarget
        {
            get;
            set;
        }

        /// <summary>
        /// Gets and sets the implementation of <see cref="VisitLabel"/> method.
        /// </summary>
        /// <value>The implementation of <see cref="VisitLabel"/> method.</value>
        public Func<YacqExpressionVisitor, LabelExpression, Expression> Label
        {
            get;
            set;
        }

        /// <summary>
        /// Gets and sets the implementation of <see cref="VisitLambda{T}"/> method.
        /// </summary>
        /// <value>The implementation of <see cref="VisitLambda{T}"/> method.</value>
        public Func<YacqExpressionVisitor, LambdaExpression, Expression> Lambda
        {
            get;
            set;
        }

        /// <summary>
        /// Gets and sets the implementation of <see cref="VisitLoop"/> method.
        /// </summary>
        /// <value>The implementation of <see cref="VisitLoop"/> method.</value>
        public Func<YacqExpressionVisitor, LoopExpression, Expression> Loop
        {
            get;
            set;
        }

        /// <summary>
        /// Gets and sets the implementation of <see cref="VisitMember"/> method.
        /// </summary>
        /// <value>The implementation of <see cref="VisitMember"/> method.</value>
        public Func<YacqExpressionVisitor, MemberExpression, Expression> Member
        {
            get;
            set;
        }

        /// <summary>
        /// Gets and sets the implementation of <see cref="VisitIndex"/> method.
        /// </summary>
        /// <value>The implementation of <see cref="VisitIndex"/> method.</value>
        public Func<YacqExpressionVisitor, IndexExpression, Expression> Index
        {
            get;
            set;
        }

        /// <summary>
        /// Gets and sets the implementation of <see cref="VisitMethodCall"/> method.
        /// </summary>
        /// <value>The implementation of <see cref="VisitMethodCall"/> method.</value>
        public Func<YacqExpressionVisitor, MethodCallExpression, Expression> MethodCall
        {
            get;
            set;
        }

        /// <summary>
        /// Gets and sets the implementation of <see cref="VisitNewArray"/> method.
        /// </summary>
        /// <value>The implementation of <see cref="VisitNewArray"/> method.</value>
        public Func<YacqExpressionVisitor, NewArrayExpression, Expression> NewArray
        {
            get;
            set;
        }

        /// <summary>
        /// Gets and sets the implementation of <see cref="VisitNew"/> method.
        /// </summary>
        /// <value>The implementation of <see cref="VisitNew"/> method.</value>
        public Func<YacqExpressionVisitor, NewExpression, Expression> New
        {
            get;
            set;
        }

        /// <summary>
        /// Gets and sets the implementation of <see cref="VisitParameter"/> method.
        /// </summary>
        /// <value>The implementation of <see cref="VisitParameter"/> method.</value>
        public Func<YacqExpressionVisitor, ParameterExpression, Expression> Parameter
        {
            get;
            set;
        }

        /// <summary>
        /// Gets and sets the implementation of <see cref="VisitRuntimeVariables"/> method.
        /// </summary>
        /// <value>The implementation of <see cref="VisitRuntimeVariables"/> method.</value>
        public Func<YacqExpressionVisitor, RuntimeVariablesExpression, Expression> RuntimeVariables
        {
            get;
            set;
        }

        /// <summary>
        /// Gets and sets the implementation of <see cref="VisitSwitchCase"/> method.
        /// </summary>
        /// <value>The implementation of <see cref="VisitSwitchCase"/> method.</value>
        public Func<YacqExpressionVisitor, SwitchCase, SwitchCase> SwitchCase
        {
            get;
            set;
        }

        /// <summary>
        /// Gets and sets the implementation of <see cref="VisitSwitch"/> method.
        /// </summary>
        /// <value>The implementation of <see cref="VisitSwitch"/> method.</value>
        public Func<YacqExpressionVisitor, SwitchExpression, Expression> Switch
        {
            get;
            set;
        }

        /// <summary>
        /// Gets and sets the implementation of <see cref="VisitCatchBlock"/> method.
        /// </summary>
        /// <value>The implementation of <see cref="VisitCatchBlock"/> method.</value>
        public Func<YacqExpressionVisitor, CatchBlock, CatchBlock> CatchBlock
        {
            get;
            set;
        }

        /// <summary>
        /// Gets and sets the implementation of <see cref="VisitTry"/> method.
        /// </summary>
        /// <value>The implementation of <see cref="VisitTry"/> method.</value>
        public Func<YacqExpressionVisitor, TryExpression, Expression> Try
        {
            get;
            set;
        }

        /// <summary>
        /// Gets and sets the implementation of <see cref="VisitTypeBinary"/> method.
        /// </summary>
        /// <value>The implementation of <see cref="VisitTypeBinary"/> method.</value>
        public Func<YacqExpressionVisitor, TypeBinaryExpression, Expression> TypeBinary
        {
            get;
            set;
        }

        /// <summary>
        /// Gets and sets the implementation of <see cref="VisitUnary"/> method.
        /// </summary>
        /// <value>The implementation of <see cref="VisitUnary"/> method.</value>
        public Func<YacqExpressionVisitor, UnaryExpression, Expression> Unary
        {
            get;
            set;
        }

        /// <summary>
        /// Gets and sets the implementation of <see cref="VisitMemberInit"/> method.
        /// </summary>
        /// <value>The implementation of <see cref="VisitMemberInit"/> method.</value>
        public Func<YacqExpressionVisitor, MemberInitExpression, Expression> MemberInit
        {
            get;
            set;
        }

        /// <summary>
        /// Gets and sets the implementation of <see cref="VisitListInit"/> method.
        /// </summary>
        /// <value>The implementation of <see cref="VisitListInit"/> method.</value>
        public Func<YacqExpressionVisitor, ListInitExpression, Expression> ListInit
        {
            get;
            set;
        }

        /// <summary>
        /// Gets and sets the implementation of <see cref="VisitElementInit"/> method.
        /// </summary>
        /// <value>The implementation of <see cref="VisitElementInit"/> method.</value>
        public Func<YacqExpressionVisitor, ElementInit, ElementInit> ElementInit
        {
            get;
            set;
        }

        /// <summary>
        /// Gets and sets the implementation of <see cref="VisitMemberBinding"/> method.
        /// </summary>
        /// <value>The implementation of <see cref="VisitMemberBinding"/> method.</value>
        public Func<YacqExpressionVisitor, MemberBinding, MemberBinding> MemberBinding
        {
            get;
            set;
        }

        /// <summary>
        /// Gets and sets the implementation of <see cref="VisitMemberAssignment"/> method.
        /// </summary>
        /// <value>The implementation of <see cref="VisitMemberAssignment"/> method.</value>
        public Func<YacqExpressionVisitor, MemberAssignment, MemberAssignment> MemberAssignment
        {
            get;
            set;
        }

        /// <summary>
        /// Gets and sets the implementation of <see cref="VisitMemberMemberBinding"/> method.
        /// </summary>
        /// <value>The implementation of <see cref="VisitMemberMemberBinding"/> method.</value>
        public Func<YacqExpressionVisitor, MemberMemberBinding, MemberMemberBinding> MemberMemberBinding
        {
            get;
            set;
        }

        /// <summary>
        /// Gets and sets the implementation of <see cref="VisitMemberListBinding"/> method.
        /// </summary>
        /// <value>The implementation of <see cref="VisitMemberListBinding"/> method.</value>
        public Func<YacqExpressionVisitor, MemberListBinding, MemberListBinding> MemberListBinding
        {
            get;
            set;
        }

        /// <summary>
        /// Gets and sets the implementation of <see cref="VisitYacq"/> method.
        /// </summary>
        /// <value>The implementation of <see cref="VisitYacq"/> method.</value>
        public Func<YacqExpressionVisitor, YacqExpression, Expression> Yacq
        {
            get;
            set;
        }

        /// <summary>
        /// Gets and sets the implementation of <see cref="VisitYacqSequence"/> method.
        /// </summary>
        /// <value>The implementation of <see cref="VisitYacqSequence"/> method.</value>
        public Func<YacqExpressionVisitor, YacqSequenceExpression, Expression> YacqSequence
        {
            get;
            set;
        }

        /// <summary>
        /// Gets and sets the implementation of <see cref="VisitAmbiguousLambda"/> method.
        /// </summary>
        /// <value>The implementation of <see cref="VisitAmbiguousLambda"/> method.</value>
        public Func<YacqExpressionVisitor, AmbiguousLambdaExpression, Expression> AmbiguousLambda
        {
            get;
            set;
        }

        /// <summary>
        /// Gets and sets the implementation of <see cref="VisitAmbiguousParameter"/> method.
        /// </summary>
        /// <value>The implementation of <see cref="VisitAmbiguousParameter"/> method.</value>
        public Func<YacqExpressionVisitor, AmbiguousParameterExpression, Expression> AmbiguousParameter
        {
            get;
            set;
        }

        /// <summary>
        /// Gets and sets the implementation of <see cref="VisitContextful"/> method.
        /// </summary>
        /// <value>The implementation of <see cref="VisitContextful"/> method.</value>
        public Func<YacqExpressionVisitor, ContextfulExpression, Expression> Contextful
        {
            get;
            set;
        }

        /// <summary>
        /// Gets and sets the implementation of <see cref="VisitDispatch"/> method.
        /// </summary>
        /// <value>The implementation of <see cref="VisitDispatch"/> method.</value>
        public Func<YacqExpressionVisitor, DispatchExpression, Expression> Dispatch
        {
            get;
            set;
        }

        /// <summary>
        /// Gets and sets the implementation of <see cref="VisitIdentifier"/> method.
        /// </summary>
        /// <value>The implementation of <see cref="VisitIdentifier"/> method.</value>
        public Func<YacqExpressionVisitor, IdentifierExpression, Expression> Identifier
        {
            get;
            set;
        }

        /// <summary>
        /// Gets and sets the implementation of <see cref="VisitIgnored"/> method.
        /// </summary>
        /// <value>The implementation of <see cref="VisitIgnored"/> method.</value>
        public Func<YacqExpressionVisitor, IgnoredExpression, Expression> Ignored
        {
            get;
            set;
        }

        /// <summary>
        /// Gets and sets the implementation of <see cref="VisitLambdaList"/> method.
        /// </summary>
        /// <value>The implementation of <see cref="VisitLambdaList"/> method.</value>
        public Func<YacqExpressionVisitor, LambdaListExpression, Expression> LambdaList
        {
            get;
            set;
        }

        /// <summary>
        /// Gets and sets the implementation of <see cref="VisitList"/> method.
        /// </summary>
        /// <value>The implementation of <see cref="VisitList"/> method.</value>
        public Func<YacqExpressionVisitor, ListExpression, Expression> List
        {
            get;
            set;
        }

        /// <summary>
        /// Gets and sets the implementation of <see cref="VisitMacro"/> method.
        /// </summary>
        /// <value>The implementation of <see cref="VisitMacro"/> method.</value>
        public Func<YacqExpressionVisitor, MacroExpression, Expression> Macro
        {
            get;
            set;
        }

        /// <summary>
        /// Gets and sets the implementation of <see cref="VisitNumber"/> method.
        /// </summary>
        /// <value>The implementation of <see cref="VisitNumber"/> method.</value>
        public Func<YacqExpressionVisitor, NumberExpression, Expression> Number
        {
            get;
            set;
        }

        /// <summary>
        /// Gets and sets the implementation of <see cref="VisitQuoted"/> method.
        /// </summary>
        /// <value>The implementation of <see cref="VisitQuoted"/> method.</value>
        public Func<YacqExpressionVisitor, QuotedExpression, Expression> Quoted
        {
            get;
            set;
        }

        /// <summary>
        /// Gets and sets the implementation of <see cref="VisitSerialized"/> method.
        /// </summary>
        /// <value>The implementation of <see cref="VisitSerialized"/> method.</value>
        public Func<YacqExpressionVisitor, SerializedExpression, Expression> Serialized
        {
            get;
            set;
        }

        /// <summary>
        /// Gets and sets the implementation of <see cref="VisitSymbolTable"/> method.
        /// </summary>
        /// <value>The implementation of <see cref="VisitSymbolTable"/> method.</value>
        public Func<YacqExpressionVisitor, SymbolTableExpression, Expression> SymbolTable
        {
            get;
            set;
        }

        /// <summary>
        /// Gets and sets the implementation of <see cref="VisitText"/> method.
        /// </summary>
        /// <value>The implementation of <see cref="VisitText"/> method.</value>
        public Func<YacqExpressionVisitor, TextExpression, Expression> Text
        {
            get;
            set;
        }

        /// <summary>
        /// Gets and sets the implementation of <see cref="VisitTypeCandidate"/> method.
        /// </summary>
        /// <value>The implementation of <see cref="VisitTypeCandidate"/> method.</value>
        public Func<YacqExpressionVisitor, TypeCandidateExpression, Expression> TypeCandidate
        {
            get;
            set;
        }

        /// <summary>
        /// Gets and sets the implementation of <see cref="VisitVector"/> method.
        /// </summary>
        /// <value>The implementation of <see cref="VisitVector"/> method.</value>
        public Func<YacqExpressionVisitor, VectorExpression, Expression> Vector
        {
            get;
            set;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="YacqExpressionVisitor"/> class.
        /// </summary>
        /// <param name="parent">Parent <see cref="YacqExpressionVisitor"/> as overriden implementations.</param>
        public YacqExpressionVisitor(YacqExpressionVisitor parent = null)
        {
            this.Parent = parent;
        }

        /// <summary>
        /// Dispatches the expression to one of the more specialized visit methods in this class.
        /// </summary>
        /// <param name="node">The expression to visit.</param>
        /// <returns>The modified expression, if it or any subexpression was modified; otherwise, returns the original expression.</returns>
        public override Expression Visit(Expression node)
        {
            return this.Expression != null
                ? this.Expression(this, node)
                : this.Parent != null
                      ? this.Parent.Expression(this, node)
                      : base.Visit(node);
        }

        /// <summary>
        /// Visits the children of the <see cref="BinaryExpression"/>.
        /// </summary>
        /// <param name="node">The expression to visit.</param>
        /// <returns>The modified expression, if it or any subexpression was modified; otherwise, returns the original expression.</returns>
        protected override Expression VisitBinary(BinaryExpression node)
        {
            return this.Binary != null
                ? this.Binary(this, node)
                : this.Parent != null
                      ? this.Parent.Binary(this, node)
                      : base.VisitBinary(node);
        }

        /// <summary>
        /// Visits the children of the <see cref="BlockExpression"/>.
        /// </summary>
        /// <param name="node">The expression to visit.</param>
        /// <returns>The modified expression, if it or any subexpression was modified; otherwise, returns the original expression.</returns>
        protected override Expression VisitBlock(BlockExpression node)
        {
            return this.Block != null
                ? this.Block(this, node)
                : this.Parent != null
                      ? this.Parent.Block(this, node)
                      : base.VisitBlock(node);
        }

        /// <summary>
        /// Visits the children of the <see cref="ConditionalExpression"/>.
        /// </summary>
        /// <param name="node">The expression to visit.</param>
        /// <returns>The modified expression, if it or any subexpression was modified; otherwise, returns the original expression.</returns>
        protected override Expression VisitConditional(ConditionalExpression node)
        {
            return this.Conditional != null
                ? this.Conditional(this, node)
                : this.Parent != null
                      ? this.Parent.Conditional(this, node)
                      : base.VisitConditional(node);
        }

        /// <summary>
        /// Visits the <see cref="ConstantExpression"/>.
        /// </summary>
        /// <param name="node">The expression to visit.</param>
        /// <returns>The modified expression, if it or any subexpression was modified; otherwise, returns the original expression.</returns>
        protected override Expression VisitConstant(ConstantExpression node)
        {
            return this.Constant != null
                ? this.Constant(this, node)
                : this.Parent != null
                      ? this.Parent.Constant(this, node)
                      : base.VisitConstant(node);
        }

        /// <summary>
        /// Visits the <see cref="DebugInfoExpression"/>.
        /// </summary>
        /// <param name="node">The expression to visit.</param>
        /// <returns>The modified expression, if it or any subexpression was modified; otherwise, returns the original expression.</returns>
        protected override Expression VisitDebugInfo(DebugInfoExpression node)
        {
            return this.DebugInfo != null
                ? this.DebugInfo(this, node)
                : this.Parent != null
                      ? this.Parent.DebugInfo(this, node)
                      : base.VisitDebugInfo(node);
        }

        /// <summary>
        /// Visits the children of the <see cref="DynamicExpression"/>.
        /// </summary>
        /// <param name="node">The expression to visit.</param>
        /// <returns>The modified expression, if it or any subexpression was modified; otherwise, returns the original expression.</returns>
        protected override Expression VisitDynamic(DynamicExpression node)
        {
            return this.Dynamic != null
                ? this.Dynamic(this, node)
                : this.Parent != null
                      ? this.Parent.Dynamic(this, node)
                      : base.VisitDynamic(node);
        }

        /// <summary>
        /// Visits the <see cref="DefaultExpression"/>.
        /// </summary>
        /// <param name="node">The expression to visit.</param>
        /// <returns>The modified expression, if it or any subexpression was modified; otherwise, returns the original expression.</returns>
        protected override Expression VisitDefault(DefaultExpression node)
        {
            return this.Default != null
                ? this.Default(this, node)
                : this.Parent != null
                      ? this.Parent.Default(this, node)
                      : base.VisitDefault(node);
        }

        /// <summary>
        /// Visits the children of the extension expression.
        /// </summary>
        /// <param name="node">The expression to visit.</param>
        /// <returns>The modified expression, if it or any subexpression was modified; otherwise, returns the original expression.</returns>
        protected override Expression VisitExtension(Expression node)
        {
            return this.Extension != null
                ? this.Extension(this, node)
                : this.Parent != null
                      ? this.Parent.Extension(this, node)
                      : (node as YacqExpression).Null(e => this.VisitYacq(e))
                            ?? base.VisitExtension(node);
        }

        /// <summary>
        /// Visits the children of the <see cref="GotoExpression"/>.
        /// </summary>
        /// <param name="node">The expression to visit.</param>
        /// <returns>The modified expression, if it or any subexpression was modified; otherwise, returns the original expression.</returns>
        protected override Expression VisitGoto(GotoExpression node)
        {
            return this.Goto != null
                ? this.Goto(this, node)
                : this.Parent != null
                      ? this.Parent.Goto(this, node)
                      : base.VisitGoto(node);
        }

        /// <summary>
        /// Visits the children of the <see cref="InvocationExpression"/>.
        /// </summary>
        /// <param name="node">The expression to visit.</param>
        /// <returns>The modified expression, if it or any subexpression was modified; otherwise, returns the original expression.</returns>
        protected override Expression VisitInvocation(InvocationExpression node)
        {
            return this.Invocation != null
                ? this.Invocation(this, node)
                : this.Parent != null
                      ? this.Parent.Invocation(this, node)
                      : base.VisitInvocation(node);
        }

        /// <summary>
        /// Visits the <see cref="LabelTarget"/>.
        /// </summary>
        /// <param name="node">The expression to visit.</param>
        /// <returns>The modified expression, if it or any subexpression was modified; otherwise, returns the original expression.</returns>
        protected override LabelTarget VisitLabelTarget(LabelTarget node)
        {
            return this.LabelTarget != null
                ? this.LabelTarget(this, node)
                : this.Parent != null
                      ? this.Parent.LabelTarget(this, node)
                      : base.VisitLabelTarget(node);
        }

        /// <summary>
        /// Visits the children of the <see cref="LabelExpression"/>.
        /// </summary>
        /// <param name="node">The expression to visit.</param>
        /// <returns>The modified expression, if it or any subexpression was modified; otherwise, returns the original expression.</returns>
        protected override Expression VisitLabel(LabelExpression node)
        {
            return this.Label != null
                ? this.Label(this, node)
                : this.Parent != null
                      ? this.Parent.Label(this, node)
                      : base.VisitLabel(node);
        }

        /// <summary>
        /// Visits the children of the <see cref="Expression{T}"/>.
        /// </summary>
        /// <typeparam name="T">The type of the delegate.</typeparam>
        /// <param name="node">The expression to visit.</param>
        /// <returns>The modified expression, if it or any subexpression was modified; otherwise, returns the original expression.</returns>
        protected override Expression VisitLambda<T>(Expression<T> node)
        {
            return this.Lambda != null
                ? this.Lambda(this, node)
                : this.Parent != null
                      ? this.Parent.Lambda(this, node)
                      : base.VisitLambda(node);
        }

        /// <summary>
        /// Visits the children of the <see cref="LoopExpression"/>.
        /// </summary>
        /// <param name="node">The expression to visit.</param>
        /// <returns>The modified expression, if it or any subexpression was modified; otherwise, returns the original expression.</returns>
        protected override Expression VisitLoop(LoopExpression node)
        {
            return this.Loop != null
                ? this.Loop(this, node)
                : this.Parent != null
                      ? this.Parent.Loop(this, node)
                      : base.VisitLoop(node);
        }

        /// <summary>
        /// Visits the children of the <see cref="MemberExpression"/>.
        /// </summary>
        /// <param name="node">The expression to visit.</param>
        /// <returns>The modified expression, if it or any subexpression was modified; otherwise, returns the original expression.</returns>
        protected override Expression VisitMember(MemberExpression node)
        {
            return this.Member != null
                ? this.Member(this, node)
                : this.Parent != null
                      ? this.Parent.Member(this, node)
                      : base.VisitMember(node);
        }

        /// <summary>
        /// Visits the children of the <see cref="IndexExpression"/>.
        /// </summary>
        /// <param name="node">The expression to visit.</param>
        /// <returns>The modified expression, if it or any subexpression was modified; otherwise, returns the original expression.</returns>
        protected override Expression VisitIndex(IndexExpression node)
        {
            return this.Index != null
                ? this.Index(this, node)
                : this.Parent != null
                      ? this.Parent.Index(this, node)
                      : base.VisitIndex(node);
        }

        /// <summary>
        /// Visits the children of the <see cref="MethodCallExpression"/>.
        /// </summary>
        /// <param name="node">The expression to visit.</param>
        /// <returns>The modified expression, if it or any subexpression was modified; otherwise, returns the original expression.</returns>
        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            return this.MethodCall != null
                ? this.MethodCall(this, node)
                : this.Parent != null
                      ? this.Parent.MethodCall(this, node)
                      : base.VisitMethodCall(node);
        }

        /// <summary>
        /// Visits the children of the <see cref="NewArrayExpression"/>.
        /// </summary>
        /// <param name="node">The expression to visit.</param>
        /// <returns>The modified expression, if it or any subexpression was modified; otherwise, returns the original expression.</returns>
        protected override Expression VisitNewArray(NewArrayExpression node)
        {
            return this.NewArray != null
                ? this.NewArray(this, node)
                : this.Parent != null
                      ? this.Parent.NewArray(this, node)
                      : base.VisitNewArray(node);
        }

        /// <summary>
        /// Visits the children of the <see cref="NewExpression"/>.
        /// </summary>
        /// <param name="node">The expression to visit.</param>
        /// <returns>The modified expression, if it or any subexpression was modified; otherwise, returns the original expression.</returns>
        protected override Expression VisitNew(NewExpression node)
        {
            return this.New != null
                ? this.New(this, node)
                : this.Parent != null
                      ? this.Parent.New(this, node)
                      : base.VisitNew(node);
        }

        /// <summary>
        /// Visits the <see cref="ParameterExpression"/>.
        /// </summary>
        /// <param name="node">The expression to visit.</param>
        /// <returns>The modified expression, if it or any subexpression was modified; otherwise, returns the original expression.</returns>
        protected override Expression VisitParameter(ParameterExpression node)
        {
            return this.Parameter != null
                ? this.Parameter(this, node)
                : this.Parent != null
                      ? this.Parent.Parameter(this, node)
                      : base.VisitParameter(node);
        }

        /// <summary>
        /// Visits the children of the <see cref="RuntimeVariablesExpression"/>.
        /// </summary>
        /// <param name="node">The expression to visit.</param>
        /// <returns>The modified expression, if it or any subexpression was modified; otherwise, returns the original expression.</returns>
        protected override Expression VisitRuntimeVariables(RuntimeVariablesExpression node)
        {
            return this.RuntimeVariables != null
                ? this.RuntimeVariables(this, node)
                : this.Parent != null
                      ? this.Parent.RuntimeVariables(this, node)
                      : base.VisitRuntimeVariables(node);
        }

        /// <summary>
        /// Visits the children of the <see cref="SwitchCase"/>.
        /// </summary>
        /// <param name="node">The expression to visit.</param>
        /// <returns>The modified expression, if it or any subexpression was modified; otherwise, returns the original expression.</returns>
        protected override SwitchCase VisitSwitchCase(SwitchCase node)
        {
            return this.SwitchCase != null
                ? this.SwitchCase(this, node)
                : this.Parent != null
                      ? this.Parent.SwitchCase(this, node)
                      : base.VisitSwitchCase(node);
        }

        /// <summary>
        /// Visits the children of the <see cref="SwitchExpression"/>.
        /// </summary>
        /// <param name="node">The expression to visit.</param>
        /// <returns>The modified expression, if it or any subexpression was modified; otherwise, returns the original expression.</returns>
        protected override Expression VisitSwitch(SwitchExpression node)
        {
            return this.Switch != null
                ? this.Switch(this, node)
                : this.Parent != null
                      ? this.Parent.Switch(this, node)
                      : base.VisitSwitch(node);
        }

        /// <summary>
        /// Visits the children of the <see cref="CatchBlock"/>.
        /// </summary>
        /// <param name="node">The expression to visit.</param>
        /// <returns>The modified expression, if it or any subexpression was modified; otherwise, returns the original expression.</returns>
        protected override CatchBlock VisitCatchBlock(CatchBlock node)
        {
            return this.CatchBlock != null
                ? this.CatchBlock(this, node)
                : this.Parent != null
                      ? this.Parent.CatchBlock(this, node)
                      : base.VisitCatchBlock(node);
        }

        /// <summary>
        /// Visits the children of the <see cref="TryExpression"/>.
        /// </summary>
        /// <param name="node">The expression to visit.</param>
        /// <returns>The modified expression, if it or any subexpression was modified; otherwise, returns the original expression.</returns>
        protected override Expression VisitTry(TryExpression node)
        {
            return this.Try != null
                ? this.Try(this, node)
                : this.Parent != null
                      ? this.Parent.Try(this, node)
                      : base.VisitTry(node);
        }

        /// <summary>
        /// Visits the children of the <see cref="TypeBinaryExpression"/>.
        /// </summary>
        /// <param name="node">The expression to visit.</param>
        /// <returns>The modified expression, if it or any subexpression was modified; otherwise, returns the original expression.</returns>
        protected override Expression VisitTypeBinary(TypeBinaryExpression node)
        {
            return this.TypeBinary != null
                ? this.TypeBinary(this, node)
                : this.Parent != null
                      ? this.Parent.TypeBinary(this, node)
                      : base.VisitTypeBinary(node);
        }

        /// <summary>
        /// Visits the children of the <see cref="UnaryExpression"/>.
        /// </summary>
        /// <param name="node">The expression to visit.</param>
        /// <returns>The modified expression, if it or any subexpression was modified; otherwise, returns the original expression.</returns>
        protected override Expression VisitUnary(UnaryExpression node)
        {
            return this.Unary != null
                ? this.Unary(this, node)
                : this.Parent != null
                      ? this.Parent.Unary(this, node)
                      : base.VisitUnary(node);
        }

        /// <summary>
        /// Visits the children of the <see cref="MemberInitExpression"/>.
        /// </summary>
        /// <param name="node">The expression to visit.</param>
        /// <returns>The modified expression, if it or any subexpression was modified; otherwise, returns the original expression.</returns>
        protected override Expression VisitMemberInit(MemberInitExpression node)
        {
            return this.MemberInit != null
                ? this.MemberInit(this, node)
                : this.Parent != null
                      ? this.Parent.MemberInit(this, node)
                      : base.VisitMemberInit(node);
        }

        /// <summary>
        /// Visits the children of the <see cref="ListInitExpression"/>.
        /// </summary>
        /// <param name="node">The expression to visit.</param>
        /// <returns>The modified expression, if it or any subexpression was modified; otherwise, returns the original expression.</returns>
        protected override Expression VisitListInit(ListInitExpression node)
        {
            return this.ListInit != null
                ? this.ListInit(this, node)
                : this.Parent != null
                      ? this.Parent.ListInit(this, node)
                      : base.VisitListInit(node);
        }

        /// <summary>
        /// Visits the children of the <see cref="ElementInit"/>.
        /// </summary>
        /// <param name="node">The expression to visit.</param>
        /// <returns>The modified expression, if it or any subexpression was modified; otherwise, returns the original expression.</returns>
        protected override ElementInit VisitElementInit(ElementInit node)
        {
            return this.ElementInit != null
                ? this.ElementInit(this, node)
                : this.Parent != null
                      ? this.Parent.ElementInit(this, node)
                      : base.VisitElementInit(node);
        }

        /// <summary>
        /// Visits the children of the <see cref="MemberBinding"/>.
        /// </summary>
        /// <param name="node">The expression to visit.</param>
        /// <returns>The modified expression, if it or any subexpression was modified; otherwise, returns the original expression.</returns>
        protected override MemberBinding VisitMemberBinding(MemberBinding node)
        {
            return this.MemberBinding != null
                ? this.MemberBinding(this, node)
                : this.Parent != null
                      ? this.Parent.MemberBinding(this, node)
                      : base.VisitMemberBinding(node);
        }

        /// <summary>
        /// Visits the children of the <see cref="MemberAssignment"/>.
        /// </summary>
        /// <param name="node">The expression to visit.</param>
        /// <returns>The modified expression, if it or any subexpression was modified; otherwise, returns the original expression.</returns>
        protected override MemberAssignment VisitMemberAssignment(MemberAssignment node)
        {
            return this.MemberAssignment != null
                ? this.MemberAssignment(this, node)
                : this.Parent != null
                      ? this.Parent.MemberAssignment(this, node)
                      : base.VisitMemberAssignment(node);
        }

        /// <summary>
        /// Visits the children of the <see cref="MemberMemberBinding"/>.
        /// </summary>
        /// <param name="node">The expression to visit.</param>
        /// <returns>The modified expression, if it or any subexpression was modified; otherwise, returns the original expression.</returns>
        protected override MemberMemberBinding VisitMemberMemberBinding(MemberMemberBinding node)
        {
            return this.MemberMemberBinding != null
                ? this.MemberMemberBinding(this, node)
                : this.Parent != null
                      ? this.Parent.MemberMemberBinding(this, node)
                      : base.VisitMemberMemberBinding(node);
        }

        /// <summary>
        /// Visits the children of the <see cref="MemberListBinding"/>.
        /// </summary>
        /// <param name="node">The expression to visit.</param>
        /// <returns>The modified expression, if it or any subexpression was modified; otherwise, returns the original expression.</returns>
        protected override MemberListBinding VisitMemberListBinding(MemberListBinding node)
        {
            return this.MemberListBinding != null
                ? this.MemberListBinding(this, node)
                : this.Parent != null
                      ? this.Parent.MemberListBinding(this, node)
                      : base.VisitMemberListBinding(node);
        }

        /// <summary>
        /// Visits the children of the <see cref="YacqExpression"/>.
        /// </summary>
        /// <param name="node">The expression to visit.</param>
        /// <returns>The modified expression, if it or any subexpression was modified; otherwise, returns the original expression.</returns>
        protected virtual Expression VisitYacq(YacqExpression node)
        {
            return this.Yacq != null
                ? this.Yacq(this, node)
                : this.Parent != null
                      ? this.Parent.Yacq(this, node)
                      : (node as YacqSequenceExpression).Null(e => this.VisitYacqSequence(e))
                            ?? (node as AmbiguousLambdaExpression).Null(e => this.VisitAmbiguousLambda(e))
                            ?? (node as AmbiguousParameterExpression).Null(e => this.VisitAmbiguousParameter(e))
                            ?? (node as ContextfulExpression).Null(e => this.VisitContextful(e))
                            ?? (node as DispatchExpression).Null(e => this.VisitDispatch(e))
                            ?? (node as IdentifierExpression).Null(e => this.VisitIdentifier(e))
                            ?? (node as IgnoredExpression).Null(e => this.VisitIgnored(e))
                            ?? (node as MacroExpression).Null(e => this.VisitMacro(e))
                            ?? (node as NumberExpression).Null(e => this.VisitNumber(e))
                            ?? (node as QuotedExpression).Null(e => this.VisitQuoted(e))
                            ?? (node as SerializedExpression).Null(e => this.VisitSerialized(e))
                            ?? (node as SymbolTableExpression).Null(e => this.VisitSymbolTable(e))
                            ?? (node as TextExpression).Null(e => this.VisitText(e))
                            ?? (node as TypeCandidateExpression).Null(e => this.VisitTypeCandidate(e));
        }

        /// <summary>
        /// Visits the children of the <see cref="YacqSequenceExpression"/>.
        /// </summary>
        /// <param name="node">The expression to visit.</param>
        /// <returns>The modified expression, if it or any subexpression was modified; otherwise, returns the original expression.</returns>
        protected virtual Expression VisitYacqSequence(YacqSequenceExpression node)
        {
            return this.YacqSequence != null
                ? this.YacqSequence(this, node)
                : this.Parent != null
                      ? this.Parent.YacqSequence(this, node)
                      : (node as LambdaListExpression).Null(e => this.VisitLambdaList(e))
                            ?? (node as ListExpression).Null(e => this.VisitList(e))
                            ?? (node as VectorExpression).Null(e => this.VisitVector(e));
        }

        /// <summary>
        /// Visits the children of the <see cref="AmbiguousLambdaExpression"/>.
        /// </summary>
        /// <param name="node">The expression to visit.</param>
        /// <returns>The modified expression, if it or any subexpression was modified; otherwise, returns the original expression.</returns>
        protected virtual Expression VisitAmbiguousLambda(AmbiguousLambdaExpression node)
        {
            return this.AmbiguousLambda != null
                ? this.AmbiguousLambda(this, node)
                : this.Parent != null
                      ? this.Parent.AmbiguousLambda(this, node)
                      : base.VisitExtension(node);
        }

        /// <summary>
        /// Visits the children of the <see cref="AmbiguousParameterExpression"/>.
        /// </summary>
        /// <param name="node">The expression to visit.</param>
        /// <returns>The modified expression, if it or any subexpression was modified; otherwise, returns the original expression.</returns>
        protected virtual Expression VisitAmbiguousParameter(AmbiguousParameterExpression node)
        {
            return this.AmbiguousParameter != null
                ? this.AmbiguousParameter(this, node)
                : this.Parent != null
                      ? this.Parent.AmbiguousParameter(this, node)
                      : base.VisitExtension(node);
        }

        /// <summary>
        /// Visits the children of the <see cref="ContextfulExpression"/>.
        /// </summary>
        /// <param name="node">The expression to visit.</param>
        /// <returns>The modified expression, if it or any subexpression was modified; otherwise, returns the original expression.</returns>
        protected virtual Expression VisitContextful(ContextfulExpression node)
        {
            return this.Contextful != null
                ? this.Contextful(this, node)
                : this.Parent != null
                      ? this.Parent.Contextful(this, node)
                      : base.VisitExtension(node);
        }

        /// <summary>
        /// Visits the children of the <see cref="DispatchExpression"/>.
        /// </summary>
        /// <param name="node">The expression to visit.</param>
        /// <returns>The modified expression, if it or any subexpression was modified; otherwise, returns the original expression.</returns>
        protected virtual Expression VisitDispatch(DispatchExpression node)
        {
            return this.Dispatch != null
                ? this.Dispatch(this, node)
                : this.Parent != null
                      ? this.Parent.Dispatch(this, node)
                      : base.VisitExtension(node);
        }

        /// <summary>
        /// Visits the children of the <see cref="IdentifierExpression"/>.
        /// </summary>
        /// <param name="node">The expression to visit.</param>
        /// <returns>The modified expression, if it or any subexpression was modified; otherwise, returns the original expression.</returns>
        protected virtual Expression VisitIdentifier(IdentifierExpression node)
        {
            return this.Identifier != null
                ? this.Identifier(this, node)
                : this.Parent != null
                      ? this.Parent.Identifier(this, node)
                      : base.VisitExtension(node);
        }

        /// <summary>
        /// Visits the children of the <see cref="IgnoredExpression"/>.
        /// </summary>
        /// <param name="node">The expression to visit.</param>
        /// <returns>The modified expression, if it or any subexpression was modified; otherwise, returns the original expression.</returns>
        protected virtual Expression VisitIgnored(IgnoredExpression node)
        {
            return this.Ignored != null
                ? this.Ignored(this, node)
                : this.Parent != null
                      ? this.Parent.Ignored(this, node)
                      : base.VisitExtension(node);
        }

        /// <summary>
        /// Visits the children of the <see cref="LambdaListExpression"/>.
        /// </summary>
        /// <param name="node">The expression to visit.</param>
        /// <returns>The modified expression, if it or any subexpression was modified; otherwise, returns the original expression.</returns>
        protected virtual Expression VisitLambdaList(LambdaListExpression node)
        {
            return this.LambdaList != null
                ? this.LambdaList(this, node)
                : this.Parent != null
                      ? this.Parent.LambdaList(this, node)
                      : base.VisitExtension(node);
        }

        /// <summary>
        /// Visits the children of the <see cref="ListExpression"/>.
        /// </summary>
        /// <param name="node">The expression to visit.</param>
        /// <returns>The modified expression, if it or any subexpression was modified; otherwise, returns the original expression.</returns>
        protected virtual Expression VisitList(ListExpression node)
        {
            return this.List != null
                ? this.List(this, node)
                : this.Parent != null
                      ? this.Parent.List(this, node)
                      : base.VisitExtension(node);
        }

        /// <summary>
        /// Visits the children of the <see cref="MacroExpression"/>.
        /// </summary>
        /// <param name="node">The expression to visit.</param>
        /// <returns>The modified expression, if it or any subexpression was modified; otherwise, returns the original expression.</returns>
        protected virtual Expression VisitMacro(MacroExpression node)
        {
            return this.Macro != null
                ? this.Macro(this, node)
                : this.Parent != null
                      ? this.Parent.Macro(this, node)
                      : base.VisitExtension(node);
        }

        /// <summary>
        /// Visits the children of the <see cref="NumberExpression"/>.
        /// </summary>
        /// <param name="node">The expression to visit.</param>
        /// <returns>The modified expression, if it or any subexpression was modified; otherwise, returns the original expression.</returns>
        protected virtual Expression VisitNumber(NumberExpression node)
        {
            return this.Number != null
                ? this.Number(this, node)
                : this.Parent != null
                      ? this.Parent.Number(this, node)
                      : base.VisitExtension(node);
        }

        /// <summary>
        /// Visits the children of the <see cref="QuotedExpression"/>.
        /// </summary>
        /// <param name="node">The expression to visit.</param>
        /// <returns>The modified expression, if it or any subexpression was modified; otherwise, returns the original expression.</returns>
        protected virtual Expression VisitQuoted(QuotedExpression node)
        {
            return this.Quoted != null
                ? this.Quoted(this, node)
                : this.Parent != null
                      ? this.Parent.Quoted(this, node)
                      : base.VisitExtension(node);
        }

        /// <summary>
        /// Visits the children of the <see cref="SerializedExpression"/>.
        /// </summary>
        /// <param name="node">The expression to visit.</param>
        /// <returns>The modified expression, if it or any subexpression was modified; otherwise, returns the original expression.</returns>
        protected virtual Expression VisitSerialized(SerializedExpression node)
        {
            return this.Serialized != null
                ? this.Serialized(this, node)
                : this.Parent != null
                      ? this.Parent.Serialized(this, node)
                      : base.VisitExtension(node);
        }

        /// <summary>
        /// Visits the children of the <see cref="SymbolTableExpression"/>.
        /// </summary>
        /// <param name="node">The expression to visit.</param>
        /// <returns>The modified expression, if it or any subexpression was modified; otherwise, returns the original expression.</returns>
        protected virtual Expression VisitSymbolTable(SymbolTableExpression node)
        {
            return this.SymbolTable != null
                ? this.SymbolTable(this, node)
                : this.Parent != null
                      ? this.Parent.SymbolTable(this, node)
                      : base.VisitExtension(node);
        }

        /// <summary>
        /// Visits the children of the <see cref="TextExpression"/>.
        /// </summary>
        /// <param name="node">The expression to visit.</param>
        /// <returns>The modified expression, if it or any subexpression was modified; otherwise, returns the original expression.</returns>
        protected virtual Expression VisitText(TextExpression node)
        {
            return this.Text != null
                ? this.Text(this, node)
                : this.Parent != null
                      ? this.Parent.Text(this, node)
                      : base.VisitExtension(node);
        }

        /// <summary>
        /// Visits the children of the <see cref="TypeCandidateExpression"/>.
        /// </summary>
        /// <param name="node">The expression to visit.</param>
        /// <returns>The modified expression, if it or any subexpression was modified; otherwise, returns the original expression.</returns>
        protected virtual Expression VisitTypeCandidate(TypeCandidateExpression node)
        {
            return this.TypeCandidate != null
                ? this.TypeCandidate(this, node)
                : this.Parent != null
                      ? this.Parent.TypeCandidate(this, node)
                      : base.VisitExtension(node);
        }

        /// <summary>
        /// Visits the children of the <see cref="VectorExpression"/>.
        /// </summary>
        /// <param name="node">The expression to visit.</param>
        /// <returns>The modified expression, if it or any subexpression was modified; otherwise, returns the original expression.</returns>
        protected virtual Expression VisitVector(VectorExpression node)
        {
            return this.Vector != null
                ? this.Vector(this, node)
                : this.Parent != null
                      ? this.Parent.Vector(this, node)
                      : base.VisitExtension(node);
        }
    }
}
// vim:set ft=cs fenc=utf-8 ts=4 sw=4 sts=4 et:
