Процесс сигнатурного анализа кода в нашем проекте [PT Application Inspector](http://www.ptsecurity.ru/products/ai/) разбит на следующие этапы:
1. парсинг в зависимое от языка представление (abstract syntax tree, AST);
2. преобразование AST в независимый от языка унифицированный формат;
3. непосредственное сопоставление с шаблонами, описанными на DSL.<br>

О первых двух этапах было рассказано в предыдущих статьях «[Теория и практика парсинга исходников с помощью ANTLR и Roslyn](https://habrahabr.ru/company/pt/blog/210772/)» и «[Обработка древовидных структур и унифицированное AST](https://habrahabr.ru/company/pt/blog/210060/)». Данная статья посвящена третьему этапу, а именно: различным способам описания шаблонов, разработке специализированного языка (DSL) для их описания, а также примерам шаблонов на этом языке.<br>

[<img align="right" src="https://habrastorage.org/getpro/habr/post_images/870/acf/bd6/870acfbd69a1c6130011251fe304c75b.png"/>](https://habrahabr.ru/company/pt/blog/300946/)

## Содержание
* [Способы описания шаблонов](#patterns-declaration-types)
    * [Hardcoded](#hardcoded)
    * [JSON, XML или другой язык разметки](#json-xml)
* [Собственный язык описания шаблонов](#dsl)
    * [Целесообразность](#practicability)
    * [Синтаксис](#dsl-syntax)
    * [Примеры шаблонов](#pattern-samples)
        * [Жестко заданный пароль (все языки)](#hardcoded-password)
        * [Слабый генератор случайных чисел (C#, Java)](#weak-random-number-generator)
        * [Утечка отладочной информации (PHP)](#debug-information-leak)
        * [Небезопасное SSL соединение (Java)](#insecure-ssl-connection)
        * [Пароль в комментарии (все языки)](#password-in-comment)
        * [SQL-инъекция (C#, Java, PHP)](#sql-injection)
        * [Куки без атрибута безопасности (PHP)](#cookie-without-secure-attribute)
        * [Пустой блок обработки исключения (все языки)](#empty-try-catch-block)
        * [Небезопасный куки (Java)](#insecure-cookie)
        * [Перехват незакрытого курсора (PL/SQL, T-SQL)](#cursor-snarfing)
        * [Чрезмерно расширенные полномочия (PL/SQL, T-SQL)](#overly-broad-grant)
* [Заключение](#conclusion)

<cut>

<anchor>patterns-declaration-types</anchor>
## Способы описания шаблонов

* Заданные в коде шаблоны (hardcoded);
* JSON, XML или другой язык разметки;
* DSL, предметно-ориентированный язык.

<anchor>hardcoded</anchor>
### Hardcoded<br>

Шаблоны можно записывать вручную прямо в коде. Для этого не требуется разрабатывать парсер. Этот способ не подходит для не разработчиков, зато может использоваться при написании юнит-тестов. Также для записи новых шаблонов требуется перекомпиляция всей программы.<br>

<anchor>json-xml</anchor>
### JSON, XML или другой язык разметки<br>

Части сопоставляемого AST можно непосредственно сохранять и загружать из JSON или других форматов. При таком подходе шаблоны можно будет загружать извне, однако синтаксис будет громоздким и не подойдет для редактирования пользователем. Однако этот способ можно использовать для сериализации древовидных структур. (О способах сериализации древовидных структур в .NET и их обходе будет рассказано в следующей статье.)<br>

<anchor>dsl</anchor>
## Собственный язык описания шаблонов, DSL<br>

Третий подход заключается в разработке специального предметно-ориентированного языка, который можно было бы легко редактировать, который был бы лаконичным, но при этом обладал достаточной выразительной мощностью для описания существующих и будущих шаблонов. Недостатком такого подхода является необходимость разработки синтаксиса и парсера для него.<br>

<anchor>practicability</anchor>
### Целесообразность<br>

Как уже говорилось в первой статье, не все шаблоны можно просто и удобно описать с помощью регулярных выражений. DSL является смесью регулярных выражений и частоиспользуемых конструкций из популярных языков программирования. Кроме того, данный язык предназначается для конкретной предметной области и не предполагается, что он будет использоваться в качестве какого-либо стандарта.<br>

<anchor>dsl-syntax</anchor>
### Синтаксис<br>

Во второй статье цикла мы говорили, что основными конструкциями в императивных языках программирования являются примитивные типы (literals), выражения (expressions) и инструкции (statements). При разработке DSL мы сделали все так же. Примеры выражений:
* `expr(args);` вызов метода;
* `Id expr = expr`; инициализация переменной;
* `expr + expr`; конкатенация;
* `new Id(args)`; создание объекта;
* `expr[expr]`; обращение по индексу или ключу.

Инструкции создаются путем добавления точки с запятой в конец выражения.

Литералами же являются примитивные типы, такие как:
* Id; идентификатор;
* String; строка, обособляется двойными кавычками;
* Int; целое число;
* Bool; булево значение.<br>

Эти литералы позволяют описывать простые конструкции, однако с помощью них нельзя, например, описывать диапазоны чисел, регулярные выражения. Для поддержки таких более сложных случаев были введены расширенные конструкции (PatternStatement, PatternExpression, PatternLiteral). Такие конструкции обособляются специальными скобками `<[` и `]>`. Подобный синтаксис был заимствован из языка [Nemerle](https://ru.wikipedia.org/wiki/Nemerle) (в нем такие скобки используются для квазицитирования, т. е. для преобразования кода внутри них в AST Nemerle).<br>

Примеры поддерживаемых расширенных конструкций представлены в списке ниже. Для некоторых также конструкций предусмотрен синтаксический сахар, позволяющий сократить запись: 
* `<[]>`; оператор расширенного выражения (например, `<[md5|sha1]>` или `<[0..2048]>`);
* `#` или `<[expr]>`; любое Expression;
* `...` или `<[args]>`; произвольное количество любых аргументов;
* `(expr.)?expr`; эквивалентно `expr.expr` или просто `expr`;
* `<[~]>expr` — отрицание выражения;
* `expr (<[||]> expr)*` — объединение нескольких выражений (ИЛИ);
* `Comment: "regex"` — поиск по комментариям.<br>

<anchor>pattern-samples</anchor>
### Примеры шаблонов<br>

<anchor>hardcoded-password</anchor>
#### Жестко заданный пароль (все языки)<br>
`(#.)?<[(?i)password(?-i)]> = <["\w*"]>`
* `#`; любое выражение, может отсутствовать;
* `<[(?i)password(?-i)]>`; регулярное выражение для типов Id, может быть записано в любом регистре;
* `<["\w*"]>`; регулярное выражение для типов String;
<br>

<anchor>weak-random-number-generator</anchor>
#### Слабый генератор случайных чисел (C#, Java)<br>
```new Random(...)```<br>

Уязвимость заключается в использовании небезопасного алгоритма генерации случайных чисел. Пока что такие случаи отслеживаются с помощью поиска конструктора стандартного класса `Random`.<br>

<anchor>debug-information-leak</anchor>
#### Утечка отладочной информации (PHP)<br>
`Configure.<[(?i)^write$]>("debug", <[1..9]>)`<br>
* `<[(?i)^write$]>`; регулярное выражение для Id, не зависит от регистра и определяет только точные вхождения;  
* `("debug", <[1..9]>)`; аргументы функции;
* `<[1..9]>`; диапазон целых чисел от 1 до 9.

<anchor>insecure-ssl-connection</anchor>
#### Небезопасное SSL-соединение (Java)<br>
`new AllowAllHostnameVerifier(...) <[||]> SSLSocketFactory.ALLOW_ALL_HOSTNAME_VERIFIER`.<br>

Использование "логического ИЛИ" для синтаксических конструкций. При использовании этого шаблона будет находится код как для левой части (конструктор `new AllowAllHostnameVerifier(...)`), так и для правой (использование константы `SSLSocketFactory.ALLOW_ALL_HOSTNAME_VERIFIER`).<br>

<anchor>password-in-comment</anchor>
#### Пароль в комментарии (все языки)<br>
`Comment: <[ "(?i)password(?-i)\s*\=" ]>`<br>

Поиск комментариев в исходном коде. Причем в C#, Java, PHP, как известно, однострочные комментарии начинаются с двойного слэша `//`, а в SQL-подобных языках - с двойного дефиса `--`.<br>

<anchor>sql-injection</anchor>
#### SQL-инъекция (C#, Java, PHP)<br>
`<["(?i)select\s\w*"]> + <[~"\w*"]>`<br>

Простая SQL инъекция: конкатенация любой строки, начинающейся с select и не строковым выражением в правой части.<br>

<anchor>cookie-without-secure-attribute</anchor>
#### Куки без атрибута безопасности (PHP)<br>
`session_set_cookie_params(#,#,#)`<br>

Установка куки без флага защищенности, который задается в четвертом аргументе.<br>

<anchor>empty-try-catch-block</anchor>
#### Пустой блок обработки исключения (все языки)
`try {...} catch { }`<br>

Пустой блок обработки исключений. В C# будет находиться такой код:<br>
```CSharp
try
{
}
catch
{
}
```

В T-SQL такой:<br>
```
BEGIN TRY
    SELECT 1/0 AS DivideByZero
END TRY
BEGIN CATCH
END CATCH
```

А в PL/SQL такой:<br>
```
PROCEDURE empty_default_exception_handler IS
BEGIN
    INSERT INTO table1 VALUES(1, 2, 3, 4);
    COMMIT;
  EXCEPTION
    WHEN OTHERS THEN NULL;
END;
```

<anchor>insecure-cookie</anchor>
#### Небезопасный куки (Java)
```
Cookie <[@cookie]> = new Cookie(...);
...
<[~]><[@cookie]>.setSecure(true);
...
response.addCookie(<[@cookie]>);
```
<br>

Добавление куки без установленного флага защищенности. Несмотря на то что данный шаблон правильнее реализовывать в taint-анализе, его удалось реализовать и с помощью более примитивного алгоритма сопоставления. В нем используется прикрепленная переменная `@cookie` (по аналогии с [обратными ссылками](https://ru.wikipedia.org/wiki/%D0%A0%D0%B5%D0%B3%D1%83%D0%BB%D1%8F%D1%80%D0%BD%D1%8B%D0%B5_%D0%B2%D1%8B%D1%80%D0%B0%D0%B6%D0%B5%D0%BD%D0%B8%D1%8F#.D0.93.D1.80.D1.83.D0.BF.D0.BF.D0.B8.D1.80.D0.BE.D0.B2.D0.BA.D0.B0) в регулярных выражениях), отрицание выражения и произвольное количество утверждений.<br>

<anchor>cursor-snarfing</anchor>
#### Перехват незакрытого курсора (PL/SQL, T-SQL)

##### PL/SQL
```
<[@cursor]> = DBMS_SQL.OPEN_CURSOR;
...
<[~]>DBMS_SQL.CLOSE_CURSOR(<[@cursor]>);
```

##### T-SQL
```
declare_cursor(<[@cursor]>);
...
<[~]>deallocate(<[@cursor]>);
```

Незакрытый курсор потенциально может эксплуатироваться менее привилегированным пользователем. Кроме того, он может приводить к нестабильности системы и возможности создания DoS злоумышленником.

В T-SQL будет находиться такой код:
```
DECLARE Employee_Cursor CURSOR FOR
SELECT EmployeeID, Title FROM AdventureWorks2012.HumanResources.Employee;
OPEN Employee_Cursor;
FETCH NEXT FROM Employee_Cursor;
WHILE @@FETCH_STATUS = 0
   BEGIN
      FETCH NEXT FROM Employee_Cursor;
   END;
--DEALLOCATE Employee_Cursor; is missing
GO
```

<anchor>overly-broad-grant</anchor>
#### Чрезмерно расширенные полномочия (PL/SQL, T-SQL)
`grant_all(...)`

Данный недостаток чреват тем, что пользователю может быть выдано больше привилегий, чем это требуется. Несмотря на то, что по `grant all`, является запросом, при сопоставлении он преобразуется в вызов функции, потому что в алгоритме нет понятия «Запрос».

Будет находиться такой код:
`GRANT ALL ON employees TO john_doe;`

<anchor>conclusion</anchor>
## Заключение
Для демонстрации работы нашего модуля мы подготовили видео, в котором показан процесс поиска определенных шаблонов в коде на различных языках программирования (C#, Java, PHP) в нашем продукте PT Application Inspector. Демонстрируется также корректная обработка синтаксических ошибок, которая была затронута в [первой](https://habrahabr.ru/company/pt/blog/210772/) статье нашей серии.

<video>https://youtu.be/NgCebmEvpJQ</video>
<br>

В следующих статьях мы расскажем:
* о сравнении, сериализации и обходе древовидных структур в .NET;
* построении CFG, DFG и taint-анализе.