# AGENTS.md — Chuẩn dự án Enterprise (.NET 10 + React)

> Hướng dẫn cho AI agent làm việc trong repo này, từ **phân tích yêu cầu** đến **coding, test,
> review, phát hành**. Đọc hết trước khi làm. Khi quy ước ở đây mâu thuẫn với yêu cầu của người
> dùng, **hỏi lại**, không tự chọn.

## 0. Nguyên tắc nền

1. **Hiểu trước, code sau.** Không viết code khi chưa rõ yêu cầu, phạm vi và tiêu chí nghiệm thu.
2. **Thay đổi nhỏ, đúng phạm vi, kiểm chứng được.** Không refactor ngoài yêu cầu.
3. **Bám pattern đã có trong repo** trước khi đưa pattern mới. Đọc 2–3 feature tương tự rồi mới viết.
4. **Bảo mật và dữ liệu là ưu tiên số một.** Không bao giờ ghi secret vào code/log/commit.
5. **Báo trung thực.** Test đỏ thì nói đỏ kèm output; bước nào bỏ qua thì nói bỏ qua.
6. **Việc khó đảo ngược thì hỏi trước:** migration trên DB dùng chung, xóa dữ liệu, push, deploy,
   đổi hợp đồng API đã công bố.

## 1. Ngôn ngữ

| Thứ                                       | Ngôn ngữ                            |
| ----------------------------------------- | ----------------------------------- |
| Trao đổi, phân tích, báo cáo              | Tiếng Việt                          |
| Tên class/method/biến, comment, XML doc   | **Tiếng Anh**                       |
| Commit message, tên nhánh, PR             | **Tiếng Anh**, Conventional Commits |
| Chuỗi giao diện, thông báo cho người dùng | Ngôn ngữ sản phẩm                   |

---

# PHẦN A — PHÂN TÍCH VÀ THIẾT KẾ

## 2. Quy trình từ yêu cầu đến code

```
Yêu cầu → Làm rõ → Phân tích → Thiết kế → Kế hoạch → Code → Test → Review → Bàn giao
```

### 2.1 Làm rõ yêu cầu (bắt buộc)

Trước khi thiết kế, tự trả lời được các câu sau; câu nào chưa rõ thì **hỏi người dùng**:

- Ai dùng (vai trò/quyền)? Mục tiêu nghiệp vụ là gì? Đo thành công bằng gì?
- Quy tắc nghiệp vụ, ngoại lệ, trạng thái hợp lệ và chuyển trạng thái?
- Dữ liệu: nguồn, khối lượng, thời gian lưu, dữ liệu nhạy cảm (PII)?
- Phi chức năng: hiệu năng, đồng thời, audit, đa tenant, đa ngôn ngữ, khả năng truy cập?
- Tích hợp hệ thống ngoài? Có ảnh hưởng API/hợp đồng đang chạy không?
- Ngoài phạm vi là gì?

### 2.2 Tài liệu phân tích — viết bằng HTML nhiều trang, KHÔNG dùng `.md`

Spec, đánh giá, thiết kế, kế hoạch đều là **bộ HTML nhiều trang** có sidebar, mở thẳng bằng
`file://` (không build, không thư viện, không fetch). Chỉ viết `.md` cho `README`/`AGENTS.md`.

**Vị trí và cấu trúc:**

```
docs/superpowers/specs/<yyyy-mm-dd>-<slug>/
├── _chung/
│   ├── style.css      khung + token màu + thành phần dùng chung  (KHÔNG sửa riêng cho một tài liệu)
│   └── khung.js       dựng sidebar từ muc-luc.js                    (KHÔNG sửa riêng cho một tài liệu)
├── muc-luc.js         MỤC LỤC — file DUY NHẤT sửa khi thêm / bớt / đổi thứ tự trang
├── index.html         Tổng quan
└── <chu-de>.html      mỗi trang một chủ đề, tên file kebab-case không dấu
```

- `<slug>` là tiếng Việt không dấu, kebab-case (`phat-hanh-custom-app-ios`). Ngày là ngày bắt đầu.
- **Thư mục mới mỗi lần**: tài liệu đã có thì không ghi đè; cập nhật thì sửa trang và ghi ngày
  vào dòng "Cập nhật" ở `index.html`. Chép `_chung/` từ một tài liệu mẫu có sẵn, giữ nguyên.
- Thêm trang = tạo file `.html` + thêm một dòng vào `muc-luc.js`. Không sửa sidebar bằng tay.

**`muc-luc.js`:**

```js
/*
 * Mục lục của tài liệu này — file duy nhất phải sửa khi thêm / bớt / đổi thứ tự trang.
 * Khung sidebar: _chung/khung.js.
 */
window.TAI_LIEU = {
  ten: 'Tên tài liệu',
  phu: 'Spec · phạm vi · dd/mm/yyyy',
  trang: [
    { tep: 'index.html', ten: 'Tổng quan' },
    { nhom: 'Phân tích' },
    { tep: 'hien-trang.html', ten: 'Hiện trạng' },
    { tep: 'yeu-cau.html', ten: 'Yêu cầu' },
    { tep: 'mo-hinh-mien.html', ten: 'Mô hình miền' },
    { nhom: 'Thiết kế' },
    { tep: 'phuong-an.html', ten: 'Các phương án' },
    { tep: 'thiet-ke-api.html', ten: 'Thiết kế API' },
    { tep: 'thiet-ke-du-lieu.html', ten: 'Thiết kế dữ liệu' },
    { tep: 'mockup.html', ten: 'Mockup giao diện' },
    { nhom: 'Triển khai' },
    { tep: 'rui-ro-migration.html', ten: 'Rủi ro và migration' },
    { tep: 'nghiem-thu.html', ten: 'Tiêu chí nghiệm thu' },
    { tep: 'cau-hoi.html', ten: 'Câu hỏi cần chốt' },
    { tep: 'lo-trinh.html', ten: 'Lộ trình và phân rã việc' },
  ],
};
```

**Khung mỗi trang `.html`** (giữ đúng cấu trúc, chỉ đổi `<title>`, nhãn, `h1`, nội dung):

```html
<!doctype html>
<html lang="vi">
<head>
<meta charset="utf-8">
<meta name="viewport" content="width=device-width, initial-scale=1">
<meta name="color-scheme" content="light dark">
<title>Tên trang · Tên tài liệu</title>
<link rel="stylesheet" href="_chung/style.css">
</head>
<body>
<div class="khung">
  <aside class="ben" data-khung-ben>
    <a class="ben__ten" href="index.html">← Mục lục</a>
  </aside>
  <main class="noi-dung" data-khung-noi-dung>
    <div class="trong">
      <header class="dau-trang">
        <div class="dau-trang__nhan">Spec · Tên tài liệu</div>
        <h1>Tên trang</h1>
        <p class="dau-trang__phu">Một đoạn tóm tắt: trang này trả lời câu hỏi gì.</p>
      </header>

      <h2 id="muc-1">Mục thứ nhất</h2>
      <!-- nội dung -->

    </div>
  </main>
</div>
<script src="muc-luc.js"></script>
<script src="_chung/khung.js"></script>
</body>
</html>
```

- Mỗi `<h2>` **phải có `id`**: sidebar tự liệt kê mục trong trang từ `h2[id]` và `[data-muc][id]`.
- Tài liệu tiếng Việt có dấu, `lang="vi"`; có `color-scheme` light/dark.
- **Không** dùng framework, CDN, font ngoài, JS riêng; chỉ `_chung/` + `muc-luc.js`.

**Thành phần dùng chung** (đã có trong `style.css`, dùng lại, đừng tự chế class mới):

| Thành phần | Dùng cho |
| --- | --- |
| `div.the` / `the--canh-bao` / `the--moi` | Thẻ nhấn mạnh: kết luận nhanh, cảnh báo, thứ mới |
| `div.hop-ghi` | Ghi chú, giả định, lưu ý ngắn |
| `div.bang-boc > table` | Bảng đối chiếu (bọc để cuộn ngang trên màn hẹp) |
| `div.pa` (`pa__dau`, `pa__ma`, `pa__ten`, `span.muc`/`muc--thap`) | Một **phương án**: mã A/B/C, tên, nhãn trạng thái |
| `div.hoi[id][data-muc]` (`hoi__ma`) | Một **câu hỏi cần chốt** Q-01, Q-02… |
| `ol.buoc` | Các bước theo thứ tự |
| `div.mau > div.may` | Mockup màn hình điện thoại |
| `div.diem` (`diem__ten`, `diem__mo`, `diem__gia`) | Thẻ chấm điểm / so sánh lựa chọn |
| `code`, `pre` | Tên kiểu, đường dẫn, route, SQL, JSON |

**Nội dung bắt buộc, theo thứ tự trang:**

1. **Tổng quan** (`index.html`): đầu trang có `dau-trang__meta` gồm *Cập nhật*, *Repo/hệ thống
   liên quan*, *Trạng thái* (ví dụ "Chỉ có spec, chưa sửa code"), *Lịch sử quyết định*. Tiếp là
   mục **Kết luận nhanh** (`the--canh-bao`): vấn đề, hướng chốt, ngoài phạm vi.
2. **Hiện trạng**: hệ thống đang chạy thế nào, **mỗi khẳng định kèm bằng chứng** (`đường/dẫn.cs:dòng`).
   Bảng đối chiếu theo hạng mục khi có nhiều thành phần.
3. **Yêu cầu**: user story + tiêu chí chấp nhận dạng Given/When/Then; phi chức năng (hiệu năng,
   đồng thời, audit, đa tenant, a11y).
4. **Mô hình miền**: entity, aggregate, value object, quy tắc bất biến, **sơ đồ trạng thái**
   (SVG/HTML nội tuyến, không ảnh ngoài).
5. **Các phương án**: ≥ 2, mỗi phương án một `div.pa`, ưu/nhược, nhãn trạng thái
   (Đề xuất / Đã bỏ / Đường lui), **một đề xuất rõ ràng và lý do**.
6. **Thiết kế**: API (route, request/response, mã lỗi, ProblemDetails), thay đổi DB, luồng UI,
   mockup bằng `div.mau`. **Mockup giữ mọi tính năng nhỏ nhất, chia theo nhóm chức năng.**
7. **Rủi ro và migration**: tương thích ngược, dữ liệu cũ, kế hoạch rollback (expand → migrate → contract).
8. **Tiêu chí nghiệm thu**: kiểm được, đúng/sai rõ ràng, đánh số.
9. **Câu hỏi cần chốt**: mỗi câu một `div.hoi` có mã Q-nn, ghi câu nào **chặn** việc nào và có
   **đề xuất mặc định** (không phản hồi thì làm theo đề xuất và ghi lại).
10. **Lộ trình**: phân rã theo lát dọc, thứ tự làm, phụ thuộc, ước lượng.

**Quy tắc viết:**

- Mở đầu mỗi trang bằng **kết luận**, chi tiết sau.
- Quyết định đã chốt ghi **ngày và lý do**; phương án đã bỏ giữ lại với nhãn "Đã bỏ" (đừng xóa).
- Phân biệt rõ **đã xác minh trong code** với **giả định**; giả định đưa vào `hop-ghi` hoặc Câu hỏi.
- Chỉ viết spec thì **không tự code**; chờ người dùng duyệt.
- Kiểm tra trước khi báo xong: mở được bằng `file://`, sidebar đủ trang, mọi `h2` có `id`, link
  nội bộ không gãy, hiển thị tốt ở màn hẹp và chế độ tối.

### 2.3 Quyết định kiến trúc (ADR)

Mỗi quyết định kiến trúc quan trọng là **một trang HTML** trong bộ spec liên quan, đặt tên
`adr-NNNN-ten-quyet-dinh.html` và thêm vào nhóm "Quyết định" của `muc-luc.js`.
Nội dung: **bối cảnh → quyết định (ngày) → hệ quả → phương án đã loại (lý do)**.
Quyết định xuyên suốt nhiều tài liệu thì gom ở một bộ `docs/superpowers/specs/<ngày>-adr/`.

---

# PHẦN B — BACKEND (.NET 10)

## 3. Stack

- **.NET 10 (LTS)**, C# mới nhất, `Nullable` bật, `TreatWarningsAsErrors` bật.
- **ASP.NET Core** Web API (Controllers hoặc Minimal APIs; theo cái repo đang dùng).
- **Clean Architecture** + **CQRS** + **MediatR**.
- **EF Core 10** (ghi/đọc), có thể **Dapper** cho truy vấn đọc nặng nếu repo đã dùng.
- **FluentValidation**, **Mapster/AutoMapper** (theo repo), **Serilog**, **OpenTelemetry**.
- Test: **xUnit**, **FluentAssertions** (hoặc Shouldly), **NSubstitute/Moq**,
  **Testcontainers** cho integration test, **Respawn** để reset DB.

> Lưu ý: MediatR và AutoMapper bản mới có giấy phép thương mại. Không tự nâng phiên bản
> major; hỏi trước.

## 4. Cấu trúc solution

```
src/
├── Domain/              # Entity, ValueObject, Enum, DomainEvent, Exception, Interface miền
├── Application/         # Use case: Command/Query/Handler/Validator/DTO, Interface hạ tầng
├── Infrastructure/      # EF Core, Repository, dịch vụ ngoài, Identity, Email, Storage
├── WebApi/              # Controller/Endpoint, Middleware, DI, cấu hình
└── (Shared/Contracts)   # Hợp đồng dùng chung nếu có
tests/
├── Domain.UnitTests/
├── Application.UnitTests/
├── Infrastructure.IntegrationTests/
└── WebApi.IntegrationTests/
```

### Quy tắc phụ thuộc (không được vi phạm)

```
WebApi ──► Application ──► Domain
   └────► Infrastructure ──► Application, Domain
```

- `Domain` **không** tham chiếu gì khác, không có EF, MediatR, JSON attribute.
- `Application` chỉ phụ thuộc `Domain`; định nghĩa **interface** (`IApplicationDbContext`,
  `IEmailSender`, `IClock`, `ICurrentUser`…), **Infrastructure** hiện thực.
- `WebApi` chỉ là lớp mỏng: nhận request → gửi MediatR → trả response. **Không có nghiệp vụ.**
- Có thể kiểm bằng **NetArchTest/ArchUnitNET** trong `tests/`; thêm vào nếu chưa có.

## 5. Domain

- **Rich domain model**: hành vi nằm trong entity, không để setter public tùy tiện.
- Entity có constructor/factory đảm bảo bất biến; thay đổi trạng thái qua method đặt tên theo
  nghiệp vụ (`Order.Confirm()`, không phải `Order.Status = 2`).
- **Aggregate root** là ranh giới giao dịch; tham chiếu giữa aggregate bằng **Id**, không bằng object.
- **Value Object** cho khái niệm không có định danh (`Money`, `Address`, `Email`).
- **Domain Event** cho tác dụng phụ trong miền; dispatch sau `SaveChanges`.
- Dùng `enum`/smart enum cho trạng thái; không dùng chuỗi/số ma thuật.
- Không ném `Exception` chung; dùng exception miền hoặc **Result pattern** (xem mục 8).

## 6. Application — CQRS với MediatR

### 6.1 Tổ chức theo feature (vertical slice)

```
Application/
└── Features/
    └── Orders/
        ├── Commands/
        │   └── CreateOrder/
        │       ├── CreateOrderCommand.cs
        │       ├── CreateOrderCommandHandler.cs
        │       ├── CreateOrderCommandValidator.cs
        │       └── CreateOrderResponse.cs
        ├── Queries/
        │   └── GetOrderById/
        │       ├── GetOrderByIdQuery.cs
        │       ├── GetOrderByIdQueryHandler.cs
        │       └── OrderDto.cs
        └── EventHandlers/
```

### 6.2 Quy tắc

- **Command** thay đổi trạng thái; **Query** chỉ đọc, **không** có tác dụng phụ.
- Một request = một handler = một use case. Handler mỏng, logic nghiệp vụ đẩy xuống Domain.
- Command/Query là `record` bất biến; đặt tên theo động từ nghiệp vụ (`ApproveInvoiceCommand`).
- Query dùng `AsNoTracking()` và **projection thẳng sang DTO** (`Select`/`ProjectTo`),
  không trả entity ra ngoài Application.
- Handler không gọi handler khác trực tiếp; cần chia sẻ logic thì dùng domain service hoặc
  tách phương thức.
- Truyền `CancellationToken` xuyên suốt.

### 6.3 Pipeline Behaviors (đăng ký đúng thứ tự)

1. `LoggingBehavior` — log request/thời gian (che dữ liệu nhạy cảm).
2. `ValidationBehavior` — chạy mọi `IValidator<TRequest>`, lỗi thì ném `ValidationException`.
3. `AuthorizationBehavior` — kiểm quyền theo attribute/policy trên request.
4. `PerformanceBehavior` — cảnh báo request chậm.
5. `TransactionBehavior` (chỉ cho Command) — bọc giao dịch + dispatch domain event.
6. `CachingBehavior` (tùy chọn, chỉ cho Query có đánh dấu).

## 7. Validation (FluentValidation)

- **Mỗi Command/Query có tham số có validator riêng**, cùng thư mục.
- Validator kiểm **hình dạng và quy tắc không cần DB** (bắt buộc, độ dài, định dạng, khoảng giá trị).
  Quy tắc cần DB (trùng, tồn tại) cũng đặt ở validator **nếu nhẹ**, còn quy tắc nghiệp vụ phức tạp
  thì ở Domain/Handler.
- Có `.WithMessage()`/mã lỗi ổn định để frontend ánh xạ; không nhúng chuỗi UI cứng trong Domain.
- Không dùng DataAnnotations cho nghiệp vụ.

```csharp
public sealed class CreateOrderCommandValidator : AbstractValidator<CreateOrderCommand>
{
    public CreateOrderCommandValidator()
    {
        RuleFor(x => x.CustomerId).NotEmpty();
        RuleFor(x => x.Lines).NotEmpty().WithErrorCode("order.lines.required");
        RuleForEach(x => x.Lines).ChildRules(l =>
        {
            l.RuleFor(x => x.Quantity).GreaterThan(0);
        });
    }
}
```

## 8. Xử lý lỗi và kết quả

- Lỗi **dự kiến** (nghiệp vụ): dùng **Result/Error** (`Result<T>`, `Error(code, message, type)`) hoặc
  exception miền có kiểm soát; **không** dùng exception cho luồng điều khiển thông thường.
- Middleware toàn cục chuyển lỗi thành **RFC 9457 ProblemDetails**:

| Loại lỗi                  | HTTP                                     |
| ------------------------- | ---------------------------------------- |
| Validation                | 400 (kèm `errors` theo field)            |
| Chưa xác thực             | 401                                      |
| Không đủ quyền            | 403                                      |
| Không tìm thấy            | 404                                      |
| Xung đột (trùng, version) | 409                                      |
| Lỗi không lường trước     | 500 (không lộ stack trace; có `traceId`) |

- Không trả message nội bộ/SQL cho client.

## 9. Infrastructure — EF Core 10

- Một `DbContext` cho mỗi bounded context; lộ ra Application qua `IApplicationDbContext`
  (hoặc repository nếu repo đã dùng — theo hiện trạng, không trộn hai kiểu).
- **Fluent API trong `IEntityTypeConfiguration<T>`**, mỗi entity một file; không dùng attribute EF trong Domain.
- **Migration**:
  - Tên có ý nghĩa (`AddOrderStatusIndex`), mỗi migration một thay đổi logic.
  - Luôn xem lại SQL sinh ra; không sửa migration đã áp dụng ở môi trường chung.
  - Thay đổi phá vỡ (đổi tên/xóa cột) tách theo **expand → migrate → contract**.
  - **Không tự chạy migration lên DB dùng chung/staging/prod**; hỏi trước.
- Hiệu năng: `AsNoTracking` cho đọc, `Include` có chủ đích, tránh N+1, dùng `AsSplitQuery`
  khi cần, phân trang bằng keyset cho dữ liệu lớn, **index** cho cột lọc/sắp xếp.
- Đồng thời: **concurrency token** (`RowVersion`) cho entity bị sửa song song.
- **Soft delete**, **audit** (`CreatedAt/By`, `UpdatedAt/By`) qua `SaveChanges` interceptor.
- **Đa tenant** (nếu có): global query filter theo `TenantId`; test bắt buộc chứng minh không rò dữ liệu.
- Giờ dùng `IClock`/`TimeProvider`, lưu **UTC**.
- Tiền tệ dùng `decimal` với precision khai báo rõ.

## 10. WebApi

- Route danh từ số nhiều, REST nhất quán: `GET /api/v1/orders/{id}`.
- **Versioning** API (`/api/v1`); thay đổi phá vỡ phải qua phiên bản mới.
- Controller/endpoint chỉ: bind → `ISender.Send` → map HTTP. Không logic.
- Trả DTO riêng cho từng use case; **không** expose entity.
- Phân trang chuẩn: `page`, `pageSize` (giới hạn tối đa), trả `items`, `totalCount`.
- Xác thực **JWT/OIDC**, phân quyền bằng **policy**; mặc định `[Authorize]`, endpoint public phải
  khai báo tường minh.
- Bật **CORS** theo whitelist, **rate limiting**, **compression**, **health checks**
  (`/health/live`, `/health/ready`), **OpenAPI** (Swagger/Scalar) cho môi trường không phải prod.
- Idempotency cho thao tác tạo quan trọng (header `Idempotency-Key`) khi cần.

## 11. Bảo mật backend (không thương lượng)

- Secret: `User Secrets` (dev) / Key Vault hoặc biến môi trường (môi trường chung). **Không commit.**
- Kiểm tra **quyền ở server** cho mọi thao tác; không tin dữ liệu từ client (kể cả `TenantId`, `UserId`).
- Chống **SQL injection** (chỉ tham số hóa), **mass assignment** (DTO tường minh), **IDOR**
  (kiểm quyền sở hữu theo Id).
- Mã hóa/băm mật khẩu bằng thuật toán chuẩn của Identity; không tự chế.
- Log không chứa mật khẩu, token, PII; che dữ liệu nhạy cảm.
- Cập nhật phụ thuộc có lỗ hổng; chạy `dotnet list package --vulnerable`.

## 12. Quy ước C#

- File-scoped namespace, `sealed` cho class không kế thừa, `record` cho DTO/Command/Query.
- Async đến cùng (`Async` hậu tố, không `.Result`/`.Wait()`), luôn có `CancellationToken`.
- Constructor injection; không Service Locator; không `static` giữ trạng thái.
- Tên: `PascalCase` kiểu/method, `camelCase` tham số, `_camelCase` field private, interface `I`.
- Không bắt `Exception` rồi nuốt; log hoặc ném lại. Dùng `ILogger<T>` có message template.
- Dùng `.editorconfig` + analyzers (Roslynator/StyleCop nếu repo có); `dotnet format` trước khi commit.

## 13. Test backend

| Tầng           | Loại test                             | Ghi chú                                          |
| -------------- | ------------------------------------- | ------------------------------------------------ |
| Domain         | Unit                                  | Kiểm bất biến, hành vi; không mock               |
| Application    | Unit (handler, validator)             | Mock interface hạ tầng; validator test từng rule |
| Infrastructure | Integration (DB thật)                 | Testcontainers + Respawn; kiểm mapping, query    |
| WebApi         | Integration (`WebApplicationFactory`) | Kiểm HTTP, auth, ProblemDetails                  |
| Kiến trúc      | Architecture tests                    | Khóa quy tắc phụ thuộc                           |

- Đặt tên: `Method_Scenario_ExpectedResult`; theo mẫu **Arrange–Act–Assert**.
- Mỗi bug sửa xong có test tái hiện.
- Không phụ thuộc thứ tự chạy, thời gian thật, mạng ngoài.

## 14. Lệnh backend thường dùng

```bash
dotnet restore
dotnet build -warnaserror
dotnet test
dotnet format --verify-no-changes
dotnet ef migrations add <Name> -p src/Infrastructure -s src/WebApi
dotnet ef migrations script --idempotent -p src/Infrastructure -s src/WebApi
```

---

# PHẦN C — FRONTEND (React)

## 15. Stack

- **React (bản ổn định mới nhất) + TypeScript strict**, build bằng **Vite**.
- **Tailwind CSS** cho style; **axios** cho HTTP.
- Server state: **TanStack Query**. Form: **React Hook Form + Zod**.
- Routing: **React Router** (hoặc theo repo). State toàn cục nhẹ: **Zustand/Context** (chỉ khi cần).
- Test: **Vitest + React Testing Library**, **MSW** để giả API, **Playwright** cho E2E.
- Lint/format: **ESLint + Prettier**.

## 16. Cấu trúc thư mục (feature-based)

```
src/
├── app/                 # providers, router, query client, layout gốc
├── features/
│   └── orders/
│       ├── api/         # hàm gọi API + query keys + hooks (useOrders, useCreateOrder)
│       ├── components/  # component riêng của feature
│       ├── pages/       # trang (route-level)
│       ├── schemas/     # Zod schema + kiểu suy ra
│       └── types.ts
├── components/          # UI dùng chung (Button, Modal, Table…) — không biết nghiệp vụ
├── lib/                 # axios instance, utils, constants
├── hooks/               # hook dùng chung
└── styles/
```

Quy tắc: feature **không import ngầm** từ feature khác; dùng chung thì đưa lên `components/`/`lib/`.

## 17. Quy ước React/TypeScript

- Function component + hooks. Không class component.
- **Không dùng `any`**; dùng `unknown` rồi thu hẹp. Bật `strict`, `noUncheckedIndexedAccess`.
- Type cho dữ liệu API khớp hợp đồng backend; ưu tiên **sinh type từ OpenAPI** (openapi-typescript/
  orval) thay vì viết tay.
- Component nhỏ, một trách nhiệm; tách logic vào custom hook.
- Props có kiểu tường minh; không truyền cả object khi chỉ cần vài field.
- `key` ổn định (id), không dùng index khi danh sách đổi thứ tự.
- Hạn chế `useEffect`: không dùng để suy ra state từ props/state hoặc gọi API (đã có TanStack Query).
- Tối ưu (`memo`, `useMemo`, `useCallback`) **chỉ khi đo được vấn đề**.
- Lazy load theo route (`React.lazy`) và dùng Suspense + Error Boundary.
- Tên file: component `PascalCase.tsx`, hook `useXxx.ts`, còn lại `camelCase`/`kebab-case` theo repo.

## 18. Gọi API với axios

- **Một axios instance duy nhất** ở `lib/http.ts` (`baseURL` từ biến môi trường, `timeout`).
- **Request interceptor**: gắn token, `Accept-Language`, `X-Correlation-Id`.
- **Response interceptor**:
  - 401 → thử refresh token **một lần** (khóa hàng đợi, tránh refresh song song), thất bại thì đăng xuất.
  - Chuẩn hóa lỗi ProblemDetails thành kiểu `ApiError` (`status`, `code`, `message`, `fieldErrors`).
- Hỗ trợ **hủy request** bằng `AbortSignal` (TanStack Query truyền sẵn).
- Mọi lời gọi API nằm trong `features/*/api/`, **component không gọi axios trực tiếp**.
- Không lưu token trong `localStorage` nếu có thể dùng cookie `HttpOnly`; nếu bắt buộc dùng bộ nhớ
  hoặc storage thì ghi rõ rủi ro và hạn chế XSS.

```ts
// lib/http.ts
export const http = axios.create({
  baseURL: import.meta.env.VITE_API_BASE_URL,
  timeout: 15_000,
});
```

## 19. Server state với TanStack Query

- Query key có cấu trúc: `['orders', 'list', filters]`, `['orders', 'detail', id]`; gom vào factory.
- Mutation xong thì **invalidate** đúng key; cập nhật lạc quan chỉ khi UX cần và có rollback.
- Luôn xử lý 3 trạng thái: **loading, error, empty**. Không để màn hình trắng.
- Không sao chép dữ liệu server vào state cục bộ.

## 20. Form và validation

- React Hook Form + Zod resolver; schema Zod nằm ở `schemas/`.
- Hiển thị lỗi gần field; lỗi server (`fieldErrors`) gán ngược vào form bằng `setError`.
- Vô hiệu hóa nút gửi khi đang submit; chống submit đúp.
- **Validation client chỉ để UX**; server vẫn là nguồn sự thật.

## 21. Tailwind CSS

- Dùng utility class; **không viết CSS riêng** trừ khi Tailwind không làm được.
- Design token (màu, spacing, font, radius) khai báo trong cấu hình Tailwind/theme; **không hard-code**
  mã màu/px lẻ trong component.
- Class dài thì gom bằng component (`<Button variant="primary">`), dùng `cva`/`tailwind-variants` +
  `clsx`/`tailwind-merge` cho biến thể; không copy-paste chuỗi class.
- Thứ tự class do `prettier-plugin-tailwindcss` quyết định; không sắp xếp tay.
- **Responsive mobile-first** (`sm: md: lg:`); kiểm tra ít nhất mobile/tablet/desktop.
- Hỗ trợ dark mode nếu sản phẩm có (`dark:`), dùng token ngữ nghĩa.
- Tránh class động nối chuỗi (`bg-${color}-500`) vì Tailwind không quét được; dùng map đầy đủ.

## 22. Accessibility, hiệu năng, bảo mật frontend

- **A11y**: thẻ ngữ nghĩa, `label` cho input, `aria-*` đúng chỗ, điều hướng bằng bàn phím,
  focus nhìn thấy, độ tương phản đạt WCAG AA.
- **Hiệu năng**: code splitting theo route, ảnh tối ưu/lazy, ảo hóa danh sách dài, theo dõi bundle size.
- **Bảo mật**: không `dangerouslySetInnerHTML` với dữ liệu chưa làm sạch; không đặt secret trong biến
  `VITE_*` (mọi thứ trong bundle đều công khai); kiểm quyền hiển thị UI chỉ là UX, quyền thật ở server.
- **UX**: thông báo lỗi dễ hiểu bằng ngôn ngữ sản phẩm, trạng thái rỗng có hướng dẫn, xác nhận trước
  thao tác phá hủy.

## 23. Test frontend

- Component: React Testing Library, **test theo hành vi người dùng** (getByRole), không test chi tiết cài đặt.
- API: **MSW**; không mock axios từng chỗ.
- Hook/logic thuần: Vitest.
- Luồng quan trọng (đăng nhập, tạo đơn…): Playwright E2E.

## 24. Lệnh frontend thường dùng

```bash
pnpm install --frozen-lockfile
pnpm dev
pnpm lint
pnpm typecheck      # tsc --noEmit
pnpm test
pnpm build
```

---

# PHẦN D — QUY TRÌNH CHUNG

## 25. Làm một tính năng, từng bước

1. **Làm rõ** yêu cầu (mục 2.1). Có thay đổi lớn → viết spec, chờ duyệt.
2. **Tách nhánh** từ nhánh tích hợp mới nhất (`develop`): `feature/<ticket>-<slug>`,
   `fix/<ticket>-<slug>`.
3. **Thiết kế hợp đồng trước**: route/DTO/lỗi (backend) và type/màn hình (frontend), thống nhất.
4. **Backend theo lát dọc**: Domain → Command/Query + Validator + Handler → cấu hình EF/migration
   → Endpoint → test.
5. **Frontend**: type/api/hook → component → trang → xử lý loading/error/empty → test.
6. **Chạy cổng chất lượng** (mục 26), **tự chạy thử** tính năng thật sự (UI/API), không chỉ dựa vào test.
7. **Tự review diff**, xóa code chết, log gỡ lỗi, `TODO` không có ticket.
8. **Commit** nhỏ, có ý nghĩa; mở PR.

## 26. Cổng chất lượng (phải xanh trước khi báo xong)

Backend: `dotnet build -warnaserror` · `dotnet test` · `dotnet format --verify-no-changes`
Frontend: `pnpm lint` · `pnpm typecheck` · `pnpm test` · `pnpm build`

Nếu một cổng đỏ do lỗi có sẵn không liên quan, **nói rõ**, đừng che hoặc tắt cổng.

## 27. Git và PR

- **Conventional Commits** bằng tiếng Anh: `feat(orders): add order cancellation (PROJ-123)`.
  Loại: `feat`, `fix`, `refactor`, `perf`, `test`, `docs`, `chore`, `build`, `ci`.
- Commit nhỏ, một ý; không trộn refactor với đổi hành vi.
- **Không commit** secret, `.env`, file build, file khóa tự sinh ngoài ý muốn.
- **Không** `push --force` lên nhánh chung, không bỏ qua hook (`--no-verify`).
- Chỉ commit/push khi được yêu cầu.
- PR gồm: **mục đích**, **thay đổi chính**, **cách kiểm thử**, **ảnh/GIF giao diện**, **ảnh hưởng
  migration/API**, **checklist rủi ro**.

## 28. Definition of Done

- [ ] Đúng tiêu chí chấp nhận; đã tự chạy thử luồng chính và luồng lỗi.
- [ ] Validator + test cho quy tắc nghiệp vụ mới; test cho bug đã sửa.
- [ ] Không phá hợp đồng API/DB đang chạy (hoặc có kế hoạch tương thích/phiên bản).
- [ ] Phân quyền và đa tenant (nếu có) đã kiểm.
- [ ] Không có secret, không log dữ liệu nhạy cảm.
- [ ] Loading/error/empty và responsive/a11y đã xử lý ở UI.
- [ ] Cổng chất lượng xanh; không còn warning mới.
- [ ] Migration đã xem SQL, có cách rollback; chưa tự áp lên DB dùng chung.
- [ ] Tài liệu/ADR/changelog cập nhật nếu thay đổi hành vi công khai.

## 29. Những điều KHÔNG được làm

| Không làm                                                | Vì sao                                         |
| -------------------------------------------------------- | ---------------------------------------------- |
| Đặt nghiệp vụ trong Controller hoặc component React      | Phá phân tầng, khó test                        |
| Để `Domain` tham chiếu EF/MediatR/Web                    | Phá Clean Architecture                         |
| Trả entity EF ra API                                     | Lộ cấu trúc, lỗi vòng lặp, over-posting        |
| Sửa migration đã áp dụng; tự chạy migration lên DB chung | Hỏng dữ liệu, khó rollback                     |
| Tin `TenantId`/`UserId` từ client                        | Lỗ hổng phân quyền                             |
| Nuốt exception, `catch {}`                               | Che lỗi                                        |
| `any`, `// @ts-ignore`, tắt rule lint để cho qua         | Nợ kỹ thuật; nếu bắt buộc phải ghi lý do       |
| Gọi axios trực tiếp trong component                      | Lặp, khó test, lệch xử lý lỗi                  |
| Tự nâng major version thư viện/framework                 | Rủi ro phá vỡ, giấy phép (MediatR, AutoMapper) |
| Thêm thư viện mới khi chưa hỏi                           | Tăng bề mặt bảo trì, bảo mật                   |
| Commit secret hoặc dữ liệu thật của khách                | Rò rỉ                                          |
| Mở rộng phạm vi, refactor ngoài yêu cầu                  | Khó review, dễ gây lỗi                         |

## 30. Khi không chắc

Dừng lại và hỏi, nêu **bạn đã đọc gì, hiểu gì, và 2–3 lựa chọn kèm đề xuất**. Không đoán trong
các việc: nghiệp vụ, bảo mật, dữ liệu, hợp đồng API.

