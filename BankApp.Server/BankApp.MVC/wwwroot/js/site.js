// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

(() => {
    const unreadBadge = document.getElementById('notificationUnreadBadge');
    const toastHost = document.getElementById('notificationToastHost');

    function renderUnreadBadge(count) {
        if (!unreadBadge) {
            return;
        }

        unreadBadge.textContent = String(count);
        unreadBadge.classList.toggle('d-none', count <= 0);
    }

    function toast(message, variant = 'primary') {
        if (!toastHost || !window.bootstrap?.Toast) {
            return;
        }

        const container = document.createElement('div');
        container.className = 'toast align-items-center text-bg-' + variant + ' border-0';
        container.setAttribute('role', 'status');
        container.setAttribute('aria-live', 'polite');
        container.setAttribute('aria-atomic', 'true');
        container.innerHTML = '<div class="d-flex"><div class="toast-body"></div><button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast" aria-label="Close"></button></div>';
        container.querySelector('.toast-body').textContent = message;
        toastHost.appendChild(container);
        const instance = new bootstrap.Toast(container, { delay: 3500 });
        container.addEventListener('hidden.bs.toast', () => container.remove());
        instance.show();
    }

    window.BankAppNotifications = { renderUnreadBadge, toast };

    if (window.NotificationStore) {
        NotificationStore.subscribe(state => {
            renderUnreadBadge(state.unreadCount);

            if (state.error) {
                toast(state.error, 'danger');
            }
        });

        NotificationStore.refreshUnreadCount();

        const currentPath = window.location.pathname.toLowerCase();
        if (window.NotificationRealtime && currentPath.includes('/notifications')) {
            NotificationRealtime.attachNotificationsSse('/api/notifications/stream');
        }
    }
})();
