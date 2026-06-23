# Use Case Diagram Cấp 0 - Mì 3AE

## Nguyên tắc bản cấp 0

Ở cấp 0, actor và use case đều được khái quát hóa để thể hiện bức tranh tổng quan của hệ thống.

- `Admin`: đại diện nhóm quản trị hệ thống/quán.
- `Người dùng`: đại diện khách vãng lai và khách hàng đã đăng nhập.

Các vai trò chi tiết hơn như `Customer`, `Guest`, `AI/Groq API`, hoặc các API kỹ thuật không nên vẽ ở cấp 0. Nếu cần, chúng được tách xuống sơ đồ cấp 1.

## Actor

| Actor | Ý nghĩa |
|---|---|
| Admin | Quản lý dữ liệu món ăn, danh mục, đơn hàng và theo dõi hoạt động hệ thống |
| Người dùng | Xem thực đơn, đặt món, quản lý tài khoản, theo dõi đơn hàng và dùng chatbot hỗ trợ |

## Use Case Cấp 0

| Mã | Use Case | Gộp từ chức năng |
|---|---|---|
| UC0.1 | Truy cập hệ thống | Trang chủ, đăng ký, đăng nhập, đăng xuất |
| UC0.2 | Quản lý tài khoản | Cập nhật họ tên, số điện thoại, địa chỉ |
| UC0.3 | Xem thực đơn & thông tin món | Xem danh sách món, xem chi tiết món, xem danh mục |
| UC0.4 | Quản lý thực đơn | Thêm, sửa, xóa món ăn, quản lý hình ảnh món |
| UC0.5 | Quản lý danh mục món | Thêm, sửa, xóa danh mục món |
| UC0.6 | Đặt món & thanh toán | Thêm giỏ hàng, cập nhật giỏ hàng, thanh toán, tạo đơn |
| UC0.7 | Quản lý đơn hàng | Xem đơn, xem chi tiết, cập nhật trạng thái đơn |
| UC0.8 | Theo dõi đơn hàng | Xem lịch sử mua hàng, xem chi tiết đơn hàng, tra cứu trạng thái |
| UC0.9 | Tư vấn & hỗ trợ khách hàng | Liên hệ, chatbot tư vấn thực đơn, chatbot hỗ trợ kiểm tra đơn hàng |

## Association trong StarUML

| Actor | Use Case |
|---|---|
| Admin | UC0.1 - Truy cập hệ thống |
| Admin | UC0.4 - Quản lý thực đơn |
| Admin | UC0.5 - Quản lý danh mục món |
| Admin | UC0.7 - Quản lý đơn hàng |
| Admin | UC0.8 - Theo dõi đơn hàng |
| Admin | UC0.9 - Tư vấn & hỗ trợ khách hàng |
| Người dùng | UC0.1 - Truy cập hệ thống |
| Người dùng | UC0.2 - Quản lý tài khoản |
| Người dùng | UC0.3 - Xem thực đơn & thông tin món |
| Người dùng | UC0.6 - Đặt món & thanh toán |
| Người dùng | UC0.8 - Theo dõi đơn hàng |
| Người dùng | UC0.9 - Tư vấn & hỗ trợ khách hàng |

## Ghi chú khi vẽ

- Không dùng `include`/`extend` ở cấp 0 nếu mục tiêu là khái quát hóa cao.
- Không vẽ `Groq API` hoặc `Chatbot AI` thành actor ở cấp 0; xem đó là thành phần hỗ trợ bên trong use case `Tư vấn & hỗ trợ khách hàng`.
- Nếu cần phân quyền chi tiết giữa khách chưa đăng nhập và khách đã đăng nhập, chuyển sang sơ đồ cấp 1.

## File đã xuất

- SVG sạch để đưa vào báo cáo: `C:\Users\starg\Desktop\TH_LAP_TRINH_WEB\GIUA_KY_WEB_3AE\output\uml\usecase_level0_mi3ae_staruml_clean.svg`
- PNG xem nhanh: `C:\Users\starg\Desktop\TH_LAP_TRINH_WEB\GIUA_KY_WEB_3AE\output\uml\usecase_level0_mi3ae_staruml_clean.png`
- PlantUML nguồn: `C:\Users\starg\Desktop\TH_LAP_TRINH_WEB\GIUA_KY_WEB_3AE\output\uml\usecase_level0_mi3ae_staruml.puml`
