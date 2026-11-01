# LinkedIn Technical Posts & Articles

Welcome! This repository serves as a personal archive for my technical posts and articles published on LinkedIn, primarily focusing on **C#, .NET, Software Architecture, and System Design**.

---

## Table of Contents

1. [01. Thread vs Task in C#](#01-thread-vs-task-in-c)
2. [02. [Upcoming] Post Title 2](#02-upcoming-post-title-2)
3. [03. [Upcoming] Post Title 3](#03-upcoming-post-title-3)
4. [04. [Upcoming] Post Title 4](#04-upcoming-post-title-4)
5. [05. [Upcoming] Post Title 5](#05-upcoming-post-title-5)
6. [06. [Upcoming] Post Title 6](#06-upcoming-post-title-6)

---

## Posts Archive

### 01. Thread vs Task in C#

> **Original LinkedIn Post:** [Read on LinkedIn](https://www.linkedin.com/feed/update/urn:li:activity:7518981442350002177/)  
>  **Tags:** `#CSharp` `#DotNet` `#AsyncAwait` `#Multithreading` `#SoftwareEngineering`

<details>
<summary><b> Click here to read full post content</b></summary>

<br>

Thread ولا Task كتير بيفتكروهم نفس الحاجة وهما مش كده

تخيل مطعم
الـ Thread هو الشيف نفسه موظف حقيقي له مكانه ومعداته ومكلف للمطعم
الـ Task هو الأوردر شغلانة محتاجة تتعمل ومش مهم مين هيعملها

عشان كده المطعم مبيوظّفش شيف جديد لكل أوردر بيعمل Thread Pool كام شيف وأوردرات كتير بتتوزع عليهم

Thread

وحدة تنفيذ على مستوى الـ OS
تقيل وليها Stack خاص بيها حوالي 1MB
أنت اللي بتتحكم فيها بإيدك بتبدأها وتستناها وتقفلها
لو عملت منها ألف واحدة الجهاز هيتخنق

Task

تمثيل لـ شغل هيخلص في المستقبل وممكن يرجّع قيمة
خفيف وبيشتغل فوق الـ Thread Pool
بيدعم async/await والـ Exception Handling والـ Cancellation والـ Continuations
أحيانًا مبيستخدمش Thread أصلًا وهو مستني مثلًا في طلب API أو قراءة من الـ Database الـ Thread بيتحرر لحد ما الرد يرجع

أستخدم أنهي واحد؟
Task في 95% من الحالات

عمليات I/O زي API Calls وDatabase وFiles مع async/await
شغل تقيل على الـ CPU مثل Task.Run
أكتر من عملية في نفس الوقت مثل Task.WhenAll

Thread في الحالات الخاصة

شغل طويل جدًا ومستمر Background Worker بيفضل شغال طول عمر التطبيق
محتاج تتحكم في الـ Priority
محتاج Thread مخصوص زي STA في بعض تطبيقات الـ Desktop

الخلاصة الـ Thread بيجاوب على سؤال مين هينفّذ والـ Task بيجاوب على إيه المطلوب يتعمل اشتغل بالـ Task ولو احتجت تنزل للـ Thread هتعرف وقتها

</details>

---

### 02. [Upcoming] Post Title 2

> 🔗 **LinkedIn Link:** _Coming Soon_  
> 📝 **Status:** Draft / Scheduled

---

### 03. [Upcoming] Post Title 3

> 🔗 **LinkedIn Link:** _Coming Soon_

---

### 04. [Upcoming] Post Title 4

> 🔗 **LinkedIn Link:** _Coming Soon_

---

### 05. [Upcoming] Post Title 5

> 🔗 **LinkedIn Link:** _Coming Soon_

---

### 06. [Upcoming] Post Title 6

> 🔗 **LinkedIn Link:** _Coming Soon_

---

### 07. [Upcoming] Post Title 6

> 🔗 **LinkedIn Link:** _Coming Soon_

---

## 👨‍💻 Connect with Me

- **LinkedIn:** [Omar Salem](https://www.linkedin.com/in/)
- **GitHub:** [omarsalem33](https://github.com/omarsalem33)
