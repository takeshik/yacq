---
layout: default
title: ホーム
---

# ようこそ

YACQ <small>(yacc と同じように発音します)</small> は .NET プラットフォーム上で動作する、 **アプリケーション組込可能なクエリおよびスクリプト処理向けのプログラミング言語** です。

## 機能

YACQ は .NET アプリケーションに実行時のクエリおよびスクリプト処理の機能を提供するライブラリです。YACQ は主に以下のような機能を持ちます:

* **Expression Trees API 拡張**。YACQ は .NET の標準機能であり LINQ エコシステムの基礎である [Expression Trees API](http://msdn.microsoft.com/ja-jp/library/bb397951.aspx) を拡張し、構文木などの基盤はこの上に構築されています。従って、YACQ は標準および非標準の LINQ クエリ プロバイダと協調して機能します。
* **言語サービス**。YACQ は [Parseq](https://github.com/linerlock/parseq) (モナディック パーザ ジェネレータ) を利用しており、文法に従ってソース コード文字列から式ツリーを構築することができます。標準の環境では、文字列から実行時に式ツリーを <small>(外部プロセスを用いずに)</small> 構築する手段は存在しません。YACQ の文法は自由に変更・拡張することができます。
* **LINQ のサポート**。YACQ は `IQueryable<T>` に代表されるクエリ インターフェイスのラッパー クラスを含んでおり、(標準) クエリ演算における選択子や述語などのラムダ引数を YACQ コードによって用いて記述できます。さらに YACQ は [Rx (Reactive Extensions) および Ix (Interactive Extensions)](https://rx.codeplex.com/) もサポートしています。
* **拡張された型システム**。YACQ は .NET のほとんどの型にアクセスできるだけでなく、.NET の型システムを透過的に拡張することもできます。拡張されたメンバへのアクセスは、YACQ によって実際のメンバ アクセスのみによって構成された式に変換されます。つまり、YACQ は式ツリーを汚染することなく型を拡張できます。
* **型ジェネレータ**。Expression Trees API の機能により、YACQ はコードをデリゲートにコンパイルし、または静的メソッドのビルダへ出力することができますが、YACQ はさらに、静的メンバだけでなくインスタンス メンバを含んだ型を定義し、アセンブリを出力することもできます。
* **式ツリー シリアライザ**。標準の環境では、式ツリーはシリアライズすることができません。YACQ はプロキシ型を提供することで、式ツリーのシリアライズをサポートします。プロキシ型は既定の `Expression.ToString()` メソッドより詳細かつ簡潔にシリアライズされた式ツリーの内容を出力できます。

## 利用方法

YACQ のコード文字列から式ツリーを構築します。

{% highlight csharp %}
Expression expr =
    YacqServices.Parse("(+ 1 2 3)");
    // ((1 + 2) + 3) と同等
{% endhighlight %}

1 引数のみのラムダはヘルパ メソッドを用いて簡単に構築できます。

{% highlight csharp %}
Expression<Func<string, string>> lambda =
    YacqServices.ParseFunc<string, string>("it.(ToUpper).(Replace 'FOO' 'bar')");
    // (string it) => it.ToUpper().Replace("foo", "bar") と同等
{% endhighlight %}

構築されたラムダはシステム標準の機能によってコンパイルし、評価することができます。

{% highlight csharp %}
Func<string, string> func = lambda.Compile();
func("foooo"); // "barOO" を返す
{% endhighlight %}

YACQ を LINQ クエリ内で利用できます。

{% highlight csharp %}
var query = Enumerable.Range(1, 100)
    .Yacq()
    .Where("(== (% it 3) 0)")
    .GroupBy("(/ it 10)")
    .Select("it.(Average)")
    .OrderByDescending("it");

// 下のコードと同等:
Enumerable.Range(1, 100)
    .Where(it => it % 3 == 0)
    .GroupBy(it => it / 10)
    .Select(it => it.Average())
    .OrderByDescending(it => it);
{% endhighlight %}

YACQ は式ツリー シリアライザを提供します。標準の環境では式ツリーはシリアライズすることができません。

{% highlight csharp %}
string xml = YacqServices.SaveText(lambda);
Expression lambda2 = YacqServices.LoadText(xml);
{% endhighlight %}

複雑な式ツリーも、YACQ によって拡張された Expression Trees システムを用いて簡単に構築できます。

{% highlight csharp %}
Expression expr2 = YacqExpression.TypeCandidate(typeof(Enumerable))
    .Method("Range", Expression.Constant(1), Expression.Constant(100))
    .Method("Reverse")
    .Method("Take", Expression.Constant(10))
    .Method("Sum")
    .Reduce();

// 下のコードと同等:
Expression.Call(typeof(Enumerable), "Sum", null,
    Expression.Call(typeof(Enumerable), "Take", new [] { typeof(int), },
        Expression.Call(typeof(Enumerable), "Reverse", new [] { typeof(int), },
            Expression.Call(typeof(Enumerable), "Range", null, Expression.Constant(1), Expression.Constant(100))
        ), Expression.Constant(10)));
{% endhighlight %}

YACQ は他のアプリケーションに組み込むことを主な利用方法としていますが、同梱のコンパイラ フロントエンドおよび REPL 環境を用いることで、YACQ を単体で用いることもできます。

## 導入

YACQ を利用するには、`Yacq.dll` をアプリケーションから参照します。システムを入手するには以下の方法が存在します:

* **[NuGet パッケージをインストールする](http://nuget.org/packages/Yacq)**: ライブラリのみが必要な場合、NuGet 経由で取得・インストールできます。<br />
```
PM> Install-Package Yacq
```
* **[バイナリ アーカイブをダウンロードする](http://yacq.net/download)**: すべてのライブラリ、実行可能ファイル、およびドキュメントを含んだバイナリ アーカイブも入手可能です。コンパイラ フロントエンドもしくは REPL 環境が必要な場合、これをダウンロードしてください。
* **[ソースコードをダウンロードする](https://github.com/takeshik/yacq)**: GitHub からソース コードを入手できます。
    * **リポジトリを clone する**: YACQ のリポジトリを clone できます。<br />
    ```
    % git clone git://github.com/takeshik/yacq.git
    ```
    * **スナップショットをダウンロードする**: リポジトリのスナップショット アーカイブも利用可能です ([.tar.gz](https://github.com/takeshik/yacq/tarball/master) | [.zip](https://github.com/takeshik/yacq/zipball/master))。

## 必要要件

* ターゲット フレームワーク:
    * .NET Framework 4 以降
    * Silverlight 5
    * Mono 2.10 以降
* 依存ライブラリ
    * Reactive Extensions ([Rx-Main](http://nuget.org/packages/Rx-Main), [Rx-Providers](http://nuget.org/packages/Rx-Providers))
    * Interactive Extensions ([Ix\_Experimental-Main](http://nuget.org/packages/Ix_Experimental-Main), [Ix_Experimental-Providers](http://nuget.org/packages/Ix_Experimental-Providers))
    * [Parseq](http://nuget.org/packages/Parseq)

## ライセンス

Copyright &copy; 2011-2013 Takeshi KIRIYA (aka takeshik) <small>([Web](http://www.takeshik.org/) | [Mail](mailto:takeshik_AT_yacq_DOT_net) | [GitHub](https://github.com/takeshik) | [Twitter](https://twitter.com/takeshik)</small>), All rights reserved.

YACQ は[フリー ソフトウェア](http://www.gnu.org/philosophy/free-sw.html)です。ソースコード、バイナリ、およびその他の全てのリソースは [MIT ライセンス](https://github.com/takeshik/yacq/blob/master/LICENSE.txt)の下で公開されています。
<!-- vim:set ft=markdown fenc=utf-8 ts=4 sw=4 sts=4 et: -->
