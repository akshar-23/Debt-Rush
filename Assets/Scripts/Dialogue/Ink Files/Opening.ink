EXTERNAL openGate()

You owe me $200,000. Pay it all back right now, or you're not going anywhere. # speaker: Gatekeeper
I don't have it..!! # speaker: You
Give me all you have then. # speaker: Gatekeeper

    + [Give all your money]
        Fine. Don't let me see you again. # speaker: Gatekeeper
        ~ openGate()
        Wait... # speaker: You
        What happened? # speaker: Partner
        He took everything we had. # speaker: Initiator
        ... # speaker: Partner
        He's gone… but more of them are coming. We have nothing left. # speaker: Initiator
        We need to get out of here. # speaker: Partner
        There's a gate on the south-east side of town. That's our way into the city. # speaker: Initiator
        Alright. See you at the gate. # speaker: Partner
        -> DONE
    + [Refuse to pay]
        Fine. I'll stand right here until you change your mind. # speaker: Gatekeeper
        -> DONE

-> END