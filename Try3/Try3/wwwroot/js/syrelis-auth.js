(function () {
    'use strict';

    if (window.__syrelisAuthReady) return;
    window.__syrelisAuthReady = true;

    function isLoggedIn() {
        return window.syrelisIsAuthenticated === true;
    }

    function $(id) { return document.getElementById(id); }

    function openAuthModal() {
        var m = $('authModal');
        if (!m) return;
        m.classList.add('active');
        document.body.style.overflow = 'hidden';
        var lp = $('syrelisLoginPanel');
        var sp = $('syrelisSignupPanel');
        if (lp) lp.style.display = 'block';
        if (sp) sp.style.display = 'none';
        var t = $('syrelisModalTitle');
        var s = $('syrelisModalSubtitle');
        if (t) t.textContent = 'SYRELIS';
        if (s) s.textContent = 'سجل دخولك للوصول إلى حسابك';
    }

    function closeAuthModal() {
        var m = $('authModal');
        if (!m) return;
        m.classList.remove('active');
        document.body.style.overflow = '';
    }

    function openRequiredModal(msg) {
        var m = $('loginRequiredModal');
        if (!m) return;
        if (msg) {
            var t = $('syrelisRequiredText');
            if (t) t.innerHTML = msg;
        }
        m.classList.add('active');
        document.body.style.overflow = 'hidden';
    }

    function closeRequiredModal() {
        var m = $('loginRequiredModal');
        if (!m) return;
        m.classList.remove('active');
        document.body.style.overflow = '';
    }

    // ✅ زر تسجيل الدخول/الخروج
    document.addEventListener('click', function (e) {
        var btn = e.target.closest('#openModalBtn');
        if (!btn) return;
        e.preventDefault();

        if (isLoggedIn()) {
            if (confirm('هل أنت متأكد من تسجيل الخروج؟')) {
                fetch('/Account/Logout', { method: 'POST' })
                    .then(function (r) { return r.json(); })
                    .then(function (d) {
                        if (d && d.success) location.reload();
                    });
            }
        } else {
            openAuthModal();
        }
    });

    // ✅ حماية رابط "أعلن عن عقارك"
    document.addEventListener('click', function (e) {
        var link = e.target.closest('a[href*="/Properties/Create"]');
        if (!link) return;
        if (isLoggedIn()) return;
        e.preventDefault();
        e.stopPropagation();
        openRequiredModal('لإعلان عقارك، يجب أن تكون مسجلاً دخولك في حسابك.<br>سجّل دخولك الآن وانشر عقارك بكل سهولة!');
    }, true);
    document.addEventListener('click', function (e) {
        var link = e.target.closest('a[href*="/Properties/MyFavorites"]');
        if (!link) return;
        if (isLoggedIn()) return;
        e.preventDefault();
        e.stopPropagation();
        openRequiredModal('للوصول إلى محفوظاتك، يجب أن تكون مسجلاً دخولك.<br>سجّل دخولك الآن لرؤية عقاراتك المفضلة!');
    }, true);
    // ✅ إغلاق وتبديل
    document.addEventListener('click', function (e) {
        if (e.target.id === 'syrelisCloseAuth') closeAuthModal();
        if (e.target.id === 'syrelisCloseRequired') closeRequiredModal();
        if (e.target.id === 'syrelisCancelRequired') closeRequiredModal();
        if (e.target.id === 'syrelisGoToLogin') {
            closeRequiredModal();
            setTimeout(openAuthModal, 200);
        }
        if (e.target.id === 'authModal' && e.target === e.currentTarget) closeAuthModal();
        if (e.target.id === 'loginRequiredModal' && e.target === e.currentTarget) closeRequiredModal();

        if (e.target.id === 'syrelisSwitchToSignup') {
            e.preventDefault();
            var lp = $('syrelisLoginPanel'), sp = $('syrelisSignupPanel');
            if (lp) lp.style.display = 'none';
            if (sp) sp.style.display = 'block';
            var t = $('syrelisModalTitle'), s = $('syrelisModalSubtitle');
            if (t) t.textContent = 'انضم إلينا';
            if (s) s.textContent = 'أفتح حسابك الآن واستمتع بخدماتنا';
        }
        if (e.target.id === 'syrelisSwitchToLogin') {
            e.preventDefault();
            var lp2 = $('syrelisLoginPanel'), sp2 = $('syrelisSignupPanel');
            if (sp2) sp2.style.display = 'none';
            if (lp2) lp2.style.display = 'block';
            var t2 = $('syrelisModalTitle'), s2 = $('syrelisModalSubtitle');
            if (t2) t2.textContent = 'SYRELIS';
            if (s2) s2.textContent = 'سجل دخولك للوصول إلى حسابك';
        }
    });

    // ✅ تسجيل الدخول
    document.addEventListener('click', function (e) {
        if (e.target.id !== 'syrelisDoLogin') return;
        e.preventDefault();
        var emailEl = $('syrelisLoginEmail');
        var passEl = $('syrelisLoginPassword');
        var email = (emailEl ? emailEl.value : '').trim();
        var password = passEl ? passEl.value : '';

        if (!email || !password) {
            alert('الرجاء إدخال البريد وكلمة المرور');
            return;
        }

        var fd = new FormData();
        fd.append('email', email);
        fd.append('password', password);

        fetch('/Account/Login', { method: 'POST', body: fd })
            .then(function (r) { return r.json(); })
            .then(function (d) {
                if (d.success) {
                    if (d.redirectUrl) window.location.href = d.redirectUrl;
                    else location.reload();
                } else {
                    alert(d.message || 'فشل تسجيل الدخول');
                }
            })
            .catch(function () { alert('فشل الاتصال'); });
    });

    // ✅ إنشاء حساب
    document.addEventListener('click', function (e) {
        if (e.target.id !== 'syrelisDoSignup') return;
        e.preventDefault();
        var nameEl = $('syrelisSignupName');
        var phoneEl = $('syrelisSignupPhone');
        var emailEl = $('syrelisSignupEmail');
        var passEl = $('syrelisSignupPassword');
        var confirmEl = $('syrelisSignupConfirm');

        var name = (nameEl ? nameEl.value : '').trim();
        var phone = (phoneEl ? phoneEl.value : '').trim();
        var email = (emailEl ? emailEl.value : '').trim();
        var password = passEl ? passEl.value : '';
        var confirm = confirmEl ? confirmEl.value : '';

        if (!name || !phone || !email || !password || !confirm) {
            alert('الرجاء ملء جميع الحقول');
            return;
        }
        if (password !== confirm) { alert('كلمتا المرور غير متطابقتين'); return; }
        if (password.length < 8) { alert('كلمة المرور يجب أن تكون 8 أحرف على الأقل'); return; }

        var fd = new FormData();
        fd.append('fullName', name);
        fd.append('email', email);
        fd.append('password', password);
        fd.append('confirmPassword', confirm);
        fd.append('phoneNumber', phone);

        fetch('/Account/Register', { method: 'POST', body: fd })
            .then(function (r) { return r.json(); })
            .then(function (d) {
                if (d.success) {
                    if (d.redirectUrl) window.location.href = d.redirectUrl;
                    else location.reload();
                } else {
                    alert(d.message || 'فشل إنشاء الحساب');
                }
            })
            .catch(function () { alert('فشل الاتصال'); });
    });

    // ✅ Escape
    document.addEventListener('keydown', function (e) {
        if (e.key === 'Escape') {
            closeAuthModal();
            closeRequiredModal();
        }
    });

    // ✅ ضبط شكل الزر عند التحميل
    function setAuthButton() {
        var btn = $('openModalBtn');
        if (!btn) return;
        if (isLoggedIn()) btn.classList.add('logout-active');
        else btn.classList.remove('logout-active');
    }

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', setAuthButton);
    } else {
        setAuthButton();
    }

    console.log('✅ SYRELIS Auth Ready | isAuthenticated =', window.syrelisIsAuthenticated);
})();