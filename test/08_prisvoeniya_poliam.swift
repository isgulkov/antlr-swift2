
class Priv
{
    var s : String = "";
}

var p : Priv = Priv()

repeat {
    p.s = p.s + "x";
} while p.s < "xxxxxxxxxx"  ;

print(p.s, " (10 xs expected)")
