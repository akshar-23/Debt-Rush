EXTERNAL openGate()

Hey! I know you. You owe me money. I don't let you pass unless you pay $300. # speaker: Gatekeeper

    + [Pay $300]
        ~ openGate()
        -> DONE
    + [Turn back]
        Come back when you have the money. # speaker: Gatekeeper
        -> DONE

-> END