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

// One-time migration: reset stale dark preference from earlier builds
if (localStorage.getItem('sp-theme') === 'dark' && !localStorage.getItem('sp-theme-migrated')) {
  localStorage.removeItem('sp-theme');
  localStorage.setItem('sp-theme-migrated', '1');
}

// ============ THEME TOGGLE (light default, dark opt-in) ============
function applyTheme(theme) {
  const isDark = theme === 'dark';
  if (isDark) {
    document.documentElement.setAttribute('data-theme', 'dark');
  } else {
    document.documentElement.removeAttribute('data-theme');
  }
  const icon = document.querySelector('#theme-toggle i');
  if (icon) icon.className = isDark ? 'bi bi-sun-fill' : 'bi bi-moon-stars-fill';
}

(function () {
  const savedTheme = localStorage.getItem('sp-theme') || 'light';
  if (savedTheme === 'dark') {
    document.documentElement.setAttribute('data-theme', 'dark');
  } else {
    document.documentElement.removeAttribute('data-theme');
  }
  const icon = document.querySelector('#theme-toggle i');
  if (icon) icon.className = savedTheme === 'dark' ? 'bi bi-sun-fill' : 'bi bi-moon-stars-fill';
})();

document.addEventListener('click', (e) => {
  if (e.target.closest('#theme-toggle')) {
    const isDark = document.documentElement.getAttribute('data-theme') === 'dark';
    const next = isDark ? 'light' : 'dark';
    applyTheme(next);
    localStorage.setItem('sp-theme', next);
  }
  if (e.target.closest('#alert-bell')) {
    fetch('/Alert/Latest')
      .then(r => r.json())
      .then(data => {
        const banner = document.getElementById('emergency-banner');
        const text   = document.getElementById('banner-text');
        if (banner && text) {
          if (data && data.message) {
            if (data.missingChildAlertId && data.childName) {
                text.innerHTML = `
                    <div style="display:flex; flex-direction:column; gap:6px;">
                        <div><span style="background:#d6242a;color:#fff;padding:4px 8px;border-radius:4px;font-size:12px;font-weight:700;letter-spacing:1px;">&#9888; EMERGENCY &middot; ALL STATIONS</span></div>
                        <div style="font-size:24px;font-weight:800;color:#d6242a;text-transform:uppercase;letter-spacing:-0.5px;margin-top:2px;">${data.childName}</div>
                        <div style="font-size:14px;color:#d6242a;display:flex;gap:8px;align-items:center;">
                            <span>Age ${data.age || '?'}</span> &middot; 
                            <span>Last seen: ${data.lastSeen || '?'}</span> &middot; 
                            <span>Assigned: ${data.assignedTo || 'Pending'}</span> &middot; 
                            <span style="background:rgba(214,36,42,0.15);color:#d6242a;padding:2px 6px;border-radius:4px;font-size:11px;font-weight:700;border:1px solid rgba(214,36,42,0.3);">HIGH</span>
                        </div>
                    </div>
                `;
            } else {
                text.textContent = 'EMERGENCY — ' + data.station + ': ' + data.message + ' (Priority: ' + data.priority + ')';
            }
            
            const iconEl = banner.querySelector('i.bi-exclamation-triangle-fill, img.banner-child-photo');
            const container = banner.querySelector('.emergency-container');
            
            if (data.missingChildAlertId) {
                banner.style.cursor = 'pointer';
                banner.onclick = function(ev) {
                    window.location.href = '/Dashboard/ZainabAlertDetails/' + data.missingChildAlertId;
                };
                banner.classList.add('clickable-banner');
                
                if (data.photoPath) {
                    if (iconEl && iconEl.tagName === 'I') {
                        const img = document.createElement('img');
                        img.src = data.photoPath;
                        img.className = 'banner-child-photo';
                        img.style.cssText = 'width:64px;height:64px;border-radius:50%;object-fit:cover;border:3px solid #d6242a;flex-shrink:0;';
                        container.replaceChild(img, iconEl);
                    } else if (iconEl && iconEl.tagName === 'IMG') {
                        iconEl.src = data.photoPath;
                        iconEl.style.cssText = 'width:64px;height:64px;border-radius:50%;object-fit:cover;border:3px solid #d6242a;flex-shrink:0;';
                    }
                }
            } else {
                banner.style.cursor = 'default';
                banner.onclick = null;
                banner.classList.remove('clickable-banner');
                
                if (iconEl && iconEl.tagName === 'IMG') {
                    const i = document.createElement('i');
                    i.className = 'bi bi-exclamation-triangle-fill';
                    container.replaceChild(i, iconEl);
                }
            }
            
            const closeBtn = banner.querySelector('.banner-close');
            if (closeBtn) {
                closeBtn.onclick = function(ev) {
                    ev.stopPropagation();
                    banner.classList.remove('show');
                };
            }
          } else {
            text.textContent = 'No active alerts on record.';
          }
          banner.classList.add('show');
          setTimeout(() => banner.classList.remove('show'), 15000);
        }
      })
      .catch(() => {
        const banner = document.getElementById('emergency-banner');
        if (banner) { banner.classList.add('show'); setTimeout(() => banner.classList.remove('show'), 15000); }
      });
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
function cssVar(name, fallback) {
  const v = getComputedStyle(document.documentElement).getPropertyValue(name).trim();
  return v || fallback;
}

function initCrimeChart(data) {
  const ctx = document.getElementById('crimeChart');
  if (!ctx || typeof Chart === 'undefined') return;
  const labels = Object.keys(data);
  const values = Object.values(data);
  const accent = cssVar('--accent', '#0066FF');
  const muted = cssVar('--muted', '#8E8E8E');
  const border = cssVar('--border', '#E8E8E5');
  const grad = ctx.getContext('2d').createLinearGradient(0, 0, 0, 280);
  grad.addColorStop(0, accent);
  grad.addColorStop(1, accent + '33');
  new Chart(ctx, {
    type: 'bar',
    data: { labels, datasets: [{ label: 'Cases', data: values, backgroundColor: grad, borderColor: accent, borderWidth: 1, borderRadius: 6 }] },
    options: {
      responsive: true,
      plugins: { legend: { display: false } },
      scales: {
        x: { grid: { color: border }, ticks: { color: muted } },
        y: { grid: { color: border }, ticks: { color: muted }, beginAtZero: true }
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

  function validateStep(s) {
    var stepDiv = document.querySelector('.wizard-step[data-step="' + s + '"]');
    if (!stepDiv) return true;
    var valid = true;

    // Clear previous inline errors in this step
    stepDiv.querySelectorAll('.field-error-inline').forEach(function(el) { el.remove(); });

    // Validate required fields
    stepDiv.querySelectorAll('input[required], select[required], textarea[required]').forEach(function(input) {
      if (!input.value || !input.value.trim()) {
        valid = false;
        showFieldError(input, 'This field is required.');
      }
    });

    // Regex validation for CNIC and Phone (Step 1)
    var cnicInput = stepDiv.querySelector('[name="CitizenCNIC"]');
    if (cnicInput && cnicInput.value.trim()) {
      if (!/^\d{13}$/.test(cnicInput.value.trim())) {
        valid = false;
        showFieldError(cnicInput, 'CNIC must be exactly 13 digits, no dashes or spaces.');
      }
    }

    var phoneInput = stepDiv.querySelector('[name="PhoneNumber"]');
    if (phoneInput && phoneInput.value.trim()) {
      if (!/^(0\d{10}|92\d{10})$/.test(phoneInput.value.trim())) {
        valid = false;
        showFieldError(phoneInput, 'Phone must be 11 digits starting with 0, or 12 digits starting with 92, no dashes or spaces.');
      }
    }

    return valid;
  }

  function showFieldError(input, message) {
    // Don't duplicate
    var parent = input.closest('.form-group') || input.parentElement;
    if (parent.querySelector('.field-error-inline')) return;
    var span = document.createElement('span');
    span.className = 'field-error-inline';
    span.textContent = message;
    span.style.cssText = 'color:var(--danger, #d6241a);font-size:0.8rem;display:block;margin-top:4px;';
    parent.appendChild(span);
    input.style.borderColor = 'var(--danger, #d6241a)';
    // Clear on input
    input.addEventListener('input', function handler() {
      span.remove();
      input.style.borderColor = '';
      input.removeEventListener('input', handler);
    }, { once: true });
  }

  prev.addEventListener('click', () => { if (step > 1) { step--; render(); } });
  next.addEventListener('click', () => {
    if (validateStep(step)) {
      if (step < total) { step++; render(); }
    }
  });
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

  const initialStatus = new URLSearchParams(window.location.search).get('status');
  if (initialStatus && filter) {
    const opts = Array.from(filter.options);
    const matched = opts.find(o => o.value.toLowerCase() === initialStatus.toLowerCase() || o.text.toLowerCase() === initialStatus.toLowerCase());
    if (matched) {
      filter.value = matched.value;
      apply();
    }
  }

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

window.addEventListener('scroll', function() {
  const nav = document.querySelector('.top-nav');
  if (!nav) return;
  if (window.scrollY > 10) nav.classList.add('scrolled');
  else nav.classList.remove('scrolled');
});

document.addEventListener('DOMContentLoaded', function() {
  const path = window.location.pathname.toLowerCase();
  document.querySelectorAll('.nav-link').forEach(link => {
    const href = (link.getAttribute('href') || '').toLowerCase();
    if (href !== '/' && path.startsWith(href)) {
      link.classList.add('active');
    }
  });
});
