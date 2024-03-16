<h1 align="center"><a href="https://github.com/LuJinLei97/CSE">The Custom Scheme Engine (cse)</a></h1>

### 简介

Custom Scheme Engine 是一个编程语言解析和执行引擎,主要功能按照配置文件解析语法树,然后对语法块执行,执行功能架构在CSE.CseCLR(高级编程语言提供的功能)上.这个引擎只要配置得当,能解析任何文件为编程语言,换句话说就是能实现任何编程语言的语法,外加用户自己新创造的编程语言.

### 示例讲解

如 简单的例子,配置字符"0"到"9"为"整数语句块", 字符"+"为"+字符", 当输入"111+222"时会调用CSE.CseCLR(C#)加法功能,参数为此"整数语句块"的参数索引,自动执行.

现如今只有简单的例子,因为需要花费精力进行CLR的功能编写.

### 发展设想

诚招合作伙伴,包括不限于投资者,宣传者,开发者,支持者,意向客户. 现阶段为价值投资,请仔细斟酌后 有意请联系 lujinlei97@qq.com

### 答疑,可能会有人问

1这么简陋的东西有必要存在吗?  就算有大用,干我什么事呢? 
答:世界上第一个轮子出现时估计也由于人这么想,然而事实证明轮子的在现代科技中的重要性是暂无可替代的,所以我的个人兴趣的项目不一定就不能流行!我之前看小说忘了是什么情节了,说释迦摩尼曾被人看轻,然后释迦摩尼就说(王子还是小龙不可轻看),因为王子是会成为王的,小龙也会长成神龙.未来可期.

2有合适的商业前景吗? 
答:我个人对于此类编程语言的项目是有极大兴趣的,何况这个编程语言基本是交由用户自己来配置,应该有较大的参与感,虽然不能代表广大程序员,但是也能体现一部分态度,个人认为有流行的可能性,衍生不知凡几的新编程语言.当流行到一定程度,成规模的组织应该也会参与进来.就是当有知名度时,客户想定制编程语言可以找我所在组织进行商业定制.

3看不出你这个引擎能做到衍生任何编程语言的技术?
不好说,如果有时候高级语言做不到的事,我基本不会扩展CLR功能,同时如果有语法解析方面的困扰,我来解决,如果出现严重问题解决不了,比如语法解析优先级错乱什么的,我将删除这个项目,最起码于用户有益无损

4功能还会继续开发吗?
如有反响,会.项目不温不火的,停止开发.
持续到有足够的动开发IDE支持,用CSE开发CLR的功能,等完成一套循环