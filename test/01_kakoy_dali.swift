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
