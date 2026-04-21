EXTERNAL sellItem()
EXTERNAL getItemName()
EXTERNAL getItemPrice()

VAR sucess = false

Hey pal, I got something for ya, it's a {getItemName()} for ${getItemPrice()}, are you interested? # speaker: Seller

    + [I am! Give me one!]
        {sellItem()}
        -> DONE
    + [No man, not interested.]
        Your loss.
        -> DONE

-> END