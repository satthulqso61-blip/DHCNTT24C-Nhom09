/**
 * khung.js — Tự động dựng sidebar từ window.TAI_LIEU và quét các mục trong trang (h2[id], [data-muc][id])
 * Tuân thủ quy chuẩn AGENTS.md: Chạy offline hoàn toàn qua giao thức file://, không dùng fetch/thư viện ngoài.
 */
(function () {
  function khoiTaoKhung() {
    var aside = document.querySelector('[data-khung-ben]');
    if (!aside) return;

    var taiLieu = window.TAI_LIEU;
    if (!taiLieu) {
      console.warn("Chưa tải được window.TAI_LIEU từ muc-luc.js");
      return;
    }

    // Lấy tên file hiện tại
    var duongDan = window.location.pathname;
    var tepHienTai = duongDan.substring(duongDan.lastIndexOf('/') + 1);
    if (!tepHienTai || tepHienTai === '') {
      tepHienTai = 'index.html';
    }

    // Dựng phần đầu sidebar
    var html = '';
    html += '<div class="ben__dau">';
    html += '  <a class="ben__ten" href="index.html">' + (taiLieu.ten || 'Tài liệu Spec') + '</a>';
    if (taiLieu.phu) {
      html += '  <div class="ben__phu">' + taiLieu.phu + '</div>';
    }
    html += '</div>';

    // Dựng danh sách điều hướng
    html += '<nav class="ben__muc-luc">';

    var trangDanhSach = taiLieu.trang || [];
    for (var i = 0; i < trangDanhSach.length; i++) {
      var item = trangDanhSach[i];
      if (item.nhom) {
        html += '<div class="ben__nhom">' + item.nhom + '</div>';
      } else if (item.tep) {
        var laHienTai = (item.tep === tepHienTai);
        var activeClass = laHienTai ? ' ben__link hien-tai' : ' ben__link';
        html += '<a class="' + activeClass + '" href="' + item.tep + '">' + item.ten + '</a>';

        // Nếu là trang hiện tại, quét các h2[id] và [data-muc][id] trong trang để sinh mục con
        if (laHienTai) {
          var cacMuc = document.querySelectorAll('main.noi-dung h2[id], main.noi-dung [data-muc][id]');
          if (cacMuc.length > 0) {
            html += '<div class="ben__muc-trong-trang">';
            for (var j = 0; j < cacMuc.length; j++) {
              var el = cacMuc[j];
              var id = el.getAttribute('id');
              var tieuDe = el.getAttribute('data-muc-ten') || el.textContent.trim();
              
              // Cắt bớt nếu tiêu đề quá dài
              if (tieuDe.length > 35) {
                tieuDe = tieuDe.substring(0, 32) + '...';
              }
              html += '<a class="ben__sublink" href="#' + id + '" title="' + el.textContent.trim().replace(/"/g, '&quot;') + '">' + tieuDe + '</a>';
            }
            html += '</div>';
          }
        }
      }
    }

    html += '</nav>';
    aside.innerHTML = html;
  }

  if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', khoiTaoKhung);
  } else {
    khoiTaoKhung();
  }
})();
