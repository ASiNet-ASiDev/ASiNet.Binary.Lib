# ASiNet.Binary.Lib
Обёртка над Span&lt;byte>

Данная библиотека предназначена для упращения работы 
со структурой Span. 

### INFO

BinaryBuffer можно использовать когда не хочется лишних аллокаций памяти на куче, а Span использовать не очень удобно.
BinaryBufferSerializer позволяет сериализовать большенство страндартных типов и массивы этих типов.

Поддерживаемые типы:

* Числовые: byte, sbyte, short, ushort, int, uint, long, ulong, float, double 
* Строки: string, char
* Перечисления: Enum : byte / sbyte / short / ushort / int / uint / long / ulong
* Остальные: DateTime, Guid

### Пример создания BinaryBuffer и записи в него простых значений:

Примечания:
* размер buffer должен быть больше или равен размеру самого большого записываемого/читаемого типа
* размер area должен быть больше или равен суммарному размеру всех записываемых данных.
* String при записи сразу записываеться в area минуя buffer, но при чтении String сначала считываеться в buffer из area!
* buffer используется только при чтении/записи обьектов с помощью методов расширения.
* buffer не используеться при чтении
```cs
using ASiNet.Binary.Lib;
using ASiNet.Binary.Lib.Expansons.Types;

var r = 0;
var w = 0;
// основное хранилище в котором хранятся все данные BinaryBuffer
var area = stackallock byte[512];
// буфер в который будут записаны данные, перед записью или чтением в/из area 
var buffer = stackallock byte[64];

var binbuf = new BinaryBuffer(area, buffer, ref r, ref w);

var a = 50;
var b = "Hello World!";

binbuf.Write(a)
binbuf.Write(b, Encoding.UTF8);

var result = binbuf.ToArray();

Concole.WriteLine(string.Join(' ', result));
```

### Пример чтения из BiharyBuffer простых значений:

```cs
using ASiNet.Binary.Lib;
using ASiNet.Binary.Lib.Expansons.Types;

var a = binbuf.ReadInt32();
var str = binbuf.ReadString(Encoding.UTF8);

Concole.WriteLine(a);
Concole.WriteLine(str);
```
## BinaryBufferSerializer

Алгоритм:
* Получает все свойства обьекта
* Сортирует их по названию без учёта регистра
* последовательно записывает/читает в/из BinaryBuffer

### Пример использования BinaryBufferSerializer : Сериализация

```cs
using ASiNet.Binary.Lib;
using ASiNet.Binary.Lib.Expansons.Types;

var binbuf = new BinaryBuffer(area, buffer, ref r, ref w);

var user = new User()
{ 
  Id = 10,
  FirstName = "Bob", 
  LastName = "None"
}

BinaryBufferSerializer.Serialize(user, binbuf);

var result = binbuf.ToArray();
Concole.WriteLine(string.Join(' ', result));

class User
{
  public int Id { get; set; }
  public string FirstName { get; set; }
  public string LastName { get; set; }
}
```

### Пример использования BinaryBufferSerializer : Весериализация

```cs
using ASiNet.Binary.Lib;
using ASiNet.Binary.Lib.Expansons.Types;



var user = BinaryBufferSerializer.Deserialize<User>(binbuf);

Concole.WriteLine(user.Id);
Concole.WriteLine(user.FirstName);
Concole.WriteLine(user.LastName);

class User
{
  public int Id { get; set; }
  public string FirstName { get; set; }
  public string LastName { get; set; }
}
```

