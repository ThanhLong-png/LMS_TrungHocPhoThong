/* =============================================================
   LMS THPT – Teacher Dashboard JS
   File: wwwroot/js/teacher-dashboard.js
   ============================================================= */

// ── Anti-forgery token ─────────────────────────────────────────
function getToken() {
    const el = document.querySelector('input[name="__RequestVerificationToken"]');
    return el ? el.value : '';
}

// ── Toast ──────────────────────────────────────────────────────
function showToast(message, type) {
    type = type || 'success';
    document.querySelectorAll('.toast-dynamic').forEach(function(t) { t.remove(); });

    var colors = {
        success: { bg:'#dcfce7', color:'#166534', border:'#bbf7d0' },
        error:   { bg:'#fee2e2', color:'#991b1b', border:'#fca5a5' },
        info:    { bg:'#dbeafe', color:'#1e40af', border:'#93c5fd' }
    };
    var c = colors[type] || colors.info;

    var icons = {
        success: '<circle cx="8" cy="8" r="7" stroke="currentColor" stroke-width="1.2"/><polyline points="5,8 7,10 11,6" stroke="currentColor" stroke-width="1.5"/>',
        error:   '<circle cx="8" cy="8" r="7" stroke="currentColor" stroke-width="1.2"/><line x1="5.5" y1="5.5" x2="10.5" y2="10.5" stroke="currentColor" stroke-width="1.5"/><line x1="10.5" y1="5.5" x2="5.5" y2="10.5" stroke="currentColor" stroke-width="1.5"/>',
        info:    '<circle cx="8" cy="8" r="7" stroke="currentColor" stroke-width="1.2"/><line x1="8" y1="6" x2="8" y2="11" stroke="currentColor" stroke-width="1.5"/><circle cx="8" cy="4.5" r=".8" fill="currentColor"/>'
    };

    var toast = document.createElement('div');
    toast.className = 'toast toast-dynamic';
    toast.style.cssText = [
        'position:fixed', 'top:14px', 'right:18px', 'z-index:9999',
        'display:flex', 'align-items:center', 'gap:8px',
        'padding:10px 16px', 'border-radius:8px',
        'font-size:13px', 'font-weight:500',
        'box-shadow:0 4px 14px rgba(0,0,0,.12)',
        'animation:toastIn .25s ease',
        'font-family:\'Be Vietnam Pro\',sans-serif',
        'background:' + c.bg, 'color:' + c.color, 'border:1px solid ' + c.border
    ].join(';');

    toast.innerHTML = '<svg width="14" height="14" viewBox="0 0 16 16" fill="none">' +
        (icons[type] || icons.info) + '</svg>' + message;

    document.body.appendChild(toast);
    setTimeout(function() { toast.remove(); }, 3500);
}

// ── Auto-dismiss server toast ──────────────────────────────────
document.addEventListener('DOMContentLoaded', function() {
    var t = document.getElementById('serverToast');
    if (t) setTimeout(function() { t.remove(); }, 3500);
});

// ── Notifications ──────────────────────────────────────────────
function showNotifications() {
    showToast('Chưa có thông báo mới', 'info');
}

// Ensure keyframes exist
(function() {
    if (document.getElementById('lms-kf')) return;
    var s = document.createElement('style');
    s.id = 'lms-kf';
    s.textContent = '@keyframes toastIn { from { transform:translateX(16px);opacity:0; } to { transform:translateX(0);opacity:1; } }';
    document.head.appendChild(s);
})();
