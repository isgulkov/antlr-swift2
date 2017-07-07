Вариант 17

Подмножество языка Swift

Доступные операторы: =, -, ^, &&, ||, операторы сравнения, *, %, !, тернарники,
print, for in-цикл, if-else, break

Доступные типы: Float, Int

Пример кода:

```
var float: Float = 1.0
var int: Int = 100

for _ in 1...int {
	float = float + 0.01
	if (float < 0.5)
		break
}

print(float);

if (float > 2.99) {
	print("Ne Norm")
} else {
	print("norm!")
}
```

Ожидаемый вывод:

```
2.0
norm!
```

// комментарий к заданию: обратите внимание, что при присвоении значения для
переменной типа Float в языке Swift не требуется явное указание типа "f" (1.0f)
