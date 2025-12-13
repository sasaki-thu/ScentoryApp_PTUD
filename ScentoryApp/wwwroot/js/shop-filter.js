// ====================================================================
// FILE: wwwroot/js/shop-filter.js (VERSION FIXED)
// Xử lý filter AJAX cho trang Shop - ĐÃ SỬA LỖI
// ====================================================================

(function () {
    'use strict';

    // ============ BIẾN TOÀN CỤC ============
    let currentPage = 1;
    let isLoading = false;

    // ============ KHỞI TẠO KHI TRANG LOAD ============
    document.addEventListener('DOMContentLoaded', function () {
        console.log('🚀 Shop filter initialized');
        initializeFilters();
        initializeSortSelect();
        setupPaginationListeners();
    });

    // ============ KHỞI TẠO CÁC BỘ LỌC ============
    function initializeFilters() {
        // Lọc theo giá (Range slider)
        const priceRange = document.getElementById('priceRange');
        if (priceRange) {
            // FIX: Thêm event listener cho input để update label realtime
            priceRange.addEventListener('input', function () {
                updatePriceLabel(this.value);
            });

            // FIX: Chỉ apply filter khi THAY ĐỔI giá trị (mouseup/touchend)
            priceRange.addEventListener('change', function () {
                console.log('💰 Price changed to:', this.value);
                applyFilters();
            });
        } else {
            console.warn('⚠️ Price range element not found');
        }

        // Lọc theo danh mục (Checkboxes)
        const categoryCheckboxes = document.querySelectorAll('.category-checkbox');
        console.log('📦 Found', categoryCheckboxes.length, 'category checkboxes');

        categoryCheckboxes.forEach(checkbox => {
            checkbox.addEventListener('change', function () {
                console.log('✅ Category changed:', this.value, 'Checked:', this.checked);
                applyFilters();
            });
        });
    }

    // ============ CẬP NHẬT LABEL GIÁ ============
    window.updatePriceLabel = function (value) {
        const label = document.getElementById('currentMaxPrice');
        if (label) {
            const formatted = parseInt(value).toLocaleString('vi-VN');
            label.textContent = formatted + 'đ';
        }
    };

    // ============ KHỞI TẠO SELECT SẮP XẾP ============
    function initializeSortSelect() {
        const sortSelect = document.getElementById('sortSelect');
        if (sortSelect) {
            sortSelect.addEventListener('change', function () {
                console.log('🔄 Sort changed to:', this.value);
                applyFilters();
            });
        } else {
            console.warn('⚠️ Sort select element not found');
        }
    }

    // ============ ÁP DỤNG CÁC BỘ LỌC ============
    function applyFilters(page = 1) {
        if (isLoading) {
            console.log('⏳ Already loading, skipping...');
            return;
        }

        console.log('🔍 Applying filters for page:', page);

        // Thu thập dữ liệu filter
        const filterData = collectFilterData(page);

        console.log('📤 Filter data:', filterData);

        // Gọi API
        sendFilterRequest(filterData);
    }

    // ============ THU THẬP DỮ LIỆU FILTER ============
    function collectFilterData(page) {
        // Lấy giá trị thanh giá
        const priceRange = document.getElementById('priceRange');
        const maxPrice = priceRange ? parseInt(priceRange.value) : null;
        const minPrice = priceRange ? parseInt(priceRange.min) : null;

        // Lấy các danh mục được chọn - CHECK ID chính xác
        const selectedCategories = [];
        document.querySelectorAll('.category-checkbox:checked').forEach(cb => {
            selectedCategories.push(cb.value);
            console.log('  ✓ Selected category:', cb.value);
        });

        // Lấy kiểu sắp xếp
        const sortSelect = document.getElementById('sortSelect');
        const sortBy = sortSelect ? sortSelect.value : 'default';

        const data = {
            minPrice: minPrice,
            maxPrice: maxPrice,
            categoryIds: selectedCategories.length > 0 ? selectedCategories : null, // FIX: Gửi null nếu không có category nào
            sortBy: sortBy,
            page: page,
            pageSize: 12
        };

        console.log('📊 Collected filter data:', JSON.stringify(data, null, 2));

        return data;
    }

    // ============ GỬI REQUEST AJAX ============
    function sendFilterRequest(filterData) {
        isLoading = true;
        showLoadingSpinner();

        console.log('🌐 Sending request to /Home/FilterProducts');

        fetch('/Home/FilterProducts', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify(filterData)
        })
            .then(response => {
                console.log('📥 Response status:', response.status);
                if (!response.ok) {
                    throw new Error('Network response was not ok: ' + response.status);
                }
                return response.json();
            })
            .then(data => {
                console.log('✅ Response data:', data);

                if (data.success) {
                    updateProductList(data.html);
                    updatePagination(data.currentPage, data.totalPages);
                    updateResultCount(data.totalItems);
                    currentPage = data.currentPage;

                    // Scroll lên đầu danh sách sản phẩm
                    const productSection = document.querySelector('.col-lg-9');
                    if (productSection) {
                        productSection.scrollIntoView({ behavior: 'smooth', block: 'start' });
                    }
                } else {
                    console.error('❌ Server returned success=false:', data.message);
                    showError(data.message || 'Có lỗi xảy ra khi lọc sản phẩm');
                }
            })
            .catch(error => {
                console.error('❌ Fetch error:', error);
                showError('Không thể kết nối đến server. Vui lòng thử lại.');
            })
            .finally(() => {
                isLoading = false;
                hideLoadingSpinner();
                console.log('🏁 Request completed');
            });
    }

    // ============ CẬP NHẬT DANH SÁCH SẢN PHẨM ============
    function updateProductList(html) {
        const productGrid = document.getElementById('productGrid');
        if (productGrid) {
            console.log('🔄 Updating product list');
            productGrid.innerHTML = html;

            // Re-attach event listeners cho nút "Thêm vào giỏ hàng"
            attachAddToCartListeners();
        } else {
            console.error('❌ Product grid element not found');
        }
    }

    // ============ CẬP NHẬT PHÂN TRANG ============
    function updatePagination(currentPage, totalPages) {
        console.log('📄 Updating pagination:', currentPage, '/', totalPages);

        const paginationContainer = document.getElementById('paginationContainer');
        if (!paginationContainer) {
            console.warn('⚠️ Pagination container not found');
            return;
        }

        if (totalPages <= 1) {
            paginationContainer.style.display = 'none';
            return;
        }

        paginationContainer.style.display = 'block';

        let html = '<ul class="pagination justify-content-center">';

        // Nút Previous
        html += `<li class="page-item ${currentPage === 1 ? 'disabled' : ''}">
                    <a class="page-link" href="#" data-page="${currentPage - 1}">←</a>
                 </li>`;

        // Các số trang
        for (let i = 1; i <= totalPages; i++) {
            html += `<li class="page-item ${i === currentPage ? 'active' : ''}">
                        <a class="page-link" href="#" data-page="${i}">${i}</a>
                     </li>`;
        }

        // Nút Next
        html += `<li class="page-item ${currentPage === totalPages ? 'disabled' : ''}">
                    <a class="page-link" href="#" data-page="${currentPage + 1}">→</a>
                 </li>`;

        html += '</ul>';

        paginationContainer.innerHTML = html;

        // Re-attach pagination listeners
        setupPaginationListeners();
    }

    // ============ CẬP NHẬT SỐ KẾT QUẢ ============
    function updateResultCount(count) {
        const resultCount = document.getElementById('resultCount');
        if (resultCount) {
            resultCount.textContent = `Hiển thị ${count} sản phẩm`;
            console.log('📊 Result count updated:', count);
        }
    }

    // ============ SETUP PAGINATION LISTENERS ============
    function setupPaginationListeners() {
        document.querySelectorAll('.pagination .page-link').forEach(link => {
            link.addEventListener('click', function (e) {
                e.preventDefault();
                const page = parseInt(this.dataset.page);
                console.log('📄 Pagination clicked, page:', page);

                if (page && page > 0 && !this.parentElement.classList.contains('disabled')) {
                    applyFilters(page);
                }
            });
        });
    }

    // ============ ATTACH ADD TO CART LISTENERS ============
    function attachAddToCartListeners() {
        console.log('🛒 Attaching add to cart listeners');

        document.querySelectorAll(".btn-add-cart").forEach(btn => {
            // Remove old listener nếu có
            btn.replaceWith(btn.cloneNode(true));
        });

        // Add new listeners
        document.querySelectorAll(".btn-add-cart").forEach(btn => {
            btn.addEventListener("click", function () {
                const productId = this.dataset.id;
                console.log('🛒 Add to cart clicked:', productId);

                fetch("/Cart/AddToCart", {
                    method: "POST",
                    credentials: "include",
                    headers: {
                        "Content-Type": "application/x-www-form-urlencoded"
                    },
                    body: `id=${productId}&quantity=1`
                })
                    .then(res => res.json())
                    .then(data => {
                        if (data.success) {
                            if (typeof loadMiniCart === 'function') {
                                loadMiniCart();
                            }
                            if (typeof showToast === 'function') {
                                showToast("Thêm vào giỏ hàng thành công", "success");
                            }
                        } else {
                            if (typeof showToast === 'function') {
                                showToast("Không thể thêm vào giỏ hàng", "error");
                            }
                        }
                    })
                    .catch(error => {
                        console.error('Error:', error);
                        if (typeof showToast === 'function') {
                            showToast("Có lỗi xảy ra", "error");
                        }
                    });
            });
        });
    }

    // ============ LOADING SPINNER ============
    function showLoadingSpinner() {
        console.log('⏳ Showing loading spinner');

        const productGrid = document.getElementById('productGrid');
        if (productGrid) {
            productGrid.style.opacity = '0.5';
            productGrid.style.pointerEvents = 'none';
        }

        // Thêm spinner nếu chưa có
        let spinner = document.getElementById('filterLoadingSpinner');
        if (!spinner) {
            spinner = document.createElement('div');
            spinner.id = 'filterLoadingSpinner';
            spinner.className = 'text-center my-5';
            spinner.innerHTML = '<div class="spinner-border text-primary" role="status"><span class="visually-hidden">Loading...</span></div>';

            const container = document.querySelector('.col-lg-9');
            if (container && productGrid) {
                container.insertBefore(spinner, productGrid);
            }
        } else {
            spinner.style.display = 'block';
        }
    }

    function hideLoadingSpinner() {
        console.log('✅ Hiding loading spinner');

        const productGrid = document.getElementById('productGrid');
        if (productGrid) {
            productGrid.style.opacity = '1';
            productGrid.style.pointerEvents = 'auto';
        }

        const spinner = document.getElementById('filterLoadingSpinner');
        if (spinner) {
            spinner.style.display = 'none';
        }
    }

    // ============ HIỂN THỊ LỖI ============
    function showError(message) {
        console.error('❌ Error:', message);

        if (typeof showToast === 'function') {
            showToast(message, 'error');
        } else {
            alert(message);
        }
    }

    // ============ EXPORT FUNCTION ============
    window.applyShopFilters = applyFilters;

    console.log('✅ Shop filter script loaded successfully');

})();
