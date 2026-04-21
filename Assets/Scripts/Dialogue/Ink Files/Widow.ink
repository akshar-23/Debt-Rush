EXTERNAL sellItem()
EXTERNAL getItemName()
EXTERNAL getItemPrice()

VAR sucess = false

My husband just passed away... I'm selling his weapon collection, would you buy some {getItemName()} for ${getItemPrice()}? # speaker: Widow

    + [I'll buy it. Thank you!]
        {sellItem()}
        -> DONE
    + [Sorry for your loss.]
        Thank you... I appreciate it.
        -> DONE

-> END