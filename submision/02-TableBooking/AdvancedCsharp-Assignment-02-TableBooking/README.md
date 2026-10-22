<<<<<<< HEAD
<<<<<<< HEAD
# Assignment 01 — Invoice export

**SIMULATION — Software House & Academy**

SwiftBite runs a nightly invoice export. For each pending order the system loads the order from a database, reads the customer profile from a file, and writes an invoice file to disk.
=======
# Assignment 02 — Table booking

**SIMULATION — Software House & Academy**

SwiftBite has a limited number of tables left for Friday night. Many customers press Book at the same time.
>>>>>>> 20ffe40 (Add Assignment starter (neutral names).)

## Run

```bash
<<<<<<< HEAD
dotnet run --project InvoiceExport
```

## What you should do

1. Run the project and observe how long it takes and how the work is ordered.
2. In `ANSWERS.md`, describe the problems with this design when many orders must be exported.
3. Change the export pipeline so waiting on the database and files does not block the way it does now. Keep the same business flow: load order → read customer → write invoice.
4. Write what you changed in `ANSWERS.md`.
=======
dotnet run --project TableBooking
```

Run the program several times.

## What you should do

1. Run the project and look at the final numbers.
2. In `ANSWERS.md`, describe what is wrong and why.
3. Fix the booking code so the results stay correct when many customers book together.
4. Explain your fix in `ANSWERS.md`.
>>>>>>> 20ffe40 (Add Assignment starter (neutral names).)

## Submit

- Updated project code
- `ANSWERS.md`
<<<<<<< HEAD
=======
# omarsalem33-Threads-Assignment
Assignment repo for assignment/1-8 (Threads Assignment)
>>>>>>> 96e250aeab2fdb70143010d204f642b11bb60cb0
=======
>>>>>>> 20ffe40 (Add Assignment starter (neutral names).)
