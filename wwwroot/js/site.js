// Thiết lập xử lý sự kiện bất đồng bộ (AJAX) khi người dùng thêm sản phẩm vào giỏ hàng
document.addEventListener("DOMContentLoaded", function () {
    // Lắng nghe sự kiện submit trên toàn bộ body cho các form có class js-add-to-cart-form
    document.body.addEventListener("submit", function (e) {
        if (e.target && e.target.classList.contains("js-add-to-cart-form")) {
            e.preventDefault(); // Ngăn chặn hành vi tải lại trang (reload) mặc định

            var form = e.target;
            var submitBtn = form.querySelector('button[type="submit"]');
            
            // Chống spam: Vô hiệu hóa nút nhấn trong lúc request đang xử lý
            if (submitBtn) {
                submitBtn.disabled = true;
                // Lưu lại nội dung cũ của nút để khôi phục sau
                var originalHtml = submitBtn.innerHTML;
                submitBtn.innerHTML = '<span class="spinner-border spinner-border-sm me-1" role="status" aria-hidden="true"></span> Đang xử lý...';
            }

            var formData = new FormData(form);

            // Gửi request POST bất đồng bộ tới CartController
            fetch(form.action, {
                method: "POST",
                body: formData,
                headers: {
                    "X-Requested-With": "XMLHttpRequest" // Đánh dấu đây là AJAX request
                }
            })
            .then(function (response) {
                if (!response.ok) {
                    throw new Error("Lỗi mạng hoặc máy chủ không phản hồi.");
                }
                return response.json();
            })
            .then(function (data) {
                if (data.success) {
                    // Cập nhật số lượng giỏ hàng trên badge Navbar
                    var badge = document.getElementById("cartCountBadge");
                    if (badge) {
                        badge.textContent = data.cartCount;
                        // Ẩn/Hiện badge tương ứng số lượng
                        if (data.cartCount > 0) {
                            badge.classList.remove("d-none");
                        } else {
                            badge.classList.add("d-none");
                        }
                    }
                    // Hiển thị popup toast thông báo thành công
                    showCartToast(data.message, "success");
                } else {
                    // Hiển thị toast báo lỗi phản hồi từ máy chủ
                    showCartToast(data.message || "Không thể thêm sản phẩm.", "danger");
                }
            })
            .catch(function (error) {
                console.error("Lỗi khi thêm giỏ hàng:", error);
                // Hiển thị toast báo lỗi kết nối
                showCartToast("Không thể thêm sản phẩm vào giỏ hàng. Vui lòng kiểm tra lại kết nối.", "danger");
            })
            .finally(function () {
                // Khôi phục trạng thái hoạt động của nút sau khi request hoàn tất
                if (submitBtn) {
                    submitBtn.disabled = false;
                    submitBtn.innerHTML = originalHtml;
                }
            });
        }
    });
});

/**
 * Hàm hiển thị Toast thông báo nổi ở góc màn hình.
 * @param {string} message - Nội dung thông báo hiển thị.
 * @param {string} type - Loại thông báo ("success" hoặc "danger").
 */
function showCartToast(message, type) {
    var toastContainer = document.getElementById("toastContainerShared");
    if (!toastContainer) {
        // Tự động tạo container chứa toast nếu không tìm thấy trong DOM
        toastContainer = document.createElement("div");
        toastContainer.id = "toastContainerShared";
        toastContainer.className = "toast-container position-fixed";
        toastContainer.style.cssText = "top: 100px; right: 24px; z-index: 1080;";
        document.body.appendChild(toastContainer);
    }

    var toastId = "toast_" + Date.now();
    var bgClass = type === "success" ? "bg-success" : "bg-danger";
    var iconClass = type === "success" ? "bi-check-circle-fill" : "bi-exclamation-triangle-fill";
    var titleText = type === "success" ? "Thành công" : "Lỗi";

    // Khởi tạo HTML cho Toast mới
    var toastHtml = 
        '<div id="' + toastId + '" class="toast align-items-center text-white ' + bgClass + ' border-0 shadow rounded-4 mb-2" role="alert" aria-live="assertive" aria-atomic="true">' +
            '<div class="d-flex">' +
                '<div class="toast-body d-flex align-items-center py-3 px-3">' +
                    '<i class="bi ' + iconClass + ' fs-4 me-3"></i>' +
                    '<div>' +
                        '<strong class="d-block text-white" style="font-size: 0.95rem;">' + titleText + '</strong>' +
                        '<span class="small opacity-90">' + message + '</span>' +
                    '</div>' +
                '</div>' +
                '<button type="button" class="btn-close btn-close-white me-3 m-auto" data-bs-dismiss="toast" aria-label="Close"></button>' +
            '</div>' +
        '</div>';

    // Thêm Toast mới vào cuối container
    toastContainer.insertAdjacentHTML("beforeend", toastHtml);
    var toastEl = document.getElementById(toastId);
    if (toastEl) {
        // Khởi tạo Bootstrap Toast và kích hoạt hiển thị
        var toast = new bootstrap.Toast(toastEl, {
            autohide: true,
            delay: 4000
        });
        toast.show();
        
        // Lắng nghe sự kiện ẩn để dọn dẹp phần tử thừa khỏi DOM
        toastEl.addEventListener("hidden.bs.toast", function () {
            toastEl.remove();
        });
    }
}
