// ============================================================
// MindBridge Global Scripts — site.js
// Toast notifications, confirm dialogs, sidebar, utilities
// ============================================================

// ---------- Toast Notification System ----------
const MindBridge = {
    toast(message, type = 'info', duration = 4000) {
        let container = document.getElementById('mb-toast-container');
        if (!container) {
            container = document.createElement('div');
            container.id = 'mb-toast-container';
            container.className = 'mb-toast-container';
            document.body.appendChild(container);
        }
        const icons = {
            success: 'bi-check-circle-fill',
            error: 'bi-x-circle-fill',
            warning: 'bi-exclamation-triangle-fill',
            info: 'bi-info-circle-fill'
        };
        const toast = document.createElement('div');
        toast.className = `mb-toast toast-${type}`;
        toast.innerHTML = `<i class="bi ${icons[type] || icons.info}"></i><span>${message}</span>`;
        container.appendChild(toast);
        setTimeout(() => {
            toast.style.animation = 'fadeOut 0.3s ease forwards';
            setTimeout(() => toast.remove(), 300);
        }, duration);
    },

    success(msg) { this.toast(msg, 'success'); },
    error(msg)   { this.toast(msg, 'error'); },
    warning(msg) { this.toast(msg, 'warning'); },
    info(msg)    { this.toast(msg, 'info'); },

    // ---------- Confirm Dialog ----------
    confirm(message, title = 'Confirm Action') {
        return new Promise((resolve) => {
            const modalId = 'mb-confirm-' + Date.now();
            const html = `
            <div class="modal fade mb-confirm-dialog" id="${modalId}" tabindex="-1" data-bs-backdrop="static">
                <div class="modal-dialog modal-dialog-centered modal-sm">
                    <div class="modal-content">
                        <div class="modal-header">
                            <h5 class="modal-title">${title}</h5>
                            <button type="button" class="btn-close" data-bs-dismiss="modal"></button>
                        </div>
                        <div class="modal-body">
                            <p class="mb-0">${message}</p>
                        </div>
                        <div class="modal-footer">
                            <button type="button" class="btn btn-secondary btn-sm" data-bs-dismiss="modal">Cancel</button>
                            <button type="button" class="btn btn-primary btn-sm" id="${modalId}-ok">Confirm</button>
                        </div>
                    </div>
                </div>
            </div>`;
            document.body.insertAdjacentHTML('beforeend', html);
            const el = document.getElementById(modalId);
            const modal = new bootstrap.Modal(el);
            el.querySelector(`#${modalId}-ok`).addEventListener('click', () => {
                modal.hide();
                resolve(true);
            });
            el.addEventListener('hidden.bs.modal', () => {
                el.remove();
                resolve(false);
            });
            modal.show();
        });
    }
};

// ---------- Sidebar Toggle ----------
document.addEventListener('DOMContentLoaded', () => {
    const toggle = document.getElementById('mb-sidebar-toggle');
    const sidebar = document.getElementById('mb-sidebar');
    if (toggle && sidebar) {
        toggle.addEventListener('click', () => {
            sidebar.classList.toggle('show');
        });
        // Close sidebar on mobile when clicking outside
        document.addEventListener('click', (e) => {
            if (window.innerWidth < 992 && sidebar.classList.contains('show') &&
                !sidebar.contains(e.target) && !toggle.contains(e.target)) {
                sidebar.classList.remove('show');
            }
        });
    }

    // ---------- Auto-dismiss TempData toasts ----------
    const flashMsg = document.getElementById('mb-flash-message');
    if (flashMsg) {
        const type = flashMsg.dataset.type || 'info';
        const msg = flashMsg.dataset.message;
        if (msg) MindBridge.toast(msg, type);
    }

    // ---------- Confirm forms ----------
    document.querySelectorAll('[data-confirm]').forEach(el => {
        el.addEventListener('click', async (e) => {
            e.preventDefault();
            const msg = el.dataset.confirm;
            const ok = await MindBridge.confirm(msg);
            if (ok) {
                if (el.tagName === 'A') {
                    window.location.href = el.href;
                } else if (el.form) {
                    el.form.submit();
                } else {
                    el.closest('form')?.submit();
                }
            }
        });
    });

    // ---------- Scroll to Top ----------
    const scrollBtn = document.getElementById('scroll-to-top');
    if (scrollBtn) {
        window.addEventListener('scroll', () => {
            scrollBtn.style.display = window.scrollY > 300 ? 'flex' : 'none';
        });
        scrollBtn.addEventListener('click', () => {
            window.scrollTo({ top: 0, behavior: 'smooth' });
        });
    }

    // ---------- Active nav link ----------
    const currentPath = window.location.pathname.toLowerCase();
    document.querySelectorAll('.mb-sidebar .nav-link, .mb-navbar .nav-link').forEach(link => {
        const href = link.getAttribute('href');
        if (href && currentPath.startsWith(href.toLowerCase()) && href !== '/') {
            link.classList.add('active');
        } else if (href === '/' && currentPath === '/') {
            link.classList.add('active');
        }
    });
});
