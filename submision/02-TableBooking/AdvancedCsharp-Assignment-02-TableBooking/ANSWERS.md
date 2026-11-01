# Assignment 02 — Answers

Student name: Omar Salem
Group / ID: Group 3 (Sunday / Wednesday 7:00 pm)

## What is wrong

- The availability check and the updates to `TablesLeft` and `Booked` were not protected as one operation. Multiple customer threads could read that a table was available at the same time, then decrement the counter separately. This race condition caused incorrect totals and allowed more bookings than the five available tables.
- `Thread.Sleep(20)` made the race easier to observe by leaving time between the availability check and the counter updates.
- Before the fix, the final results varied between runs:
  - Run 1: `Booked=15, TablesLeft=2`
  - Run 2: `Booked=19, TablesLeft=4`
  - Run 3: `Booked=7, TablesLeft=4`
- The correct final result with five tables is `Booked=5, TablesLeft=0`.

## How you fixed it

- I added a lock around the availability check and the counter updates so only one customer thread can perform the booking operation at a time. This prevents two threads from booking the same remaining table.
- I saved the remaining-table count while holding the lock, then printed that saved value after leaving the lock so each message matches that booking.
- After the fix, one observed run ended with `Booked=5, TablesLeft=0`. Run the program multiple times and add the actual final result for each run here.
