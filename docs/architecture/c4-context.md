# System Context: Mars Rover

The operator steers a rover on a 100×100 wrapping grid. The whole system runs as local processes, so there are no external systems.

```mermaid
C4Context
  title System Context diagram for Mars Rover

  Person(operator, "Operator", "Mission control on Earth. Sends command sequences and reads rover reports.")
  System(marsRover, "Mars Rover", "Translates command sequences into rover moves on a wrapping grid that has obstacles.")

  Rel(operator, marsRover, "Sends command sequences to", "CLI args")
  Rel(marsRover, operator, "Reports position, heading and obstacles to", "stdout, plain text")
```

See [c4-containers.md](c4-containers.md) for the processes inside the system. This view is the same for the C# and F# implementations.
