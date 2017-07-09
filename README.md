Вариант 19

Подмножество языка Swift

Доступные операторы: =, -, ^, &&, ||, операторы сравнения, *, %, !, тернарники, print, repeat-while-цикл, break, class declaration

Доступные типы: String, Bool

Пример кода:

```
class MyClass {
var inner: String = "aaaaaa"
}

var str: String = "a"
var flag: Bool = true
var myClass: MyClass = MyClass()

repeat {
	str = str + "a";
	flag = !flag
} while (str < myClass.inner);

print(flag)
```

Ожидаемый вывод: `false`

## Отличия от варианта 17

#### Операторы

Без изменений: print, break

Убрано: for in-цикл, if-else

Добавлено: repeat-while-цикл, class declaration

#### Операции

Без изменений: =, -, ^, &&, ||, операторы сравнения, *, %, !, тернарники

#### Типы

Убрано: Float, Int

Добавлено: String, Bool

