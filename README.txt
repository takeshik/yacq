===============================================================================
Yacq - Yet Another Compilable Query
===============================================================================

Overview
--------
Yacq is a embedded language / interpreter for .NET, based on Expression Trees
API. Yacq only uses .NET standard run-time code generation framework, and not
based on DLR (Dynamic Language Runtime) -- so you can use this with a little
impact for your codes. Yacq codes are compiled to Expression Trees (Expression
objects in System.Linq.Expressions namespace). Yacq is suitable with the world
of LINQ.

Usage
-----
First, build whole code in Yacq.sln. Then you can use YacqRun, Yacq Runner.
This repository has sample codes, so you can run them with YacqRun.

path\to\yacqrun path\to\samples\01_hello.yacq

to run 01_hello.yacq. You can see how code compiles to add "-debug" option to
the tail of the command. If you run YacqRun with no arguments, YacqRun will
run as REPL mode.

Documentation
-------------
Documents are in https://github.com/takeshik/yacq/wiki .

Licensing
---------
Yacq is licensed under the MIT License.

Yacq uses, and the repository contains third-party assembly in lib/ directory.
They are not part of target of licensing.

Author
------
Yacq is created by Takeshi KIRIYA (aka takeshik).
You can get contact in Twitter (@takeshik), or GitHub
(https://github.com/takeshik).
