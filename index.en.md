---
layout: default
title: Home
---

# YACQ, a Programming Language

YACQ <small>(pronounced as yacc)</small> is an **application-embeddable programming language for querying and scripting**, runs on .NET platform.

## Features

YACQ is a library that provides run-time scripting and querying for .NET applications. YACQ has the following features mainly:

* **Extension of Expression Trees API**. YACQ is based on [Expression Trees API](http://msdn.microsoft.com/library/bb397951.aspx), a standard feature and one of basis of LINQ ecosystem. Since YACQ extends this and uses as its native syntax tree, YACQ fits with standard and other LINQ query providers.
* **Language Services**. YACQ can construct expression trees from source code string by [Parseq](https://github.com/linerlock/parseq), a monadic parser combinator. There are no way to construct expression trees from string in run-time <small>(without external processes)</small> in vanilla environments. You can modify and extend the grammar of YACQ.
* **LINQ Support**. YACQ contains wrapper class of querying interfaces such as `IQueryable<T>`. You can use (standard) query operators along with YACQ, in order to specify selectors, predicates, and other lambdas by code strings. Moreover, YACQ supports [Rx (Reactive Extensions) and Ix (Interactive Extensions)](https://rx.codeplex.com/).
* **Extended Type System**. YACQ not only can access to almost of .NET types, but also can extend the type system of .NET transparently. When you access to extension members, YACQ transforms to expression which is composed only of real member accesses. In short, YACQ can extend types without pollution of expression trees.
* **Type Generator**. Since the power of Expression Trees API, YACQ can compile codes to delegate or emits to static method builders, furthermore, YACQ also can define types with instance and static members, and output assemblies.
* **Expression Tree Serializer**. In vanilla environments, expression trees are not serializable. YACQ provides proxy types to support serialization. Proxy type describes the contents of the serialized expression tree more detailed and concisely than default `Expression.ToString()` outputs.

## Usages

Construct an expression tree from YACQ code string.

{% highlight csharp %}
Expression expr =
    YacqServices.Parse("(+ 1 2 3)");
    // same as ((1 + 2) + 3)
{% endhighlight %}

Lambdas which has only one argument can construct easily by the helper method.

{% highlight csharp %}
Expression<Func<string, string>> lambda =
    YacqServices.ParseFunc<string, string>("it.(ToUpper).(Replace 'FOO' 'bar')");
    // same as (string it) => it.ToUpper().Replace("foo", "bar")
{% endhighlight %}

Constructed lambdas can compile and evaluate by standard feature in run-time.

{% highlight csharp %}
Func<string, string> func = lambda.Compile();
func("foooo"); // returns "barOO"
{% endhighlight %}

You can use YACQ in LINQ queries.

{% highlight csharp %}
var query = Enumerable.Range(1, 100)
    .Yacq()
    .Where("(== (% it 3) 0)")
    .GroupBy("(/ it 10)")
    .Select("it.(Average)")
    .OrderByDescending("it");

// Same as:
Enumerable.Range(1, 100)
    .Where(it => it % 3 == 0)
    .GroupBy(it => it / 10)
    .Select(it => it.Average())
    .OrderByDescending(it => it);
{% endhighlight %}

YACQ provides expression trees serializer. In vanilla environments, they are not serializable.

{% highlight csharp %}
string xml = YacqServices.SaveText(lambda);
Expression lambda2 = YacqServices.LoadText(xml);
{% endhighlight %}

You can construct complex expression trees with YACQ-extended Expression Trees system.

{% highlight csharp %}
Expression expr2 = YacqExpression.TypeCandidate(typeof(Enumerable))
    .Method("Range", Expression.Constant(1), Expression.Constant(100))
    .Method("Reverse")
    .Method("Take", Expression.Constant(10))
    .Method("Sum")
    .Reduce();

// Same as:
Expression.Call(typeof(Enumerable), "Sum", null,
    Expression.Call(typeof(Enumerable), "Take", new [] { typeof(int), },
        Expression.Call(typeof(Enumerable), "Reverse", new [] { typeof(int), },
            Expression.Call(typeof(Enumerable), "Range", null, Expression.Constant(1), Expression.Constant(100))
        ), Expression.Constant(10)));
{% endhighlight %}

Although the main use of YACQ is to embed in other applications, YACQ also can use in standalone. Complier frontend and REPL environment are bundled.

## Get Started

To use YACQ, you have to refer `Yacq.dll` by your application. There are some way to get the system:

* **[Install NuGet package](http://nuget.org/packages/Yacq)**: If you only need the library, you can get and install it via NuGet.<br />
```
PM> Install-Package Yacq
```
* **[Download binary archive](http://yacq.net/download)**: You can download the binary archive. This contains all libraries, executables, and documents. If you want compiler frontend or REPL environment, please download this.
* **[Download source code in GitHub](https://github.com/takeshik/yacq)**: You can get the source codes in GitHub.
    * **Clone the repository**: You can clone the Git repository.<br />
```
% git clone git://github.com/takeshik/yacq.git
```
    * **Download snapshot**: Snapshot archive of the repository is also available ([tar.gz](https://github.com/takeshik/yacq/tarball/master) | [zip](https://github.com/takeshik/yacq/zipball/master)).

Other resources are also available in the [Download Page](/download).

## Prerequisites

* Target Framework:
    * .NET Framework 4 or later
    * Silverlight 5
    * Mono 2.10 or later
* Library Dependencies
    * Reactive Extensions ([Rx-Main](http://nuget.org/packages/Rx-Main), [Rx-Providers](http://nuget.org/packages/Rx-Providers))
    * Interactive Extensions ([Ix\_Experimental-Main](http://nuget.org/packages/Ix_Experimental-Main), [Ix_Experimental-Providers](http://nuget.org/packages/Ix_Experimental-Providers))
    * [Parseq](http://nuget.org/packages/Parseq)

## Licensing

Copyright &copy; 2011-2013 Takeshi KIRIYA (aka takeshik) <small>([Web](http://www.takeshik.org/) | [Mail](mailto:takeshik_AT_yacq_DOT_net) | [GitHub](https://github.com/takeshik) | [Twitter](https://twitter.com/takeshik)</small>), All rights reserved.

YACQ is [Free Software](http://www.gnu.org/philosophy/free-sw.html). Its source codes, binaries, and all other resources are licensed under the [MIT License](/license).
