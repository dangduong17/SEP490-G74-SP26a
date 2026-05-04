/**
 * cv-editor-shared.js
 * Shared CV editor engine for Admin (CvTemplateCreate/Edit) and Candidate (CV/Edit).
 * Requires: interact.js (loaded before this file)
 */

/* ══ CONSTANTS ══════════════════════════════════════════════════════════ */
const A4_W = 794, A4_H = 1123, SNAP_DIST = 8, GRID_SIZE = 10;

/* ══ SECTION DEFINITIONS — 14 types ════════════════════════════════════ */
const SEC = {
  header: {
    label: 'Thông tin chính', defaultW: 750, defaultH: 160,
    build: () => {
      const w = div('cv-content-wrap');
      w.innerHTML = `
      <div class="cv-header">
        <label class="cv-avatar-wrap" title="Nhấn để thay ảnh">
          <div class="cv-avatar">
            <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.5"><circle cx="12" cy="8" r="4"/><path d="M4 20c0-4 3.6-7 8-7s8 3 8 7"/></svg>
          </div>
          <div class="cv-avatar-overlay">📷</div>
          <input type="file" class="cv-avatar-input" accept="image/*" style="display:none"/>
        </label>
        <div style="flex:1;min-width:0">
          <div class="cv-name" contenteditable="true" spellcheck="false" data-ph="Họ và Tên">Tên Của Bạn</div>
          <div class="cv-jobtitle" contenteditable="true" spellcheck="false" data-ph="Vị trí ứng tuyển">Vị trí ứng tuyển</div>
          <div class="cv-contacts">
            <span class="cv-citem"><span class="cv-dot"></span><span contenteditable="true" spellcheck="false" data-ph="Ngày sinh">Ngày sinh</span></span>
            <span class="cv-citem"><span class="cv-dot"></span><span contenteditable="true" spellcheck="false" data-ph="Điện thoại">Số điện thoại</span></span>
            <span class="cv-citem"><span class="cv-dot"></span><span contenteditable="true" spellcheck="false" data-ph="Email">email@gmail.com</span></span>
            <span class="cv-citem"><span class="cv-dot"></span><span contenteditable="true" spellcheck="false" data-ph="Địa chỉ">Địa chỉ</span></span>
            <span class="cv-citem"><span class="cv-dot"></span><span contenteditable="true" spellcheck="false" data-ph="LinkedIn / Website">LinkedIn / Website</span></span>
          </div>
        </div>
      </div>`;
      bindAvatarUpload(w);
      return w;
    }
  },
  vcard: {
    label: 'Danh thiếp', defaultW: 200, defaultH: 300,
    build: () => {
      const w = div('cv-content-wrap');
      w.innerHTML = `
      <div class="cv-std cv-vcard">
        <label class="cv-vcard-avatar" title="Nhấn để thay ảnh">
          <div class="cv-avatar-sm">
            <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.5"><circle cx="12" cy="8" r="4"/><path d="M4 20c0-4 3.6-7 8-7s8 3 8 7"/></svg>
          </div>
          <div class="cv-avatar-overlay" style="border-radius:50%">📷</div>
          <input type="file" class="cv-avatar-input" accept="image/*" style="display:none"/>
        </label>
        <div class="cv-vcard-name" contenteditable="true" spellcheck="false" data-ph="Tên">Tên Của Bạn</div>
        <div class="cv-vcard-title" contenteditable="true" spellcheck="false" data-ph="Chức danh">Chức danh</div>
        <div class="cv-vcard-items">
          <div class="cv-vcard-item"><span class="vci">📞</span><span contenteditable="true" spellcheck="false" data-ph="SĐT">0123 456 789</span></div>
          <div class="cv-vcard-item"><span class="vci">✉️</span><span contenteditable="true" spellcheck="false" data-ph="Email">email@gmail.com</span></div>
          <div class="cv-vcard-item"><span class="vci">📍</span><span contenteditable="true" spellcheck="false" data-ph="Địa chỉ">Hà Nội</span></div>
          <div class="cv-vcard-item"><span class="vci">🌐</span><span contenteditable="true" spellcheck="false" data-ph="Website">linkedin.com/in/username</span></div>
        </div>
      </div>`;
      bindAvatarUpload(w);
      return w;
    }
  },
  contact: {
    label: 'Thông tin liên hệ', defaultW: 350, defaultH: 140,
    build: () => {
      const w = div('cv-content-wrap');
      w.innerHTML = `
      <div class="cv-std">
        <div class="cv-sec-head" contenteditable="true" spellcheck="false">THÔNG TIN LIÊN HỆ</div>
        <div class="cv-contact-grid">
          <div class="cv-cg-item"><span class="cv-cg-label">SĐT</span><span contenteditable="true" spellcheck="false" data-ph="Số điện thoại" class="cv-cg-val">0123 456 789</span></div>
          <div class="cv-cg-item"><span class="cv-cg-label">Email</span><span contenteditable="true" spellcheck="false" data-ph="Email" class="cv-cg-val">email@gmail.com</span></div>
          <div class="cv-cg-item"><span class="cv-cg-label">Địa chỉ</span><span contenteditable="true" spellcheck="false" data-ph="Địa chỉ" class="cv-cg-val">Hà Nội</span></div>
          <div class="cv-cg-item"><span class="cv-cg-label">LinkedIn</span><span contenteditable="true" spellcheck="false" data-ph="LinkedIn URL" class="cv-cg-val">linkedin.com/in/username</span></div>
          <div class="cv-cg-item"><span class="cv-cg-label">Website</span><span contenteditable="true" spellcheck="false" data-ph="Website / Portfolio" class="cv-cg-val">portfolio.com</span></div>
        </div>
      </div>`;
      return w;
    }
  },
  summary: {
    label: 'Mục tiêu NN', defaultW: 350, defaultH: 120,
    build: () => {
      const w = div('cv-content-wrap');
      w.innerHTML = `
      <div class="cv-std">
        <div class="cv-sec-head" contenteditable="true" spellcheck="false">MỤC TIÊU NGHỀ NGHIỆP</div>
        <div contenteditable="true" spellcheck="false" class="cv-simple" data-ph="Mục tiêu nghề nghiệp...">Mô tả ngắn gọn về định hướng nghề nghiệp và giá trị bạn mang lại cho nhà tuyển dụng.</div>
      </div>`;
      return w;
    }
  },
  experience: {
    label: 'Kinh nghiệm LV', defaultW: 350, defaultH: 200,
    build: () => {
      const w = div('cv-content-wrap');
      w.innerHTML = `
      <div class="cv-std">
        <div class="cv-sec-head" contenteditable="true" spellcheck="false">KINH NGHIỆM LÀM VIỆC</div>
        <div class="cv-exp-item">
          <div class="cv-exp-header">
            <span class="cv-company" contenteditable="true" spellcheck="false" data-ph="Tên công ty">Tên công ty ABC</span>
            <span class="cv-date" contenteditable="true" spellcheck="false">01/2022 – Hiện tại</span>
          </div>
          <span class="cv-pos" contenteditable="true" spellcheck="false" data-ph="Vị trí">Vị trí làm việc</span>
          <div class="cv-desc" contenteditable="true" spellcheck="false" data-ph="Mô tả công việc...">• Mô tả chi tiết trách nhiệm và thành tựu của bạn&#10;• Sử dụng số liệu cụ thể khi có thể</div>
        </div>
      </div>`;
      return w;
    }
  },
  education: {
    label: 'Học vấn', defaultW: 350, defaultH: 160,
    build: () => {
      const w = div('cv-content-wrap');
      w.innerHTML = `
      <div class="cv-std">
        <div class="cv-sec-head" contenteditable="true" spellcheck="false">HỌC VẤN</div>
        <div class="cv-exp-item">
          <div class="cv-exp-header">
            <span class="cv-company" contenteditable="true" spellcheck="false" data-ph="Tên trường">Đại học của bạn</span>
            <span class="cv-date" contenteditable="true" spellcheck="false">2018 – 2022</span>
          </div>
          <span class="cv-pos" contenteditable="true" spellcheck="false" data-ph="Ngành học">Ngành học chuyên môn</span>
          <div class="cv-desc" contenteditable="true" spellcheck="false" data-ph="Thành tích...">GPA: x.y / 4.0 · Danh hiệu Sinh viên Giỏi</div>
        </div>
      </div>`;
      return w;
    }
  },
  skills: {
    label: 'Kỹ năng', defaultW: 350, defaultH: 130,
    build: () => {
      const w = div('cv-content-wrap');
      w.innerHTML = `
      <div class="cv-std">
        <div class="cv-sec-head" contenteditable="true" spellcheck="false">KỸ NĂNG</div>
        <div class="cv-skills">
          <span class="cv-tag" contenteditable="true" spellcheck="false">Kỹ năng chuyên môn</span>
          <span class="cv-tag" contenteditable="true" spellcheck="false">Tiếng Anh B2</span>
          <span class="cv-tag" contenteditable="true" spellcheck="false">Làm việc nhóm</span>
          <span class="cv-tag" contenteditable="true" spellcheck="false">Quản lý thời gian</span>
        </div>
        <button type="button" class="cv-add-tag-btn" onclick="addSkillTag(this)">+ Thêm kỹ năng</button>
      </div>`;
      return w;
    }
  },
  projects: {
    label: 'Dự án', defaultW: 350, defaultH: 200,
    build: () => {
      const w = div('cv-content-wrap');
      w.innerHTML = `
      <div class="cv-std">
        <div class="cv-sec-head" contenteditable="true" spellcheck="false">DỰ ÁN</div>
        <div class="cv-exp-item">
          <div class="cv-exp-header">
            <span class="cv-company" contenteditable="true" spellcheck="false" data-ph="Tên dự án">Tên Dự Án</span>
            <span class="cv-date" contenteditable="true" spellcheck="false">2023</span>
          </div>
          <span class="cv-pos" contenteditable="true" spellcheck="false" data-ph="Vai trò / Công nghệ">Vai trò · React, Node.js</span>
          <div class="cv-desc" contenteditable="true" spellcheck="false" data-ph="Mô tả dự án...">• Mô tả mục tiêu, phạm vi và kết quả của dự án&#10;• Link: github.com/username/project</div>
        </div>
      </div>`;
      return w;
    }
  },
  awards: {
    label: 'Giải thưởng', defaultW: 350, defaultH: 130,
    build: () => {
      const w = div('cv-content-wrap');
      w.innerHTML = `
      <div class="cv-std">
        <div class="cv-sec-head" contenteditable="true" spellcheck="false">DANH HIỆU & GIẢI THƯỞNG</div>
        <div class="cv-exp-item">
          <div class="cv-exp-header">
            <span class="cv-company" contenteditable="true" spellcheck="false" data-ph="Tên giải thưởng">Tên Giải Thưởng</span>
            <span class="cv-date" contenteditable="true" spellcheck="false">2023</span>
          </div>
          <div class="cv-desc" contenteditable="true" spellcheck="false" data-ph="Mô tả...">Tổ chức trao tặng</div>
        </div>
      </div>`;
      return w;
    }
  },
  certs: {
    label: 'Chứng chỉ', defaultW: 350, defaultH: 130,
    build: () => {
      const w = div('cv-content-wrap');
      w.innerHTML = `
      <div class="cv-std">
        <div class="cv-sec-head" contenteditable="true" spellcheck="false">CHỨNG CHỈ</div>
        <div class="cv-exp-item">
          <div class="cv-exp-header">
            <span class="cv-company" contenteditable="true" spellcheck="false" data-ph="Tên chứng chỉ">IELTS 7.0 / AWS Solutions Architect</span>
            <span class="cv-date" contenteditable="true" spellcheck="false">2022</span>
          </div>
          <div class="cv-desc" contenteditable="true" spellcheck="false" data-ph="Tổ chức cấp...">Tổ chức cấp · Mã số chứng chỉ</div>
        </div>
      </div>`;
      return w;
    }
  },
  activities: {
    label: 'Hoạt động', defaultW: 350, defaultH: 160,
    build: () => {
      const w = div('cv-content-wrap');
      w.innerHTML = `
      <div class="cv-std">
        <div class="cv-sec-head" contenteditable="true" spellcheck="false">HOẠT ĐỘNG</div>
        <div class="cv-exp-item">
          <div class="cv-exp-header">
            <span class="cv-company" contenteditable="true" spellcheck="false" data-ph="Tên tổ chức">CLB / Tổ chức</span>
            <span class="cv-date" contenteditable="true" spellcheck="false">2021 – 2022</span>
          </div>
          <span class="cv-pos" contenteditable="true" spellcheck="false" data-ph="Vai trò">Thành viên / Ban tổ chức</span>
          <div class="cv-desc" contenteditable="true" spellcheck="false" data-ph="Mô tả...">Mô tả đóng góp và kết quả đạt được.</div>
        </div>
      </div>`;
      return w;
    }
  },
  refs: {
    label: 'Người tham chiếu', defaultW: 350, defaultH: 140,
    build: () => {
      const w = div('cv-content-wrap');
      w.innerHTML = `
      <div class="cv-std">
        <div class="cv-sec-head" contenteditable="true" spellcheck="false">NGƯỜI THAM CHIẾU</div>
        <div class="cv-exp-item">
          <span class="cv-company" contenteditable="true" spellcheck="false" data-ph="Họ tên">Nguyễn Văn A</span>
          <span class="cv-pos" contenteditable="true" spellcheck="false" data-ph="Chức danh · Công ty">Giám đốc · Công ty XYZ</span>
          <div class="cv-cg-item"><span class="cv-cg-label">Email</span><span contenteditable="true" spellcheck="false" data-ph="Email" class="cv-cg-val">ref@company.com</span></div>
          <div class="cv-cg-item"><span class="cv-cg-label">SĐT</span><span contenteditable="true" spellcheck="false" data-ph="SĐT" class="cv-cg-val">0900 000 000</span></div>
        </div>
      </div>`;
      return w;
    }
  },
  hobbies: {
    label: 'Sở thích', defaultW: 350, defaultH: 110,
    build: () => {
      const w = div('cv-content-wrap');
      w.innerHTML = `
      <div class="cv-std">
        <div class="cv-sec-head" contenteditable="true" spellcheck="false">SỞ THÍCH</div>
        <div class="cv-skills">
          <span class="cv-tag" contenteditable="true" spellcheck="false">Đọc sách</span>
          <span class="cv-tag" contenteditable="true" spellcheck="false">Thể thao</span>
          <span class="cv-tag" contenteditable="true" spellcheck="false">Âm nhạc</span>
        </div>
        <button type="button" class="cv-add-tag-btn" onclick="addSkillTag(this)">+ Thêm</button>
      </div>`;
      return w;
    }
  },
  extra: {
    label: 'Thông tin thêm', defaultW: 350, defaultH: 130,
    build: () => {
      const w = div('cv-content-wrap');
      w.innerHTML = `
      <div class="cv-std">
        <div class="cv-sec-head" contenteditable="true" spellcheck="false">THÔNG TIN THÊM</div>
        <div contenteditable="true" spellcheck="false" class="cv-simple" data-ph="Thông tin bổ sung...">Thông tin thêm mà bạn muốn nhà tuyển dụng biết (ngôn ngữ, kỹ năng đặc biệt, v.v.)</div>
      </div>`;
      return w;
    }
  },
};

/* ══ HELPER ═════════════════════════════════════════════════════════════ */
function div(cls) { const d = document.createElement('div'); if (cls) d.className = cls; return d; }

function addSkillTag(btn) {
  const wrap = btn.previousElementSibling;
  if (!wrap) return;
  const tag = document.createElement('span');
  tag.className = 'cv-tag';
  tag.contentEditable = 'true';
  tag.spellcheck = false;
  tag.textContent = 'Kỹ năng mới';
  wrap.appendChild(tag);
  tag.focus();
  // Select all text in new tag
  const range = document.createRange();
  range.selectNodeContents(tag);
  window.getSelection().removeAllRanges();
  window.getSelection().addRange(range);
}

function bindAvatarUpload(wrap) {
  const input = wrap.querySelector('.cv-avatar-input');
  const preview = wrap.querySelector('.cv-avatar, .cv-avatar-sm');
  if (!input || !preview) return;
  input.addEventListener('change', (e) => {
    const file = e.target.files[0];
    if (!file) return;
    const reader = new FileReader();
    reader.onload = (ev) => {
      // Try Cropper modal first; fall back to direct replace if modal not available
      const modal = document.getElementById('cropper-modal');
      if (modal && window.Cropper) {
        openCropperModal(ev.target.result, (croppedB64) => {
          preview.innerHTML = `<img src="${croppedB64}" style="width:100%;height:100%;object-fit:cover;border-radius:inherit;" alt="avatar"/>`;
          const labelEl = input.closest('label');
          if (labelEl) labelEl.dataset.pendingAvatar = croppedB64;
        });
      } else {
        preview.innerHTML = `<img src="${ev.target.result}" style="width:100%;height:100%;object-fit:cover;border-radius:inherit;" alt="avatar"/>`;
        const labelEl = input.closest('label');
        if (labelEl) labelEl.dataset.pendingAvatar = ev.target.result;
      }
    };
    reader.readAsDataURL(file);
  });
}

/* Open Cropper.js modal, call onConfirm(base64) when user confirms */
let _cropperInstance = null;
let _cropperCallback = null;

function openCropperModal(imgSrc, onConfirm) {
  const modal = document.getElementById('cropper-modal');
  const imgEl = document.getElementById('cropper-img');
  if (!modal || !imgEl) return;
  _cropperCallback = onConfirm;
  imgEl.src = imgSrc;
  modal.classList.add('open');
  // Destroy previous instance
  if (_cropperInstance) { _cropperInstance.destroy(); _cropperInstance = null; }
  // Init after image loads
  imgEl.onload = () => {
    _cropperInstance = new Cropper(imgEl, {
      aspectRatio: 1,
      viewMode: 1,
      background: false,
      autoCropArea: 0.8,
      responsive: true,
    });
  };
}

function closeCropperModal() {
  const modal = document.getElementById('cropper-modal');
  if (modal) modal.classList.remove('open');
  if (_cropperInstance) { _cropperInstance.destroy(); _cropperInstance = null; }
  _cropperCallback = null;
}

function confirmCrop() {
  if (!_cropperInstance || !_cropperCallback) { closeCropperModal(); return; }
  const canvas = _cropperInstance.getCroppedCanvas({ width: 300, height: 300 });
  const b64 = canvas.toDataURL('image/jpeg', 0.9);
  _cropperCallback(b64);
  closeCropperModal();
}

/* ══ MIGRATION: old grid layout → freeform ══════════════════════════════ */
function migratePagesToFreeform(rawPages) {
  return rawPages.map(p => {
    if (p.columns > 0) {
      let y1 = 20, y2 = 20;
      const newSecs = (p.sections || []).map(s => {
        let top = s.top, left = s.left, w = s.width, h = s.height;
        if (top == null || top === 0) {
          const def = SEC[s.type] || { defaultW: 350, defaultH: 150 };
          if (p.columns === 2 && s.column === 2) {
            left = 410; top = y2; y2 += def.defaultH + 16; w = w || 360; h = h || def.defaultH;
          } else {
            left = 20; top = y1; y1 += def.defaultH + 16;
            w = w || (p.columns === 2 ? 360 : 754); h = h || def.defaultH;
          }
        }
        return { type: s.type, top, left, width: w, height: h, html: s.html || null };
      });
      return { columns: 0, sections: newSecs };
    }
    return p;
  });
}

function migrateLegacyConfig(raw) {
  if (!raw) return [{ columns: 0, sections: [] }];
  if (raw.pages && raw.pages.length > 0) return migratePagesToFreeform(raw.pages);
  if (raw.sections && raw.sections.length > 0) {
    let y = 20;
    return [{ columns: 0, sections: raw.sections.map(s => {
      const def = SEC[s.type] || { defaultW: 754, defaultH: 150 };
      const r = { type: s.type, top: y, left: 20, width: def.defaultW, height: def.defaultH, html: null };
      y += def.defaultH + 16;
      return r;
    })}];
  }
  return [{ columns: 0, sections: [] }];
}

/* ══ SNAP & GUIDES ══════════════════════════════════════════════════════ */
function snapCoords(rX, rY, w, h, a4) {
  let tX = [0, A4_W / 2, A4_W], tY = [0, A4_H / 2, A4_H];
  a4.querySelectorAll('.cv-sec:not(.is-dragging)').forEach(el => {
    let l = parseFloat(el.style.left) || 0, t = parseFloat(el.style.top) || 0;
    let ew = parseFloat(el.style.width) || el.offsetWidth, eh = parseFloat(el.style.height) || el.offsetHeight;
    tX.push(l, l + ew / 2, l + ew); tY.push(t, t + eh / 2, t + eh);
  });
  let bX = rX, gX = -1, minD = SNAP_DIST;
  for (let m of [rX, rX + w / 2, rX + w]) for (let t of tX) if (Math.abs(m - t) < minD) { minD = Math.abs(m - t); bX = rX + (t - m); gX = t; }
  let bY = rY, gY = -1; minD = SNAP_DIST;
  for (let m of [rY, rY + h / 2, rY + h]) for (let t of tY) if (Math.abs(m - t) < minD) { minD = Math.abs(m - t); bY = rY + (t - m); gY = t; }
  if (gX === -1) bX = Math.round(rX / GRID_SIZE) * GRID_SIZE;
  if (gY === -1) bY = Math.round(rY / GRID_SIZE) * GRID_SIZE;
  bX = Math.max(0, Math.min(A4_W - w, bX)); bY = Math.max(0, Math.min(A4_H - h, bY));
  return { l: bX, t: bY, gx: gX, gy: gY };
}

function drawGuides(a4, gx, gy) {
  a4.querySelectorAll('.snap-guide,.snap-label').forEach(g => g.remove());
  if (gx !== -1) {
    const d = div('snap-guide guide-v');
    d.style.left = gx + 'px';
    a4.appendChild(d);
    const lb = div('snap-label');
    lb.textContent = Math.round(gx) + 'px';
    lb.style.cssText = `left:${gx + 3}px;top:4px`;
    a4.appendChild(lb);
  }
  if (gy !== -1) {
    const d = div('snap-guide guide-h');
    d.style.top = gy + 'px';
    a4.appendChild(d);
    const lb = div('snap-label');
    lb.textContent = Math.round(gy) + 'px';
    lb.style.cssText = `top:${gy + 3}px;left:4px`;
    a4.appendChild(lb);
  }
}

/* ══ INTERACT.JS SETUP (resize only) ═════════════════════════════════════ */
function initInteract(onDragEnd, onResizeEnd) {
  // Store callbacks for custom drag
  initInteract._onDragEnd = onDragEnd;
  initInteract._onResizeEnd = onResizeEnd;

  interact('.resize-drag')
    .resizable({
      ignoreFrom: '.sec-bar', // Prevent resizing when clicking toolbar
      edges: { left: true, right: true, bottom: true, top: true },
      margin: 5,
      modifiers: [
        interact.modifiers.restrictEdges({ outer: 'parent' }),
        interact.modifiers.restrictSize({ min: { width: 120, height: 50 } }),
        interact.modifiers.snapSize({ targets: [interact.createSnapGrid({ x: GRID_SIZE, y: GRID_SIZE })] }),
      ],
      listeners: {
        start(e) { 
          e.target.classList.add('is-resizing'); 
          document.body.classList.add('is-interacting');
          window.getSelection().removeAllRanges();
        },
        move(e) {
          let r = e.rect;
          e.target.style.width  = r.width  + 'px';
          e.target.style.height = r.height + 'px';
          
          let scale = (typeof _currentZoom !== 'undefined') ? _currentZoom : 1;
          // deltaRect is already scaled if we zoom via CSS transform, but interactjs might need division
          let dx = e.deltaRect.left / scale;
          let dy = e.deltaRect.top / scale;
          
          let l = (parseFloat(e.target.style.left) || 0) + dx;
          let t = (parseFloat(e.target.style.top)  || 0) + dy;
          e.target.style.left = l + 'px';
          e.target.style.top  = t + 'px';
        },
        end(e) {
          e.target.classList.remove('is-resizing');
          document.body.classList.remove('is-interacting');
          resolveAllOverlaps(e.target.closest('.a4'));
          if (initInteract._onResizeEnd) initInteract._onResizeEnd(e.target);
        }
      }
    });
}

/* ══ CUSTOM DRAG (Works flawlessly outside interact.js bounding rect) ════ */
function _initCustomDrag(el) {
  const handle = el.querySelector('.drag-handle');
  const secBar = el.querySelector('.sec-bar');
  if (!handle || handle._dragBound) return;
  handle._dragBound = true;

  // Prevent interact.js from triggering resize when clicking ANYWHERE on the sec-bar
  if (secBar) {
    secBar.addEventListener('mousedown', e => e.stopPropagation());
  }

  handle.addEventListener('mousedown', function(e) {
    if (e.button !== 0) return;
    e.preventDefault();
    e.stopPropagation();

    const a4    = el.closest('.a4');
    const scale = (typeof _currentZoom !== 'undefined') ? _currentZoom : 1;

    let startX = e.clientX;
    let startY = e.clientY;
    let startL = parseFloat(el.style.left) || 0;
    let startT = parseFloat(el.style.top)  || 0;
    let w = parseFloat(el.style.width)  || el.offsetWidth;
    let h = parseFloat(el.style.height) || el.offsetHeight;

    el.classList.add('is-dragging');
    document.body.classList.add('is-interacting');
    window.getSelection().removeAllRanges();

    function onMove(e) {
      const dx = (e.clientX - startX) / scale;
      const dy = (e.clientY - startY) / scale;
      
      let newL = startL + dx;
      let newT = startT + dy;

      const s = snapCoords(newL, newT, w, h, a4);
      
      newL = Math.max(0, Math.min(A4_W - w, s.l));
      newT = Math.max(0, Math.min(A4_H - h, s.t));
      
      el.style.left = newL + 'px';
      el.style.top  = newT + 'px';
      
      drawGuides(a4, s.gx, s.gy);
    }

    function onUp(e) {
      el.classList.remove('is-dragging');
      document.body.classList.remove('is-interacting');
      document.querySelectorAll('.snap-guide,.snap-label').forEach(g => g.remove());
      
      resolveAllOverlaps(a4);
      
      if (typeof isCvDirty !== 'undefined') isCvDirty = true;
      if (initInteract._onDragEnd) initInteract._onDragEnd(el);
      
      document.removeEventListener('mousemove', onMove);
      document.removeEventListener('mouseup',   onUp);
    }

    document.addEventListener('mousemove', onMove);
    document.addEventListener('mouseup',   onUp);
  });
}

/* ══ BUILD SECTION DOM ELEMENT ══════════════════════════════════════════ */
function buildSectionEl(s, pi, mode) {
  const def = SEC[s.type];
  if (!def) return null;

  const el = div('cv-sec resize-drag');
  el.dataset.type = s.type;
  el.dataset.page = pi;
  el.style.top = (s.top || 10) + 'px';
  el.style.left = (s.left || 10) + 'px';
  if (s.width) el.style.width = s.width + 'px';
  if (s.height) el.style.height = s.height + 'px';

  const barHtml = `<div class="sec-bar">
    <span class="drag-handle" title="Kéo thả để di chuyển mục">❖</span>
    <div class="sec-bar-sep"></div>
    <button type="button" class="sec-btn" title="Di chuyển lên" onclick="moveSecUp(this.closest('.cv-sec'))">↑</button>
    <button type="button" class="sec-btn" title="Di chuyển xuống" onclick="moveSecDown(this.closest('.cv-sec'))">↓</button>
    <div class="sec-bar-sep"></div>
    <button type="button" class="sec-btn danger" title="Xoá khối" onclick="removeSec(this.closest('.cv-sec'))">Xóa</button>
  </div>`;
  el.innerHTML = barHtml;

  // Initialize custom drag for the drag-handle
  _initCustomDrag(el);

  if (s.html && mode === 'candidate') {
    const cw = div('cv-content-wrap');
    cw.innerHTML = s.html;
    el.appendChild(cw);
  } else {
    el.appendChild(def.build());
  }

  // Candidate-only: inject add/remove sub-item buttons after DOM is ready
  if (mode === 'candidate') {
    requestAnimationFrame(() => injectAddItemButtons(el));
  }

  return el;
}

/* ══ THEME APPLICATION ══════════════════════════════════════════════════ */
function applyThemeToCanvas(primaryColor, font) {
  document.documentElement.style.setProperty('--pc', primaryColor);
  document.querySelectorAll('.a4').forEach(a => a.style.fontFamily = `'${font}', sans-serif`);
  document.querySelectorAll('.cv-sec-head').forEach(h => h.style.borderBottomColor = primaryColor);
  document.querySelectorAll('.cv-header').forEach(h => h.style.borderBottomColor = primaryColor);
  document.querySelectorAll('.cv-tag').forEach(t => {
    t.style.color = primaryColor;
    t.style.background = primaryColor + '18';
    t.style.borderColor = primaryColor + '50';
  });
  document.querySelectorAll('.cv-pos, .cv-vcard-title').forEach(p => p.style.color = primaryColor);
  document.querySelectorAll('.cv-cg-label').forEach(l => l.style.color = primaryColor);
}

/* ══ FLOATING FORMAT TOOLBAR (Tiptap + plain contenteditable) ══════════ */
function _injectTiptapToolbar() {
  if (document.getElementById('tt-toolbar')) return;
  const tb = document.createElement('div');
  tb.id = 'tt-toolbar';
  tb.innerHTML = `
    <select class="tt-sel" id="tt-size" title="Cỡ chữ"><option value="">Cỡ</option>
      <option>8</option><option>9</option><option>10</option><option>11</option>
      <option>12</option><option>13</option><option>14</option><option>16</option>
      <option>18</option><option>20</option><option>24</option><option>28</option><option>32</option>
    </select>
    <select class="tt-sel tt-font-sel" id="tt-font" title="Font chữ" style="max-width:110px">
      <option value="Inter">Inter</option>
      <option value="Be Vietnam Pro">Be Vietnam Pro</option>
      <option value="Roboto">Roboto</option>
      <option value="Open Sans">Open Sans</option>
      <option value="Lora">Lora</option>
    </select>
    <label class="tt-color-wrap" title="Màu chữ">
      <span class="tt-color-dot" id="tt-color-dot" style="background:#111827"></span>
      <input type="color" id="tt-color-input" value="#111827"/>
    </label>
    <div class="tt-sep"></div>
    <button data-cmd="bold" title="Đậm"><b>B</b></button>
    <button data-cmd="italic" title="Nghiêng"><i>I</i></button>
    <button data-cmd="underline" title="Gạch chân"><u>U</u></button>
    <div class="tt-sep"></div>
    <button data-cmd="orderedList" class="tt-tiptap-only" title="Danh sách số" style="font-size:.75rem">1≡</button>
    <button data-cmd="bulletList"  class="tt-tiptap-only" title="Danh sách chấm" style="font-size:.75rem">•≡</button>
    <div class="tt-sep tt-tiptap-only"></div>
    <button data-cmd="alignLeft"   class="tt-tiptap-only" title="Căn trái"   style="font-size:.85rem">&#8676;</button>
    <button data-cmd="alignCenter" class="tt-tiptap-only" title="Căn giữa"  style="font-size:.85rem">&#8596;</button>
    <button data-cmd="alignRight"  class="tt-tiptap-only" title="Căn phải"  style="font-size:.85rem">&#8677;</button>
    <button data-cmd="alignJustify" class="tt-tiptap-only" title="Căn đều"  style="font-size:.7rem">&#9776;</button>
    <div class="tt-sep"></div>
    <button data-cmd="uppercase" title="CHỮ HOA" style="font-size:.62rem;letter-spacing:.03em">AA</button>
    <button data-cmd="lowercase" title="chữ thường" style="font-size:.62rem">aa</button>
  `;
  (document.body || document.documentElement).appendChild(tb);

  // Font size
  document.getElementById('tt-size').addEventListener('change', function() {
    const sz = this.value; if (!sz) return;
    if (_ttActiveEditor) {
      _ttActiveEditor.chain().focus().setMark('textStyle', { fontSize: sz + 'px' }).run();
    } else {
      _execOnSelection(sel => {
        const range = sel.getRangeAt(0);
        if (!range.collapsed) { const sp = document.createElement('span'); sp.style.fontSize = sz + 'px'; try { range.surroundContents(sp); } catch(e) {} }
      });
    }
  });

  // Font family
  document.getElementById('tt-font').addEventListener('change', function() {
    const ff = this.value; if (!ff) return;
    if (_ttActiveEditor) {
      _ttActiveEditor.chain().focus().setFontFamily(ff).run();
    } else {
      _execOnSelection(sel => { const range = sel.getRangeAt(0); if (!range.collapsed) { const sp = document.createElement('span'); sp.style.fontFamily = ff; try { range.surroundContents(sp); } catch(e) {} } });
    }
  });

  // Text color
  document.getElementById('tt-color-input').addEventListener('input', function() {
    const c = this.value;
    document.getElementById('tt-color-dot').style.background = c;
    if (_ttActiveEditor) _ttActiveEditor.chain().focus().setColor(c).run();
    else document.execCommand('foreColor', false, c);
  });
}
if (document.body) {
  _injectTiptapToolbar();
} else {
  document.addEventListener('DOMContentLoaded', _injectTiptapToolbar);
}

let _ttActiveEditor = null;
let _ttHideTimer = null;

function _ttUpdateActiveButtons() {
  if (!_ttActiveEditor) return;
  const tb = document.getElementById('tt-toolbar');
  if (!tb) return;
  tb.querySelectorAll('button[data-cmd]').forEach(btn => {
    const cmd = btn.dataset.cmd;
    btn.classList.toggle('is-active', _ttActiveEditor.isActive(cmd));
  });
}

function _ttPositionToolbar(el) {
  const tb = document.getElementById('tt-toolbar');
  if (!tb) return;
  const rect = el.getBoundingClientRect();
  tb.style.display = 'flex';
  const tbH = 36;
  let top = rect.top - tbH - 6 + window.scrollY;
  let left = rect.left + window.scrollX;
  if (top < 4) top = rect.bottom + 6 + window.scrollY;
  const tbW = tb.offsetWidth || 200;
  if (left + tbW > window.innerWidth - 8) left = window.innerWidth - tbW - 8;
  tb.style.top = top + 'px';
  tb.style.left = left + 'px';
}

function initTiptapFloating() {
  // Tiptap loads as an ESM module which may resolve after DOMContentLoaded.
  // If window.tiptap is already set, proceed immediately; otherwise wait.
  if (window.tiptap && window.tiptap.Editor) {
    _startTiptap();
  } else {
    document.addEventListener('tiptap-ready', _startTiptap, { once: true });
  }
}

function _startTiptap() {
  const tt = window.tiptap;
  if (!tt || !tt.Editor || !tt.StarterKit) { console.warn('Tiptap not fully loaded'); return; }

  const { Editor, StarterKit, Underline, TextStyle, Color, TextAlign, FontFamily } = tt;
  const extensions = [StarterKit];
  if (Underline) extensions.push(Underline);
  if (TextStyle) {
    const TSExt = TextStyle.extend({
      addAttributes() {
        return { ...this.parent?.(), fontSize: {
          default: null,
          parseHTML: el => el.style.fontSize || null,
          renderHTML: attrs => attrs.fontSize ? { style: `font-size:${attrs.fontSize}` } : {}
        }};
      }
    });
    extensions.push(TSExt);
  }
  if (Color && TextStyle) extensions.push(Color.configure({ types: ['textStyle'] }));
  if (FontFamily) extensions.push(FontFamily.configure({ types: ['textStyle'] }));
  if (TextAlign) extensions.push(TextAlign.configure({ types: ['heading', 'paragraph'] }));

  const tb = document.getElementById('tt-toolbar');
  if (!tb) return;

  // Toolbar button clicks — attach once
  if (!tb._ttBound) {
    tb._ttBound = true;
    tb.addEventListener('mousedown', e => {
      const btn = e.target.closest('button[data-cmd]');
      if (!btn) return;
      e.preventDefault();
      const cmd = btn.dataset.cmd;

      if (cmd === 'uppercase' || cmd === 'lowercase') {
        const sel = window.getSelection();
        if (sel && !sel.isCollapsed) {
          const text = sel.toString();
          document.execCommand('insertText', false, cmd === 'uppercase' ? text.toUpperCase() : text.toLowerCase());
        } else if (_ttActiveEditor) {
          const { from, to } = _ttActiveEditor.state.selection;
          const text = _ttActiveEditor.state.doc.textBetween(from, to);
          if (text) _ttActiveEditor.chain().focus().insertContentAt({ from, to }, cmd === 'uppercase' ? text.toUpperCase() : text.toLowerCase()).run();
        }
        return;
      }

      if (_ttActiveEditor) {
        const chain = _ttActiveEditor.chain().focus();
        if (cmd === 'bold')           chain.toggleBold().run();
        else if (cmd === 'italic')    chain.toggleItalic().run();
        else if (cmd === 'underline') chain.toggleUnderline().run();
        else if (cmd === 'bulletList')  chain.toggleBulletList().run();
        else if (cmd === 'orderedList') chain.toggleOrderedList().run();
        else if (cmd === 'alignLeft')    chain.setTextAlign('left').run();
        else if (cmd === 'alignCenter')  chain.setTextAlign('center').run();
        else if (cmd === 'alignRight')   chain.setTextAlign('right').run();
        else if (cmd === 'alignJustify') chain.setTextAlign('justify').run();
        _ttUpdateActiveButtons();
      } else {
        if (cmd === 'bold')           document.execCommand('bold');
        else if (cmd === 'italic')    document.execCommand('italic');
        else if (cmd === 'underline') document.execCommand('underline');
      }
    });
  }

  // Show toolbar when plain contenteditable (non-Tiptap) is focused
  document.addEventListener('focusin', (e) => {
    const ce = e.target.closest('[contenteditable="true"]');
    if (!ce || ce.classList.contains('tiptap-rich') || ce.closest('.cv-sec') === null) return;
    if (ce._tiptapEditor) return; // handled by Tiptap
    // For plain CE: show all buttons but mark tiptap-only as dimmed
    document.querySelectorAll('.tt-tiptap-only').forEach(el => {
      el.style.display = '';
      el.style.opacity = '0.35';
      el.style.pointerEvents = 'none';
    });
    _ttActiveEditor = null;
    _ttPositionToolbar(ce);
  });

  document.addEventListener('focusout', (e) => {
    const ce = e.target.closest('[contenteditable="true"]');
    if (!ce || ce.classList.contains('tiptap-rich')) return;
    setTimeout(() => {
      const tb2 = document.getElementById('tt-toolbar');
      if (tb2 && !tb2.contains(document.activeElement)) tb2.style.display = 'none';
    }, 200);
  });
  // Show tiptap-only when Tiptap is active
  function _showTiptapControls(show) {
    document.querySelectorAll('.tt-tiptap-only').forEach(el => {
      el.style.display = '';
      el.style.opacity = show ? '' : '0.35';
      el.style.pointerEvents = show ? '' : 'none';
    });
  }

  function mountTiptap(el) {
    if (el._tiptapEditor) return;
    const initialHtml = el.innerHTML;
    el.innerHTML = '';
    el.removeAttribute('contenteditable');
    el.removeAttribute('data-ph');
    el.classList.add('tiptap-rich');

    const editor = new Editor({
      element: el,
      extensions,
      content: initialHtml || '<p></p>',
      onFocus() {
        clearTimeout(_ttHideTimer);
        _ttActiveEditor = editor;
        _showTiptapControls(true);
        _ttPositionToolbar(el);
        _ttUpdateActiveButtons();
      },
      onBlur() {
        _ttHideTimer = setTimeout(() => {
          if (_ttActiveEditor === editor) {
            _ttActiveEditor = null;
            const tb2 = document.getElementById('tt-toolbar');
            if (tb2) tb2.style.display = 'none';
          }
        }, 200);
      },
      onUpdate() {
        _ttUpdateActiveButtons();
        if (typeof isCvDirty !== 'undefined') isCvDirty = true;
      }
    });

    el._tiptapEditor = editor;
  }

  // Observe canvas for dynamically added sections
  const canvasEl = document.getElementById('canvas') || document.body;
  const observer = new MutationObserver(mutations => {
    mutations.forEach(m => {
      m.addedNodes.forEach(node => {
        if (node.nodeType !== 1) return;
        const fields = node.matches('.cv-desc,.cv-simple')
          ? [node]
          : Array.from(node.querySelectorAll('.cv-desc,.cv-simple'));
        fields.forEach(mountTiptap);
      });
    });
  });
  observer.observe(canvasEl, { childList: true, subtree: true });

  // Mount on already-rendered fields
  document.querySelectorAll('.cv-desc,.cv-simple').forEach(mountTiptap);
}

/* Public helper: get rich HTML from a field (Tiptap or plain contenteditable) */
function getRichHtml(el) {
  if (el && el._tiptapEditor) return el._tiptapEditor.getHTML();
  return el ? el.innerHTML : '';
}

/* ══ SUB-ITEM TEMPLATES ═════════════════════════════════════════════════ */
const SUB_ITEM_TEMPLATES = {
  experience: () => `
    <div class="cv-exp-item">
      <button type="button" class="cv-item-del" onclick="removeSubItem(this)" title="Xoá">✕</button>
      <div class="cv-exp-header">
        <span class="cv-company" contenteditable="true" spellcheck="false" data-ph="Tên công ty">Tên công ty mới</span>
        <span class="cv-date" contenteditable="true" spellcheck="false">Tháng/Năm – Tháng/Năm</span>
      </div>
      <span class="cv-pos" contenteditable="true" spellcheck="false" data-ph="Vị trí">Vị trí làm việc</span>
      <div class="cv-desc" contenteditable="true" spellcheck="false" data-ph="Mô tả công việc...">• Mô tả chi tiết trách nhiệm và thành tựu</div>
    </div>`,
  education: () => `
    <div class="cv-exp-item">
      <button type="button" class="cv-item-del" onclick="removeSubItem(this)" title="Xoá">✕</button>
      <div class="cv-exp-header">
        <span class="cv-company" contenteditable="true" spellcheck="false" data-ph="Tên trường">Tên trường</span>
        <span class="cv-date" contenteditable="true" spellcheck="false">Năm – Năm</span>
      </div>
      <span class="cv-pos" contenteditable="true" spellcheck="false" data-ph="Ngành học">Ngành học</span>
      <div class="cv-desc" contenteditable="true" spellcheck="false" data-ph="Thành tích...">GPA: x.y / 4.0</div>
    </div>`,
  projects: () => `
    <div class="cv-exp-item">
      <button type="button" class="cv-item-del" onclick="removeSubItem(this)" title="Xoá">✕</button>
      <div class="cv-exp-header">
        <span class="cv-company" contenteditable="true" spellcheck="false" data-ph="Tên dự án">Tên dự án</span>
        <span class="cv-date" contenteditable="true" spellcheck="false">Năm</span>
      </div>
      <span class="cv-pos" contenteditable="true" spellcheck="false" data-ph="Vai trò / Công nghệ">Vai trò · Công nghệ</span>
      <div class="cv-desc" contenteditable="true" spellcheck="false" data-ph="Mô tả...">• Mô tả mục tiêu và kết quả đạt được</div>
    </div>`,
  awards: () => `
    <div class="cv-exp-item">
      <button type="button" class="cv-item-del" onclick="removeSubItem(this)" title="Xoá">✕</button>
      <div class="cv-exp-header">
        <span class="cv-company" contenteditable="true" spellcheck="false" data-ph="Tên giải thưởng">Tên giải thưởng</span>
        <span class="cv-date" contenteditable="true" spellcheck="false">Năm</span>
      </div>
      <div class="cv-desc" contenteditable="true" spellcheck="false" data-ph="Tổ chức trao...">Tổ chức trao tặng</div>
    </div>`,
  certs: () => `
    <div class="cv-exp-item">
      <button type="button" class="cv-item-del" onclick="removeSubItem(this)" title="Xoá">✕</button>
      <div class="cv-exp-header">
        <span class="cv-company" contenteditable="true" spellcheck="false" data-ph="Tên chứng chỉ">Tên chứng chỉ</span>
        <span class="cv-date" contenteditable="true" spellcheck="false">Năm</span>
      </div>
      <div class="cv-desc" contenteditable="true" spellcheck="false" data-ph="Tổ chức cấp...">Tổ chức cấp · Mã chứng chỉ</div>
    </div>`,
  activities: () => `
    <div class="cv-exp-item">
      <button type="button" class="cv-item-del" onclick="removeSubItem(this)" title="Xoá">✕</button>
      <div class="cv-exp-header">
        <span class="cv-company" contenteditable="true" spellcheck="false" data-ph="Tên tổ chức">CLB / Tổ chức</span>
        <span class="cv-date" contenteditable="true" spellcheck="false">Năm – Năm</span>
      </div>
      <span class="cv-pos" contenteditable="true" spellcheck="false" data-ph="Vai trò">Vai trò</span>
      <div class="cv-desc" contenteditable="true" spellcheck="false" data-ph="Mô tả...">Mô tả đóng góp và kết quả</div>
    </div>`
};

const SUB_ITEM_LABELS = {
  experience: '+ Thêm kinh nghiệm',
  education: '+ Thêm học vấn',
  projects: '+ Thêm dự án',
  awards: '+ Thêm giải thưởng',
  certs: '+ Thêm chứng chỉ',
  activities: '+ Thêm hoạt động'
};

/* Append a new sub-item and auto-grow section height */
function appendSubItem(btn) {
  const secEl = btn.closest('.cv-sec');
  if (!secEl) return;
  const type = secEl.dataset.type;
  const tpl = SUB_ITEM_TEMPLATES[type];
  if (!tpl) return;
  const itemsWrap = btn.closest('.cv-std').querySelector('.cv-items-wrap');
  if (!itemsWrap) return;
  const tmp = document.createElement('div');
  tmp.innerHTML = tpl().trim();
  const newItem = tmp.firstElementChild;
  itemsWrap.appendChild(newItem);
  // auto-grow height
  const needed = secEl.querySelector('.cv-content-wrap').scrollHeight + 26;
  secEl.style.height = Math.max(parseFloat(secEl.style.height) || 0, needed) + 'px';
  if (typeof isCvDirty !== 'undefined') isCvDirty = true;
}

function removeSubItem(btn) {
  const item = btn.closest('.cv-exp-item');
  if (!item) return;
  item.remove();
  if (typeof isCvDirty !== 'undefined') isCvDirty = true;
}

/* ══ CONTEXT TOOLBAR FOR SECTIONS ══════════════════════════════════════ */
let _ctxTarget = null;

function _ensureCtxToolbar() {
  if (document.getElementById('ctx-toolbar')) return;
  const tb = document.createElement('div');
  tb.id = 'ctx-toolbar';
  tb.innerHTML = `
    <span class="ctx-section-title" id="ctx-lbl">Khối</span>
    <div class="ctx-sep"></div>
    <label class="ctx-color-btn" title="Màu nền khối">
      <div class="ctx-color-preview" id="ctx-bg-preview"></div>
      <input type="color" id="ctx-bg-color" value="#ffffff"/>
    </label>
    <div class="ctx-sep"></div>
    <button class="ctx-btn" onclick="_ctxDup()" title="Nhân đôi">⧉ Sao chép</button>
    <button class="ctx-btn danger" onclick="_ctxDel()" title="Xoá khối">✕ Xoá</button>
  `;
  document.body.appendChild(tb);

  document.getElementById('ctx-bg-color').addEventListener('input', (e) => {
    if (!_ctxTarget) return;
    _ctxTarget.style.background = e.target.value;
    document.getElementById('ctx-bg-preview').style.background = e.target.value;
    if (typeof isCvDirty !== 'undefined') isCvDirty = true;
  });
}

function _positionCtxToolbar(secEl) {
  const tb = document.getElementById('ctx-toolbar');
  if (!tb) return;
  const rect = secEl.getBoundingClientRect();
  tb.classList.add('visible');
  let top = rect.top - 46;
  let left = rect.left;
  if (top < 6) top = rect.bottom + 6;
  const tbW = tb.offsetWidth || 240;
  if (left + tbW > window.innerWidth - 8) left = window.innerWidth - tbW - 8;
  tb.style.top = top + 'px';
  tb.style.left = left + 'px';
}

function _hideCtxToolbar() {
  const tb = document.getElementById('ctx-toolbar');
  if (tb) tb.classList.remove('visible');
  if (_ctxTarget) {
    _ctxTarget.classList.remove('ctx-selected');
    _ctxTarget = null;
  }
}

function _ctxDup() {
  if (!_ctxTarget || typeof dupSec !== 'function') return;
  dupSec(_ctxTarget);
  _hideCtxToolbar();
}

function _ctxDel() {
  if (!_ctxTarget || typeof removeSec !== 'function') return;
  removeSec(_ctxTarget);
  _hideCtxToolbar();
}

function initContextualToolbar() {
  _ensureCtxToolbar();

  // Click on a section (but not on a contenteditable, button, or drag-handle)
  document.addEventListener('mousedown', (e) => {
    const secEl = e.target.closest('.cv-sec');
    // Ignore clicks on editable text, buttons, or the toolbar itself
    if (e.target.closest('#ctx-toolbar') || e.target.closest('#tt-toolbar')) return;

    if (!secEl) {
      _hideCtxToolbar();
      return;
    }

    // Don't steal focus from contenteditable
    if (e.target.hasAttribute('contenteditable') || e.target.closest('[contenteditable]') || e.target.closest('button') || e.target.closest('label') || e.target.closest('.drag-handle')) {
      _hideCtxToolbar();
      return;
    }

    // Show toolbar for this section
    if (_ctxTarget) _ctxTarget.classList.remove('ctx-selected');
    _ctxTarget = secEl;
    secEl.classList.add('ctx-selected');

    const SEC_local = typeof SEC !== 'undefined' ? SEC : {};
    const lbl = document.getElementById('ctx-lbl');
    if (lbl) lbl.textContent = SEC_local[secEl.dataset.type]?.label || 'Khối';

    // Sync current background color
    const curBg = secEl.style.background || '#ffffff';
    const pick = document.getElementById('ctx-bg-color');
    const prev = document.getElementById('ctx-bg-preview');
    if (pick && /^#/.test(curBg)) { pick.value = curBg; }
    if (prev) prev.style.background = curBg;

    requestAnimationFrame(() => _positionCtxToolbar(secEl));
  });

  // Hide when clicking outside any section
  document.addEventListener('keydown', (e) => {
    if (e.key === 'Escape') _hideCtxToolbar();
  });
}

/* ══ ZOOM CONTROL ═══════════════════════════════════════════════════════ */
let _currentZoom = 1.0;

function initZoomControl() {
  if (document.getElementById('zoom-bar')) return;
  const bar = document.createElement('div');
  bar.id = 'zoom-bar';
  bar.innerHTML = `
    <button class="zoom-btn" onclick="setZoom(_currentZoom - 0.1)" title="Thu nhỏ">−</button>
    <span class="zoom-lbl" id="zoom-lbl">100%</span>
    <button class="zoom-btn" onclick="setZoom(_currentZoom + 0.1)" title="Phóng to">+</button>
    <div class="zoom-sep"></div>
    <button class="zoom-btn" id="zoom-50" onclick="setZoom(0.5)">50%</button>
    <button class="zoom-btn" id="zoom-75" onclick="setZoom(0.75)">75%</button>
    <button class="zoom-btn active" id="zoom-100" onclick="setZoom(1.0)">100%</button>
    <button class="zoom-btn" id="zoom-fit" onclick="zoomFit()">Fit</button>
  `;
  document.body.appendChild(bar);
}

function setZoom(val) {
  _currentZoom = Math.min(1.5, Math.max(0.3, val));
  const pct = Math.round(_currentZoom * 100);
  document.querySelectorAll('.a4').forEach(a4 => {
    a4.style.transformOrigin = 'top center';
    a4.style.transform = `scale(${_currentZoom})`;
    // Adjust the pgwrap height so scrolling still works
    const wrap = a4.closest('.pgwrap');
    if (wrap) {
      wrap.style.marginBottom = `${(1123 * _currentZoom) - 1123 + 24}px`;
    }
  });
  const lbl = document.getElementById('zoom-lbl');
  if (lbl) lbl.textContent = pct + '%';
  // Update active button
  ['50','75','100'].forEach(z => {
    const btn = document.getElementById('zoom-' + z);
    if (btn) btn.classList.toggle('active', parseInt(z) === pct);
  });
}

function zoomFit() {
  const canvas = document.getElementById('canvas');
  if (!canvas) return;
  const available = canvas.clientWidth - 60;
  const ratio = available / 794;
  setZoom(ratio);
}

/* ══ CANDIDATE: inject sub-item add-button into sections ════════════════ */
function injectAddItemButtons(sectionEl) {
  const type = sectionEl.dataset.type;
  const label = SUB_ITEM_LABELS[type];
  if (!label) return;
  const std = sectionEl.querySelector('.cv-std');
  if (!std) return;
  // Wrap existing exp-items in a container for easy append
  if (!std.querySelector('.cv-items-wrap')) {
    const items = Array.from(std.querySelectorAll('.cv-exp-item'));
    const wrap = document.createElement('div');
    wrap.className = 'cv-items-wrap';
    items.forEach(i => wrap.appendChild(i));
    // Insert before the add button position (after .cv-sec-head)
    const head = std.querySelector('.cv-sec-head');
    if (head) head.after(wrap);
    else std.prepend(wrap);
  }
  // Add delete button to existing items if not present
  std.querySelectorAll('.cv-exp-item').forEach(item => {
    if (!item.querySelector('.cv-item-del')) {
      const delBtn = document.createElement('button');
      delBtn.type = 'button';
      delBtn.className = 'cv-item-del';
      delBtn.title = 'Xoá';
      delBtn.textContent = '✕';
      delBtn.onclick = function() { removeSubItem(this); };
      item.appendChild(delBtn);
    }
  });
  // Add the + button if not present
  if (!std.querySelector('.cv-add-item-btn')) {
    const addBtn = document.createElement('button');
    addBtn.type = 'button';
    addBtn.className = 'cv-add-item-btn';
    addBtn.textContent = label;
    addBtn.onclick = function() { appendSubItem(this); };
    std.appendChild(addBtn);
  }
}

/* ══ OVERLAP PREVENTION ════════════════════════════════════════════════ */

function resolveAllOverlaps(a4) {
  if (!a4) return;
  const secs = Array.from(a4.querySelectorAll('.cv-sec'));
  // Sort elements vertically by top position
  secs.sort((a, b) => (parseFloat(a.style.top) || 0) - (parseFloat(b.style.top) || 0));

  for (let i = 0; i < secs.length; i++) {
    const cur = secs[i];
    const ct = parseFloat(cur.style.top) || 0;
    const cl = parseFloat(cur.style.left) || 0;
    const cw = parseFloat(cur.style.width) || cur.offsetWidth;
    const ch = parseFloat(cur.style.height) || cur.offsetHeight;

    for (let j = i + 1; j < secs.length; j++) {
      const next = secs[j];
      const nt = parseFloat(next.style.top) || 0;
      const nl = parseFloat(next.style.left) || 0;
      const nw = parseFloat(next.style.width) || next.offsetWidth;
      const nh = parseFloat(next.style.height) || next.offsetHeight;

      // Check horizontal intersection
      if (cl < nl + nw && cl + cw > nl) {
        // If vertically overlapping, push the next element down
        if (nt < ct + ch) {
          next.style.top = (ct + ch) + 'px';
          // Re-sort the remaining array since we modified `next.style.top`
          secs.sort((a, b) => (parseFloat(a.style.top) || 0) - (parseFloat(b.style.top) || 0));
        }
      }
    }
  }

  // Clamp to A4 boundaries
  secs.forEach(el => {
    const h = parseFloat(el.style.height) || el.offsetHeight;
    const t = parseFloat(el.style.top) || 0;
    el.style.top = Math.max(0, Math.min(A4_H - h, t)) + 'px';
  });
}


/* helper: run fn with current Selection if any */
function _execOnSelection(fn) {
  const sel = window.getSelection();
  if (sel && sel.rangeCount > 0) fn(sel);
}

