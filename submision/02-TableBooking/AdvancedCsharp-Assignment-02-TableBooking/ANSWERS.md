<<<<<<< HEAD
# Assignment 01 — Answers

Student name:Omar Salem  
Group / ID: Group 3 (sunday / wednesday 7:00pm)

## Problems with the current design

- The export is processed sequentially. Each order must finish loading, reading the customer file, and writing the invoice before the next order starts. As the number of orders grows, the total export time grows with the time spent waiting for every operation.

- The database and file operations use Thread.Sleep to simulate waiting. Thread.Sleep blocks the current thread during that time, so it cannot do other useful work.

- The baseline run took 6.0 seconds and wrote five invoices, from invoice-1001.txt to invoice-1005.txt.

## What you changed

- I replaced Thread.Sleep with Task.Delay and used asynchronous file read and write operations.

- I changed the export flow to start a task for each order, then await the tasks one by one. The steps for each invoice remain in the same order: load the order, read the customer profile, and write the invoice.

- After the changes, the run took 1.8 seconds and still produced all five invoices. The Wrote messages appeared in a different order because the invoice tasks were running concurrently.

# Console output before changes

[
Starting invoice export...
Customer files: D:.NET Course\Assignments\C# Assingments\omarsalem33-Threads-Assignment\submision\01-InvoiceExport\InvoiceExport\AdvancedCsharp-Assignment-01-InvoiceExport\InvoiceExport\bin\Debug\net9.0\Data
Invoice output: D:.NET Course\Assignments\C# Assingments\omarsalem33-Threads-Assignment\submision\01-InvoiceExport\InvoiceExport\AdvancedCsharp-Assignment-01-InvoiceExport\invoices
Wrote D:.NET Course\Assignments\C# Assingments\omarsalem33-Threads-Assignment\submision\01-InvoiceExport\InvoiceExport\AdvancedCsharp-Assignment-01-InvoiceExport\invoices\invoice-1001.txt
Wrote D:.NET Course\Assignments\C# Assingments\omarsalem33-Threads-Assignment\submision\01-InvoiceExport\InvoiceExport\AdvancedCsharp-Assignment-01-InvoiceExport\invoices\invoice-1002.txt
Wrote D:.NET Course\Assignments\C# Assingments\omarsalem33-Threads-Assignment\submision\01-InvoiceExport\InvoiceExport\AdvancedCsharp-Assignment-01-InvoiceExport\invoices\invoice-1003.txt
Wrote D:.NET Course\Assignments\C# Assingments\omarsalem33-Threads-Assignment\submision\01-InvoiceExport\InvoiceExport\AdvancedCsharp-Assignment-01-InvoiceExport\invoices\invoice-1004.txt
Wrote D:.NET Course\Assignments\C# Assingments\omarsalem33-Threads-Assignment\submision\01-InvoiceExport\InvoiceExport\AdvancedCsharp-Assignment-01-InvoiceExport\invoices\invoice-1005.txt
Export finished in 6.0 s
]

# Console output after changes

[
Starting invoice export...
Customer files: D:\.NET Course\Assignments\C# Assingments\omarsalem33-Threads-Assignment\submision\01-InvoiceExport\InvoiceExport\AdvancedCsharp-Assignment-01-InvoiceExport\InvoiceExport\bin\Debug\net9.0\Data
Invoice output: D:\.NET Course\Assignments\C# Assingments\omarsalem33-Threads-Assignment\submision\01-InvoiceExport\InvoiceExport\AdvancedCsharp-Assignment-01-InvoiceExport\invoices

Wrote D:\.NET Course\Assignments\C# Assingments\omarsalem33-Threads-Assignment\submision\01-InvoiceExport\InvoiceExport\AdvancedCsharp-Assignment-01-InvoiceExport\invoices\invoice-1002.txt
Wrote D:\.NET Course\Assignments\C# Assingments\omarsalem33-Threads-Assignment\submision\01-InvoiceExport\InvoiceExport\AdvancedCsharp-Assignment-01-InvoiceExport\invoices\invoice-1001.txt
Wrote D:\.NET Course\Assignments\C# Assingments\omarsalem33-Threads-Assignment\submision\01-InvoiceExport\InvoiceExport\AdvancedCsharp-Assignment-01-InvoiceExport\invoices\invoice-1004.txt
Wrote D:\.NET Course\Assignments\C# Assingments\omarsalem33-Threads-Assignment\submision\01-InvoiceExport\InvoiceExport\AdvancedCsharp-Assignment-01-InvoiceExport\invoices\invoice-1003.txt
Wrote D:\.NET Course\Assignments\C# Assingments\omarsalem33-Threads-Assignment\submision\01-InvoiceExport\InvoiceExport\AdvancedCsharp-Assignment-01-InvoiceExport\invoices\invoice-1005.txt
Export finished in 1.8 s
]
=======
# Assignment 02 — Answers

Student name:  
Group / ID:

## What is wrong

-

## How you fixed it

-
>>>>>>> 20ffe40 (Add Assignment starter (neutral names).)
