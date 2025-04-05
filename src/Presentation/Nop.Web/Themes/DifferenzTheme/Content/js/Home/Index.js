$(document).ready((function () {
  $(".owl-carousel1").owlCarousel({ margin: 0, items: 1, nav: !1, autoplay: !0, loop: !0, autoplayTimeout: 6e3, animateOut: "fadeOut", animateIn: !0, smartSpeed: 1e3 }),
    $(".owl-carousel3").owlCarousel({ loop: !0, margin: 20, dots: !0, nav: !0, responsive: { 0: { items: 1 }, 1e3: { items: 2 } } }),
    $(".owl-carousel4").owlCarousel({
    autoplay: 1,
    autoplayTimeout: 10000,
    autoplaySpeed: 10000,
    autoplayHoverPause: 1,
    lazyLoadEager: 1,
      lazyLoad: 1,
      loop: !0,
      margin: 5,
      nav: !0,
      responsive: {
        0: { items: 2, stagePadding: 0 },
        500: { items: 2, stagePadding: 30 },
        1024: { items: 3, stagePadding: 50 },
        1200: { items: 4, stagePadding: 100 }
      }
  })
}));