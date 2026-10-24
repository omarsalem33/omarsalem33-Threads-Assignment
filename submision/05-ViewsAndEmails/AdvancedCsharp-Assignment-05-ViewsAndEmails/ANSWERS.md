# Assignment 05 — Answers

Student name: Omar Salem  
Group / ID: Group 3

## Failures you observed

- Before the fix, the view count was incorrect or inconsistent. The expected count is 50,000, but my run showed: 
```text
Views expected=50000, actual=17211
Email jobs placed=20, unique sent=19, total sends=19
```
- The email workers did not always process the queue correctly. In my run.

## What you changed

- I replaced the shared `Dictionary` with a `ConcurrentDictionary` and used `AddOrUpdate` to update each product's view count safely.
- I replaced the shared `Queue` with a `ConcurrentQueue` and used `TryDequeue` so each worker safely attempts to take one email job at a time.
- I protected writes to the shared `SentEmails` list with a `lock`.

## Collection safety vs business-rule safety

A thread-safe collection protects its individual operations, but it does not automatically make a sequence of operations behave as one business operation. For example, checking a value and then updating it in separate steps can still cause incorrect results. I used `AddOrUpdate` for the view counter and `TryDequeue` for removing an email job so those updates happen safely.

## Console output after the fix

```text
Views expected=50000, actual=50000
Email jobs placed=20, unique sent=20, total sends=20
```