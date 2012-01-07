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
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using XSpect.Yacq.Expressions;

namespace XSpect.Yacq.LanguageServices
{
    partial class Reader
    {
        private void InitializeRules()
        {
            this.Rules = new List<Action<ReaderCursor, ReaderResult>>()
            {
                #region Whitespace
                (c, r) =>
                {
                    var length = c.PeekWhileStringForward(_ => _ == ' ' || _ == '\n' || _ == '\r' || _ == '\t')
                        .Count();
                    if (length > 0)
                    {
                        c.MoveForward(length);
                    }
                },
                #endregion
                #region Comma
                (c, r) =>
                {
                    if (c.PeekCharForward(0) == ',')
                    {
                        c.MoveForward(1);
                    }
                },
                #endregion
                #region One-line Comment
                (c, r) =>
                {
                    if (c.PeekCharForward(0) == ';')
                    {
                        c.MoveForward(1);
                        c.MoveForward(c.PeekWhileStringForward(_ => _ != '\n' && _ != '\r').Length + 1);
                        if (c.PeekCharForward(0) == '\n')
                        {
                            c.MoveForward(1);
                        }
                    }
                },
                #endregion
                #region Multi-line Comment
                (c, r) =>
                {
                    if (c.MatchForward("#|") || c.MatchForward("|#"))
                    {
                        do
                        {
                            c.MoveForward(c.PeekWhileStringForward(_ => _ != '#' && _ != '|').Length);
                            if (c.PeekCharForward(0) == '\0')
                            {
                                return;
                            }
                            else if (c.PeekStringForward(2) == "#|")
                            {
                                r.BeginScope("Comment", c.Position);
                                c.MoveForward(2);
                            }
                            else if (c.PeekStringForward(2) == "|#")
                            {
                                r.EndScope("Comment");
                                c.MoveForward(2);
                            }
                        } while (r.Current.Tag == "Comment");
                    }
                },
                #endregion
                #region Expression Comment
                (c, r) =>
                {
                    if (c.MatchForward("#;"))
                    {
                        r.Current.RegisterHook(_ => _.Drop());
                        c.MoveForward(2);
                    }
                },
                #endregion
                #region Open Parenthesis
                (c, r) =>
                {
                    if (c.PeekCharForward(0) == '(')
                    {
                        r.BeginScope("Parenthesis", c.Position);
                        c.MoveForward(1);
                    }
                },
                #endregion
                #region Close Parenthesis
                (c, r) =>
                {
                    if (c.PeekCharForward(0) == ')')
                    {
                        r.Current.StartPosition.Let(p =>
                            YacqExpression.List(r.EndScope("Parenthesis"))
                                .Apply(e => e.SetPosition(p, c.Position))
                        ).Apply(r.Current.Add);
                        c.MoveForward(1);
                    }
                },
                #endregion
                #region Open Bracket
                (c, r) =>
                {
                    if (c.PeekCharForward(0) == '[')
                    {
                        r.BeginScope("Bracket", c.Position);
                        c.MoveForward(1);
                    }
                },
                #endregion
                #region Close Bracket
                (c, r) =>
                {
                    if (c.PeekCharForward(0) == ']')
                    {
                        r.Current.StartPosition.Let(p =>
                            YacqExpression.Vector(r.EndScope("Bracket"))
                                .Apply(e => e.SetPosition(p, c.Position))
                        ).Apply(r.Current.Add);
                        c.MoveForward(1);
                    }
                },
                #endregion
                #region Open Brace
                (c, r) =>
                {
                    if (c.PeekCharForward(0) == '{')
                    {
                        r.BeginScope("Brace", c.Position);
                        c.MoveForward(1);
                    }
                },
                #endregion
                #region Close Brace
                (c, r) =>
                {
                    if (c.PeekCharForward(0) == '}')
                    {
                        r.Current.StartPosition.Let(p =>
                            YacqExpression.LambdaList(r.EndScope("Brace"))
                                .Apply(e => e.SetPosition(p, c.Position))
                        ).Apply(r.Current.Add);
                        c.MoveForward(1);
                    }
                },
                #endregion
                #region Period
                (c, r) =>
                {
                    if (c.PeekCharForward(0) == '.')
                    {
                        var left = r.Current.Drop();
                        var id = YacqExpression.Identifier(".").Apply(e => e.SetPosition(c, 1));
                        r.Current.RegisterHook(_ => _.Add(
                            _.Drop().Let(right => (left.List(":") != null
                                ? ((ListExpression) left).Let(l => YacqExpression.List(l.Elements
                                      .SkipLast(1)
                                      .Concat(EnumerableEx.Return(
#if SILVERLIGHT
                                          (Expression)
#endif
                                          YacqExpression.List(id, l.Elements.Last(), right)
                                      ))
                                  ))
                                : YacqExpression.List(id, left, right)
                            ).Apply(e => e.SetPosition(left.StartPosition, right.EndPosition)))
                        ));
                    }
                },
                #endregion
                #region Colon
                (c, r) =>
                {
                    if (c.PeekCharForward(0) == ':')
                    {
                        var left = r.Current.Drop();
                        var id = YacqExpression.Identifier(":").Apply(e => e.SetPosition(c, 1));
                        r.Current.RegisterHook(_ => _.Add(
                            _.Drop().Let(right => YacqExpression.List(
                                id,
                                left,
                                right
                            ).Apply(e => e.SetPosition(left.StartPosition, right.EndPosition)))
                        ));
                    }
                },
                #endregion
                #region Identifier
                (c, r) =>
                {
                    var _0 = c.PeekCharForward(0);
                    if (!((
                        Char.IsControl(_0) ||
                        _0 == ' ' ||
                        _0 == '"' ||
                        _0 == '#' ||
                        (_0 >= '\'' && _0 <= ')') || // ' ( )
                        _0 == ',' ||
                        (_0 >= '0' && _0 <= '9') || // 0-9
                        _0 == ';' ||
                        _0 == '[' ||
                        _0 == ']' ||
                        _0 == '`' ||
                        _0 == '{' ||
                        _0 == '}'
                    ) || (
                        c.PeekCharForward(1).Let(_1 => (
                            (_0 == '+' || _0 == '-') &&
                            (_1 >= '0' && _1 <= '9') // 0-9
                        ) || (
                            (_0 == '.' && _1 != '.') ||
                            (_0 == ':' && _1 != ':')
                        ))
                    )))
                    {
                        var str = c.PeekWhileStringForward((_0 == '.' || _0 == ':')
                            ? ((Func<Char, Int32, Boolean>) ((_, i) => _ == _0))
                            : (_, i) => i == 0 || !(
                                  Char.IsControl(_) ||
                                  _ == ' ' ||
                                  _ == '"' ||
                                  (_ >= '\'' && _ <= ')') || // ' ( )
                                  _ == ',' ||
                                  _ == '.' ||
                                  _ == ':' ||
                                  _ == ';' ||
                                  _ == '[' ||
                                  _ == ']' ||
                                  _ == '`' ||
                                  _ == '{' ||
                                  _ == '}'
                            )
                        );
                        r.Current.Add(YacqExpression.Identifier(str).Apply(e => e.SetPosition(c, str.Length)));
                    }
                },
                #endregion
                #region Number
                (c, r) =>
                {
                    var c_ = c.Clone();
                    var _0 = c.PeekCharForward(0);
                    if ((_0 >= '0' && _0 <= '9') || // 0-9
                        ((_0 == '+' || _0 == '-') && c.PeekCharForward(0).Let(_1 => _1 >= '0' && _1 <= '9'))
                    ) 
                    {
                        var number = c.PeekWhileStringForward((_, i) =>
                            i == 0 ||
                            (i == ((_0 == '+' || _0 == '-') ? 2 : 1) && _ == 'o' || _ == 'x') ||
                            (_ >= '0' && _ <= '9') || // 0-9
                            (_ >= 'A' && _ <= 'F') || // A-F
                            (_ >= 'a' && _ <= 'f') || // a-f
                            _ == '.' ||
                            _ == '_'
                        );
                        c.MoveForward(number.Length);
                        if (number.Last().Let(_ => _ == 'e' || _ == 'E') &&
                            c.PeekCharForward(0).Let(_ => _ == '+' || _ == '-')
                        )
                        {
                            var exponent = c.PeekWhileStringForward((_, i) =>
                                i == 0 ||
                                (_ >= '0' && _ <= '9') || // 0-9
                                _ == '_'
                            );
                            c.MoveForward(exponent.Length);
                            number += exponent;
                        }
                        if (c.PeekCharForward(0).Let(_ =>
                            _ == 'D' || _ == 'd' ||
                            _ == 'F' || _ == 'f' ||
                            _ == 'L' || _ == 'l' ||
                            _ == 'M' || _ == 'm' ||
                            _ == 'U' || _ == 'u'
                        ))
                        {
                            var type = c.PeekStringForward(
                                c.PeekStringForward(2)
                                .ToUpper()
                                .Let(_ => _ == "UL")
                                    ? 2
                                    : 1
                            );
                            number += type;
                        }
                        r.Current.Add(YacqExpression.Number(number).Apply(e => e.SetPosition(c_, number.Length)));
                    }
                },
                #endregion
                #region String
                (c, r) =>
                {
                    var _0 = c.PeekCharForward(0);
                    if (_0 == '\'' || _0 == '"' || _0 == '`')
                    {
                        var c_ = c.Clone();
                        var str = new StringBuilder();
                        c.MoveForward(1);
                        do
                        {
                            if (c.PeekCharForward(0) == _0)
                            {
                                str.Append(c.PeekCharForward(0));
                                c.MoveForward(1);
                            }
                            str.Append(c.PeekWhileStringForward(_ => _ != _0).Apply(_ => c.MoveForward(_.Length)));
                        } while (str.Length > 0 && str[str.Length - 1] == '\\');
                        r.Current.Add(YacqExpression.Text(_0, str.ToString()).Apply(e => e.SetPosition(c_, str.Length)));
                        c.MoveForward(1);
                    }
                },
                #endregion
            };
        }
    }
}