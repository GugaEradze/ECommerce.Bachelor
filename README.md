# 🛒 E-Commerce Microservices Architecture

მოცემული პროექტი წარმოადგენს ელექტრონული კომერციის პლატფორმის მასშტაბურ, მოვლენებზე ორიენტირებულ (Event-Driven) მიკროსერვისულ არქიტექტურას, რომელიც აგებულია **.NET 8.0** Framework-ის ბაზაზე. პროექტში განაწილებული ტრანზაქციების მართვისა და მონაცემთა მთლიანობის პრობლემა გადაჭრილია **Saga Orchestration Pattern**-ის გამოყენებით, ხოლო მონაცემთა სწრაფი ასახვისთვის რეალურ დროში ინტეგრირებულია **CQRS (Read-Model)** მიდგომა.

## 🚀 ტექნოლოგიური სტეკი (Tech Stack)
* **Framework:** .NET 8.0 (C#)
* **არქიტექტურული შაბლონები:** Microservices, Clean Architecture, Event-Driven, Saga Orchestration, CQRS
* **Message Broker:** RabbitMQ (MassTransit აბსტრაქციით)
* **State Machine:** MassTransit Saga State Machine
* **მონაცემთა ბაზები:**
  * **PostgreSQL (Marten-ის დახმარებით):** გამოიყენება `Order.API`-ში შეკვეთების საწყისი მონაცემებისა და Saga State-ის (მიმდინარე სტატუსების) პერსისტენციისთვის.
  * **MongoDB:** გამოიყენება `Notification.API`-ში როგორც CQRS Read-Model, მოვლენების (Event Logs) ქრონოლოგიური და სწრაფი წაკითხვისთვის.
* **API Gateway:** YARP (Yet Another Reverse Proxy)
* **მომხმარებლის ინტერფეისი (UI):** Blazor WebAssembly (WASM)

## 🛠️ სისტემის ლოკალურ გარემოში გამართვის ინსტრუქცია

### 1. წინაპირობები (Prerequisites)
სისტემის გასაშვებად თქვენს კომპიუტერზე დაინსტალირებული უნდა იყოს:
* .NET 8.0 SDK
* Docker Desktop

### 2. ინფრასტრუქტურის მომზადება (Docker კონტეინერები)
გახსენით ტერმინალი (Command Prompt ან PowerShell) ადმინისტრატორის უფლებებით და მიმდევრობით გაუშვით შემდეგი ბრძანებები საჭირო ბაზებისა და ბროკერის ასაწევად:

```bash
# RabbitMQ Message Broker-ის გაშვება (მენეჯმენტის პანელით)
docker run -d --name ecommerce-rabbitmq -p 5672:5672 -p 15672:15672 rabbitmq:3-management

# MongoDB NoSQL მონაცემთა ბაზის გაშვება (Read-Model-ისთვის)
docker run -d --name ecommerce-mongodb -p 27017:27017 mongo:latest

# PostgreSQL მონაცემთა ბაზის გაშვება (OrderDB და Saga State-ისთვის)
docker run -d --name ecommerce-postgres -e POSTGRES_USER=postgres -e POSTGRES_PASSWORD=postgres -e POSTGRES_DB=OrderDatabase -p 5432:5432 postgres:latest
```

### 3. მიკროსერვისების გაშვება Visual Studio-ში
1. გახსენით პროექტის მთავარი სოლუშენ ფაილი (`ECommerce.Microservices.sln`) Visual Studio-ში.
2. Solution Explorer-ში დააწკაპუნეთ მაუსის მარჯვენა ღილაკით მთავარ Solution-ზე და აირჩიეთ **Configure Startup Projects...**
3. მონიშნეთ ოფცია **Multiple startup projects**.
4. ჩამოთვლილ პროექტებში შემდეგ სერვისებს Action სვეტში აურჩიეთ **Start**:
   * `Order.API`
   * `Inventory.API`
   * `Payment.API`
   * `Notification.API`
   * `YarpGateway`
   * `ECommerce.Web`
5. დააჭირეთ **Apply**, შემდეგ **OK** და გაუშვით პროექტი ხელახლა **Start** (F5) ღილაკზე დაჭერით.

## 💡 ძირითადი ფუნქციონალი და სატესტო სცენარები (Test Cases)
პლატფორმის Blazor ინტერფეისიდან შესაძლებელია 4 სხვადასხვა ბიზნეს სცენარის სიმულაცია და ივენთების მუშაობის Live რეჟიმში დაკვირვება:

* **წარმატებული ქეისი (1200 GEL):** შეკვეთა რეგისტრირდება, საწყობში პროდუქტი წარმატებით რეზერვდება, გადახდა დასტურდება და შეკვეთა სრულდება (ყველა ქარდი მწვანეა).
* **გადახდის ჩავარდნის ტესტი (999 GEL):** საწყობში რეზერვი წარმატებით სრულდება, თუმცა `Payment.API` აუქმებს ტრანზაქციას. Saga მომენტალურად რთავს კომპენსაციურ მექანიზმს და საწყობს უგზავნის ბრძანებას ნაშთების აღსადგენად.
* **საწყობის ჩავარდნის ტესტი (უხარისხო პროდუქტი):** `Inventory.API` აფიქსირებს, რომ პროდუქტი (`bad_product`) არ არის მარაგში. Saga ადრეულ ეტაპზევე ბლოკავს პროცესს, გადახდის სერვისთან მოთხოვნა აღარ იგზავნება და ეკრანზე გამოდის საწყობის შეცდომის წითელი ქარდი.
* **კომპლექსური ჩავარდნა (უხარისხო პროდუქტი + 999 GEL):** ავლენს სისტემის მდგრადობას რთული Edge Case-ების დროს, სადაც Saga იცავს მონაცემთა აბსოლუტურ სინქრონიზაციას.
