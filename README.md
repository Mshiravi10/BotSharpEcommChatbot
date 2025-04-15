# 🧠 SemanticKernel LLaMA3 Agent

یک ایجنت هوشمند ساخته‌شده با [Microsoft Semantic Kernel](https://github.com/microsoft/semantic-kernel) که به کمک یک پروکسی سفارشی به مدل‌های LLaMA3 از طریق [Ollama](https://ollama.com/) + [Groq](https://groq.com/) متصل می‌شود و قابلیت فراخوانی خودکار توابع داخلی (Tools / Functions) را داراست.

---

## 📁 ساختار پروژه

```
SemanticKernel-Llama3-Agent/
│
├── ConsoleAgent/        # ایجنت کنسولی که ورودی کاربر را گرفته و پاسخ هوشمند برمی‌گرداند
│
├── ProxyApi/            # پروکسی برای تطبیق API با فرمت OpenAI مورد انتظار Semantic Kernel
│
└── README.md            # همین فایل!
```

---

## 🚀 اجرا

### 1. اجرای Proxy API

پروکسی باید روی پورت `5111` اجرا شود تا Semantic Kernel به آن متصل شود:

```bash
cd ProxyApi
dotnet run
```

> اطمینان حاصل کنید که **Ollama** با مدل `llama3` در حال اجرا است:
```bash
ollama run llama3
```

---

### 2. اجرای Console Agent

در ترمینال جدید:

```bash
cd ConsoleAgent
dotnet run
```

ایجنت آماده پاسخ‌گویی است:

```
🛒 Agent is ready. Ask a question:
```

---

## 💡 مثال‌های پرامپت

```
You: What is the price of Coat?
🤖 Agent: Navy formal coat, $750

You: Tell me about the Phone
🤖 Agent: XY model Z5 phone with 128GB, $8.5 million

You: How much are the Shoes?
🤖 Agent: Black sports shoes, $350
```

---

## 🛠 تکنولوژی‌ها

- [.NET 8](https://dotnet.microsoft.com/)
- Semantic Kernel
- Ollama (with LLaMA3)
- Groq (local proxy endpoint)
- JSON Tool Calling
- AutoInvokeKernelFunctions
- Custom Plugin (`ProductFunctions`)

---

## 📦 توابع داخلی (Plugin)

در `ProductFunctions.cs` یک تابع تعریف شده که ایجنت می‌تواند به صورت **اتوماتیک** آن را هنگام نیاز فراخوانی کند:

```csharp
[KernelFunction, Description("Retrieve information of a store product using its name")]
public string GetProductInfo(string productName)
```

---

## 🌐 چرا پروکسی؟

Semantic Kernel انتظار دارد که مدل‌ها ساختار پاسخ‌دهی مشابه OpenAI داشته باشند. ما یک پروکسی ساده نوشتیم تا بین SK و Ollama قرار گیرد و این ساختار را تطبیق دهد.

---

## 🤝 مشارکت

اگر دوست داری این پروژه رو گسترش بدی — مثل اضافه کردن توابع بیشتر، اتصال به دیتابیس، یا حتی مدل‌های دیگه — خوشحال می‌شم Pull Request بدی یا Issue باز کنی ✨

---

## 🧑‍💻 نویسنده

Created with ❤️ by MohammadAmin Shiravi 
[GitHub Profile](https://github.com/MShiravi10)

---
