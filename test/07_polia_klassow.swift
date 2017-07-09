
class Priv
{
    var s : String = "priv";
    var b : Bool = true;
}

class O_Priv
{
    var  priv  :  Priv = Priv() 
}

var op : O_Priv = O_Priv()

print(op.priv.s == "priv" ? "expected" : "unexpected")
print(op.priv.b ? "expected" : "unexpected") 
