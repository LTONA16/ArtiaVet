// Carousel de Testimonios
document.addEventListener('DOMContentLoaded', function() {
    const slider = document.getElementById('testimonialSlider');
    const prevBtn = document.getElementById('prevBtn');
    const nextBtn = document.getElementById('nextBtn');
    const indicators = document.querySelectorAll('.carousel-indicator');
    
    let currentSlide = 0;
    const totalSlides = 4;
    let autoPlayInterval;

    function goToSlide(slideIndex) {
        currentSlide = slideIndex;
        if (currentSlide < 0) currentSlide = totalSlides - 1;
        if (currentSlide >= totalSlides) currentSlide = 0;
        
        const offset = -currentSlide * 100;
        slider.style.transform = `translateX(${offset}%)`;
        
        // Actualizar indicadores
        indicators.forEach((indicator, index) => {
            if (index === currentSlide) {
                indicator.classList.remove('bg-gray-300');
                indicator.classList.add('bg-[#1a7487]');
            } else {
                indicator.classList.remove('bg-[#1a7487]');
                indicator.classList.add('bg-gray-300');
            }
        });
    }

    function nextSlide() {
        goToSlide(currentSlide + 1);
    }

    function prevSlide() {
        goToSlide(currentSlide - 1);
    }

    function startAutoPlay() {
        autoPlayInterval = setInterval(nextSlide, 5000);
    }

    function stopAutoPlay() {
        clearInterval(autoPlayInterval);
    }

    // Event Listeners
    prevBtn.addEventListener('click', () => {
        prevSlide();
        stopAutoPlay();
        startAutoPlay();
    });

    nextBtn.addEventListener('click', () => {
        nextSlide();
        stopAutoPlay();
        startAutoPlay();
    });

    indicators.forEach((indicator, index) => {
        indicator.addEventListener('click', () => {
            goToSlide(index);
            stopAutoPlay();
            startAutoPlay();
        });
    });

    // Touch/Swipe support
    let touchStartX = 0;
    let touchEndX = 0;

    slider.addEventListener('touchstart', (e) => {
        touchStartX = e.changedTouches[0].screenX;
    });

    slider.addEventListener('touchend', (e) => {
        touchEndX = e.changedTouches[0].screenX;
        handleSwipe();
    });

    function handleSwipe() {
        if (touchEndX < touchStartX - 50) {
            nextSlide();
            stopAutoPlay();
            startAutoPlay();
        }
        if (touchEndX > touchStartX + 50) {
            prevSlide();
            stopAutoPlay();
            startAutoPlay();
        }
    }

    // Pausar autoplay cuando el mouse está sobre el carousel
    const carouselContainer = document.getElementById('testimonialCarousel');
    carouselContainer.addEventListener('mouseenter', stopAutoPlay);
    carouselContainer.addEventListener('mouseleave', startAutoPlay);

    // Iniciar autoplay
    startAutoPlay();
});