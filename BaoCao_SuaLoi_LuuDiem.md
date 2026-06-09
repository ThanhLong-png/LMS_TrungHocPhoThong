# Báo Cáo Sửa Lỗi: Điểm Lưu Thành Công Nhưng Mất Khi Chuyển Tab/Tải Lại Trang

## 1. Mô tả lỗi ban đầu
- **Hiện tượng 1:** Khi giáo viên nhập điểm, hệ thống báo lưu thành công và ghi vào DB. Tuy nhiên, khi F5 hoặc chuyển trang thì điểm vừa nhập bị biến mất. 
- **Nguyên nhân 1:** Frontend không gửi thông tin `NamHoc` khi request lưu điểm, dẫn đến Controller dùng năm học mặc định. Khi đọc điểm lên, hệ thống lại filter theo một năm học khác (năm hiện tại trên UI), làm truy vấn không khớp dữ liệu.
- **Hiện tượng 2:** Điểm chỉ có thể lưu bằng cách gọi thủ công hàm Javascript qua console, việc nhập rồi click ra ngoài input không kích hoạt lưu dữ liệu tự động.
- **Nguyên nhân 2:** Dấu ngoặc kép trong sự kiện `@(ro ? "readonly" : "onchange=\"handleDiemChange(this)\"")` bị cơ chế Razor HTML Encoding dịch ra thành `onchange=&quot;handleDiemChange(this)&quot;`, khiến trình duyệt không hiểu đây là một Event Listener.

---

## 2. Các file đã chỉnh sửa và chi tiết giải pháp

### Bước 1: Bổ sung thuộc tính `NamHoc` vào Model Request
**File:** `ViewModels/TeacherDashboardViewModel.cs`
- Thêm thuộc tính `NamHoc` vào `LuuDiemRequest` để API có thể tiếp nhận đúng năm học từ màn hình người dùng đang xem.
```csharp
public class LuuDiemRequest
{
    // ... các trường khác
    public int? HocKy { get; set; }
    public string? NamHoc { get; set; } // Bổ sung
}
```

### Bước 2: Frontend đính kèm `NamHoc` trong HTTP Payload
**File:** `Areas/GiaoVien/Views/GiaoVien/QuanLyDiemSo.cshtml`
- Trong hàm `buildPayload`, thêm `NamHoc` lấy giá trị trực tiếp từ các biến ViewBag của Razor rendering.
```javascript
const payload = {
    HocSinhId : row.dataset.hsId,
    MonHocId  : parseInt(row.dataset.monHocId),
    HocKy     : @Html.Raw(selHK.HasValue ? selHK.Value.ToString() : "null"),
    NamHoc    : '@(string.IsNullOrEmpty(selNam) ? namHienTai : selNam)', // Bổ sung
    // ...
};
```

### Bước 3: Cập nhật luồng lưu dữ liệu trên Controller
**File:** `Areas/GiaoVien/Controllers/GiaoVienController.cs` (Action `LuuDiem`)
- Cập nhật logic để ưu tiên lấy `model.NamHoc` (do request gửi lên) thay vì `hs.NamHoc` trong cơ sở dữ liệu. Đồng thời bổ sung debug log.
```csharp
string fallbackNamHoc = DateTime.Now.Month >= 9
    ? $"{DateTime.Now.Year}-{DateTime.Now.Year + 1}"
    : $"{DateTime.Now.Year - 1}-{DateTime.Now.Year}";

// Sửa đổi từ hs.NamHoc thành model.NamHoc
string namHoc = model.NamHoc ?? fallbackNamHoc;

Console.WriteLine("========== LUU DIEM ==========");
Console.WriteLine($"HocSinh={hs.HoTen}");
Console.WriteLine($"NamHoc={namHoc}");
Console.WriteLine($"HocKy={targetHocKy}");
Console.WriteLine($"MonHoc={model.MonHocId}");
Console.WriteLine("==============================");
```

### Bước 4: Khắc phục lỗi Auto-Save (Event Listener HTML Encoding)
**File:** `Areas/GiaoVien/Views/GiaoVien/QuanLyDiemSo.cshtml`
- Ngăn chặn Razor HTML Encoding bằng cách bọc chuỗi qua hàm `@Html.Raw()` và sử dụng nháy đơn cho các sự kiện DOM. Đã áp dụng trên toàn bộ 6 cột điểm `(Miệng 1, 2, 3, 4, Giữa Kỳ, Cuối Kỳ)`.
```html
<!-- Dùng @Html.Raw thay vì @() bình thường và sử dụng nháy đơn (') -->
<input type="number" class="input-inline dm1-input" value="@g.DiemMieng"
       min="0" max="10" step="0.1" placeholder="--"
       @Html.Raw(ro ? "readonly" : "onchange='handleDiemChange(this)' onblur='handleDiemChange(this)'")
       style="@(ro ? "background:#f8fafc;cursor:default;" : "")" />
```

---

## 3. Tiêu chí nghiệm thu hoàn thành
- [x] Tương tác: Nhập điểm và nhấn Enter hoặc click ra ngoài sẽ tự động kích hoạt tính năng lưu (debounce 400ms).
- [x] Hiển thị: Trả về Toast thông báo thành công và highlight màu xanh nhạt (`#dcfce7`) lên ô input điểm.
- [x] Consistency: Dữ liệu điểm ghi vào Database đồng nhất chính xác với bộ filter `Năm Học` / `Học Kỳ` trên giao diện.
- [x] Reload Test: Tải lại trang (F5) hoặc đổi tab, dữ liệu mới nhập vẫn hiển thị bình thường.

---

## 4. Bổ sung: Lỗi chấm điểm bài tập không lưu vào sổ điểm và không phân biệt học kỳ

### Mô tả lỗi bổ sung:
- **Hiện tượng 1:** Khi giáo viên chấm điểm bài tập (trong màn hình Danh sách nộp bài - Submissions), điểm số được lưu vào bài nộp nhưng **không xuất hiện trong sổ điểm chính thức** của học sinh.
- **Nguyên nhân 1:** Controller chấm bài `ChamDiemBaiTap` chỉ đồng bộ điểm số sang bảng cũ `DiemSos`, mà không đồng bộ sang bảng `DiemHocKys` (bảng đang được dùng để hiển thị trên Sổ điểm).
- **Hiện tượng 2:** Khi giáo viên tạo bài tập ghi sổ điểm, hệ thống không phân biệt bài tập thuộc về Học kỳ nào. Nếu học kỳ trước đó đã chốt điểm mà giáo viên sửa hoặc chấm điểm, hệ thống sẽ gây xung đột dữ liệu hoặc không cho biết điểm thuộc về kỳ nào.

### Giải pháp khắc phục:
1. **Model:** Bổ sung thuộc tính `HocKy` (int) vào model `BaiTap` và tạo/chạy Migration cơ sở dữ liệu (`AddHocKyToBaiTap`) để tạo cột tương ứng trong database.
2. **View:** Cập nhật giao diện Tạo bài tập (`CreateBaiTap.cshtml`) và Sửa bài tập (`EditBaiTap.cshtml`) bổ sung dropdown chọn Học kỳ (Học kỳ 1 / Học kỳ 2) với định dạng grid `col-md-3`.
3. **Controller:** 
   - Cập nhật action `CreateBaiTap` và `EditBaiTap` nhận giá trị học kỳ được giáo viên chọn từ View và lưu lại vào database.
   - Cập nhật action `ChamDiemBaiTap` để:
     - Tự động lấy `NamHoc` của học sinh và `HocKy` của bài tập.
     - Kiểm tra trạng thái chốt điểm tương ứng trong `DiemHocKys` (`IsChotMieng`, `IsChotGiuaKy`, `IsChotCuoiKy`). Nếu học kỳ/loại điểm đó đã được chốt, API sẽ chặn lưu và trả về thông báo lỗi chi tiết cho giáo viên.
     - Nếu chưa chốt, tiến hành cập nhật song song vào cả hai bảng `DiemSos` và `DiemHocKys` (đồng thời tính điểm tổng kết học kỳ tự động nếu cả 3 cột điểm đã chốt).

