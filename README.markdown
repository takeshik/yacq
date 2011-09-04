# YACQ - Yet Another Compilable Query

YACQ はソフトウェア組込型の .NET Framework 4 (Client Profile / Full) 以上をターゲットとするクエリ言語です。

> YACQ is a software-embeddable querying language for .NET Framework 4 (Client Profile / Full).

YACQ は [Expression Trees API](http://msdn.microsoft.com/ja-jp/library/bb397951.aspx) 上で動作し、
既存の機構と完全に協調して動作し、既存の機構と互換性を持って動作します。従って、LINQ to Objects は
もちろん、LINQ to SQL や LINQ to Entities、その他のクエリ プロバイダと共に利用することが可能です。

> YACQ is based on [Expression Trees API](http://msdn.microsoft.com/en-us/library/bb397951.aspx),
> suit with existing LINQ systems, such as LINQ to SQL, to Entities, to Objects of course, and
> so on.

## 類似ライブラリとの比較 (Comparison with Similar Libraries)

YACQ に類似した機能を提供するライブラリとして [DynamicQuery](http://weblogs.asp.net/scottgu/archive/2008/01/07/dynamic-linq-part-1-using-the-linq-dynamic-query-library.aspx)
が挙げられます。これは YACQ のインスピレーションの源泉であり、同じように Expression Trees API 上で
動作します。

> [DynamicQuery](http://weblogs.asp.net/scottgu/archive/2008/01/07/dynamic-linq-part-1-using-the-linq-dynamic-query-library.aspx)
> in MSDN sample code is similar to YACQ. YACQ is inspirated from it, and its mechanism is same:
> Expression Trees.

しかしながら、DynamicQuery は言語としての機能は制限されており、コーディングの幅は狭く、拡張性に
欠けたものとなっています。YACQ は DynamicQuery に無い機能を提供することで完全なクエリ環境を
提供します。

> However, DynamicQuery is limited its function, is not enough to write what you want to do, and
> its extendability is extremely poor. YACQ provides more and more features than DynamicQuery.

## 特徴 (Features)

* Expression Trees API に依拠したシステム。
    * Expression Trees API-based system
* 完全なメンバ呼び出し機能。型パラメタの明示、params 引数、拡張メソッドのサポートを含む。
    * Complete member referring support, includes specific type arguments, "params" arguments, extension methods.
* ファーストクラスな関数の定義。関数内での関数の定義も可能。静的スコープ (もどき？) のサポート。
    * First-class functions (lambda expressions), higher-order functions, (pseudo?-)closures.
* マクロ風機能。拡張可能な言語。
    * Macro-like feature, extensible language system.
* その他
    * etc.

## 利用方法 (Usage)

ソースコードを GitHub から取得できます: https://github.com/takeshik/yacq

> You can get source codes from GitHub: https://github.com/takeshik/yacq

ドキュメントは https://github.com/takeshik/yacq/wiki にあります。

> Documents are in https://github.com/takeshik/yacq/wiki .

## ライセンス (Licensing)

LICENSE.txt を参照してください。

> See LICENSE.txt .

lib/ ディレクトリ以下にあるライブラリは YACQ とは別の条件によってライセンスされます。

> Libraries in lib/ directory are licensed under the other condition.
