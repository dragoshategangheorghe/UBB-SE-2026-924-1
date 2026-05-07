(() => {
    const listeners = new Set();
    const state = {
        items: [],
        unreadCount: 0,
        isLoading: false,
        error: null,
        lastUpdated: null
    };

    function notify() {
        listeners.forEach(listener => listener({ ...state }));
    }

    function normalizeNotification(notification) {
        return {
            id: notification.id ?? notification.Id,
            userId: notification.userId ?? notification.UserId,
            recipientUserIds: notification.recipientUserIds ?? notification.RecipientUserIds,
            title: notification.title ?? notification.Title ?? '',
            message: notification.message ?? notification.Message ?? '',
            type: notification.type ?? notification.Type ?? '',
            isRead: Boolean(notification.isRead ?? notification.IsRead),
            createdAt: notification.createdAt ?? notification.CreatedAt ?? '',
            actionUrl: notification.actionUrl ?? notification.ActionUrl ?? null,
            source: notification.source ?? notification.Source ?? 'System'
        };
    }

    window.NotificationStore = {
        subscribe(listener) {
            listeners.add(listener);
            listener({ ...state });
            return () => listeners.delete(listener);
        },
        getState() {
            return { ...state };
        },
        async refresh(query = {}) {
            state.isLoading = true;
            state.error = null;
            notify();

            try {
                const result = await NotificationApi.fetchNotifications(query);
                state.items = (result?.items ?? result?.Items ?? []).map(normalizeNotification);
                state.unreadCount = result?.unreadCount ?? result?.UnreadCount ?? state.items.filter(item => !item.isRead).length;
                state.lastUpdated = new Date().toISOString();
            } catch (error) {
                state.error = error instanceof Error ? error.message : 'Unable to load notifications.';
                throw error;
            } finally {
                state.isLoading = false;
                notify();
            }
        },
        async refreshUnreadCount() {
            try {
                const result = await NotificationApi.getUnreadCount();
                state.unreadCount = result?.unreadCount ?? result?.UnreadCount ?? 0;
                state.lastUpdated = new Date().toISOString();
                notify();
            } catch (error) {
                state.error = error instanceof Error ? error.message : 'Unable to load unread count.';
                notify();
            }
        },
        upsert(notification) {
            const normalized = normalizeNotification(notification);
            const index = state.items.findIndex(item => item.id === normalized.id);
            if (index >= 0) {
                state.items[index] = normalized;
            } else {
                state.items.unshift(normalized);
            }
            state.unreadCount = state.items.filter(item => !item.isRead).length;
            state.lastUpdated = new Date().toISOString();
            notify();
        },
        markAsRead(id) {
            const item = state.items.find(notification => notification.id === id);
            if (item) {
                item.isRead = true;
                state.unreadCount = state.items.filter(notification => !notification.isRead).length;
                notify();
            }
        },
        markAllRead() {
            state.items.forEach(notification => { notification.isRead = true; });
            state.unreadCount = 0;
            notify();
        },
        remove(id) {
            state.items = state.items.filter(notification => notification.id !== id);
            state.unreadCount = state.items.filter(notification => !notification.isRead).length;
            notify();
        },
        clear() {
            state.items = [];
            state.unreadCount = 0;
            notify();
        }
    };
})();
