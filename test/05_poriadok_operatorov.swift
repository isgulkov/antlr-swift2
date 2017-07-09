
var x : Bool = true || false && false;

print("true || false && false =", x, "(true expected)")

x = (true || false) && false;

print("(true || false) && false =", x, "(false expected)")
