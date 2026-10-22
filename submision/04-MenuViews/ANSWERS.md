# Assignment 04 — Answers

Student name: Omar Salem
Group / ID: Group 3

## Why Expected and Actual disagree

`Views++` is not an atomic operation. It consists of reading the current value, adding one, and writing the new value. When several threads do these steps at the same time, some updates overwrite others, so increments are lost. With 8 threads registering 50,000 views each, the expected count is 400,000, but my run before the fix produced 138,972.

## What you changed

I replaced `Views++` with `Interlocked.Increment(ref Views)`. `Interlocked.Increment` performs the increment as one atomic operation, so concurrent threads do not overwrite one another's updates. After the change, the actual count matched the expected count.

## When would you protect the update differently

I would use `lock` when the operation requires multiple steps or updates to multiple values that must remain consistent together, such as checking a condition and then updating related state. `Interlocked` is the better fit for this single counter increment.

## Console output before the change

```text
Expected: 400000
Actual:   138972
```

## Console output after the change

```text
Expected: 400000
Actual:   400000
```
