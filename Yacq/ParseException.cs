// -*- mode: csharp; encoding: utf-8; tab-width: 4; c-basic-offset: 4; indent-tabs-mode: nil; -*-
// vim:set ft=cs fenc=utf-8 ts=4 sw=4 sts=4 et:
// $Id$
/* YACQ
 *   Yet Another Compilable Query Language, based on Expression Trees API
 * Copyright © 2011-2012 Takeshi KIRIYA (aka takeshik) <takeshik@users.sf.net>
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
using XSpect.Yacq.LanguageServices;

namespace XSpect.Yacq
{
    /// <summary>
    /// The exception that is thrown when the language system encountered errors about parsing.
    /// </summary>
    public partial class ParseException
        : Exception
    {
        /// <summary>
        /// Gets the expression to explain the cause of the expression.
        /// </summary>
        /// <value>The expression to explain the cause of the expression.</value>
        public Expression Expression
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the start position in the source for the exception.
        /// </summary>
        /// <value>The start position in the source for the exception.</value>
        public Nullable<TextPosition> StartPosition
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the end position in the source for the exception.
        /// </summary>
        /// <value>The end position in the source for the exception.</value>
        public Nullable<TextPosition> EndPosition
        {
            get;
            private set;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ParseException"/> class.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        public ParseException(String message)
            : base(message)
        {
        }

        private ParseException(
            String message,
            Expression expression,
            Nullable<TextPosition> startPosition,
            Nullable<TextPosition> endPosition
        )
            : base(message + " (at " + GetPositionString(expression, startPosition, endPosition) + ")")
        {
            this.Expression = expression;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ParseException"/> class.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="expression">The expression that explains the cause of the expression.</param>
        public ParseException(
            String message,
            Expression expression
        )
            : this(message, expression, null, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ParseException"/> class.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="position">The position in the source for the exception.</param>
        public ParseException(
            String message,
            TextPosition position
        )
            : this(message, null, position, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ParseException"/> class.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="startPosition">The start position in the source for the exception.</param>
        /// <param name="endPosition">The end position in the source for the exception.</param>
        public ParseException(
            String message,
            TextPosition startPosition,
            TextPosition endPosition
        )
            : this(message, null, startPosition, endPosition)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ParseException"/> class.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="expression">The expression that explains the cause of the expression.</param>
        /// <param name="startPosition">The start position in the source for the exception.</param>
        /// <param name="endPosition">The end position in the source for the exception.</param>
        public ParseException(
            String message,
            Expression expression,
            TextPosition startPosition,
            TextPosition endPosition
        )
            : this(message, expression, (Nullable<TextPosition>) startPosition, endPosition)
        {
        }

#if !SILVERLIGHT
        /// <summary>
        /// Initializes a new instance of the <see cref="ParseException"/> class with serialized data.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="StreamingContext"/> that contains contextual information about the source or destination.</param>
        protected ParseException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
#endif

        private static String GetPositionString(Expression expression, Nullable<TextPosition> startPosition, Nullable<TextPosition> endPosition)
        {
            return ((expression as YacqExpression)
                .Null(e => Tuple.Create(
                    (startPosition ?? e.StartPosition).ToString(),
                    (endPosition ?? e.EndPosition).ToString()
                )) ?? Tuple.Create(
                    (startPosition != null ? startPosition.Value.ToString() : null),
                    (endPosition != null ? endPosition.Value.ToString() : null)
                )).Let(_ => _.Item2 != null
                    ? _.Item1 + " - " + _.Item1
                    : _.Item1
                );
        }
    }

#if !SILVERLIGHT
    // Hack for the XML document comment of ParseException class
    // (C# compiler doesn't recognize XML document comments for members which
    // is divided by compiler directive.)
    [Serializable()]
    public partial class ParseException
    {
    }
#endif
}