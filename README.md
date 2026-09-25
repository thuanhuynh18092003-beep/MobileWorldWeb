# ShopMobileWorld

Website bán điện thoại bằng **ASP.NET Core MVC (.NET 10)**, **Entity Framework Core** và **SQL Server**. Giao diện tiếng Việt, hỗ trợ màn hình máy tính và điện thoại.

## Chức năng

- Trang chủ, danh mục điện thoại, tìm kiếm, lọc thương hiệu, sắp xếp và phân trang.
- Trang chi tiết sản phẩm, thông số kỹ thuật và sản phẩm liên quan.
- Giỏ hàng cho cả khách chưa đăng nhập; cập nhật số lượng, xóa sản phẩm.
- Đặt hàng với thanh toán khi nhận hàng hoặc chọn chuyển khoản thủ công. Đơn hàng và chi tiết đơn được lưu vào SQL Server; tồn kho được kiểm tra trong giao dịch khi đặt hàng.
- Đăng ký, đăng nhập, đăng xuất, xem lịch sử đơn hàng. Mật khẩu được băm bằng ASP.NET Core Identity PasswordHasher.
- Trang quản trị riêng để thêm, sửa, ẩn/hiện sản phẩm, cập nhật tồn kho và trạng thái đơn hàng. Tài khoản Admin không truy cập giỏ hàng hoặc đặt hàng.

## Chạy dự án

1. Cài **.NET 10 SDK** và SQL Server; bảo đảm dịch vụ SQL Server đang chạy.
2. Sửa `ConnectionStrings:DefaultConnection` trong `appsettings.json` cho phù hợp với máy của bạn. Cấu hình mặc định dùng SQL Server trên cùng máy với Windows Authentication. Cấu hình `Encrypt=False` dành cho môi trường phát triển cục bộ; dùng kết nối có mã hóa và chứng chỉ hợp lệ khi triển khai thật.
3. Trong thư mục dự án, chạy:

   ```powershell
   dotnet restore
   dotnet run --launch-profile http
   ```

4. Mở <http://localhost:5144/>. Cơ sở dữ liệu `ShopMobileWorld` và 12 sản phẩm mẫu sẽ được tạo khi chạy lần đầu.

### Dùng SQL Server ngay trong Visual Studio

Máy phát triển có thể dùng **SQL Server Express LocalDB**, là thành phần cài cùng Visual Studio. Trong danh sách cấu hình chạy của Visual Studio, chọn **Visual Studio LocalDB** rồi chạy dự án. Web sẽ mở ở <http://localhost:5145> và tự tạo database `ShopMobileWorldVS` trong instance `(localdb)\ShopMobileWorldVS`.

Để xem bảng và dữ liệu: mở **View → SQL Server Object Explorer → SQL Server → (localdb)\ShopMobileWorldVS → Databases → ShopMobileWorldVS**. Nếu chưa thấy server, chọn **Add SQL Server** và nhập `(localdb)\ShopMobileWorldVS` với Windows Authentication. Không cần chạy một dịch vụ SQL Server đầy đủ riêng bên ngoài cho cấu hình này. Nếu chuyển sang máy khác, tạo instance một lần bằng `SqlLocalDB create ShopMobileWorldVS` (thường nằm trong thư mục công cụ SQL Server của Visual Studio).

Database cũ `ShopMobileWorld` trên `localhost` được giữ nguyên. Hai cấu hình là **hai database riêng**; tài khoản và đơn hàng từ database cũ sẽ không tự xuất hiện trong LocalDB. Để quay lại dữ liệu cũ, chọn cấu hình `http` hoặc `https`.

### Tài khoản quản trị

Khi chạy trên máy phát triển và database chưa có Admin, bấm **Thiết lập Admin** trên thanh menu hoặc mở `/quan-tri/thiet-lap`. Nhập tên, email và mật khẩu do bạn chọn. Nếu muốn dùng tài khoản khách đã đăng ký, nhập đúng email và mật khẩu của tài khoản đó. Sau khi thiết lập, web tự đăng nhập và mở trang quản trị `/quan-tri`. Màn hình thiết lập chỉ hoạt động từ `localhost` trong môi trường Development và tự đóng khi đã có Admin.

Khi triển khai trên máy chủ, tạo Admin bằng biến môi trường **trước khi chạy ứng dụng**:

```powershell
$env:ADMIN_EMAIL = "admin@example.com"
$env:ADMIN_PASSWORD = "<mat-khau-manh-cua-ban>"
dotnet run --launch-profile http
```

Không lưu mật khẩu quản trị trong `appsettings.json` hoặc mã nguồn. Nếu cơ sở dữ liệu đã được tạo, ứng dụng vẫn tạo tài khoản quản trị mới khi có hai biến môi trường và email đó chưa tồn tại.

## Lưu ý trước khi đưa vào kinh doanh

Dữ liệu, giá và hình điện thoại dạng minh họa CSS là mẫu. Cần cập nhật sản phẩm, thông tin liên hệ, chính sách giao hàng/bảo hành thực tế. Chuyển khoản hiện là quy trình xác nhận thủ công; chưa tích hợp cổng thanh toán, email tự động hoặc đơn vị vận chuyển. Giỏ hàng dùng session lưu trên máy chủ; khi triển khai nhiều máy chủ, cần thay bằng kho session dùng chung. Lược đồ được tạo bằng `EnsureCreated`; nếu thay đổi model sau khi đã có dữ liệu, hãy bổ sung EF Core migrations để nâng cấp cơ sở dữ liệu an toàn.
