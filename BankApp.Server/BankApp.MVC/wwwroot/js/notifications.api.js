(() => {
    async function request(handler, { method = 'GET', query = {}, body } = {}) {
        const url = new URL(`/Notifications`, window.location.origin);
        url.searchParams.set('handler', handler);

        Object.entries(query).forEach(([key, value]) => {
            if (value !== undefined && value !== null) {
                url.searchParams.set(key, String(value));
            }
        });

        const response = await fetch(url.toString(), {
            method,
            headers: {
                'Accept': 'application/json',
                'Content-Type': 'application/json',
                'X-Requested-With': 'XMLHttpRequest'
            },
            credentials: 'same-origin',
            body: body === undefined ? undefined : JSON.stringify(body)
        });

        if (!response.ok) {
            const errorText = await response.text();
            throw new Error(errorText || `Request failed with status ${response.status}`);
        }

        const contentType = response.headers.get('content-type') || '';
        return contentType.includes('application/json') ? response.json() : null;
    }

    window.NotificationApi = {
        fetchNotifications: ({ filter = 'all', pageNumber = 1, pageSize = 8 } = {}) =>
            request('List', { query: { filter, pageNumber, pageSize } }),
        getUnreadCount: () => request('UnreadCount'),
        markAsRead: (id) => request('MarkRead', { method: 'POST', query: { id } }),
        markAllAsRead: () => request('MarkAllRead', { method: 'POST' }),
        deleteNotification: (id) => request('Delete', { method: 'POST', query: { id } }),
        deleteAll: () => request('ClearAll', { method: 'POST' }),
        createNotification: (payload) => request('Create', { method: 'POST', body: payload })
    };
})();
