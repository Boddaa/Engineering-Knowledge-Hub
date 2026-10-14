# 📘 منصة إدارة المعرفة الهندسية — Engineering Knowledge Hub
### التقرير والتوثيق الشامل للمشروع (Executive Project Summary)

---

## 🌟 1. نظرة عامة على المشروع (Project Overview)

**Engineering Knowledge Hub** هي منصة مؤسسية داخلية لإدارة المعرفة التقنية وحوكمة الأنظمة البرمجية، مصممة لحفظ الخبرات الهندسية، تسريع حل الحوادث البرمجية (Incidents)، وتوفير مساعد ذكاء اصطناعي توليدي محلي (Local RAG) بدون أي تسريب للبيانات خارج الشبكة الداخلية (**Zero Cloud Egress**).

تم بناء المشروع بالكامل وفق معايير **Clean Architecture** الصارمة، مع فصل كامل بين طبقات النظام واعتمادية بنسبة 100% في الاختبارات الآلية.

---

## 🏗️ 2. المعمارية التقنية ومكونات النظام (Tech Stack & Architecture)

### 🔹 الباك إند (Backend)
* **الإطار واللغة**: .NET 10 / C# 14 / ASP.NET Core Web API.
* **المعمارية**: Clean Architecture (4 طبقات مستقلة):
  1. `EngineeringKnowledgeHub.Domain`: الكيانات والقواعد الأساسية (خالية من أي مكتبات خارجية).
  2. `EngineeringKnowledgeHub.Application`: واجهات العمليات، الـ DTOs، وقواعد التحقق والخدمات.
  3. `EngineeringKnowledgeHub.Infrastructure`: تطبيق EF Core 10، قواعد بيانات SQL Server، ومحولات الذكاء الاصطناعي.
  4. `EngineeringKnowledgeHub.Api`: 13 وحدة تحكم REST API، توثيق OpenAPI/Swagger، معالجة استثناءات شاملة (RFC 7807)، ونظام حماية الصلاحيات (RBAC).
* **الذكاء الاصطناعي المحلي (Local AI)**:
  - تشغيل نماذج الـ LLM والتضمين المتجهي محلياً عبر **Ollama** (`llama3`, `nomic-embed-text`).
  - فحص أبعاد المتجهات تلقائياً مع وجود بدائل حسابية في حال عدم توفر الخادم المحلي.
* **الأمان والحوكمة**:
  - سياسة حوكمة التدقيق `AuditGovernancePolicy` تقصر الوصول لسجلات الرقابة على دوري `Admin` و `Auditor`.

### 🔹 الفرونت إند (Frontend)
* **الإطار واللغة**: React 19 / TypeScript 5.7 / Vite 6 / React Router 7.
* **نظام التصميم**: Pure CSS Modules + Custom Design Tokens (`tokens.css`) بدون أي مكتبات UI خارجية لضمان السرعة والتحكم الكامل.
* **استقرار الواجهة والأداء**:
  - حماية كاملة من الـ Race Conditions باستخدام `AbortController` ومطابقة الـ `requestId`.
  - تقسيم عرض البيانات (Client-side Bounded Pagination) لمنع تجمد المتصفح مع السجلات الكبيرة.

---

## 🚀 3. سجل الإنجازات وما تم تنفيذه بالكامل

### أولاً: مراحل الباك إند الأساسية (Backend Phases 0 → 10) ✅
1. **Phase 0: Foundation**: إرساء Clean Architecture وإعداد EF Core 10 و SQL Server.
2. **Phase 1: Identity & Domain**: بناء كيانات المستخدمين، الهوية، ونظام الصلاحيات.
3. **Phase 2: Knowledge Core**: إدارة المقالات التقنية، نظام الإصدارات (`KnowledgeVersions`)، ودورة حياة المقال.
4. **Phase 3: Search Engine**: محرك البحث النصي، الـ Tokenization، وفلاتر المنتجات والتصنيفات.
5. **Phase 4: Incidents**: تسجيل الحوادث الهندسية وتتبعها، وتحويل الحادث المحلول تلقائياً إلى مقال معرفي.
6. **Phase 5: Documents & Sources**: استيراد المستندات ومعالجتها واستخراج النصوص وتتبع المصادر.
7. **Phase 6: Governance**: دورة مراجعة المقالات: الإرسال، موافقة أو رفض المراجعين مع ذكر الأسباب، والأرشفة.
8. **Phase 7: Semantic Search**: البحث الدلالي بالمتجهات وحساب نسبة التشابه (Cosine Similarity).
9. **Phase 8: RAG Assistant**: مساعد الذكاء الاصطناعي التوليدي مع كروت الأدلة وإسناد المصادر.
10. **Phase 9: Online Enrichment**: استيراد وتحديث توثيق التقنيات الخارجية.
11. **Phase 10: Local AI**: تشغيل Ollama محلياً ومراقبة صحة وجاهزية النماذج (`/api/ai/local/health`).
12. **REST API Coverage**: بناء **13 Controller** وتغطيتها باختبارات تكاملية شاملة.

### ثانياً: مراحل واجهة المستخدم (Frontend Phases F0 → F10) ✅
1. **F0 (Shell & Design System)**: الهيكل العام، القائمة الجانبية التفاعلية، ونظام الألوان والأبعاد.
2. **F1 & F2 (Knowledge Base)**: تصفح المقالات، قراءة المقال بتنسيق Markdown متقدم، وعرض بيانات الكاتب والتصنيف.
3. **F3 (Knowledge Authoring)**: محرر المقالات مع المعاينة الفورية وحفظ المسودات والإرسال للمراجعة.
4. **F4 (Governance UI)**: شاشة مراجعة المقالات، نوافذ القبول والرفض مع إبداء الأسباب، وسجل المقارنة بين الإصدارات.
5. **F5 (Search & Discovery)**: البحث اللحظي، الفلترة حسب المنتجات والأنظمة، وتظليل الكلمات المطابقة.
6. **F6 (Incidents & Feedback)**: إدارة الحوادث الهندسية، أداة تحويل الحادث لمقال، ونظام تقييمات المقالات.
7. **F7 (Taxonomies & Sources)**: شاشات إدارة المنتجات، التقنيات، الفئات، ومصادر المعرفة الخارجية.
8. **F8 (Documents Workspace)**: استعراض المستندات المستوردة، ونافذة اقتراح مقال مع تحقق إلزامي صارم لـ `KnowledgeTypeId`.
9. **F9 (AI Knowledge Assistant & RAG)**: شاشة المحادثة الذكية، كروت الأدلة الموثقة بروابط المقالات ونسب الصلة، ومؤشر جاهزية الذكاء الاصطناعي.
10. **F10 (Audit & Compliance Log)**: سجل التدقيق والحوكمة: جدول العمليات، فلترة بالكيان أو المستخدم، إلغاء الطلبات المتعارضة، ونافذة تفاصيل العمليات.

---

## 📊 4. الوضع الحالي لحالة النظام (Current System Status)

| العنصر | الحالة الحالية | الملاحظات |
| :--- | :---: | :--- |
| **Backend API** | 🟢 مكتمل 100% | 13 Controller جاهزة للعمل ومربوطة بالكامل |
| **Backend Tests** | 🟢 864 اختبار ناجح | نسبة نجاح 100% (0 فشل / 0 تخطي) |
| **Frontend UI** | 🟢 مكتمل 100% | تم بناء حزمة الإنتاج (Production Build) بنجاح |
| **Frontend Lint** | 🟢 0 أخطاء | لا توجد أي أخطاء TypeScript أو Linting |
| **Local AI / RAG** | 🟢 جاهز للعمل | تكامل محلي كامل مع Ollama بدون خروج بيانات |
| **Security & RBAC** | 🟢 مفعل ومختبر | حماية مسارات الـ Audit بالـ Bearer Token والصلاحيات |

---

## 🛠️ 5. خطوات التشغيل السريع (Quickstart Guide)

### 1. تشغيل الباك إند (Backend)
```bash
dotnet run --project src/EngineeringKnowledgeHub.Api/EngineeringKnowledgeHub.Api.csproj
```
* **رابط التوثيق التفاعلي (Swagger)**: `http://localhost:5000/swagger`
* **رابط فحص صحة النظام (Health Check)**: `http://localhost:5000/health`

### 2. تشغيل الفرونت إند (Frontend)
```bash
cd frontend
npm install
npm run dev
```
* **رابط الواجهة للمستخدمين**: `http://localhost:5173`

### 3. تشغيل الذكاء الاصطناعي المحلي (Local AI - اختياري)
```bash
ollama serve
ollama pull llama3
ollama pull nomic-embed-text
```

### 4. تشغيل الاختبارات الآلية (Verification)
```bash
# تشغيل اختبارات الباك إند بالكامل
dotnet test

# فحص بناء الفرونت إند
cd frontend
npm run lint
npm run build
```

---

## 📂 6. هيكل المشروع (Project Directory)

```text
EngineeringKnowledgeHub/
├── src/                                        # كود الباك إند (Clean Architecture)
│   ├── EngineeringKnowledgeHub.Domain/         # الطبقة الأساسية (الكيانات والقواعد)
│   ├── EngineeringKnowledgeHub.Application/    # الخدمات والعمليات والـ DTOs
│   ├── EngineeringKnowledgeHub.Infrastructure/ # قواعد البيانات والاتصال بالـ AI
│   └── EngineeringKnowledgeHub.Api/            # وحدات التحكم وإعدادات الأمان
├── tests/                                      # 864 اختبار آلي شامل
├── frontend/                                   # كود الواجهة (React 19 + TypeScript)
│   ├── src/
│   │   ├── api/                                # دوال الاتصال بالسيرفر
│   │   ├── features/                           # الشاشات والميزات (RAG, Audit, Incidents, etc.)
│   │   ├── components/                         # المكونات المشتركة والهيكل
│   │   └── styles/tokens.css                   # نظام التصميم الموحد
├── README.md                                   # التوثيق الإنجليزي الشامل للمشروع
├── README_AR.md                                # التوثيق العربي التنفيذي للمشروع
├── ARCHITECTURE.md                             # المعايير المعمارية التفصيلية
└── AGENT_RULES.md                              # القواعد الهندسية للنظام
```
