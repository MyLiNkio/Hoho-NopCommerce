document.addEventListener("DOMContentLoaded",function () {
  $(".header").on("mouseenter", "#topcartlink", (function () {
    $("#flyout-cart").addClass("active")
  }));
  $(".header").on("mouseleave", "#topcartlink", (function () {
    $("#flyout-cart").removeClass("active")
  }));
  $(".header").on("mouseenter", "#flyout-cart", (function () {
    $("#flyout-cart").addClass("active")
  }));
  $(".header").on("mouseleave", "#flyout-cart", (function () {
    $("#flyout-cart").removeClass("active")
  }));

  $(".owl-carousel2").owlCarousel({
    loop: 0,
    margin: 10,
    nav: !0,
    lazyLoad: !0,
    responsive: {
      0: {
        items: 1,
        margin: 5,
        stagePadding: 60,
      },
      500: {
        items: 2,
        margin: 5,
        stagePadding: 60,
      },
      991: {
        items: 3,
        stagePadding: 100,
      },
      1200: {
        items: 4,
        stagePadding: 100,
      }
    },

    // Callback to handle image loading
    onInitialized: function (event) {
      // Find the currently visible item and load the next item if it's lazy-loaded
      const currentIndex = $('.owl-carousel2 .owl-item.active').last().index();
      const nextIndex = (currentIndex + 1) % $('.owl-carousel .owl-item').length;

      // Trigger the loading of the next image
      const nextImage = $('.owl-carousel2 .owl-item').eq(nextIndex).find('img.owl-lazy');
      if (nextImage.length) {

        nextImage.each(function () {
          const imgSrc = $(this).data('srcset');
          if (imgSrc) {
            $(this).attr('src', imgSrc); // Load the image
            $(this).attr('style', 'opacity: 1;');
          }
        });
      }

      const nextIndexPlus1 = (currentIndex + 2) % $('.owl-carousel .owl-item').length;
      // Trigger the loading of the next image
      const nextImagePlus1 = $('.owl-carousel2 .owl-item').eq(nextIndexPlus1).find('img.owl-lazy');
      if (nextImagePlus1.length) {

        nextImagePlus1.each(function () {
          const imgSrc = $(this).data('srcset');
          if (imgSrc) {
            $(this).attr('src', imgSrc); // Load the image
            $(this).attr('style', 'opacity: 1;');
          }
        });
      }
    },
    onChanged: function (event) {
      // Find the currently visible item and load the next item if it's lazy-loaded
      const currentIndex = $('.owl-carousel2 .owl-item.active').last().index();
      const nextIndex = (currentIndex + 1) % $('.owl-carousel .owl-item').length;

      // Trigger the loading of the next image
      const nextImage = $('.owl-carousel2 .owl-item').eq(nextIndex).find('img.owl-lazy');
      if (nextImage.length) {

        nextImage.each(function () {
          const imgSrc = $(this).data('srcset');
          if (imgSrc) {
            $(this).attr('src', imgSrc); // Load the image
            $(this).attr('style', 'opacity: 1;');
          }
        });
      }

      const nextIndexPlus1 = (currentIndex + 2) % $('.owl-carousel .owl-item').length;
      // Trigger the loading of the next image
      const nextImagePlus1 = $('.owl-carousel2 .owl-item').eq(nextIndexPlus1).find('img.owl-lazy');
      if (nextImagePlus1.length) {

        nextImagePlus1.each(function () {
          const imgSrc = $(this).data('srcset');
          if (imgSrc) {
            $(this).attr('src', imgSrc); // Load the image
            $(this).attr('style', 'opacity: 1;');
          }
        });
      }
    },

  });
});

