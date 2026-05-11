// ============ LIVE CLOCK ============
function tickClock() {
  const el = document.getElementById('live-clock');
  if (!el) return;
  const now = new Date();
  const date = now.toLocaleDateString('en-GB', { weekday: 'short', day: '2-digit', month: 'short' });
  const time = now.toLocaleTimeString('en-GB', { hour12: false });
  el.textContent = `${date}  ${time}`;
}
setInterval(tickClock, 1000);
tickClock();

// ============ THEME TOGGLE ============
(function () {
  const saved = localStorage.getItem('sp-theme') || 'dark';
  document.documentElement.setAttribute('data-theme', saved);
})();
document.addEventListener('click', (e) => {
  if (e.target.closest('#theme-toggle')) {
    const cur = document.documentElement.getAttribute('data-theme') || 'dark';
    const next = cur === 'dark' ? 'light' : 'dark';
    document.documentElement.setAttribute('data-theme', next);
    localStorage.setItem('sp-theme', next);
    const icon = e.target.closest('#theme-toggle').querySelector('i');
    if (icon) icon.className = next === 'dark' ? 'bi bi-moon-stars-fill' : 'bi bi-sun-fill';
  }
  if (e.target.closest('#alert-bell')) {
    fetch('/Alert/Latest')
      .then(r => r.json())
      .then(data => {
        const banner = document.getElementById('emergency-banner');
        const text   = document.getElementById('banner-text');
        if (banner && text) {
          if (data && data.message) {
            text.textContent = 'EMERGENCY — ' + data.station + ': ' + data.message + ' (Priority: ' + data.priority + ')';
          } else {
            text.textContent = 'No active alerts on record.';
          }
          banner.classList.add('show');
          setTimeout(() => banner.classList.remove('show'), 10000);
        }
      })
      .catch(() => {
        const banner = document.getElementById('emergency-banner');
        if (banner) { banner.classList.add('show'); setTimeout(() => banner.classList.remove('show'), 8000); }
      });
  }
  if (e.target.closest('#sidebar-toggle')) {
    document.getElementById('sidebar')?.classList.toggle('open');
  }
});

// ============ DASHBOARD COUNTERS ============
function initCounters() {
  document.querySelectorAll('.counter').forEach(el => {
    const target = parseInt(el.dataset.target || '0', 10);
    let current = 0;
    const duration = 1200;
    const startTime = performance.now();
    function step(now) {
      const progress = Math.min((now - startTime) / duration, 1);
      current = Math.floor(progress * target);
      el.textContent = current;
      if (progress < 1) requestAnimationFrame(step);
      else el.textContent = target;
    }
    requestAnimationFrame(step);
  });
}

// ============ CHART ============
function initCrimeChart(data) {
  const ctx = document.getElementById('crimeChart');
  if (!ctx || typeof Chart === 'undefined') return;
  const labels = Object.keys(data);
  const values = Object.values(data);
  const grad = ctx.getContext('2d').createLinearGradient(0, 0, 0, 280);
  grad.addColorStop(0, 'rgba(232,184,75,0.9)');
  grad.addColorStop(1, 'rgba(200,150,42,0.3)');
  new Chart(ctx, {
    type: 'bar',
    data: { labels, datasets: [{ label: 'Cases', data: values, backgroundColor: grad, borderColor: '#c8962a', borderWidth: 1, borderRadius: 6 }] },
    options: {
      responsive: true,
      plugins: { legend: { display: false } },
      scales: {
        x: { grid: { color: 'rgba(168,152,128,0.08)' }, ticks: { color: '#a89880' } },
        y: { grid: { color: 'rgba(168,152,128,0.08)' }, ticks: { color: '#a89880' }, beginAtZero: true }
      }
    }
  });
}

// ============ FIR WIZARD ============
function initWizard() {
  let step = 1;
  const total = 3;
  const fill = document.getElementById('progress-fill');
  const prev = document.getElementById('prev-btn');
  const next = document.getElementById('next-btn');
  const submit = document.getElementById('submit-btn');

  function render() {
    document.querySelectorAll('.wizard-step').forEach(s => s.classList.toggle('hidden', +s.dataset.step !== step));
    document.querySelectorAll('.wizard-steps .step').forEach(s => s.classList.toggle('active', +s.dataset.step <= step));
    fill.style.width = `${(step / total) * 100}%`;
    prev.disabled = step === 1;
    next.classList.toggle('hidden', step === total);
    submit.classList.toggle('hidden', step !== total);
  }
  prev.addEventListener('click', () => { if (step > 1) { step--; render(); } });
  next.addEventListener('click', () => { if (step < total) { step++; render(); } });
  render();
}

// ============ CASES PAGE ============
function initCasesPage() {
  const search = document.getElementById('case-search');
  const filter = document.getElementById('status-filter');
  const rows = document.querySelectorAll('#cases-table tbody tr');

  function apply() {
    const q = (search.value || '').toLowerCase();
    const s = (filter.value || '').toLowerCase();
    rows.forEach(r => {
      const text = r.textContent.toLowerCase();
      const matchQ = !q || text.includes(q);
      const matchS = !s || text.includes(s);
      r.style.display = (matchQ && matchS) ? '' : 'none';
    });
  }
  search?.addEventListener('input', apply);
  filter?.addEventListener('change', apply);

  rows.forEach(r => r.addEventListener('click', () => {
    const data = JSON.parse(r.dataset.case);
    document.getElementById('m-title').textContent = `${data.CaseId} — ${data.Title}`;
    document.getElementById('m-body').innerHTML = `
      <div class="kv"><label>Officer</label><div>${data.AssignedOfficer}</div></div>
      <div class="kv"><label>Status</label><div><span class="status-badge ${data.Status.toLowerCase()}">${data.Status}</span></div></div>
      <div class="kv"><label>Priority</label><div>${data.Priority}</div></div>
      <div class="kv"><label>Opened</label><div>${new Date(data.DateOpened).toLocaleString()}</div></div>
      <div class="kv"><label>Last Updated</label><div>${new Date(data.LastUpdated).toLocaleString()}</div></div>
      <div style="margin-top:1rem"><a href="/Cases/Details/${data.CaseId}" class="btn-primary-amber" style="display:inline-flex;width:auto">Open Full Details</a></div>
    `;
    document.getElementById('case-modal').classList.remove('hidden');
  }));
}
function closeCaseModal() { document.getElementById('case-modal')?.classList.add('hidden'); }
