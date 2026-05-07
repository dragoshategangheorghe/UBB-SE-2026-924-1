(() => {
    function attachNotificationsSse(url) {
        if (!('EventSource' in window)) {
            return null;
        }

        const source = new EventSource(url, { withCredentials: true });

        source.addEventListener('notification', event => {
            try {
                const payload = JSON.parse(event.data);
                NotificationStore.upsert(payload);
                NotificationStore.refreshUnreadCount();
                window.BankAppNotifications?.toast('New banking alert received.');
            } catch {
                // Ignore malformed event payloads.
            }
        });

        source.onerror = () => {
            window.BankAppNotifications?.toast('Live updates paused. Reconnecting will be handled by the browser.', 'warning');
        };

        return source;
    }

    window.NotificationRealtime = { attachNotificationsSse };
})();
