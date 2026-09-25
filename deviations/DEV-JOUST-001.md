# DEV-JOUST-001

- Departs from: RULE-JOUST-002, RULE-JOUST-003
- Reason: The practice and dragon workers run passes as fast as the processor allows, and each pass moves the lance and, in the contact frames, adds to the error totals. How far the lance moves per second and how large the totals grow depend on the speed of the machine. The rebuild runs one pass for each movie frame shown, catching up missed frames in order.
- Setting: None
- Default: mandatory
- Justification: The original has no pass rate of its own to reproduce, since every machine and every DOSBox cycle setting gives a different one. One pass per movie frame keeps the arithmetic of each pass and the frames the contact window covers, and removes only the dependency on host speed.
- Dropped: no
