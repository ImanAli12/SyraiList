// ============================================================
// 📁 الملف: wwwroot/js/search.js
// ✅ خاص بصفحة البحث عن العقارات (مع دعم التوجيه للمدير)
// ============================================================

console.log("✅ search.js تم تحميله بنجاح!");

$(document).ready(function () {
    console.log("🔍 jQuery جاهز في صفحة البحث");

    // ============================================================
    // عناصر المودال
    // ============================================================
    const authModal = document.getElementById('authModal');
    const closeModalBtn = document.getElementById('closeModalBtn');
    const loginPanel = document.getElementById('loginPanel');
    const signupPanel = document.getElementById('signupPanel');
    const modalTitle = document.getElementById('modalTitle');
    const modalSubtitle = document.getElementById('modalSubtitle');
    const switchToSignup = document.getElementById('switchToSignupBtn');
    const switchToLogin = document.getElementById('switchToLoginBtn');
    const doLoginBtn = document.getElementById('doLoginBtn');
    const doSignupBtn = document.getElementById('doSignupBtn');
    const loginEmail = document.getElementById('loginEmail');
    const loginPassword = document.getElementById('loginPassword');
    const signupName = document.getElementById('signupName');
    const signupEmail = document.getElementById('signupEmail');
    const signupPassword = document.getElementById('signupPassword');
    const signupConfirm = document.getElementById('signupConfirm');
    const messageBox = document.getElementById('messageBox');

    // ============================================================
    // دوال المودال
    // ============================================================
    function showMessage(text, type) {
        if (!messageBox) return;
        messageBox.textContent = text;
        messageBox.className = 'message ' + type;
        messageBox.style.display = 'block';
        setTimeout(() => {
            messageBox.style.display = 'none';
            messageBox.className = 'message';
        }, 4000);
    }

    function openAuthModal() {
        console.log("🔓 فتح مودال تسجيل الدخول");
        if (!authModal) return;
        authModal.classList.add('active');
        loginPanel.style.display = 'block';
        signupPanel.style.display = 'none';
        modalTitle.textContent = 'SYRELIS';
        modalSubtitle.textContent = 'سجل دخولك للوصول إلى حسابك';
        if (messageBox) {
            messageBox.style.display = 'none';
            messageBox.className = 'message';
        }
    }

    function closeAuthModal() {
        console.log("🔒 إغلاق مودال تسجيل الدخول");
        if (authModal) authModal.classList.remove('active');
    }

    // ============================================================
    // أحداث المودال
    // ============================================================

    const openModalBtn = document.getElementById('openModalBtn');
    if (openModalBtn) {
        openModalBtn.addEventListener('click', function (e) {
            e.preventDefault();
            openAuthModal();
        });
    }

    if (closeModalBtn) {
        closeModalBtn.addEventListener('click', closeAuthModal);
    }

    if (authModal) {
        authModal.addEventListener('click', function (e) {
            if (e.target === authModal) closeAuthModal();
        });
    }

    if (switchToSignup) {
        switchToSignup.addEventListener('click', function (e) {
            e.preventDefault();
            loginPanel.style.display = 'none';
            signupPanel.style.display = 'block';
            modalTitle.textContent = 'انضم إلينا';
            modalSubtitle.textContent = 'أفتح حسابك الآن واستمتع بخدماتنا';
            if (messageBox) {
                messageBox.style.display = 'none';
                messageBox.className = 'message';
            }
        });
    }

    if (switchToLogin) {
        switchToLogin.addEventListener('click', function (e) {
            e.preventDefault();
            signupPanel.style.display = 'none';
            loginPanel.style.display = 'block';
            modalTitle.textContent = 'SYRELIS';
            modalSubtitle.textContent = 'سجل دخولك للوصول إلى حسابك';
            if (messageBox) {
                messageBox.style.display = 'none';
                messageBox.className = 'message';
            }
        });
    }

    // ============================================================
    // تسجيل الدخول - ✅ تم التعديل
    // ============================================================
    if (doLoginBtn) {
        doLoginBtn.addEventListener('click', async function (e) {
            e.preventDefault();
            const email = loginEmail?.value?.trim() || '';
            const password = loginPassword?.value || '';

            if (!email || !password) {
                showMessage('الرجاء إدخال البريد الإلكتروني وكلمة المرور', 'error');
                return;
            }

            try {
                const formData = new FormData();
                formData.append('email', email);
                formData.append('password', password);

                const response = await fetch('/Account/Login', {
                    method: 'POST',
                    body: formData
                });

                const result = await response.json();

                if (result.success) {
                    showMessage(result.message, 'success');
                    localStorage.setItem('syrelis_logged_in', 'true');
                    setTimeout(() => {
                        closeAuthModal();
                        // ✅ التوجيه إلى الرابط المرسل من الخادم
                        if (result.redirectUrl) {
                            window.location.href = result.redirectUrl;
                        } else {
                            location.reload();
                        }
                    }, 1500);
                } else {
                    showMessage(result.message, 'error');
                }
            } catch (error) {
                console.error('خطأ في تسجيل الدخول:', error);
                showMessage('فشل الاتصال بالخادم', 'error');
            }
        });
    }

    // ============================================================
    // إنشاء حساب - ✅ تم التعديل
    // ============================================================
    if (doSignupBtn) {
        doSignupBtn.addEventListener('click', async function (e) {
            e.preventDefault();
            const name = signupName?.value?.trim() || '';
            const email = signupEmail?.value?.trim() || '';
            const password = signupPassword?.value || '';
            const confirm = signupConfirm?.value || '';

            if (!name || !email || !password || !confirm) {
                showMessage('الرجاء ملء جميع الحقول', 'error');
                return;
            }

            if (password.length < 8) {
                showMessage('⚠️ كلمة المرور يجب أن تكون 8 أحرف على الأقل!', 'error');
                return;
            }

            if (password !== confirm) {
                showMessage('كلمتا المرور غير متطابقتين', 'error');
                return;
            }

            try {
                const formData = new FormData();
                formData.append('fullName', name);
                formData.append('email', email);
                formData.append('password', password);
                formData.append('confirmPassword', confirm);

                const response = await fetch('/Account/Register', {
                    method: 'POST',
                    body: formData
                });

                const result = await response.json();

                if (result.success) {
                    showMessage(result.message, 'success');
                    localStorage.setItem('syrelis_logged_in', 'true');
                    setTimeout(() => {
                        closeAuthModal();
                        // ✅ التوجيه إلى الرابط المرسل من الخادم (إن وجد)
                        if (result.redirectUrl) {
                            window.location.href = result.redirectUrl;
                        } else {
                            location.reload();
                        }
                    }, 1500);
                } else {
                    showMessage(result.message, 'error');
                }
            } catch (error) {
                console.error('خطأ في إنشاء الحساب:', error);
                showMessage('فشل الاتصال بالخادم', 'error');
            }
        });
    }

    // ============================================================
    // مفتاح Escape و Enter
    // ============================================================
    document.addEventListener('keydown', function (e) {
        if (e.key === 'Escape') {
            if (authModal?.classList.contains('active')) {
                closeAuthModal();
            }
        }
    });

    document.addEventListener('keydown', function (e) {
        if (e.key === 'Enter') {
            if (authModal?.classList.contains('active')) {
                if (loginPanel?.style.display !== 'none') {
                    doLoginBtn?.click();
                } else {
                    doSignupBtn?.click();
                }
            }
        }
    });

    // ============================================================
    // وظائف البحث
    // ============================================================

    // زر العودة للأعلى
    $(window).on('scroll', function () {
        const btn = $('#scrollBtn');
        if ($(window).scrollTop() > 300) {
            btn.addClass('show');
        } else {
            btn.removeClass('show');
        }
    });

    // التحقق من صحة الحقول
    function validateField(input) {
        const validationType = $(input).attr('data-validate');
        if (!validationType) return true;
        const value = $(input).val().trim();
        const errorElement = $('#error-' + $(input).attr('id'));
        let isValid = true;
        if (validationType === 'positive') {
            if (value !== '') {
                const numValue = parseFloat(value);
                if (isNaN(numValue) || numValue < 0) isValid = false;
            }
        }
        if (!isValid) {
            $(input).addClass('input-error');
            errorElement.addClass('show');
        } else {
            $(input).removeClass('input-error');
            errorElement.removeClass('show');
        }
        return isValid;
    }

    $('input[data-validate]').on('input blur', function () {
        validateField(this);
    });

    // إعادة تعيين
    $('#resetBtn').on('click', function (e) {
        e.preventDefault();
        $('#searchForm')[0].reset();
        $('select.form-control').prop('selectedIndex', 0);
        $('.form-control.input-error').removeClass('input-error');
        $('.error-message.show').removeClass('show');
        $('#resultsContainer').html('');
    });

    // ============================================================
    // البحث (يدعم min/max للسعر والمساحة)
    // ✅ تم التعديل: استخدام neighborhood كنص بدلاً من neighborhoodId
    // ============================================================
    $('#searchForm').on('submit', function (e) {
        e.preventDefault();

        let isFormValid = true;
        $('input[data-validate]').each(function () {
            if (!validateField(this)) isFormValid = false;
        });

        if (!isFormValid) {
            const firstError = $('.form-control.input-error').first();
            if (firstError.length) {
                firstError[0].scrollIntoView({ behavior: 'smooth', block: 'center' });
                firstError.focus();
            }
            return;
        }

        // جمع البيانات
        const params = new URLSearchParams({
            propertyTypeId: $('#searchType').val(),
            status: $('#searchStatus').val(),
            cityId: $('#searchCity').val(),
            neighborhood: $('#searchNeighborhood').val(),
            minPrice: $('#searchMinPrice').val(),
            maxPrice: $('#searchMaxPrice').val(),
            currency: $('#searchCurrency').val(),
            minArea: $('#searchMinArea').val(),
            maxArea: $('#searchMaxArea').val(),
            rooms: $('#searchRooms').val(),
            bathrooms: $('#searchBathrooms').val(),
            floor: $('#searchFloor').val(),
            totalFloors: $('#searchTotalFloors').val()
        });

        // حذف القيم الفارغة
        for (let [key, value] of params.entries()) {
            if (value === '' || value === null || value === undefined) {
                params.delete(key);
            }
        }

        const container = $('#resultsContainer');
        container.html('<div style="text-align:center; padding:40px; color:var(--gold);"><i class="fa-solid fa-spinner fa-spin fa-2x"></i><p style="margin-top:10px;">جاري البحث...</p></div>');

        fetch('/Search/Search?' + params.toString(), {
            method: 'GET',
            headers: { 'X-Requested-With': 'XMLHttpRequest' }
        })
            .then(response => {
                if (!response.ok) throw new Error('خطأ في الخادم');
                return response.text();
            })
            .then(html => {
                container.html(html);
                container[0].scrollIntoView({ behavior: 'smooth', block: 'start' });
            })
            .catch(error => {
                container.html('<div style="text-align:center; padding:40px; color:#dc2626;"><i class="fa-solid fa-circle-exclamation fa-2x"></i><p style="margin-top:10px;">حدث خطأ أثناء البحث. حاول مجدداً.</p></div>');
                console.error('Error:', error);
            });
    });

    console.log("✅ search.js جاهز بالكامل!");
});

function loadPage(page) {
    const form = document.getElementById('searchForm');
    const formData = new FormData(form);
    const params = new URLSearchParams();

    // تحويل بيانات النموذج إلى باراميترات
    formData.forEach((value, key) => {
        if (value && value !== '') {
            let paramName = key;
            if (key === 'searchType') paramName = 'propertyTypeId';
            else if (key === 'searchStatus') paramName = 'status';
            else if (key === 'searchCity') paramName = 'cityId';
            else if (key === 'searchNeighborhood') paramName = 'neighborhood';
            else if (key === 'searchMinPrice') paramName = 'minPrice';
            else if (key === 'searchMaxPrice') paramName = 'maxPrice';
            else if (key === 'searchCurrency') paramName = 'currency';
            else if (key === 'searchMinArea') paramName = 'minArea';
            else if (key === 'searchMaxArea') paramName = 'maxArea';
            else if (key === 'searchRooms') paramName = 'rooms';
            else if (key === 'searchBathrooms') paramName = 'bathrooms';
            else if (key === 'searchFloor') paramName = 'floor';
            else if (key === 'searchTotalFloors') paramName = 'totalFloors';
            params.append(paramName, value);
        }
    });

    // إضافة رقم الصفحة
    params.append('page', page);

    // عرض مؤقت "جاري التحميل"
    document.getElementById('resultsContainer').innerHTML = '<div style="text-align:center; padding:40px; color:var(--gold);"><i class="fa-solid fa-spinner fa-spin fa-2x"></i><p style="margin-top:10px;">جاري تحميل الصفحة...</p></div>';

    // إرسال الطلب
    fetch('/Search/Search?' + params.toString())
        .then(response => response.text())
        .then(html => {
            document.getElementById('resultsContainer').innerHTML = html;
            window.scrollTo({ top: 0, behavior: 'smooth' });
        })
        .catch(error => {
            document.getElementById('resultsContainer').innerHTML = `
                <div class="card" style="margin-top:30px;">
                    <div class="empty-state">
                        <i class="fa-regular fa-circle-exclamation" style="color:#dc2626;"></i>
                        <h3 style="color:#dc2626;">حدث خطأ أثناء تحميل الصفحة</h3>
                        <p style="color:#666;">${error.message}</p>
                    </div>
                </div>
            `;
            console.error('Error loading page:', error);
        });
}