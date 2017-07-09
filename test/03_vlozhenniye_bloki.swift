

repeat {
    print("first nesting level")

    repeat {
        print("second nesting level")
        
        repeat {
            print("third nesting level")
        } while(false);
    } while(false)
        
} while false;
