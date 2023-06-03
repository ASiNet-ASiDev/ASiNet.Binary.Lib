# ASiNet.Binary.Lib
Обёртка над Span&lt;byte>

Данная библиотека предназначена для упращения работы 
со структурой Span. 

ASiNet.Binary.Lib.BinaryBuffer

Содержит такие методы как:
Read и Write

Которые позволяют записать/прочитать данные в/из BinaryBuffer последовательно. 

Также благодаря пространствам имён
ASiNet.Binary.Lib.Expansons.Arrays и
ASiNet.Binary.Lib.Expansons.Types
Можно записывать/читать массивы и простые типы в/из BinaryBuffer

Пример создания BinaryBuffer и записи в него простых значений:

var area = stackallock byte[512];
var buffer = stackallock byte[64];

var binbuf = new BunaryBuffer(area, buffer);

var a = 50;
var b = "Hello World!";

binbuf.Write(a)
binbuf.Write(b, Encoding.UTF8);

var result = binbuf.ToArray();

Concole.WriteLine(string.Join(' ', result));
