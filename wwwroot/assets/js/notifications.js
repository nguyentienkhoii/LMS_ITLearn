document.addEventListener('DOMContentLoaded', function () {
  if (!window.signalR) { console.error('SignalR chưa load'); return; }

  const countEl     = document.getElementById('notifyCount');
  const listEl      = document.getElementById('notifyList');
  const markReadBtn = document.getElementById('notifyMarkRead');
  if (!countEl || !listEl) return;

  const USER_ID    = window.AppUserId || 'guest'; // gán server-side nếu có
  const STORAGE_KEY = `notify:user:${USER_ID}:items`; // Lưu mảng notify
  const MAX_ITEMS   = 50;
  const MAX_AGE_MS  = 7 * 24 * 60 * 60 * 1000; // 7 ngày

  // -------- Storage helpers --------
  function saveItems(arr){
    try { localStorage.setItem(STORAGE_KEY, JSON.stringify(arr.slice(0, MAX_ITEMS))); }
    catch(e){ console.warn('Save notify failed:', e); }
  }
  function loadItems(){
    try {
      const raw = localStorage.getItem(STORAGE_KEY);
      const arr = raw ? JSON.parse(raw) : [];
      const now = Date.now();
      return arr.filter(n => now - new Date(n.createdAt).getTime() < MAX_AGE_MS);
    } catch { return []; }
  }

  // -------- Badge / Count --------
  function unreadCount(arr){
    return arr.reduce((k,x)=> k + (x.readAt ? 0 : 1), 0);
  }
  function setBadge(n){
    if (n > 0) {
      countEl.textContent = n;
      countEl.style.display = 'inline-block';
    } else {
      countEl.style.display = 'none';
    }
  }
  function recomputeBadge(){
    setBadge(unreadCount(loadItems()));
  }

  // -------- Render --------
  function buildItemHTML(item){
    const readCls = item.readAt ? 'read' : '';
    const dateStr = new Date(item.createdAt || Date.now()).toLocaleString();
    return `
      <div class="d-flex align-items-start">
         <span class="notify-dot mt-2"></span>
        <div>
          <div class="font-weight-bold mb-1">${escapeHtml(item.title||'Thông báo')}</div>
          <div class="text-muted">${escapeHtml(item.message||'')}</div>
          <div class="text-muted mt-1" style="font-size:11px;">${dateStr}</div>
        </div>
      </div>`;
  }
  function escapeHtml(s){
    return String(s).replace(/[&<>"']/g, c => ({'&':'&amp;','<':'&lt;','>':'&gt;','"':'&quot;',"'":'&#39;'}[c]));
  }

  function renderItem(item){
    const empty = document.getElementById('notifyEmpty');
    if (empty) empty.remove();
    const a = document.createElement('a');
    a.className = `dropdown-item small notify-item ${item.readAt ? 'read' : ''}`;
    a.href = item.link || '#';
    a.setAttribute('data-id', item.id);
    a.innerHTML = buildItemHTML(item);
    listEl.prepend(a);
  }

  function renderList(items){
    // làm sạch placeholder nếu có dữ liệu
    const empty = document.getElementById('notifyEmpty');
    if (empty && items.length) empty.remove();

    // render cũ → mới (để khi prepend item mới sẽ lên trên)
    for (let i = items.length - 1; i >= 0; i--) {
      renderItem(items[i]);
    }
  }

  // -------- Manipulate items --------
  function makeId(p){
    // ưu tiên id server; nếu không có, tạo id ổn định theo nội dung
    const created = p?.createdAt || new Date().toISOString();
    return p?.id || `${created}|${p?.title || ''}|${p?.message || ''}|${p?.link || ''}`;
  }

  function addNotification(payload){
    const item = {
      id: makeId(payload),
      title: payload?.title || 'Thông báo',
      message: payload?.message || '',
      link: payload?.link || '#',
      createdAt: payload?.createdAt || new Date().toISOString(),
      readAt: null
    };
    let arr = loadItems();

    // Chống trùng (theo id)
    if (!arr.some(x => x.id === item.id)) {
      arr.unshift(item);
      saveItems(arr);
      renderItem(item);
      recomputeBadge();
    }
  }

  function markOneRead(id){
    let arr = loadItems();
    const idx = arr.findIndex(x => x.id === id);
    if (idx >= 0 && !arr[idx].readAt) {
      arr[idx].readAt = new Date().toISOString();
      saveItems(arr);
      // cập nhật UI item
      const el = listEl.querySelector(`.notify-item[data-id="${CSS.escape(id)}"]`);
      if (el) el.classList.add('read');
      recomputeBadge();
    }
  }

  function markAllRead(){
    let arr = loadItems();
    const nowIso = new Date().toISOString();
    let changed = false;
    for (const it of arr) {
      if (!it.readAt) { it.readAt = nowIso; changed = true; }
    }
    if (changed) saveItems(arr);
    recomputeBadge();

    // Ẩn dot trên UI hiện tại
    listEl.querySelectorAll('.notify-item').forEach(el => el.classList.add('read'));
  }

  // -------- Hydrate on load --------
  const stored = loadItems();
  renderList(stored);
  recomputeBadge();

  // Nút “Đánh dấu đã đọc”
  markReadBtn?.addEventListener('click', function(e){ e.preventDefault(); markAllRead(); });

  // Click 1 thông báo: mark read rồi mới điều hướng
  listEl.addEventListener('click', function(e){
    const a = e.target.closest('.notify-item');
    if (!a) return;
    const id = a.getAttribute('data-id');
    if (id) { markOneRead(id); }
    // Cho phép điều hướng diễn ra bình thường
  });

  // -------- SignalR --------
  const connection = new signalR.HubConnectionBuilder()
    .withUrl("/hubs/notifications")
    .withAutomaticReconnect()
    .build();

  connection.on("ReceiveNotification", function(payload){
    addNotification(payload || {});
    if (window.Notyf) new Notyf({ duration: 2500 }).success(payload?.title || 'Có thông báo mới');
  });

  connection.start()
    .then(()=>console.log('SignalR connected'))
    .catch(err=>console.error('SignalR start error:', err));
});
