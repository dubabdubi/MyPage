// 조서희 포트폴리오 — 화면 동작
// 테마 전환, 상단바, 스크롤 등장, 숫자 올라가기, 이력 필터, 이미지 크게 보기

const root = document.documentElement;
const prefersReducedMotion = window.matchMedia("(prefers-reduced-motion: reduce)").matches;

/* 테마 전환 ------------------------------------------------ */
function setupThemeToggle() {
  const button = document.querySelector(".theme-toggle");
  const systemDark = window.matchMedia("(prefers-color-scheme: dark)");

  button.addEventListener("click", function () {
    const current = root.dataset.theme || (systemDark.matches ? "dark" : "light");
    const next = current === "dark" ? "light" : "dark";
    root.dataset.theme = next;
    try {
      localStorage.setItem("theme", next);
    } catch (e) {
      // 저장이 막혀 있어도 전환은 동작함
    }
  });
}

/* 상단바: 스크롤 시 테두리 + 현재 섹션 표시 ----------------- */
function setupTopbar() {
  const topbar = document.querySelector(".topbar");
  const links = document.querySelectorAll(".topbar__links a");

  window.addEventListener("scroll", function () {
    topbar.classList.toggle("is-scrolled", window.scrollY > 8);
  }, { passive: true });

  const observer = new IntersectionObserver(function (entries) {
    entries.forEach(function (entry) {
      if (!entry.isIntersecting) return;
      links.forEach(function (link) {
        const isCurrent = link.getAttribute("href") === "#" + entry.target.id;
        link.classList.toggle("is-current", isCurrent);
      });
    });
  }, { rootMargin: "-45% 0px -50% 0px" });

  links.forEach(function (link) {
    const section = document.querySelector(link.getAttribute("href"));
    if (section) observer.observe(section);
  });
}

/* 스크롤 등장 + 차트 그리기 --------------------------------- */
function setupReveal() {
  const targets = document.querySelectorAll(".reveal, .chart");

  const observer = new IntersectionObserver(function (entries) {
    entries.forEach(function (entry) {
      if (!entry.isIntersecting) return;
      entry.target.classList.add("is-visible");
      observer.unobserve(entry.target);
    });
  }, { rootMargin: "0px 0px -10% 0px" });

  targets.forEach(function (target) {
    observer.observe(target);
  });
}

/* 숫자 올라가기 -------------------------------------------- */
function countUp(element) {
  const goal = Number(element.dataset.count);
  const duration = 900;
  const start = performance.now();

  function step(now) {
    const progress = Math.min((now - start) / duration, 1);
    const eased = 1 - Math.pow(1 - progress, 3);
    element.textContent = Math.round(goal * eased);
    if (progress < 1) requestAnimationFrame(step);
  }

  requestAnimationFrame(step);
}

function setupCounters() {
  if (prefersReducedMotion) return;
  document.querySelectorAll("[data-count]").forEach(countUp);
}

/* 이력 필터 ------------------------------------------------ */
function setupHistoryFilter() {
  const buttons = document.querySelectorAll(".filter");
  const items = document.querySelectorAll(".history li");

  buttons.forEach(function (button) {
    button.addEventListener("click", function () {
      const filter = button.dataset.filter;

      buttons.forEach(function (other) {
        const isActive = other === button;
        other.classList.toggle("is-active", isActive);
        other.setAttribute("aria-pressed", String(isActive));
      });

      items.forEach(function (item) {
        const types = item.dataset.type.split(" ");
        item.hidden = filter !== "all" && !types.includes(filter);
      });
    });
  });
}

/* 이미지 크게 보기 ----------------------------------------- */
function setupLightbox() {
  const dialog = document.querySelector(".lightbox");
  const image = dialog.querySelector(".lightbox__img");
  const closeButton = dialog.querySelector(".lightbox__close");

  document.querySelectorAll("[data-lightbox]").forEach(function (shot) {
    shot.addEventListener("click", function () {
      const source = shot.querySelector("img");
      image.src = source.src;
      image.alt = source.alt;
      dialog.showModal();
    });
  });

  closeButton.addEventListener("click", function () {
    dialog.close();
  });

  // 바깥(배경)을 누르면 닫기
  dialog.addEventListener("click", function (event) {
    if (event.target === dialog) dialog.close();
  });
}

setupThemeToggle();
setupTopbar();
setupReveal();
setupCounters();
setupHistoryFilter();
setupLightbox();
