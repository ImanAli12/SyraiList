// ============================================================
// 📁 الملف: wwwroot/js/index.js
// ✅ النسخة النهائية مع دعم التوجيه للمدير (Dashboard) + رقم الهاتف
// ============================================================

// ============================================================
// عناصر DOM
// ============================================================
const $ = (id) => document.getElementById(id);

const el = {
    openModalBtn: $("openModalBtn"),
    closeModalBtn: $("closeModalBtn"),
    authModal: $("authModal"),
    loginPanel: $("loginPanel"),
    signupPanel: $("signupPanel"),
    modalTitle: $("modalTitle"),
    modalSubtitle: $("modalSubtitle"),
    switchToSignup: $("switchToSignupBtn"),
    switchToLogin: $("switchToLoginBtn"),
    doLoginBtn: $("doLoginBtn"),
    doSignupBtn: $("doSignupBtn"),
    loginEmail: $("loginEmail"),
    loginPassword: $("loginPassword"),
    signupName: $("signupName"),
    signupEmail: $("signupEmail"),
    signupPassword: $("signupPassword"),
    signupConfirm: $("signupConfirm"),
    signupPhone: $("signupPhone"),    // ✅ حقل رقم الهاتف
    messageBox: $("messageBox"),
    loginRequiredModal: $("loginRequiredModal"),
    closeRequiredModalBtn: $("closeRequiredModalBtn"),
    cancelRequiredBtn: $("cancelRequiredBtn"),
    goToLoginBtn: $("goToLoginBtn"),
    loginRequiredText: $("loginRequiredText"),
    announceLink: $("announceLink"),
    myAccountLink: $("myAccountLink"),
    contactNavLink: $("contactNavLink"),
    sendMsgBtn: $("sendMsgBtn"),
    contactName: $("contactName"),
    contactEmail: $("contactEmail"),
    contactMsg: $("contactMsg"),
    formFeedback: $("formFeedback"),
    exploreBtn: $("explorePropertiesBtn"),
    requestsBtn: $("requestsBtn"),
    citiesContainer: $("citiesContainer"),
};

console.log("✅ SYRELIS: جميع عناصر الصفحة جاهزة!");

// ============================================================
// دوال مساعدة
// ============================================================

function showMessage(text, type) {
    const msg = el.messageBox;
    if (!msg) return;
    msg.textContent = text;
    msg.className = "message " + type;
    msg.style.display = "block";
    setTimeout(() => {
        msg.style.display = "none";
        msg.className = "message";
    }, 4000);
}

function openAuthModal() {
    console.log("🔓 فتح مودال تسجيل الدخول");
    if (!el.authModal) return;
    el.authModal.classList.add("active");
    el.loginPanel.style.display = "block";
    el.signupPanel.style.display = "none";
    if (el.modalTitle) el.modalTitle.textContent = "SYRELIS";
    if (el.modalSubtitle) el.modalSubtitle.textContent = "سجل دخولك للوصول إلى حسابك";
    if (el.messageBox) {
        el.messageBox.style.display = "none";
        el.messageBox.className = "message";
    }
}

function closeAuthModal() {
    console.log("🔒 إغلاق مودال تسجيل الدخول");
    if (el.authModal) el.authModal.classList.remove("active");
}

function openLoginRequiredModal(message) {
    console.log("⚠️ فتح مودال 'يجب تسجيل الدخول'");
    if (!el.loginRequiredModal) {
        alert("⚠️ المودال غير موجود!");
        return;
    }
    if (el.loginRequiredText) {
        el.loginRequiredText.innerHTML = message;
    }
    el.loginRequiredModal.classList.add("active");
}

function closeLoginRequiredModal() {
    console.log("❌ إغلاق مودال 'يجب تسجيل الدخول'");
    if (el.loginRequiredModal) el.loginRequiredModal.classList.remove("active");
}

// ============================================================
// دالة للتحقق من حالة تسجيل الدخول
// ============================================================
function isLoggedIn() {
    return localStorage.getItem("syrelis_logged_in") === "true";
}

// ============================================================
// الأحداث الأساسية
// ============================================================

if (el.openModalBtn) {
    el.openModalBtn.addEventListener("click", function (e) {
        e.preventDefault();
        console.log("🖱️ ضغطت على زر تسجيل الدخول/الخروج");
    });
}

if (el.closeModalBtn) {
    el.closeModalBtn.addEventListener("click", closeAuthModal);
}

if (el.authModal) {
    el.authModal.addEventListener("click", function (e) {
        if (e.target === el.authModal) closeAuthModal();
    });
}

if (el.switchToSignup) {
    el.switchToSignup.addEventListener("click", function (e) {
        e.preventDefault();
        if (el.loginPanel) el.loginPanel.style.display = "none";
        if (el.signupPanel) el.signupPanel.style.display = "block";
        if (el.modalTitle) el.modalTitle.textContent = "انضم إلينا";
        if (el.modalSubtitle) el.modalSubtitle.textContent = "أفتح حسابك الآن واستمتع بخدماتنا";
        if (el.messageBox) {
            el.messageBox.style.display = "none";
            el.messageBox.className = "message";
        }
    });
}

if (el.switchToLogin) {
    el.switchToLogin.addEventListener("click", function (e) {
        e.preventDefault();
        if (el.signupPanel) el.signupPanel.style.display = "none";
        if (el.loginPanel) el.loginPanel.style.display = "block";
        if (el.modalTitle) el.modalTitle.textContent = "SYRELIS";
        if (el.modalSubtitle) el.modalSubtitle.textContent = "سجل دخولك للوصول إلى حسابك";
        if (el.messageBox) {
            el.messageBox.style.display = "none";
            el.messageBox.className = "message";
        }
    });
}

if (el.closeRequiredModalBtn) {
    el.closeRequiredModalBtn.addEventListener("click", closeLoginRequiredModal);
}

if (el.cancelRequiredBtn) {
    el.cancelRequiredBtn.addEventListener("click", closeLoginRequiredModal);
}

if (el.loginRequiredModal) {
    el.loginRequiredModal.addEventListener("click", function (e) {
        if (e.target === el.loginRequiredModal) closeLoginRequiredModal();
    });
}

if (el.goToLoginBtn) {
    el.goToLoginBtn.addEventListener("click", function (e) {
        e.preventDefault();
        closeLoginRequiredModal();
        setTimeout(openAuthModal, 300);
    });
}

// ============================================================
// تسجيل الدخول (متصل بـ AccountController)
// ============================================================
if (el.doLoginBtn) {
    el.doLoginBtn.addEventListener("click", async function (e) {
        e.preventDefault();
        const email = el.loginEmail?.value?.trim() || "";
        const password = el.loginPassword?.value || "";

        if (!email || !password) {
            showMessage("الرجاء إدخال البريد الإلكتروني وكلمة المرور", "error");
            return;
        }

        try {
            const formData = new FormData();
            formData.append("email", email);
            formData.append("password", password);

            const response = await fetch("/Account/Login", {
                method: "POST",
                body: formData
            });

            if (!response.ok) {
                throw new Error("فشل الاتصال بالخادم (حالة " + response.status + ")");
            }

            const result = await response.json();

            if (result.success) {
                showMessage(result.message, "success");
                localStorage.setItem("syrelis_logged_in", "true");
                setTimeout(() => {
                    closeAuthModal();
                    updateAuthUI();
                    if (result.redirectUrl) {
                        window.location.href = result.redirectUrl;
                    } else {
                        location.reload();
                    }
                }, 1500);
            } else {
                showMessage(result.message, "error");
            }
        } catch (error) {
            console.error("خطأ في تسجيل الدخول:", error);
            showMessage("❌ " + error.message, "error");
        }
    });
}

// ============================================================
// ✅ إنشاء حساب (متصل بـ AccountController) - مع رقم الهاتف
// ============================================================
if (el.doSignupBtn) {
    el.doSignupBtn.addEventListener("click", async function (e) {
        e.preventDefault();

        // 🔥 اقرأ القيم باستخدام getElementById (للتأكد)
        const name = document.getElementById('signupName')?.value?.trim() || '';
        const email = document.getElementById('signupEmail')?.value?.trim() || '';
        const password = document.getElementById('signupPassword')?.value || '';
        const confirm = document.getElementById('signupConfirm')?.value || '';
        const phoneInput = document.getElementById('signupPhone');
        const phone = phoneInput ? phoneInput.value.trim() : '';

        // 🛑 أهم خطوة: تأكد أن القيمة موجودة في الـ Console
        console.log("📞 phoneInput:", phoneInput);
        console.log("📞 phone value:", phone);

        // التحقق من الحقول
        if (!name || !email || !password || !confirm) {
            showMessage("⚠️ الرجاء ملء جميع الحقول", "error");
            return;
        }

        // 🔥 التحقق من رقم الهاتف (هنا رح تلاحظ إذا كان فاضي)
        if (!phone) {
            showMessage("⚠️ رقم الهاتف مطلوب", "error");
            return;
        }

        // تحقق من تطابق كلمة المرور
        if (password !== confirm) {
            showMessage("⚠️ كلمتا المرور غير متطابقتين", "error");
            return;
        }

        if (password.length < 8) {
            showMessage("⚠️ كلمة المرور يجب أن تكون 8 أحرف على الأقل", "error");
            return;
        }

        // إرسال البيانات
        const formData = new FormData();
        formData.append('fullName', name);
        formData.append('email', email);
        formData.append('password', password);
        formData.append('confirmPassword', confirm);
        formData.append('phoneNumber', phone); // ✅ الاسم صحيح

        try {
            const response = await fetch('/Account/Register', {
                method: 'POST',
                body: formData
            });

            const result = await response.json();
            if (result.success) {
                showMessage(result.message, 'success');
                setTimeout(() => {
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
            showMessage('❌ حدث خطأ في الاتصال', 'error');
        }
    });
}
// ============================================================
// ✅ روابط التنقل مع مودال "يجب تسجيل الدخول"
// ============================================================

function handleNavigation(e, url, targetName) {
    e.preventDefault();

    if (isLoggedIn()) {
        window.location.href = url;
    } else {
        let message = "للوصول إلى هذه الصفحة، يجب أن تكون مسجلاً دخولك في حسابك.<br>سجّل دخولك الآن واستمتع بجميع خدماتنا!";

        if (targetName === "announce") {
            message = "لإعلان عقارك، يجب أن تكون مسجلاً دخولك في حسابك.<br>سجّل دخولك الآن وانشر عقارك بكل سهولة!";
        } else if (targetName === "account") {
            message = "للوصول إلى حسابك، يجب أن تكون مسجلاً دخولك.<br>سجّل دخولك الآن لإدارة عقاراتك وبياناتك!";
        }

        if (el.loginRequiredModal) {
            el.loginRequiredModal.dataset.targetUrl = url;
        }
        openLoginRequiredModal(message);
    }
}

// ============================================================
// ✅ زر "أعلن عن عقارك"
// ============================================================
if (el.announceLink) {
    el.announceLink.addEventListener("click", function (e) {
        handleNavigation(e, "/Properties/Create", "announce");
    });
}

// ============================================================
// ✅ رابط "حسابي" (يوجه إلى Dashboard)
// ============================================================
if (el.myAccountLink) {
    el.myAccountLink.addEventListener("click", function (e) {
        if (isLoggedIn()) {
            window.location.href = "/Dashboard/Index";
        } else {
            handleNavigation(e, "/Dashboard/Index", "account");
        }
    });
}

// ============================================================
// ✅ رابط "تواصل معنا"
// ============================================================
if (el.contactNavLink) {
    el.contactNavLink.addEventListener("click", function (e) {
        e.preventDefault();
        document.getElementById("contactSection")?.scrollIntoView({
            behavior: "smooth",
        });
    });
}

// ============================================================
// ✅ أزرار الهيرو
// ============================================================

if (el.exploreBtn) {
    el.exploreBtn.addEventListener("click", function () {
        window.location.href = "/Properties/All";
    });
}

if (el.requestsBtn) {
    el.requestsBtn.addEventListener("click", function () {
        window.location.href = "/Requests/Index";
    });
}

// ============================================================
// المدن (النقر على بطاقة المدينة)
// ============================================================

if (el.citiesContainer) {
    el.citiesContainer.addEventListener("click", function (e) {
        const cityCard = e.target.closest(".container-city");
        if (cityCard) {
            const cityName = cityCard.dataset.city;
            window.location.href = "/PropertiesByCity/City/" + encodeURIComponent(cityName);
        }
    });
}

// ============================================================
// إرسال رسالة التواصل
// ============================================================

const contactForm = document.getElementById("contactForm");
if (contactForm) {
    contactForm.addEventListener("submit", async function (e) {
        e.preventDefault();

        const formData = new FormData(this);
        const feedback = el.formFeedback;

        try {
            const response = await fetch(this.action, {
                method: 'POST',
                body: formData,
                headers: {
                    'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]')?.value || ''
                }
            });

            if (!response.ok) throw new Error("فشل الاتصال بالخادم");

            const result = await response.json();

            if (result.success) {
                if (feedback) {
                    feedback.innerHTML = '<span style="color:#D4AF37;">✓ ' + result.message + '</span>';
                }
                this.reset();
                setTimeout(() => { if (feedback) feedback.innerHTML = ''; }, 3000);
            } else {
                if (feedback) {
                    feedback.innerHTML = '<span style="color:#dc2626;">⚠️ حدث خطأ، حاول مجدداً</span>';
                }
            }
        } catch (error) {
            console.error("خطأ في الإرسال:", error);
            if (feedback) {
                feedback.innerHTML = '<span style="color:#dc2626;">⚠️ فشل الاتصال بالخادم</span>';
            }
        }
    });
}

// ============================================================
// تحديث واجهة زر الدخول/الخروج
// ============================================================

function updateAuthUI() {
    const loggedIn = isLoggedIn();
    const btn = el.openModalBtn;
    if (!btn) return;

    if (loggedIn) {
        btn.textContent = "تسجيل الخروج";
        btn.classList.add("logout-active");
        btn.onclick = async function () {
            if (confirm("هل أنت متأكد من تسجيل الخروج؟")) {
                try {
                    const response = await fetch("/Account/Logout", { method: "POST" });
                    if (!response.ok) throw new Error("فشل الاتصال");
                    const result = await response.json();
                    if (result.success) {
                        localStorage.removeItem("syrelis_logged_in");
                        btn.textContent = "تسجيل الدخول";
                        btn.classList.remove("logout-active");
                        btn.onclick = openAuthModal;
                        location.reload();
                    } else {
                        alert("حدث خطأ أثناء تسجيل الخروج");
                    }
                } catch (error) {
                    console.error("خطأ في تسجيل الخروج:", error);
                    alert("حدث خطأ أثناء تسجيل الخروج");
                }
            }
        };
    } else {
        btn.textContent = "تسجيل الدخول";
        btn.classList.remove("logout-active");
        btn.onclick = openAuthModal;
    }
}

// ============================================================
// بدء التشغيل عند تحميل الصفحة
// ============================================================

document.addEventListener("DOMContentLoaded", function () {
    console.log("🚀 الصفحة الرئيسية (Index) جاهزة!");
    updateAuthUI();

    console.log("📌 مودال تسجيل الدخول:", !!el.authModal);
    console.log("📌 مودال 'يجب تسجيل الدخول':", !!el.loginRequiredModal);
});

// مفتاح Escape يغلق المودالات
document.addEventListener("keydown", function (e) {
    if (e.key === "Escape") {
        if (el.authModal?.classList.contains("active")) {
            closeAuthModal();
        }
        if (el.loginRequiredModal?.classList.contains("active")) {
            closeLoginRequiredModal();
        }
    }
});

// مفتاح Enter لتسجيل الدخول
document.addEventListener("keydown", function (e) {
    if (e.key === "Enter") {
        if (el.authModal?.classList.contains("active")) {
            if (el.loginPanel?.style.display !== "none") {
                el.doLoginBtn?.click();
            } else {
                el.doSignupBtn?.click();
            }
        }
    }
});

// زر "عقاراتي"
var myPropsBtn = document.getElementById("myPropertiesLink");
if (myPropsBtn) {
    myPropsBtn.addEventListener("click", function (e) {
        window.location.href = "/Properties/MyProperties";
    });
}

// ====== البحث حسب المدينة ======
function searchByCity(cityName) {
    window.location.href = '/Search/Index?city=' + encodeURIComponent(cityName);
}

console.log("✅ كل شيء جاهز في ملف index.js (متصل بـ Identity)!");