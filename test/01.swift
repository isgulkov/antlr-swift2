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
