# ShopMobileWorld

## Demo trực tuyến trên GitHub Pages

**[Mở website demo](https://thuanhuynh18092003-beep.github.io/MobileWorldWeb/)** · **[Khám phá Admin](https://thuanhuynh18092003-beep.github.io/MobileWorldWeb/#admin)**

Không cần mở laptop hoặc cài Visual Studio để xem bản demo. Trải nghiệm được giao diện Burgundy Galaxy, mô hình 3D, chọn màu/dung lượng, tồn kho, giỏ hàng, ưu đãi, đặt đơn mẫu và quản trị.

### Trải nghiệm trong 3 phút

1. Mở iPhone 18 Pro Max, chọn màu **Xanh** và dung lượng **256GB**. Màu Xanh 128GB đang hết hàng để minh họa tồn kho theo phiên bản.
2. Thêm vào giỏ, áp mã **GALAXY10** (giảm 10%, tối đa 1 triệu đồng, đơn từ 5 triệu đồng).
3. Tạo đơn bằng thông tin mẫu đã điền sẵn, không cần nhập thông tin cá nhân thật.
4. Vào **Admin → Đơn hàng**, chuyển đơn sang **Hoàn thành**, rồi xem doanh thu và biểu đồ cập nhật ở Tổng quan.
5. Thử quản lý sản phẩm, các tổ hợp màu/dung lượng, danh mục và ưu đãi. Bấm **Khôi phục dữ liệu mẫu** ở chân trang để bắt đầu lại.

**Phạm vi demo:** thư mục `docs` là bản HTML/CSS/JavaScript chạy trên GitHub Pages; dữ liệu mẫu chỉ lưu bằng `localStorage` trong trình duyệt, không kết nối SQL Server, không đăng nhập hay thanh toán thật. Admin demo mở công khai để nhà tuyển dụng thử chức năng; bản MVC bên dưới có phân quyền riêng. Hình ảnh, giá và thông số trong demo phục vụ minh họa portfolio. Ảnh và thư viện Three.js được lưu cùng dự án, không phụ thuộc CDN; font có dự phòng khi không tải được.

GitHub Pages được cấu hình xuất bản nhánh `main`, thư mục `/docs`. Bản ứng dụng MVC và SQL Server đầy đủ vẫn nằm ở thư mục gốc, chạy theo hướng dẫn bên dưới.

Website bán điện thoại bằng **ASP.NET Core MVC (.NET 10)**, **Entity Framework Core** và **SQL Server**. Giao diện tiếng Việt, hỗ trợ màn hình máy tính và điện thoại.

## Chức năng

- Trang chủ, danh mục điện thoại hai cấp (nhóm và dòng máy), tìm kiếm, lọc thương hiệu, sắp xếp và phân trang.
- Trang chi tiết sản phẩm, thông số kỹ thuật và sản phẩm liên quan. Khách chọn màu và dung lượng để xem đúng ảnh, giá, tồn kho của từng phiên bản.
- Giỏ hàng cho cả khách chưa đăng nhập; cập nhật số lượng, xóa sản phẩm.
- Đặt hàng với thanh toán khi nhận hàng hoặc chọn chuyển khoản thủ công. Khách có thể nhập mã ưu đãi ở bước thanh toán; tổng tiền sau giảm được lưu trong đơn hàng. Tồn kho và lượt dùng ưu đãi được kiểm tra trong giao dịch khi đặt hàng.
- Đăng ký, đăng nhập, đăng xuất, xem lịch sử đơn hàng. Mật khẩu được băm bằng ASP.NET Core Identity PasswordHasher.
- Trang quản trị riêng để thêm, sửa, ẩn/hiện sản phẩm, quản lý danh mục biến thể màu/dung lượng dùng chung và các tổ hợp phiên bản của từng sản phẩm (mỗi phiên bản có ảnh, giá và tồn kho riêng). Admin quản lý trạng thái đơn hàng, danh mục lớn/nhỏ, mã ưu đãi và xem biểu đồ doanh thu theo đơn hoàn thành cùng xu hướng đơn hàng trong 7/30/90 ngày. Tài khoản Admin không truy cập giỏ hàng hoặc đặt hàng.
- Giao diện không gian 3D với dải sao Three.js, hiệu ứng theo chuột/cuộn trang và mô phỏng điện thoại có thể xoay, phóng to trong cửa sổ 360°. Thư viện được lưu ngay trong `wwwroot/lib/three`, không cần CDN.
- Tông Burgundy dùng xuyên suốt trang khách và Admin. Trang chủ có mô hình iPhone 18 Pro Max Burgundy xoay 360° dựng theo thiết kế Apple công bố, tinh vân động, khu vực khám phá công nghệ theo hành tinh và nút bật/tắt ánh sáng Burgundy; lựa chọn này được lưu trong trình duyệt.

## Công nghệ sử dụng

| Thành phần | Công nghệ | Vai trò trong dự án |
| --- | --- | --- |
| Ngôn ngữ và nền tảng | C#, .NET 10, ASP.NET Core MVC | Xử lý yêu cầu web theo mô hình Model–View–Controller. |
| Giao diện | Razor Views (`.cshtml`), HTML, CSS, JavaScript | Tạo trang khách, trang Admin, form và giao diện đáp ứng màn hình điện thoại. |
| Truy cập dữ liệu | Entity Framework Core với gói `Microsoft.EntityFrameworkCore.SqlServer` 10.0.1 | Ánh xạ model C# với các bảng và truy vấn dữ liệu. |
| Cơ sở dữ liệu | Microsoft SQL Server; SQL Server Express LocalDB là lựa chọn trong Visual Studio | Lưu sản phẩm, biến thể, tài khoản, đơn hàng và ưu đãi. Hai cấu hình dùng hai database riêng. |
| Đăng nhập và phân quyền | ASP.NET Core Cookie Authentication, vai trò `Admin`/`Customer`, `PasswordHasher` | Đăng nhập, lưu mật khẩu dạng băm và giới hạn truy cập trang quản trị. |
| Giỏ hàng | ASP.NET Core Session và bộ nhớ đệm trên máy chủ | Giữ giỏ hàng cho cả khách chưa đăng nhập; giỏ hàng hiện không lưu thành bảng SQL. |
| Hiệu ứng 3D | Three.js, WebGL, OrbitControls | Vẽ nền không gian và mô hình điện thoại có thể xoay, phóng to. |
| Biểu đồ doanh thu | JavaScript và Canvas 2D | Vẽ cột doanh thu và các đường xu hướng đơn hàng từ dữ liệu tổng hợp. |

## Cơ sở dữ liệu

Ứng dụng có **10 bảng chính**. Tên bảng dưới đây là tên thật trong SQL Server; cột `Id` là khóa chính của mỗi bảng.

| Bảng | Dữ liệu chính | Quan hệ và mục đích |
| --- | --- | --- |
| `ParentCategories` | `Name`, `Slug`, `DisplayOrder` | Danh mục lớn, ví dụ iPhone hoặc Samsung; một danh mục lớn có nhiều `Subcategories`. |
| `Subcategories` | `ParentCategoryId`, `Name`, `Slug`, `DisplayOrder` | Danh mục nhỏ, ví dụ iPhone 16/17/18; `ParentCategoryId` trỏ đến `ParentCategories`. |
| `Products` | `Name`, `Slug`, `Brand`, `SubcategoryId`, `Description`, `Screen`, `Camera`, `Battery`, `Chip`, `IsActive` | Sản phẩm chung của một dòng máy; `SubcategoryId` trỏ đến `Subcategories`. Các cột giá, ảnh, màu và tồn kho của bảng này giữ thông tin đại diện để hiển thị nhanh. |
| `VariantColors` | `Name`, `Hex`, `DisplayOrder` | Danh mục màu dùng chung để Admin chọn khi tạo phiên bản. |
| `VariantStorages` | `Name`, `DisplayOrder` | Danh mục dung lượng dùng chung, ví dụ 128GB hoặc 256GB. |
| `ProductVariants` | `ProductId`, `VariantColorId`, `VariantStorageId`, `Price`, `OldPrice`, `Stock`, `ImageUrl`, `IsActive`, `IsDefault` | Mỗi hàng là một tổ hợp màu và dung lượng của một sản phẩm, có giá, ảnh và tồn kho riêng; liên kết đến `Products`, `VariantColors` và `VariantStorages`. |
| `Users` | `FullName`, `Email`, `PasswordHash`, `Role` | Tài khoản khách và Admin. |
| `Orders` | `Code`, `UserId`, thông tin người nhận, `Status`, `Subtotal`, `DiscountAmount`, `DiscountCode`, `Total`, `CompletedAt` | Đơn hàng; `UserId` có thể trống nếu khách đặt hàng không đăng nhập. Mã ưu đãi đã dùng được lưu vào đơn để giữ lịch sử. |
| `OrderItems` | `OrderId`, `ProductId`, `ProductVariantId`, `ProductName`, `VariantLabel`, `UnitPrice`, `Quantity` | Các sản phẩm trong một đơn; trỏ đến `Orders`, `Products` và phiên bản đã mua. Tên, phiên bản và đơn giá được lưu lại để lịch sử đơn không đổi khi sản phẩm được chỉnh sửa. |
| `DiscountOffers` | `Code`, `Type`, `Value`, `MinimumOrder`, `MaximumDiscount`, `StartsAt`, `EndsAt`, `UsageLimit`, `UsedCount` | Mã ưu đãi và điều kiện giảm giá. Doanh thu được tính từ đơn hàng hoàn thành, không cần bảng doanh thu riêng. |

Ảnh sản phẩm là tệp trong `wwwroot/uploads/products`; database chỉ lưu đường dẫn ảnh ở `ImageUrl`. Dữ liệu giỏ hàng nằm trong session, còn báo cáo doanh thu được tổng hợp từ `Orders` và `OrderItems`.

## Chạy dự án

1. Cài **.NET 10 SDK** và SQL Server; bảo đảm dịch vụ SQL Server đang chạy.
2. Sửa `ConnectionStrings:DefaultConnection` trong `appsettings.json` cho phù hợp với máy của bạn. Cấu hình mặc định dùng SQL Server trên cùng máy với Windows Authentication. Cấu hình `Encrypt=False` dành cho môi trường phát triển cục bộ; dùng kết nối có mã hóa và chứng chỉ hợp lệ khi triển khai thật.
3. Trong thư mục dự án, chạy:

   ```powershell
   dotnet restore
   dotnet run --launch-profile http
   ```

4. Mở <http://localhost:5144/>. Cơ sở dữ liệu `ShopMobileWorld` và 12 sản phẩm mẫu sẽ được tạo khi chạy lần đầu.

### Thêm sản phẩm và phiên bản trong Admin

1. Vào **Danh mục biến thể** (`/quan-tri/danh-muc-bien-the`) để thêm hoặc sửa tên màu, mã màu và dung lượng dùng chung.
2. Vào **Sản phẩm → Thêm sản phẩm**. Form này chỉ lưu tên, đường dẫn, danh mục và các thông tin chung. Sản phẩm mới được ẩn tạm cho đến khi có phiên bản đang bán.
3. Sau khi lưu, trang tự chuyển sang **Thêm phiên bản**. Chọn màu và dung lượng từ danh mục, nhập giá, tồn kho và tải ảnh đúng màu. Dùng mục **Màu / dung lượng** trong danh sách sản phẩm để thêm những tổ hợp khác. Không tạo sản phẩm riêng cho từng màu.

Các lựa chọn màu/dung lượng trên trang khách dẫn đến đúng phiên bản và cập nhật ảnh, giá, trạng thái còn hàng theo phiên bản đó. iPhone 18 Pro Max hiện có ba màu Đỏ Burgundy, Đen và Xanh cùng dưới một sản phẩm ở cả SQL Server mặc định và Visual Studio LocalDB; màu Đen và Xanh đang có tồn kho bằng 0 nên hiển thị **Tạm hết hàng**. Khi chỉnh sửa màu hoặc dung lượng dùng chung, tên/mã màu trên các phiên bản liên quan được cập nhật; không thể xóa giá trị đang được sử dụng.

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

Dữ liệu và giá điện thoại là mẫu. Sản phẩm cũ được chuyển thành một phiên bản mặc định, giữ nguyên giá và tồn kho; các màu mẫu chưa có tên cụ thể được đưa thành từng giá trị riêng trong danh mục để không vô tình đổi màu của nhiều sản phẩm cùng lúc. Ảnh JPG/PNG/WebP tối đa 5 MB được lưu trong `wwwroot/uploads/products` và hiện trên thẻ sản phẩm ở trang chủ/danh mục, trang chi tiết và giỏ hàng. Hero đầu trang dùng mô hình 3D dựng bằng mã hoặc hình minh họa CSS khi không có WebGL. Nếu chưa tải ảnh sản phẩm, giao diện sẽ báo “Chưa có ảnh sản phẩm”. Cần cập nhật sản phẩm, thông tin liên hệ, chính sách giao hàng/bảo hành thực tế. Chuyển khoản hiện là quy trình xác nhận thủ công; chưa tích hợp cổng thanh toán, email tự động hoặc đơn vị vận chuyển. Giỏ hàng dùng session lưu trên máy chủ; khi triển khai nhiều máy chủ, cần thay bằng kho session dùng chung. Lược đồ được tạo bằng `EnsureCreated`; khi chạy, ứng dụng tự bổ sung các bảng/cột danh mục, ưu đãi, doanh thu và phiên bản vào database cũ. Với những thay đổi model sau này, nên chuyển sang EF Core migrations để quản lý lược đồ dài hạn.

Mô hình 3D là hình minh họa chung, có đổi màu theo sản phẩm; ảnh do Admin tải lên vẫn là ảnh sản phẩm thực tế. Trình duyệt không hỗ trợ WebGL sẽ dùng hình tĩnh sẵn có và các liên kết xem chi tiết vẫn hoạt động.

Mô hình iPhone 18 Pro Max Burgundy tại đầu trang được dựng bằng mã dựa trên [thiết kế Apple công bố](https://www.apple.com/newsroom/2026/09/apple-debuts-iphone-18-pro-and-iphone-18-pro-max/). Đây là mô phỏng trực quan, không phải tệp 3D hay ảnh chính thức từ Apple và không tự tạo sản phẩm hoặc giá bán trong SQL Server. Nút **Mua ngay** dẫn đến danh mục iPhone thực tế đang có trong cửa hàng.
