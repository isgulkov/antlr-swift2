
class Priv
{
    var s : String = "";
}

var p : Priv = Priv()

repeat {
    p = p + "x";
} while p < "xxxxxxxxxx"  ;

print(p, "(10 xs expected)")
