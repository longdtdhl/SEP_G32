// MindBridge — Notification polling for logged-in users
// Used by both _Layout (public) and _DashboardLayout (dashboard)
(function() {
    const badge = document.getElementById('notifBadge');
    const notifList = document.getElementById('notifList');
    const notifEmpty = document.getElementById('notifEmpty');
    const markAllBtn = document.getElementById('markAllReadBtn');
    const sidebarBadge = document.getElementById('sb-notif-badge');

    async function fetchUnreadCount() {
        try {
            const resp = await fetch('/Notifications?handler=UnreadCount');
            if (!resp.ok) return;
            const json = await resp.json();
            if (!json.success) return;
            const count = json.data || 0;
            // Header bell badge
            if (badge) {
                badge.textContent = count > 99 ? '99+' : count;
                badge.classList.toggle('d-none', count === 0);
            }
            // Sidebar badge
            if (sidebarBadge) {
                sidebarBadge.textContent = count > 99 ? '99+' : count;
                sidebarBadge.classList.toggle('d-none', count === 0);
            }
        } catch(e) { /* silent */ }
    }

    async function fetchRecentNotifs() {
        try {
            const resp = await fetch('/Notifications?handler=Recent');
            if (!resp.ok) return;
            const json = await resp.json();
            const notifs = json.data || [];
            if (!notifList) return;
            if (notifs.length === 0) {
                if (notifEmpty) notifEmpty.classList.remove('d-none');
                return;
            }
            if (notifEmpty) notifEmpty.classList.add('d-none');
            let html = '';
            notifs.forEach(n => {
                const isUnread = !n.isRead;
                const timeAgo = getTimeAgo(n.createdAt);
                const icon = getNotifIcon(n.type);
                const url = getNotifUrl(n.relatedEntityType, n.relatedEntityId);
                html += `<a href="${url}" class="d-flex align-items-start gap-2 px-3 py-2 text-decoration-none border-bottom" 
                            style="background:${isUnread ? '#f0f7ff' : '#fff'};transition:background 0.2s;" 
                            data-notif-id="${n.id}"
                            onmouseover="this.style.background='#f5f5f5'" onmouseout="this.style.background='${isUnread ? '#f0f7ff' : '#fff'}'">
                    <div style="font-size:1.3rem;min-width:28px;text-align:center;">${icon}</div>
                    <div style="flex:1;min-width:0;">
                        <div class="fw-semibold text-dark" style="font-size:0.82rem;white-space:nowrap;overflow:hidden;text-overflow:ellipsis;">${n.title}</div>
                        <div class="text-muted" style="font-size:0.75rem;display:-webkit-box;-webkit-line-clamp:2;-webkit-box-orient:vertical;overflow:hidden;">${n.message}</div>
                        <div class="text-muted" style="font-size:0.68rem;margin-top:2px;">${timeAgo}</div>
                    </div>
                    ${isUnread ? '<span class="rounded-circle bg-primary" style="width:8px;height:8px;min-width:8px;margin-top:6px;"></span>' : ''}
                </a>`;
            });
            notifList.innerHTML = html;
        } catch(e) { /* silent */ }
    }

    function getNotifIcon(type) {
        const icons = { 'Appointment': '📅', 'Verification': '✅', 'Subscription': '💳', 'Package': '📦', 'System': '🔔', 'Reminder': '⏰', 'ConsultationNote': '📋' };
        return icons[type] || '🔔';
    }

    function getNotifUrl(entityType, entityId) {
        if (!entityType || !entityId) return '/Notifications';
        const routes = {
            'Appointment': '/Patient/Appointments/Index',
            'AppointmentReminder': '/Patient/Appointments/Index',
            'ConsultationNote': '/Patient/ConsultationRecords/Index',
            'TreatmentPackage': '/Patient/TreatmentPackages/Index'
        };
        return routes[entityType] || '/Notifications';
    }

    function getTimeAgo(dateStr) {
        const now = new Date();
        const date = new Date(dateStr);
        const diff = Math.floor((now - date) / 1000);
        if (diff < 60) return 'Just now';
        if (diff < 3600) return `${Math.floor(diff/60)} minutes ago`;
        if (diff < 86400) return `${Math.floor(diff/3600)} hours ago`;
        if (diff < 604800) return `${Math.floor(diff/86400)} days ago`;
        return date.toLocaleDateString('en-US');
    }

    if (markAllBtn) {
        markAllBtn.addEventListener('click', async function(e) {
            e.stopPropagation();
            try {
                await fetch(`/Notifications?handler=MarkAllReadAjax`, { method: 'POST' });
                fetchUnreadCount();
                fetchRecentNotifs();
            } catch(e) { /* silent */ }
        });
    }

    const bellBtn = document.getElementById('notificationBellBtn');
    if (bellBtn) {
        bellBtn.addEventListener('click', function() { fetchRecentNotifs(); });
    }

    // Initial fetch + polling
    fetchUnreadCount();
    setInterval(fetchUnreadCount, 30000);
})();
